namespace Cost.Accounting.Automation.Infrastructure.Options
{
    /// <summary>
    /// Açılışta yapılan güncelleme kontrolünün ayarları.
    ///
    /// <para>
    /// Uygulama bir yayın sunucusundan küçük bir JSON manifestosu indirir ve
    /// sürümü kendi sürümüyle karşılaştırır. Yeni sürüm varsa kullanıcıya
    /// bildirim gösterilir; indirme bağlantısı kullanıcı tarafından açılır.
    /// Program kendisini kendiliğinden güncellemez.
    /// </para>
    /// </summary>
    public sealed class UpdateOptions
    {
        public const string SectionName = "Update";

        /// <summary>
        /// Güncelleme kontrolü açık mı? Kapalıysa hiçbir ağ isteği yapılmaz.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Sürüm manifestosunun tam adresi (https://.../update.json).
        /// Boş bırakılırsa kontrol yapılmaz.
        /// </summary>
        public string? ManifestUrl { get; set; }

        /// <summary>
        /// İndirme sırasında ağ zaman aşımı (saniye). Açılışı geciktirmemek için
        /// kısa tutulur; hata durumunda kontrol sessizce atlanır.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Kontrol kullanıcı girişinden önce mi yapılsın? Şu an giriş ekranından
        /// önce gösterilir; gelecekte giriş sonrasına taşınabilir.
        /// </summary>
        public bool CheckBeforeLogin { get; set; } = true;
    }
}