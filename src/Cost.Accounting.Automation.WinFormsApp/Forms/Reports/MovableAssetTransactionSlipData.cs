namespace Cost.Accounting.Automation.WinFormsApp.Forms.Reports
{
    public sealed class MovableAssetTransactionSlipRow
    {
        public int? SiraNo { get; set; }
        public string Kodu { get; set; } = string.Empty;
        public string BarkodNo { get; set; } = string.Empty;
        public string Adi { get; set; } = string.Empty;
        public string OlcuBirimi { get; set; } = string.Empty;
        public decimal Miktari { get; set; }
        public decimal? BirimFiyati { get; set; }
        public decimal Tutari { get; set; }
        public bool IsSubtotal { get; set; }
        public bool IsGrandTotal { get; set; }
    }

    public sealed class MovableAssetTransactionSlipData
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string IslemCesidi { get; set; } = string.Empty;
        public string NeredenGeldigi { get; set; } = string.Empty;
        public string KimeVerildigi { get; set; } = string.Empty;
        public string NereyeVerildigi { get; set; } = string.Empty;
        public string IlIlceAdi { get; set; } = string.Empty;
        public string IlIlceKodu { get; set; } = string.Empty;
        public string HarcamaBirimiAdi { get; set; } = string.Empty;
        public string HarcamaBirimiKodu { get; set; } = string.Empty;
        public string AmbarAdi { get; set; } = string.Empty;
        public string AmbarKodu { get; set; } = string.Empty;
        public string MuhasebeBirimiAdi { get; set; } = string.Empty;
        public string MuhasebeBirimiKodu { get; set; } = string.Empty;
        public DateTime? DayanakTarihi { get; set; }
        public string DayanakKodu { get; set; } = string.Empty;
        public List<MovableAssetTransactionSlipRow> Rows { get; set; } = [];

        public int DetailLineCount => Rows.Count(r => !r.IsSubtotal && !r.IsGrandTotal);
        public decimal TotalQuantity => Rows.Where(r => !r.IsSubtotal && !r.IsGrandTotal).Sum(r => r.Miktari);

        public void ApplyCodeTotals(int targetLevel)
        {
            List<MovableAssetTransactionSlipRow> leaves =
                Rows.Where(r => !r.IsSubtotal && !r.IsGrandTotal).ToList();

            List<MovableAssetTransactionSlipRow> output = [];

            foreach (IGrouping<string, MovableAssetTransactionSlipRow> group in leaves
                         .GroupBy(r => GetLevelCode(r.Kodu, targetLevel), StringComparer.Ordinal)
                         .OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                MovableAssetTransactionSlipRow[] ordered = group.OrderBy(r => r.Kodu, StringComparer.Ordinal).ToArray();
                output.AddRange(ordered);

                if (ordered.Length > 1 && !string.IsNullOrWhiteSpace(group.Key))
                {
                    output.Add(new MovableAssetTransactionSlipRow
                    {
                        Adi = $"{group.Key} TOPLAMI",
                        Miktari = ordered.Sum(r => r.Miktari),
                        Tutari = ordered.Sum(r => r.Tutari),
                        IsSubtotal = true
                    });
                }
            }

            if (leaves.Count > 0)
            {
                output.Add(new MovableAssetTransactionSlipRow
                {
                    Adi = "GENEL TOPLAM",
                    Miktari = leaves.Sum(r => r.Miktari),
                    Tutari = leaves.Sum(r => r.Tutari),
                    IsGrandTotal = true
                });
            }

            int order = 0;
            foreach (MovableAssetTransactionSlipRow row in output)
            {
                if (!row.IsSubtotal && !row.IsGrandTotal)
                {
                    row.SiraNo = ++order;
                }
            }

            Rows = output;
        }

        internal static string GetLevelCode(string code, int targetLevel)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return string.Empty;
            }

            string[] parts = code.Split('.');
            if (parts.Length <= targetLevel)
            {
                return code;
            }

            return string.Join(".", parts.Take(targetLevel));
        }
    }
}