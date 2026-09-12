namespace Cost.Accounting.Automation.Application.Users;

public sealed record PhotoInput(string FileName, string ContentType, byte[] Data, bool IsDefault);

public sealed record PhotoDto(string FileName, string ContentType, byte[] Data, bool IsDefault);