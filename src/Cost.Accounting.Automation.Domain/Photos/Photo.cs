using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Users;

namespace Cost.Accounting.Automation.Domain.Photos;

public sealed class Photo : Entity
{
    private Photo()
    {
    }

    public Photo(
        IdentityId userId,
        string fileName,
        string contentType,
        byte[] data,
        bool isDefault)
    {
        SetUserId(userId);
        SetFileName(fileName);
        SetContentType(contentType);
        SetData(data);
        SetDefault(isDefault);
    }

    public IdentityId UserId { get; private set; } = default!;
    public User User { get; private set; } = default!;
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public byte[] Data { get; private set; } = default!;
    public bool IsDefault { get; private set; }

    public void SetUserId(IdentityId userId)
    {
        UserId = userId;
    }

    public void SetFileName(string fileName)
    {
        FileName = fileName;
    }

    public void SetContentType(string contentType)
    {
        ContentType = contentType;
    }

    public void SetData(byte[] data)
    {
        Data = data;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }
}