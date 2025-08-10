using InvoiceAssistant.Application.Common;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Common;

public sealed class PeriodResolver
{
    private readonly PeriodResolverOptions _opt;
    private TimeZoneInfo Tz => TimeZoneInfo.FindSystemTimeZoneById(_opt.TenantTimeZoneId);

    public PeriodResolver(PeriodResolverOptions opt) => _opt = opt;

    public (DateTimeOffset start, DateTimeOffset end) Resolve(string raw)
    {
        string p = (raw ?? "").Trim().ToLowerInvariant();
        var nowLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Tz);
        var today = nowLocal.Date;

        // Arabic quick terms
        if (Regex.IsMatch(p, @"\bاليوم\b")) return StartEndOfDay(today, today);
        if (Regex.IsMatch(p, @"\bأمس\b")) return StartEndOfDay(today.AddDays(-1), today.AddDays(-1));
        if (Regex.IsMatch(p, @"\bهذا الأسبوع\b")) return ThisWeek(today);
        if (Regex.IsMatch(p, @"\bالأسبوع الماضي\b")) return LastWeek(today);
        if (Regex.IsMatch(p, @"\bهذا الشهر\b")) return ThisMonth(today);
        if (Regex.IsMatch(p, @"\bالشهر الماضي\b")) return LastMonth(today);

        // English quick terms
        if (p == "today") return StartEndOfDay(today, today);
        if (p == "yesterday") return StartEndOfDay(today.AddDays(-1), today.AddDays(-1));
        if (p == "this week") return ThisWeek(today);
        if (p == "last week") return LastWeek(today);
        if (p is "this month" or "mtd") return ThisMonth(today);
        if (p == "last month") return LastMonth(today);
        if (p == "ytd") return YearToDate(today);
        if (p is "qtd" or "this quarter") return ThisQuarter(today);
        if (p == "last quarter") return LastQuarter(today);
        if (p == "last 7 days") return RollingDays(today, 7);
        if (p == "last 30 days") return RollingDays(today, 30);
        if (p == "last 90 days") return RollingDays(today, 90);

        // between/from ... to ...
        var between = Regex.Match(p, @"(?:between|from)\s+(.+?)\s+(?:and|to)\s+(.+)");
        if (between.Success && TryParseDate(between.Groups[1].Value, out var s) && TryParseDate(between.Groups[2].Value, out var e))
            return StartEndInclusive(s, e);

        // in Month Year
        var inMonth = Regex.Match(p, @"\bin\s+([A-Za-z]+)\s+(\d{4})");
        if (inMonth.Success && TryMonth(inMonth.Groups[1].Value, out int mo))
        {
            int yr = int.Parse(inMonth.Groups[2].Value, CultureInfo.InvariantCulture);
            var first = new DateTime(yr, mo, 1);
            var last = first.AddMonths(1).AddDays(-1);
            return StartEndOfDay(first, last);
        }

        // Q1/Q2/Q3/Q4 YYYY
        var qMatch = Regex.Match(p, @"\bq([1-4])\s*(\d{4})\b");
        if (qMatch.Success)
        {
            int q = int.Parse(qMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            int yr = int.Parse(qMatch.Groups[2].Value, CultureInfo.InvariantCulture);
            int startMonth = 1 + (q - 1) * 3;
            var first = new DateTime(yr, startMonth, 1);
            var last = first.AddMonths(3).AddDays(-1);
            return StartEndOfDay(first, last);
        }

        // FY2024
        var fy = Regex.Match(p, @"\bfy\s*(\d{4})\b");
        if (fy.Success)
        {
            int yr = int.Parse(fy.Groups[1].Value, CultureInfo.InvariantCulture);
            var first = new DateTime(yr, _opt.FiscalYearStartMonth, 1);
            var last = first.AddYears(1).AddDays(-1);
            return StartEndOfDay(first, last);
        }

        // Year only
        var yearOnly = Regex.Match(p, @"\b(\d{4})\b");
        if (yearOnly.Success)
        {
            int yr = int.Parse(yearOnly.Groups[1].Value, CultureInfo.InvariantCulture);
            var first = new DateTime(yr, 1, 1);
            var last = new DateTime(yr, 12, 31);
            return StartEndOfDay(first, last);
        }

        // Fallback: today
        return StartEndOfDay(today, today);
    }

