namespace Cost.Accounting.Automation.Application.Services;

/// <summary>
/// Bağlantılı olunan veritabanının uygulama tarafından kullanılabilir olup
/// olmadığını belirtir.
/// </summary>
/// <remarks>
/// Bu değerler <b>giriş ekranı açılmadan</b> ve <b>hiçbir veri yazılmadan</b>
/// üretilir. Merkezi sunucuya bağlanan istemcilerde uygulama bu kontrolü her
/// açılışta yapar ve eksik olan her şeyi yöneticiye devreder.
/// </remarks>
public enum DatabaseSchemaState
{
    /// <summary>Şema güncel; uygulama çalışabilir.</summary>
    Ready,

    /// <summary>Ana (master) veritabanı sunucuda yok.</summary>
    DatabaseMissing,

    /// <summary>
    /// Veritabanı var ama bekleyen migration'lar var. Uygulamanın bu sürümü,
    /// sunucudaki şemadan yeni bir şema bekliyor.
    /// </summary>
    SchemaOutdated,

    /// <summary>
    /// Sunucuya ulaşılamadı ya da beklenmeyen bir hata oluştu. Kurulum
    /// yapılamaz; ekranda bu ayrım yapılır çünkü iki durumun çözümü farklıdır.
    /// </summary>
    Unreachable
}

/// <summary>
/// Salt okunur şema kontrolünün sonucu.
/// </summary>
/// <param name="State">Genel durum.</param>
/// <param name="PendingMigrations">
/// Sunucuda uygulanmamış migration kimlikleri (yoksa boş). Yalnızca
/// <see cref="DatabaseSchemaState.SchemaOutdated"/> durumunda anlamlıdır.
/// </param>
/// <param name="Detail">
/// Ekranda gösterilecek, kullanıcının işine yarayan tek satırlık açıklama
/// (hata metni, bekleyen migration sayısı vb.).
/// </param>
public sealed record DatabaseSchemaCheckResult(
    DatabaseSchemaState State,
    IReadOnlyList<string> PendingMigrations,
    string? Detail)
{
    /// <summary>Uygulamanın çalışmaya devam edebileceği durum mu?</summary>
    public bool IsReady => State == DatabaseSchemaState.Ready;

    /// <summary>Bekleyen migration sayısı.</summary>
    public int PendingMigrationCount => PendingMigrations.Count;

    public static DatabaseSchemaCheckResult Ready() =>
        new(DatabaseSchemaState.Ready, [], null);

    public static DatabaseSchemaCheckResult DatabaseMissing(string databaseName) =>
        new(DatabaseSchemaState.DatabaseMissing, [],
            $"Sunucuda '{databaseName}' veritabanı bulunamadı.");

    public static DatabaseSchemaCheckResult SchemaOutdated(
        IReadOnlyList<string> pendingMigrations) =>
        new(DatabaseSchemaState.SchemaOutdated, pendingMigrations,
            $"{pendingMigrations.Count} bekleyen migration var.");

    public static DatabaseSchemaCheckResult Unreachable(string detail) =>
        new(DatabaseSchemaState.Unreachable, [], detail);
}
