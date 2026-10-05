using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.Messages;

/// <summary>
/// Mesaj konusu. Zorunlu değildir; duyurularda genelde kullanılır.
/// </summary>
/// <remarks>
/// <see cref="Shared.Description"/> ile aynı kurallara sahiptir: en fazla bir
/// veritabanı alanı uzunluğunda tutulur ve beyaz boşluk kırpılır.
/// </remarks>
public sealed record MessageSubject
{
    private MessageSubject() { }

    public MessageSubject(string value) => Value = value.Trim();

    public string Value { get; private set; } = default!;
}

/// <summary>
/// Mesaj gövdesi.
/// </summary>
/// <remarks>
/// <para>
/// Gövde tek satır değildir; çok satırlı olabilir ve <c>nvarchar(MAX)</c> olarak
/// saklanır (uygulamadaki varsayılan dize eşlemesi zaten böyle).
/// </para>
/// <para>
/// Uzunluk sınırı uygulanır: istemcinin kazara yapıştırdığı devasa bir metin
/// satırını sessizce kırpmak yerine kullanıcıya bildirilmesi gerekir.
/// </para>
/// </remarks>
public sealed record MessageBody
{
    /// <summary>İzin verilen en fazla karakter sayısı.</summary>
    public const int MaxLength = 4000;

    private MessageBody() { }

    public MessageBody(string value)
    {
        Value = (value ?? string.Empty).Trim();
    }

    public string Value { get; private set; } = default!;

    /// <summary>
    /// Gövde bu uzunluğu aşıyorsa <c>false</c> döner.
    /// </summary>
    public bool IsWithinLimits() => Value.Length <= MaxLength;
}