    private (DateTimeOffset start, DateTimeOffset end) ThisWeek(DateTime local)
    {
        int delta = ((int)local.DayOfWeek - (int)_opt.WeekStart + 7) % 7;
        var startLocal = local.AddDays(-delta);
        var endLocal = startLocal.AddDays(6);
        return StartEndOfDay(startLocal, endLocal);
    }

    private (DateTimeOffset start, DateTimeOffset end) LastWeek(DateTime local)
    {
        int delta = ((int)local.DayOfWeek - (int)_opt.WeekStart + 7) % 7;
        var startLocal = local.AddDays(-delta - 7);
        var endLocal = startLocal.AddDays(6);
        return StartEndOfDay(startLocal, endLocal);
    }

    private (DateTimeOffset start, DateTimeOffset end) ThisMonth(DateTime local)
    {
        var first = new DateTime(local.Year, local.Month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        return StartEndOfDay(first, last);
    }

    private (DateTimeOffset start, DateTimeOffset end) LastMonth(DateTime local)
    {
        var lm = local.AddMonths(-1);
        var first = new DateTime(lm.Year, lm.Month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        return StartEndOfDay(first, last);
    }

    private (DateTimeOffset start, DateTimeOffset end) YearToDate(DateTime local)
        => StartEndOfDay(new DateTime(local.Year, 1, 1), local);

    private (DateTimeOffset start, DateTimeOffset end) ThisQuarter(DateTime local)
    {
        int q = ((local.Month - 1) / 3) + 1;
        int startMonth = 1 + (q - 1) * 3;
        var first = new DateTime(local.Year, startMonth, 1);
        var last = first.AddMonths(3).AddDays(-1);
        return StartEndOfDay(first, last);
    }

    private (DateTimeOffset start, DateTimeOffset end) LastQuarter(DateTime local)
    {
        int q = ((local.Month - 1) / 3) + 1;
        int prevQ = q == 1 ? 4 : q - 1;
        int year = q == 1 ? local.Year - 1 : local.Year;
        int startMonth = 1 + (prevQ - 1) * 3;
        var first = new DateTime(year, startMonth, 1);
        var last = first.AddMonths(3).AddDays(-1);
        return StartEndOfDay(first, last);
    }

    private (DateTimeOffset start, DateTimeOffset end) RollingDays(DateTime local, int days)
    {
        var startLocal = local.AddDays(-days + 1);
        return StartEndOfDay(startLocal, local);
    }

    private (DateTimeOffset start, DateTimeOffset end) StartEndInclusive(DateTimeOffset s, DateTimeOffset e)
    {
        if (s > e) (s, e) = (e, s);
        var sLocal = TimeZoneInfo.ConvertTime(s, Tz).Date;
        var eLocal = TimeZoneInfo.ConvertTime(e, Tz).Date;
        return StartEndOfDay(sLocal, eLocal);
    }

    private (DateTimeOffset start, DateTimeOffset end) StartEndOfDay(DateTime startLocalDate, DateTime endLocalDate)
    {
        var start = new DateTimeOffset(startLocalDate.Year, startLocalDate.Month, startLocalDate.Day, 0, 0, 0, Tz.GetUtcOffset(startLocalDate)).ToUniversalTime();
        var end = new DateTimeOffset(endLocalDate.Year, endLocalDate.Month, endLocalDate.Day, 23, 59, 59, 999, Tz.GetUtcOffset(endLocalDate)).ToUniversalTime();
        return (start, end);
    }

    private static bool TryParseDate(string text, out DateTimeOffset dto)
    {
        text = text.Trim();
        var cultures = new[] { CultureInfo.InvariantCulture, new CultureInfo("en-US"), new CultureInfo("ar-EG") };
        var fmts = new[]
        {
            "yyyy-MM-dd","dd/MM/yyyy","M/d/yyyy","d/M/yyyy","dd-MM-yyyy","d-M-yyyy",
            "MMMM d, yyyy","d MMMM yyyy","MMM d, yyyy","d MMM yyyy"
        };
        foreach (var c in cultures)
        {
            if (DateTimeOffset.TryParse(text, c, DateTimeStyles.AssumeLocal, out dto))
                return true;
            foreach (var f in fmts)
                if (DateTimeOffset.TryParseExact(text, f, c, DateTimeStyles.AssumeLocal, out dto))
                    return true;
        }
        dto = default;
        return false;
    }

    private static bool TryMonth(string name, out int month)
    {
        try { month = DateTime.ParseExact(name, "MMMM", CultureInfo.InvariantCulture).Month; return true; }
        catch { month = 0; return false; }
    }
}
