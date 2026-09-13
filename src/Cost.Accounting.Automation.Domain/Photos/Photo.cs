using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Users;

namespace Cost.Accounting.Automation.Domain.Photos;

public enum PhotoOwnerType
{
    User = 1,
    Product = 2
}

public sealed class Photo : Entity
{
    private Photo()
    {
    }

    public Photo(
        PhotoOwnerType ownerType,
        IdentityId ownerId,
        string fileName,
        string contentType,
        string path,
        bool isDefault)
    {
        OwnerType = ownerType;
        SetOwnerId(ownerId);
        SetFileName(fileName);
        SetContentType(contentType);
        SetPath(path);
        SetDefault(isDefault);
    }

    public PhotoOwnerType OwnerType { get; private set; }
    public IdentityId? UserId { get; private set; }
    public User? User { get; private set; }
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public string Path { get; private set; } = default!;
    public bool IsDefault { get; private set; }

    private void SetOwnerId(IdentityId ownerId)
    {
        UserId = OwnerType == PhotoOwnerType.User ? ownerId : null;
    }

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