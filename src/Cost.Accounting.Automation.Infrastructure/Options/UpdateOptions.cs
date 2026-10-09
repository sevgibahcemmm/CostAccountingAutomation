namespace Cost.Accounting.Automation.Infrastructure.Options
{
    /// <summary>
    /// Açılışta yapılan güncelleme kontrolünün ayarları.
    ///
    /// <para>
    /// Uygulama, merkezi master veritabanındaki <c>AppReleases</c> tablosundan
    /// en güncel yayın sürümünü okur ve kendi sürümüyle karşılaştırır. Yeni
    /// sürüm varsa kurulum dosyası veritabanından indirilir ve uygulama
    /// kendisini o dosyayla günceller; harici bir sunucu/URL gerekmez.
    /// </para>
    /// </summary>
    public sealed class UpdateOptions
    {
        public const string SectionName = "Update";

        /// <summary>
        /// Güncelleme kontrolü açık mı? Kapalıysa veritabanına hiç sorulmaz.
        /// </summary>
        public bool Enabled { get; set; } = true;

/// <summary>
        /// Kontrol sırasında zaman aşımı (saniye). Kontrolü geciktirmemek için
        /// kısa tutulur; hata durumunda kontrol sessizce atlanır.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 5;
    }
}
