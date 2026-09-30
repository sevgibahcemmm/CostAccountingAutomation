using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Photos;

/// <summary>
/// Ürün görseli. Kullanıcı fotoğrafları master veritabanında
/// <c>User.AvatarPath</c> alanında tutulduğu için bu tablo yalnızca
/// ürün görsellerine aittir ve yıl veritabanında kalır.
/// </summary>
public sealed class Photo : Entity
{
    private Photo()
    {
    }

    public Photo(
        string fileName,
        string contentType,
        string path,
        bool isDefault)
    {
        SetFileName(fileName);
        SetContentType(contentType);
        SetPath(path);
        SetDefault(isDefault);
    }

    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public string Path { get; private set; } = default!;
    public bool IsDefault { get; private set; }

    public void SetFileName(string fileName)
    {
        FileName = fileName;
    }

    public void SetContentType(string contentType)
    {
        ContentType = contentType;
    }

    public void SetPath(string path)
    {
        Path = path;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }
}
