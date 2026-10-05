namespace Cost.Accounting.Automation.Domain.Abstractions;

public abstract class Entity
{
    protected Entity()
    {
        Id = new IdentityId(Guid.CreateVersion7());
        IsActive = true;
    }

    public IdentityId Id { get; private set; }
    public bool IsActive { get; private set; }
    public string? DuplicateKey { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IdentityId CreatedBy { get; private set; } = default!;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public IdentityId? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public IdentityId? DeletedBy { get; private set; }

    /// <summary>
    /// Eşzamanlı düzenleme koruması. Veritabanı tarafından üretilen sürüm
    /// numarasıdır; her yazma işleminde kendiliğinden değişir.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Merkezi sunucuda aynı kayıt üzerinde birden fazla kişi çalışabilir.
    /// Bu belirteç olmadan iki kişi aynı faturayı açtığında ikinci kaydeden
    /// birincinin değişikliklerini <b>sessizce</b> ezer; kimse hata görmez.
    /// </para>
    /// <para>
    /// Değer uygulama tarafından hiç okunmaz veya yazılmaz: sunucu üretir, EF
    /// yalnızca karşılaştırır. Karşılaştırma tutmazsa
    /// <c>DbUpdateConcurrencyException</c> fırlatılır ve
    /// <c>TransactionBehavior</c> bunu kullanıcıya anlaşılır bir mesajla
    /// dönüştürür ("Bu kaydı Ahmet Yılmaz değiştirdi").
    /// </para>
    /// <para>
    /// Konfigürasyon <c>AuditedDbContext.ApplySharedModelConfiguration</c>
    /// içindeki <c>RowVersion</c> kuralıyla yapılır; buradaki tek görev alanın
    /// varlığıdır.
    /// </para>
    /// </remarks>
    public byte[]? RowVersion { get; private set; }

    public void SetDuplicateKey(string? duplicateKey)
    {
        DuplicateKey = duplicateKey;
    }

    public void SetStatus(bool isActive)
    {
        IsActive = isActive;
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }
}

public sealed record IdentityId(Guid Value)
{
    public static implicit operator Guid(IdentityId id) => id.Value;
    public static implicit operator string(IdentityId id) => id.Value.ToString();
}