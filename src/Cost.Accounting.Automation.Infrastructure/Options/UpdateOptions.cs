namespace Cost.Accounting.Automation.Infrastructure.Options
{
    /// <summary>
    /// Açılışta yapılan güncelleme kontrolünün ayarları.
    ///
    /// <para>
    /// Uygulama merkezi Master veritabanındaki <c>AppReleases</c> tablosundan en
    /// güncel sürüm kaydını okur ve sürümü kendi sürümüyle karşılaştırır. Yeni
    /// sürüm varsa kullanıcıya bildirim gösterilir; kurulum dosyası kayıttaki
    /// UNC paylaşım adresinden (ör. <c>\\192.168.1.5\CostAccountingUpdates\...
    /// .exe</c>) yerel klasöre kopyalanır ve kurulum başlatılır.
    /// Program kendisini kendiliğinden güncellemez.
    /// </para>
    /// </summary>
    public sealed class UpdateOptions
    {
        public const string SectionName = "Update";

        /// <summary>
        /// Güncelleme kontrolü açık mı? Kapalıysa hiçbir veritabanı sorgusu yapılmaz.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Sorgu/bağlantı zaman aşımı (saniye). Açılışı geciktirmemek için kısa
        /// tutulur; hata durumunda kontrol sessizce atlanır.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Kontrol kullanıcı girişinden önce mi yapılsın? Şu an giriş ekranından
        /// önce gösterilir; gelecekte giriş sonrasına taşınabilir.
        /// </summary>
        public bool CheckBeforeLogin { get; set; } = true;
    }
}