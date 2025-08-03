
using InvoiceAssistant.Application.Contracts;
using InvoiceAssistant.Application.Dtos;
using System.Globalization;
using System.Text.RegularExpressions;

namespace InvoiceAssistant.Application.Services;

public class ChatbotService(
       ILLMClient llmClient,
       IInvoiceService invoiceService)
       : IChatbotService
{
    public async Task<string> AskQuestionAsync(QuestionDto userQuestion)
    {
        userQuestion.Question = userQuestion.Question?.Trim() ?? "";

        // 1. Step 1: Ask the LLM to route to a function call, not answer directly
        var now = DateTime.UtcNow;
        var systemPrompt = $@"
DO NOT answer the user's question. Only output the function name and parameters in the specified format if ALL required parameters are present.
If any required parameter (like a period) is missing or unclear, DO NOT output a function—politely ask the user to specify what is missing (e.g., 'Please specify the period you are asking about, such as ""this month"" or ""June {now.Year}"".')

You are an intelligent backend assistant in an invoice management system.
Your job is to analyze the user's question and determine which backend API function should be called,
along with the required parameters.

Supported functions and when to use them:
- GetInvoiceCount(period): Use for any question about the NUMBER of invoices within a specific time period.
  Example queries: 'How many invoices did we issue last week?', 'Number of invoices in June {now.Year}', etc.
- GetInvoiceSummary(invoiceNumber): Use when the user wants a SUMMARY of a specific invoice, by number.
  Example: 'Give me a summary of invoice {{ invoice number}}'
- GetTotalInvoiceValue(thePeriod): Use for any question about the TOTAL value/amount of invoices for a period.
  If no period is specified, assume an empty parameter.

Format your response strictly as follows:
Function: FUNCTION_NAME
Params: {{param1: '...', param2: '...'}}


If the user Question is not clear ignore the format response and ask him to be specific .

Now, given this user question, reply with the function and parameters and
If the user Question is not clear ignore the format response and ask him to be specific .

Q: {userQuestion.Question}
A:
";

        var routingReply = await llmClient.AskAsync(systemPrompt, "llama3.1");

        // 2. Parse function and params from LLM reply
        var functionMatch = Regex.Match(routingReply, @"Function:\s*(\w+)", RegexOptions.IgnoreCase);
        if (!functionMatch.Success)
            return "I'm not sure how to answer that. " +
       "Please ask about invoice totals, summaries, or counts for specific periods. ";

        var paramsMatch = Regex.Match(routingReply, @"Params:\s*\{([^\}]+)\}", RegexOptions.IgnoreCase);
        var function = functionMatch.Success ? functionMatch.Groups[1].Value : "";
        var paramsText = paramsMatch.Success ? paramsMatch.Groups[1].Value : "";

        // Utility: Convert params text to dictionary
        Dictionary<string, string> paramDict = new();
        foreach (Match m in Regex.Matches(paramsText, @"(\w+):\s*'([^']*)'"))
            paramDict[m.Groups[1].Value] = m.Groups[2].Value;

        // 3. Handle routed functions thePeriod
        switch (function)
        {
            case "GetTotalInvoiceValue":
                if (paramDict.TryGetValue("thePeriod", out var thePeriod))
                {
                    var (start, end) = ParsePeriod(thePeriod);
                    var invoices = await invoiceService.GetInvoicesAsync(new InvoiceFilterDto
                    {
                        StartDate = start,
                        EndDate = end
                    });
                    var total = invoices.Sum(i => i.TotalAmount);
                    // Step 2: Send summary to LLM for user-friendly answer
                    var prompt = $"User asked: {userQuestion.Question}\n" +
                                 $"Data You Need to answer is : Invoice value issued from {start:MMMM d, yyyy} to {end:MMMM d, yyyy} is SAR {total:N2}\n" +
                                 $"ONLY reply with one sentence. Do not add anything else.";
                    return await llmClient.AskAsync(prompt, "llama3.1");
                }
                break;

            case "GetInvoiceSummary":
                if (paramDict.TryGetValue("invoiceNumber", out var invoiceNumber))
                {
                    var invoices = await invoiceService.GetInvoicesAsync(new InvoiceFilterDto { InvoiceNumber = invoiceNumber });
                    var invoice = invoices.FirstOrDefault();
                    if (invoice == null)
                        return $"Invoice {invoiceNumber} not found.";
                    var prompt = $"User asked: {userQuestion.Question}\n" +
                                 $"Data :Invoice {invoice.InvoiceNumber} issued on {invoice.IssueDate:MMMM dd} for client '{invoice.ClientName}' totals SAR {invoice.TotalAmount:N2} with {invoice.InvoiceDetails?.Count ?? 0} items\n" +
                                 $"ONLY reply with one sentence. Do not add anything else.";
                    return await llmClient.AskAsync(prompt, "llama3.1");
                }
                break;

            case "GetInvoiceCount":
                if (paramDict.TryGetValue("period", out var period))
                {
                    var (start, end) = ParsePeriod(period);
                    var invoices = await invoiceService.GetInvoicesAsync(new InvoiceFilterDto
                    {
                        StartDate = start,
                        EndDate = end
                    });
                    var count = invoices.Count;
                    var prompt = $"User asked: {userQuestion.Question}\n" +
                                 $"Data You Need to answer is :You issued {count} invoices from {start:MMMM d, yyyy} to {end:MMMM d, yyyy}\n" +
                                 $"ONLY reply with one sentence Like the above data. Do not add anything else.";
                    //$"Example for replay : You issued {{count}} invoices from {{startDate}} to {{endDate}}.";

                    return await llmClient.AskAsync(prompt, "llama3.1");
                }
                return "Please specify the period you are asking about (e.g., 'last week', 'this month', 'June 2024').";
        }

        // Fallback: ask LLM directly if not routed
        return "I'm not sure how to answer that. " +
       "Please ask about invoice totals, summaries, or counts for specific periods. " +
       "For other questions, I will try my best to assist you.";
    }

    private (DateTime start, DateTime end) ParsePeriod(string period)
    {
        var now = DateTime.UtcNow.Date;

        // "between June 1, 2024 and June 10, 2024"
        var between = Regex.Match(period, @"between\s+(.+)\s+and\s+(.+)", RegexOptions.IgnoreCase);
        if (between.Success)
        {
            if (DateTime.TryParse(between.Groups[1].Value, out var s) && DateTime.TryParse(between.Groups[2].Value, out var e))
                return (s, e);
        }

        // "in July 2025"
        var inMonth = Regex.Match(period, @"in\s+([A-Za-z]+)\s+(\d{4})", RegexOptions.IgnoreCase);
        if (inMonth.Success)
        {
            var monthName = inMonth.Groups[1].Value;
            var year = int.Parse(inMonth.Groups[2].Value);
            var month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.InvariantCulture).Month;
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            return (start, end);
        }

        // "last week", "this month", etc.
        if (period.Equals("last week", StringComparison.OrdinalIgnoreCase))
        {
            var daysSinceMonday = ((int)now.DayOfWeek + 6) % 7;
            var lastWeekEnd = now.AddDays(-daysSinceMonday - 1);
            var lastWeekStart = lastWeekEnd.AddDays(-6);
            return (lastWeekStart, lastWeekEnd);
        }
        if (period.Equals("this month", StringComparison.OrdinalIgnoreCase))
        {
            var first = new DateTime(now.Year, now.Month, 1);
            var last = first.AddMonths(1).AddDays(-1);
            return (first, last);
        }
        if (period.Equals("last month", StringComparison.OrdinalIgnoreCase))
        {
            var lastMonth = now.AddMonths(-1);
            var first = new DateTime(lastMonth.Year, lastMonth.Month, 1);
            var last = first.AddMonths(1).AddDays(-1);
            return (first, last);
        }

        // "2023"
        var yearMatch = Regex.Match(period, @"(\d{4})");
        if (yearMatch.Success)
        {
            var year = int.Parse(yearMatch.Groups[1].Value);
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31);
            return (start, end);
        }

        // fallback: today
        return (now, now);
    }

}

