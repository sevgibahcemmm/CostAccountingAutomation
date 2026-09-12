namespace Cost.Accounting.Automation.Domain.Abstractions;

public abstract class EntityDto
{
    [Column("Aktif", Order = 90, Width = 80, Alignment = "Center", TrueText = "Aktif", FalseText = "Pasif")]
    public bool IsActive { get; set; }

    [Column("Oluşturma Tarihi", Order = 91, Width = 140, Format = "g")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("Oluşturan", Order = 92, Width = 140)]
    public string CreatedFullName { get; set; } = default!;

    [Column("Güncelleme Tarihi", Order = 93, Width = 140, Format = "g")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Column("Güncelleyen", Order = 94, Width = 140)]
    public string? UpdatedFullName { get; set; }

    [Column("Id", IsVisible = false)]
    public Guid Id { get; set; } = default!;

    [Column("Oluşturan Kullanıcı Id", IsVisible = false)]
    public Guid CreatedBy { get; set; } = default!;

    [Column("Güncelleyen Kullanıcı Id", IsVisible = false)]
    public Guid? UpdatedBy { get; set; }
}