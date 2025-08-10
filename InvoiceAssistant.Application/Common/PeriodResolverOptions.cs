using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceAssistant.Application.Common
{
    public sealed class PeriodResolverOptions
    {
        public string TenantTimeZoneId { get; init; } = "Africa/Cairo";
        public DayOfWeek WeekStart { get; init; } = DayOfWeek.Monday;
        public int FiscalYearStartMonth { get; init; } = 1; // January
    }
}
