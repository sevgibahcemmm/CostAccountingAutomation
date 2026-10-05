using Cost.Accounting.Automation.Application.Auth;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>Mesaj alıcısı arar.</summary>
/// <remarks>
/// Arama dört alanda birden çalışır: <b>sicil numarası</b>, <b>TC kimlik numarası</b>,
/// <b>kullanıcı adı</b> ve <b>ad / soyad</b>. Metin alanlarında büyük/küçük harf
/// farkının sonucu değiştirmemesi için veritabanı collation'ından bağımsız
/// karşılaştırma uygulanır (bkz. <see cref="LoginNameMatcher"/>). Sicil ve TC
/// alanlarında ise ham <c>Contains</c> kullanılır: bu alanlar sayısal karakter
/// içerir ve normalleştirmek numarayı bozabilir.
/// </remarks>
[Permission(MessagePermissions.View)]
public sealed record MessageRecipientSearchQuery(
    string? Term,
    bool ExcludeSelf = true,
    int MaxResults = 30) : IRequest<Result<List<MessageRecipientDto>>>;

/// <summary>Alıcı seçiminde gösterilen kullanıcı.</summary>
public sealed class MessageRecipientDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    /// <summary>Sicil numarası; personel kaydıyla eşleştirmek için gösterilir.</summary>
    public string? RegistryNumber { get; set; }

    public string? TcNo { get; set; }

    public string? CompanyName { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

internal sealed class MessageRecipientSearchQueryHandler(
    IUserRepository userRepository,
    ICompanyRepository companyRepository,
    IRoleRepository roleRepository,
    IClaimContext claimContext) : IRequestHandler<MessageRecipientSearchQuery, Result<List<MessageRecipientDto>>>
{
    private const int AbsoluteMaxResults = 200;

    /// <summary>Kurum ve rol adları sonradan tek sorguda çözülür.</summary>
    private sealed record Projection(
        Guid Id,
        string FullName,
        string UserName,
        string? RegistryNumber,
        string? TcNo,
        Guid CompanyId,
        Guid RoleId);

    public async Task<Result<List<MessageRecipientDto>>> Handle(
        MessageRecipientSearchQuery request,
        CancellationToken cancellationToken)
    {
        string term = LoginNameMatcher.Clean(request.Term);

        var me = new IdentityId(claimContext.GetUserId());

        var query = userRepository.Where(u => u.IsActive);

        if (request.ExcludeSelf)
        {
            query = query.Where(u => u.Id != me);
        }

        if (term.Length > 0)
        {
            query = query.Where(u =>
                EF.Functions.Collate(u.UserName.Value, LoginNameMatcher.Collation) == term
                || EF.Functions.Collate(u.FirstName.Value, LoginNameMatcher.Collation).Contains(term)
                || EF.Functions.Collate(u.LastName.Value, LoginNameMatcher.Collation).Contains(term)
                || EF.Functions.Collate(u.Email.Value, LoginNameMatcher.Collation).Contains(term)
                || (u.RegistryNumber != null && u.RegistryNumber.Contains(term))
                || (u.TRIdentityNumber!.Value != null
                    && u.TRIdentityNumber.Value.Contains(term)));
        }

        int take = Math.Clamp(request.MaxResults, 1, AbsoluteMaxResults);

        var users = (await query
            .Select(u => new Projection(
                u.Id.Value,
                u.FirstName.Value + " " + u.LastName.Value,
                u.UserName.Value,
                u.RegistryNumber,
                u.TRIdentityNumber!.Value,
                u.CompanyId.Value,
                u.RoleId.Value))
            .ToListAsync(cancellationToken))
            .OrderBy(u => u.FullName, StringComparer.CurrentCultureIgnoreCase)
            .Take(take)
            .ToList();

        if (users.Count == 0)
        {
            return new List<MessageRecipientDto>();
        }

        Dictionary<Guid, string> companyNames = await LoadCompanyNamesAsync(users, cancellationToken);
        Dictionary<Guid, string> roleNames = await LoadRoleNamesAsync(users, cancellationToken);

        return users.Select(u => new MessageRecipientDto
        {
            Id = u.Id,
            FullName = u.FullName,
            UserName = u.UserName,
            RegistryNumber = string.IsNullOrWhiteSpace(u.RegistryNumber) ? null : u.RegistryNumber,
            TcNo = string.IsNullOrWhiteSpace(u.TcNo) ? null : u.TcNo,
            CompanyName = companyNames.GetValueOrDefault(u.CompanyId),
            RoleName = roleNames.GetValueOrDefault(u.RoleId, string.Empty)
        }).ToList();
    }

    private async Task<Dictionary<Guid, string>> LoadCompanyNamesAsync(
        List<Projection> users,
        CancellationToken cancellationToken)
    {
        // Contains filtresi model tipini (IdentityId) kullanmalidir; Guid listesi
        // verilirse EF deger donusturucusunu atlar ve ifade SQL'e cevrilemez.
        var ids = users.Select(u => new IdentityId(u.CompanyId)).Distinct().ToArray();

        var companies = await companyRepository
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { Id = c.Id.Value, Name = c.Name.Value })
            .ToListAsync(cancellationToken);

        return companies.ToDictionary(c => c.Id, c => c.Name);
    }

    private async Task<Dictionary<Guid, string>> LoadRoleNamesAsync(
        List<Projection> users,
        CancellationToken cancellationToken)
    {
        var ids = users.Select(u => new IdentityId(u.RoleId)).Distinct().ToArray();

        var roles = await roleRepository
            .Where(r => ids.Contains(r.Id))
            .Select(r => new { Id = r.Id.Value, Name = r.Name.Value })
            .ToListAsync(cancellationToken);

        return roles.ToDictionary(r => r.Id, r => r.Name);
    }
}
