using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.AccountingYears;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;

/// <summary>
/// Girilen kullanıcı adına (e-posta veya kullanıcı adı) karşılık gelen oturum
/// kapsamını çözer. Kullanıcı normal bir roldeyse yalnızca kendi kurumunun
/// açık yılları döner ve kurum seçimi sunulmaz; <c>sys_admin</c> ise tüm
/// kurumların listesini seçebilir.
/// </summary>
public sealed record LoginScopeGetQuery(
    string UserName) : IRequest<Result<LoginScopeDto?>>;

public sealed record LoginScopeDto
{
    /// <summary>Kullanıcının bağlı olduğu kurum.</summary>
    public required Guid CompanyId { get; init; }

    public required string CompanyName { get; init; }

    public required bool IsSysAdmin { get; init; }

    /// <summary>
    /// Giriş ekranında kurum seçtirilecek kayıtlar. Normal kullanıcıda tek
    /// elemanlıdır ve <see cref="IsSysAdmin"/> false ise formda gizlenir.
    /// </summary>
    public required List<LoginScopeCompanyDto> Companies { get; init; }
}

public sealed record LoginScopeCompanyDto
{
    public required Guid CompanyId { get; init; }
    public required string CompanyName { get; init; }
    public required List<LoginScopeYearDto> Years { get; init; }
}

public sealed record LoginScopeYearDto
{
    public required Guid CompanyYearId { get; init; }
    public required int Year { get; init; }
    public required string DatabaseName { get; init; }
    public required bool IsClosed { get; init; }
}

/// <summary>
/// Giriş ekranının kullanıcı adı ve e-posta karşılaştırmalarını veritabanı
/// collation'ından bağımsız hâle getirir.
/// </summary>
/// <remarks>
/// <para>
/// <b>Neden gerekli?</b> Doğrudan <c>p.UserName.Value == userName</c> yazıldığında
/// duyarlılık tamamen veritabanının collation'ına bağlıdır. SQL Server'ın varsayılanı
/// (<c>SQL_Latin1_General_CP1_CI_AS</c>) büyük/küçük harf duyarsızdır, ama sunucu
/// <c>_CS</c> ile oluşturulmuşsa <c>Admin</c> ile <c>admin</c> eşleşmez ve kullanıcı
/// "Kullanıcı bilgileri doğrulanamadı" hatası alır. Veritabanını kuran kişinin
/// tercihi davranışı sessizce değiştirebilmemeli.
/// </para>
/// <para>
/// <b>Neden <c>UPPER()</c> değil?</b> Kolon tarafında <c>UPPER()</c> çağırmak
/// Türkçe I sorununu yaratır: sunucunun dili Türkçe olduğunda <c>UPPER('i')</c>
/// sonucu <c>'İ'</c> olur, oysa .NET'in <c>ToUpperInvariant()</c> ile ürettiği
/// değer <c>'I'</c>'dır. İki taraf örtüşmez ve giriş bozulur.
/// </para>
/// <para>
/// <b>Çözüm.</b> Karşılaştırma tarafına açık bir <c>COLLATE</c> uygulanır. Bu,
/// sunucuda tam eşleşme yapar, büyük/küçük harfe duyarsızdır, aksanlara duyarlıdır
/// ve ne kolonun ne de veritabanının mevcut collation'ına bağlıdır. Türkçe'ye özgü
/// <c>ı</c>/<c>I</c> çifti de doğru şekilde farklı harf olarak kalır.
/// </para>
/// </remarks>
public static class LoginNameMatcher
{
    /// <summary>
    /// Karşılaştırmada kullanılacak açık collation.
    /// </summary>
    /// <remarks>
    /// <c>CI</c> = büyük/küçük harf duyarsız, <c>AS</c> = aksan duyarlı,
    /// <c>KS</c> = yerel ayarlara duyarsız (Türkçe I sorunu oluşmaz).
    /// </remarks>
    public const string Collation = "Latin1_General_100_CI_AS_KS";

    /// <summary>Girdi metnini karşılaştırma öncesi normalize eder.</summary>
    public static string Clean(string? value)
        => (value ?? string.Empty).Trim();
}

internal sealed class LoginScopeGetQueryHandler(

    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IMasterCompanyNameLookup companyNameLookup,
    ICompanyYearRepository companyYearRepository) : IRequestHandler<LoginScopeGetQuery, Result<LoginScopeDto?>>
{
    public async Task<Result<LoginScopeDto?>> Handle(
        LoginScopeGetQuery request,
        CancellationToken cancellationToken)
    {
        // Eşleşme veritabanı collation'ına değil koda bağlıdır; bkz. LoginNameMatcher.
        string userName = LoginNameMatcher.Clean(request.UserName);

        if (userName.Length == 0)
        {
            return Result<LoginScopeDto?>.Succeed(null);
        }

        var user = await userRepository.FirstOrDefaultAsync(p =>
            EF.Functions.Collate(p.UserName.Value, LoginNameMatcher.Collation) == userName
            || EF.Functions.Collate(p.Email.Value, LoginNameMatcher.Collation) == userName,
            cancellationToken);

        if (user is null)
        {
            return Result<LoginScopeDto?>.Succeed(null);
        }

        var role = await roleRepository.FirstOrDefaultAsync(r => r.Id == user.RoleId, cancellationToken);
        bool isSysAdmin = string.Equals(
            role?.Name.Value,
            SystemRoles.SysAdmin,
            StringComparison.OrdinalIgnoreCase);

        Guid ownCompanyId = user.CompanyId.Value;

        Dictionary<Guid, string> companyNames = await companyNameLookup.GetNamesAsync(
            [ownCompanyId],
            cancellationToken);

        string ownCompanyName = companyNames.GetValueOrDefault(ownCompanyId) ?? "Tanımsız kurum";

        List<CompanyYear> years = await companyYearRepository.GetAllAsync(cancellationToken);
        Dictionary<Guid, List<LoginScopeYearDto>> yearsByCompany = years
            .GroupBy(cy => cy.CompanyId.Value)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(cy => cy.Year.Value)
                    .Select(cy => new LoginScopeYearDto
                    {
                        CompanyYearId = cy.Id.Value,
                        Year = cy.Year.Value,
                        DatabaseName = cy.DatabaseName.Value,
                        IsClosed = cy.IsClosed
                    })
                    .ToList());

        List<LoginScopeCompanyDto> companies;

        if (isSysAdmin)
        {
            List<MasterCompanyInfo> allCompanies = await companyNameLookup.GetAllAsync(cancellationToken);

            companies = allCompanies
                .OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(c => new LoginScopeCompanyDto
                {
                    CompanyId = c.Id,
                    CompanyName = c.Name,
                    Years = yearsByCompany.GetValueOrDefault(c.Id) ?? []
                })
                .ToList();
        }
        else
        {
            companies =
            [
                new LoginScopeCompanyDto
                {
                    CompanyId = ownCompanyId,
                    CompanyName = ownCompanyName,
                    Years = yearsByCompany.GetValueOrDefault(ownCompanyId) ?? []
                }
            ];
        }

        return Result<LoginScopeDto?>.Succeed(new LoginScopeDto
        {
            CompanyId = ownCompanyId,
            CompanyName = ownCompanyName,
            IsSysAdmin = isSysAdmin,
            Companies = companies
        });
    }
}
