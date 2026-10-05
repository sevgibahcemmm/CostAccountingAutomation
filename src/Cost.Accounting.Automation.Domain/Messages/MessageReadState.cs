namespace Cost.Accounting.Automation.Domain.Messages;

/// <summary>
/// Mesajın okunup okunmadığı.
/// </summary>
/// <remarks>
/// Neden <c>bool</c> değil de ayrı bir değer nesnesi: diğer alanlarla aynı desene
/// uymak ve ileride üçüncü bir durum (ör. arşiv) eklenirse yer varması.
/// </remarks>
public sealed record MessageReadState
{
    private MessageReadState() { }

    public MessageReadState(bool value) => Value = value;

    public bool Value { get; private set; }
}
