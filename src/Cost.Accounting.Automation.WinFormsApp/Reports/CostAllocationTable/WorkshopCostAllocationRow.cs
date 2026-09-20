using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable
{
    public sealed class ProductCostAllocationReportRow
    {
        public string WorkshopName { get; init; } = string.Empty;

        public decimal Account710 { get; init; }
        public decimal Account720_1 { get; init; }
        public decimal Account720_2 { get; init; }
        public decimal Account730_01 { get; init; }
        public decimal Account730_02 { get; init; }
        public decimal Account730_03 { get; init; }
        public decimal Account730_04 { get; init; }
        public decimal Account730_05 { get; init; }
        public decimal Account730_06 { get; init; }
        public decimal Account730_07 { get; init; }

        public decimal Total =>
            Account710 + Account720_1 + Account720_2 +
            Account730_01 + Account730_02 + Account730_03 +
            Account730_04 + Account730_05 + Account730_06 + Account730_07;

        public static ProductCostAllocationReportRow FromGiderDagilimRow(GiderDagilimRow row) => new()
        {
            WorkshopName = row.WorkshopName,
            Account710 = row.Values.GetValueOrDefault(ExpenseAccountType.Account710),
            Account720_1 = row.Values.GetValueOrDefault(ExpenseAccountType.Account720_1),
            Account720_2 = row.Values.GetValueOrDefault(ExpenseAccountType.Account720_2),
            Account730_01 = row.Values.GetValueOrDefault(ExpenseAccountType.Account730_01),
            Account730_02 = row.Values.GetValueOrDefault(ExpenseAccountType.Account730_02),
            Account730_03 = row.Values.GetValueOrDefault(ExpenseAccountType.Account730_03),
            Account730_04 = row.Values.GetValueOrDefault(ExpenseAccountType.Account730_04),
            Account730_05 = row.Values.GetValueOrDefault(ExpenseAccountType.Account730_05),
            Account730_06 = row.Values.GetValueOrDefault(ExpenseAccountType.Account730_06),
            Account730_07 = row.Values.GetValueOrDefault(ExpenseAccountType.Account730_07),
        };
    }
}