namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>
/// Arayüzden gelen fotoğraf içeriği: dosya adı, içerik tipi ve byte'lar.
///
/// <para>
/// <c>Application.Users.PhotoInput</c> ile aynı biçimde olmasına rağmen burada
/// bilinçli olarak ayrı bir tip tanımlanır: personel kayıtları kullanıcı
/// (login) kayıtlarından bağımsızdır ve iki modülün birbirine bağlanması
/// ileride birini değiştirdiğimizde diğerini bozmamıza yol açar.
/// </para>
/// </summary>
public sealed record PhotoInput(string FileName, string ContentType, byte[] Data, bool IsDefault);