namespace Cost.Accounting.Automation.Domain.DirectProcurement.ValueObjects;

/// <summary>Dosya numarası (kaynak: <c>IHALE.DOSYANO</c>).</summary>
public sealed record FileNumber(string Value);

/// <summary>
/// İşin adı (kaynak: <c>IHALE.ISINADI</c>). Doğrudan temin dosyasının en
/// belirleyici alanıdır: dosya kopyalama ve ihale senaryosu aramaları bu
/// metin üzerinden yapılır.
/// </summary>
public sealed record WorkName(string Value);

/// <summary>Proje numarası (kaynak: <c>IHALE.PROJENO</c>).</summary>
public sealed record ProjectNumber(string Value);

/// <summary>Yönetici kurul karar numarası (kaynak: <c>IHALE.KARARNO</c>).</summary>
public sealed record DecisionNumber(string Value);

/// <summary>
/// İşin niteliği serbest metin (kaynak: <c>IHALE.ISINNITELIGI</c>). Yapım
/// işlerinde zorunlu, diğerlerinde serbest bırakılan alandır.
/// </summary>
public sealed record WorkNature(string Value);

/// <summary>
/// İşin miktarı serbest metin (kaynak: <c>IHALE.ISINMIKTARI</c>). Ölçü birimi
/// kaynak sistemde serbest metin olarak tutulduğu için sayısal alana
/// zorlanmaz.
/// </summary>
public sealed record WorkQuantity(string Value);