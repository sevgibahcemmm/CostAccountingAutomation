namespace Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips
{
    /// <summary>
    /// Taşınır işlem fişinin belgelediği işlem.
    ///
    /// <para>
    /// Fısın "İŞLEM ÇEŞİDİ" alanında yazan metin bu değere dönüştürülür ve
    /// imza kutularının hangilerinin doldurulacağı buna göre belirlenir:
    /// giriş fişinde yalnızca kayıt yetkilisi, çıkış (tüketim) fişinde
    /// yalnızca çıkış kaydı, atölye transferinde çıkış kaydı ve teslim alan
    /// (atölye şefi) kutuları doldurulur.
    /// </para>
    /// </summary>
    public enum MovableAssetTransactionSlipKind
    {
        /// <summary>Depoya giriş (satın alma faturası) kaydı.</summary>
        Entry = 1,

        /// <summary>Depodan çıkış / tüketim kaydı.</summary>
        Exit = 2,

        /// <summary>Depodan atölyeye taşınır transferi.</summary>
        AtelierTransfer = 3
    }

    public sealed class MovableAssetTransactionSlipRow
    {
        public int? RowNumber { get; set; }

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

        public string Code { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Adi { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }

    public sealed class MovableAssetTransactionSlipDepotTotal
    {
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }

    public sealed class MovableAssetTransactionSlipData
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public string SourceParty { get; set; } = string.Empty;
        public string RecipientParty { get; set; } = string.Empty;
        public string DestinationParty { get; set; } = string.Empty;
        public string ProvinceDistrictName { get; set; } = string.Empty;
        public string ProvinceDistrictCode { get; set; } = string.Empty;
        public string ExpenditureUnitName { get; set; } = string.Empty;
        public string ExpenditureUnitCode { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string StoreCode { get; set; } = string.Empty;
        public string AccountingUnitName { get; set; } = string.Empty;
        public string AccountingUnitCode { get; set; } = string.Empty;
        public DateTime? ReferenceDate { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;

/// <summary>
        /// Fişin hangi işlemi belgelediği. <see cref="OperationType"/> yalnızca
        /// basılacak metin olduğu için imza kutularının hangilerinin
        /// doldurulacağı bu değere bakılarak belirlenir.
        /// </summary>
        public MovableAssetTransactionSlipKind Kind => ResolveKind(OperationType);

        /// <summary>
        /// Atölye transferinde taşınırın gittiği atölyenin kimliği.
        /// Atölye transferinde teslim alan kutusunu atölye şefi doldurur;
        /// bu görev atölyeye bağlı tanımlandığı için atölye bilgisi gerekir.
        /// Tüketimde teslim alan muhasebe memurudur, o da kurum geneli bir
        /// görev olduğu için bu alan boş kalır.
        /// </summary>
        public Guid? RecipientWorkshopId { get; set; }

        /// <summary>
        /// İmza bloğunda "Taşınır Kayıt ve Yetkilisi" satırına basılacak
        /// personelin adı soyadı. Görev tanımı veritabanında olduğu için
        /// personel listesinden çözülür; bulunamazsa boş kalır.
        /// </summary>
        public string SignatoryFullName { get; set; } = string.Empty;

        /// <summary>İmza bloğunda gösterilecek personelin ünvanı.</summary>
        public string SignatoryTitle { get; set; } = string.Empty;

        /// <summary>
        /// "Teslim Alan" kutusunun "Adı Soyadı" satırına basılacak kişi.
        /// Tüketim fişinde malzeme muhasebe birimine teslim edildiği için
        /// teslimi <b>muhasebe memuru</b> alır; atölye transferinde ise
        /// taşınırı alan atölyenin şefi yazılır. Giriş fişinde bu kutu
        /// doldurulmaz.
        /// </summary>
        public string RecipientSignatoryFullName { get; set; } = string.Empty;

        /// <summary>Teslim alan kutusunun ünvan satırı.</summary>
        public string RecipientSignatoryTitle { get; set; } = string.Empty;

        /// <summary>
        /// Kayıt yetkilisinin "Adı Soyadı" satırına basılacak metin.
        /// Yetkili çözülemezse satır boş bırakılmaz, noktalı çizgi basılır:
        /// "kim imzalayacak" sorusu belge üzerinde açık kalsın.
        /// </summary>
        public string SignatureName => ValueOrBlank(SignatoryFullName);

        /// <summary>Kayıt yetkilisinin "Ünvanı" satırına basılacak metin.</summary>
        public string SignatureTitle => ValueOrBlank(SignatoryTitle);

        /// <summary>
        /// "Tarih" satırına basılacak metin. Kayıt yetkilisinin imza tarihi
        /// belgenin kendi tarihidir; ayrı bir alan istenirse <see cref="Date"/>
        /// değiştirilir.
        /// </summary>
        public string SignatureDate => Date.ToString("dd.MM.yyyy");

        /// <summary>Atölye şefinin teslim alan kutusundaki "Adı Soyadı" satırı.</summary>
        public string RecipientSignatureName => ValueOrBlank(RecipientSignatoryFullName);

        /// <summary>Atölye şefinin teslim alan kutusundaki "Ünvanı" satırı.</summary>
        public string RecipientSignatureTitle => ValueOrBlank(RecipientSignatoryTitle);

        /// <summary>
        /// <see cref="OperationType"/> metnini fiş çeşidine çevirir.
        /// Metinler belgeyi üreten formlarda yazıyla verilir; tanınmayan bir
        /// metin (ör. gelecekte eklenen bir işlem çeşidi) giriş kutusuna
        /// düşer, çünkü en az hasar gören davranış budur.
        /// </summary>
        private static MovableAssetTransactionSlipKind ResolveKind(string? operationType)
        {
            if (string.Equals(operationType, "Atölye Transferi", StringComparison.OrdinalIgnoreCase))
            {
                return MovableAssetTransactionSlipKind.AtelierTransfer;
            }

            if (string.Equals(operationType, "Tüketim", StringComparison.OrdinalIgnoreCase)
                || string.Equals(operationType, "Çıkış", StringComparison.OrdinalIgnoreCase))
            {
                return MovableAssetTransactionSlipKind.Exit;
            }

            return MovableAssetTransactionSlipKind.Entry;
        }

        private static string ValueOrBlank(string? value)
            => string.IsNullOrWhiteSpace(value) ? "..............................." : value;

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

        public decimal GrandQuantity { get; private set; }
        public decimal GrandAmount { get; private set; }

        public int DetailLineCount => Rows.Count;
        public decimal TotalQuantity => Rows.Sum(r => r.Quantity);
        public decimal TotalAmount => Rows.Sum(r => r.Amount);

        /// <summary>
        /// Kalemleri hesap koduna göre gruplar. Code "150.98" ile başlayan kalemler 4. düzeyde,
        /// diğer kalemler 3. düzeyde gruplanır. Sıra numaraları nihai basım sırasına göre atanır,
        /// depo toplamları ve genel toplam hesaplanır. Toplam satırları belgeye eklenmez;
        /// rapor tarafında grup dip notu (GroupFooter) ve rapor sonunda gösterilir.
        /// </summary>
        public void Prepare()
        {
            List<MovableAssetTransactionSlipRow> ordered = Rows
                .OrderBy(r => ResolveGroupCode(r.Code), StringComparer.Ordinal)
                .ThenBy(r => r.Code, StringComparer.Ordinal)
                .ThenBy(r => r.Adi, StringComparer.Ordinal)
                .ToList();

            int order = 0;
            foreach (MovableAssetTransactionSlipRow row in ordered)
            {
                row.GroupCode = ResolveGroupCode(row.Code);
                row.GroupName = ResolveGroupName(row.GroupCode);
                row.RowNumber = ++order;
            }

            Rows = ordered;

            DepotTotals = ordered
                .GroupBy(GetWarehouseKey, StringComparer.Ordinal)
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => new MovableAssetTransactionSlipDepotTotal
                {
                    WarehouseCode = g.First().WarehouseCode,
                    WarehouseName = g.First().WarehouseName,
                    Quantity = g.Sum(r => r.Quantity),
                    Amount = g.Sum(r => r.Amount)
                })
                .ToList();

            GrandQuantity = ordered.Sum(r => r.Quantity);
            GrandAmount = ordered.Sum(r => r.Amount);
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
            if (!string.IsNullOrWhiteSpace(row.WarehouseCode))
            {
                return row.WarehouseCode;
            }

            if (!string.IsNullOrWhiteSpace(row.WarehouseName))
            {
                return row.WarehouseName;
            }

            if (string.IsNullOrWhiteSpace(row.Code))
            {
                return string.Empty;
            }

            if (IsProductionCode(row.Code))
            {
                return "150.98";
            }

            string[] parts = row.Code.Split('.');
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
