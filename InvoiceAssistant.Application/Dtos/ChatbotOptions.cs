
namespace InvoiceAssistant.Application.Dtos
{
    public sealed class ChatbotOptions
    {
        public string RouterModel { get; init; } = "llama3.1";
        public string SummaryModel { get; init; } = "llama3.1";
        public string TenantTimeZoneId { get; init; } = "Africa/Cairo";
        public string TenantCulture { get; init; } = "en-US"; // or "ar-EG"
        public string CurrencySymbol { get; init; } = "EGP";
        public int FiscalYearStartMonth { get; init; } = 1; // January
        public DayOfWeek WeekStart { get; init; } = DayOfWeek.Monday;
    }
}
