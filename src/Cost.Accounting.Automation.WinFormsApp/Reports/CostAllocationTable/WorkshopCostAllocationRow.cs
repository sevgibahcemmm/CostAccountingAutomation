using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips;
using DevExpress.XtraReports.UI;

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

        public decimal Account740_1 { get; init; }
        public decimal Account740_2 { get; init; }
        public decimal Account740_3_01 { get; init; }
        public decimal Account740_3_02 { get; init; }
        public decimal Account740_4 { get; init; }
        public decimal Account740_5 { get; init; }
        public decimal Account740_6 { get; init; }
        public decimal Account740_7 { get; init; }

        public decimal Account750_780 { get; init; }

        public decimal Total =>
            Account710 + Account720_1 + Account720_2 +
            Account730_01 + Account730_02 + Account730_03 +
            Account730_04 + Account730_05 + Account730_06 + Account730_07 +
            Account750_780;

        public decimal ServiceTotal =>
            Account740_1 + Account740_2 + Account740_3_01 + Account740_3_02 +
            Account740_4 + Account740_5 + Account740_6 + Account740_7 +
            Account750_780;

        public static ProductCostAllocationReportRow FromExpenseDistributionRow(ExpenseDistributionRow row) => new()
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
            Account740_1 = row.Values.GetValueOrDefault(ExpenseAccountType.Account740_1),
            Account740_2 = row.Values.GetValueOrDefault(ExpenseAccountType.Account740_2),
            Account740_3_01 = row.Values.GetValueOrDefault(ExpenseAccountType.Account740_3_01),
            Account740_3_02 = row.Values.GetValueOrDefault(ExpenseAccountType.Account740_3_02),
            Account740_4 = row.Values.GetValueOrDefault(ExpenseAccountType.Account740_4),
            Account740_5 = row.Values.GetValueOrDefault(ExpenseAccountType.Account740_5),
            Account740_6 = row.Values.GetValueOrDefault(ExpenseAccountType.Account740_6),
            Account740_7 = row.Values.GetValueOrDefault(ExpenseAccountType.Account740_7),
            Account750_780 = SumOf(row, ExpenseAccountType.Account750, ExpenseAccountType.Account760,
                ExpenseAccountType.Account770, ExpenseAccountType.Account780),
        };

        private static decimal SumOf(ExpenseDistributionRow row, params ExpenseAccountType[] types)
            => types.Sum(t => row.Values.GetValueOrDefault(t));
    }

    public interface ICostAllocationTableReport
    {
        void SetData(DateOnly startDate, DateOnly endDate, ExpenseDistributionReportResult result, string companyName = "", CostSlipType type = CostSlipType.Product);

        void PrintReport();
    }

    internal static class CostAllocationHeaderFormatter
    {
        public static string BuildVertical(string text, int maxLineLength = 16)
        {
            var lines = new List<string>();

            foreach (string paragraph in text.Split('\n'))
            {
                System.Text.StringBuilder line = new();

                foreach (string word in paragraph.Split(' '))
                {
                    if (line.Length == 0)
                    {
                        line.Append(word);
                    }
                    else if (line.Length + 1 + word.Length <= maxLineLength)
                    {
                        line.Append(' ').Append(word);
                    }
                    else
                    {
                        lines.Add(line.ToString());
                        line.Length = 0;
                        line.Append(word);
                    }
                }

                if (line.Length > 0)
                {
                    lines.Add(line.ToString());
                }
            }

            return string.Join("\n", lines);
        }

        public static void Rotate(XRTableCell cell) => cell.Angle = 270;

        public static string SpacedTitle(string text)
        {
            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join("   ", words.Select(w => string.Join(" ", w.ToCharArray()))) + " ";
        }
    }
}