namespace Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips
{
    public sealed class MovableAssetTransactionSlipRow
    {
        public int? SiraNo { get; set; }

        /// <summary>
        /// Kalemin gruplanacağı hesap kodu (üretim kodlarında 4., diğerlerinde 3. düzey).
        /// </summary>
        public string GroupCode { get; set; } = string.Empty;

        /// <summary>
        /// Grup başlığında kodun yanında gösterilecek hesap adı.
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        public string GroupDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(GroupCode))
                {
                    return GroupName;
                }

                return string.IsNullOrWhiteSpace(GroupName)
                    ? GroupCode
                    : $"{GroupCode}   {GroupName}";
            }
        }

        public string Kodu { get; set; } = string.Empty;
        public string DepoKodu { get; set; } = string.Empty;
        public string DepoAdi { get; set; } = string.Empty;
        public string BarkodNo { get; set; } = string.Empty;
        public string Adi { get; set; } = string.Empty;
        public string OlcuBirimi { get; set; } = string.Empty;
        public decimal Miktari { get; set; }
        public decimal? BirimFiyati { get; set; }
        public decimal Tutari { get; set; }
    }

    public sealed class MovableAssetTransactionSlipDepotTotal
    {
        public string DepoKodu { get; set; } = string.Empty;
        public string DepoAdi { get; set; } = string.Empty;
        public decimal Miktari { get; set; }
        public decimal Tutari { get; set; }
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

        /// <summary>
        /// Grup başlıklarında hesap kodunun yanında gösterilecek hesap adları.
        /// Anahtar: hesap kodu (ör. "150.98.12.04"), değer: hesap adı.
        /// </summary>
        public IReadOnlyDictionary<string, string> AccountNames { get; set; }
            = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>
        /// Depo bazında toplanan miktar/tutar bilgileri (rapor sonundaki depo toplamları).
        /// </summary>
        public List<MovableAssetTransactionSlipDepotTotal> DepotTotals { get; private set; } = [];

        public decimal GrandMiktar { get; private set; }
        public decimal GrandTutar { get; private set; }

        public int DetailLineCount => Rows.Count;
        public decimal TotalQuantity => Rows.Sum(r => r.Miktari);
        public decimal TotalAmount => Rows.Sum(r => r.Tutari);

        /// <summary>
        /// Kalemleri hesap koduna göre gruplar. Kodu "150.98" ile başlayan kalemler 4. düzeyde,
        /// diğer kalemler 3. düzeyde gruplanır. Sıra numaraları nihai basım sırasına göre atanır,
        /// depo toplamları ve genel toplam hesaplanır. Toplam satırları belgeye eklenmez;
        /// rapor tarafında grup dip notu (GroupFooter) ve rapor sonunda gösterilir.
        /// </summary>
        public void Prepare()
        {
            List<MovableAssetTransactionSlipRow> ordered = Rows
                .OrderBy(r => ResolveGroupCode(r.Kodu), StringComparer.Ordinal)
                .ThenBy(r => r.Kodu, StringComparer.Ordinal)
                .ThenBy(r => r.Adi, StringComparer.Ordinal)
                .ToList();

            int order = 0;
            foreach (MovableAssetTransactionSlipRow row in ordered)
            {
                row.GroupCode = ResolveGroupCode(row.Kodu);
                row.GroupName = ResolveGroupName(row.GroupCode);
                row.SiraNo = ++order;
            }

            Rows = ordered;

            DepotTotals = ordered
                .GroupBy(GetWarehouseKey, StringComparer.Ordinal)
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => new MovableAssetTransactionSlipDepotTotal
                {
                    DepoKodu = g.First().DepoKodu,
                    DepoAdi = g.First().DepoAdi,
                    Miktari = g.Sum(r => r.Miktari),
                    Tutari = g.Sum(r => r.Tutari)
                })
                .ToList();

            GrandMiktar = ordered.Sum(r => r.Miktari);
            GrandTutar = ordered.Sum(r => r.Tutari);
        }

        private static string ResolveGroupCode(string code)
        {
            return GetLevelCode(code, GetTargetLevel(code));
        }

        private string ResolveGroupName(string code)
        {
            return AccountNames.TryGetValue(code, out string? name) ? name : string.Empty;
        }

        private static string GetWarehouseKey(MovableAssetTransactionSlipRow row)
        {
            if (!string.IsNullOrWhiteSpace(row.DepoKodu))
            {
                return row.DepoKodu;
            }

            if (!string.IsNullOrWhiteSpace(row.DepoAdi))
            {
                return row.DepoAdi;
            }

            if (string.IsNullOrWhiteSpace(row.Kodu))
            {
                return string.Empty;
            }

            if (IsProductionCode(row.Kodu))
            {
                return "150.98";
            }

            string[] parts = row.Kodu.Split('.');
            return parts.Length > 0 ? parts[0] : string.Empty;
        }

        private static int GetTargetLevel(string code)
        {
            return IsProductionCode(code) ? 4 : 3;
        }

        private static bool IsProductionCode(string code)
        {
            return !string.IsNullOrWhiteSpace(code)
                && (code.Equals("150.98", StringComparison.Ordinal)
                    || code.StartsWith("150.98.", StringComparison.Ordinal));
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
