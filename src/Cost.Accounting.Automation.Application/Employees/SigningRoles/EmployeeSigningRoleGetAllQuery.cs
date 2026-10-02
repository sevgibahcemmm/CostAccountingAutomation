using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Employees;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Employees.SigningRoles;

/// <summary>
/// Yetkili görev tanımlarının tam listesi (tanım ekranı).
/// </summary>
[Permission("employee:view")]
public sealed record EmployeeSigningRoleGetAllQuery(
    bool OnlyDeleted = false) : IRequest<IQueryable<EmployeeSigningRoleDto>>
{
    public EmployeeSigningRoleGetAllQuery() : this(false) { }
}

internal sealed class EmployeeSigningRoleGetAllQueryHandler(
    IEmployeeSigningRoleRepository repository)
    : IRequestHandler<EmployeeSigningRoleGetAllQuery, IQueryable<EmployeeSigningRoleDto>>
{
    public Task<IQueryable<EmployeeSigningRoleDto>> Handle(
        EmployeeSigningRoleGetAllQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<EmployeeSigningRole>> source = request.OnlyDeleted
            ? repository.GetAllWithAuditIncludingDeleted().Where(r => r.Entity.IsDeleted)
            : repository.GetAllWithAudit();

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}

/// <summary>
/// Personel görev tablosundaki seçim kutusunu besleyen hafif liste. Yalnızca
/// aktif görevler döner; silinen görev geçmiş belgelerdeki imza adı olarak
/// korunabilsin diye listeden düşürülür ama tanım kaydı silinmez.
/// </summary>
[Permission("employee:view")]
public sealed record EmployeeSigningRoleLookUpQuery : IRequest<IQueryable<EmployeeSigningRoleOption>>;

internal sealed class EmployeeSigningRoleLookUpQueryHandler(
    IEmployeeSigningRoleRepository repository)
    : IRequestHandler<EmployeeSigningRoleLookUpQuery, IQueryable<EmployeeSigningRoleOption>>
{
    public async Task<IQueryable<EmployeeSigningRoleOption>> Handle(
        EmployeeSigningRoleLookUpQuery request,
        CancellationToken cancellationToken)
    {
        List<EmployeeSigningRole> roles = await repository.GetAllIncludingDeletedAsync(cancellationToken);

        // Name owned navigasyon olarak materyalize edilir; Include zinciri
        // eksik kaldığında null olabilir. Sıralama ve metin okuması null
        // güvenli yapılır; adı okunamayan bir görev listede boş satır olarak
        // düşer, komut doğrulaması zaten adı boş kaydedilmesini engeller.
        List<EmployeeSigningRoleOption> options =
        [
            .. roles
                .Where(r => !r.IsDeleted && r.IsActive)
                .OrderBy(r => r.SortOrder)
                .ThenBy(r => r.Name?.Value ?? string.Empty)
                .Select(r => new EmployeeSigningRoleOption
                {
                    Id = r.Id?.Value ?? Guid.Empty,
                    Name = r.Name?.Value ?? string.Empty,
                    Description = r.Description,
                    RequiresWorkshop = r.RequiresWorkshop,
                    SortOrder = r.SortOrder
                })
        ];

        return options.AsQueryable();
    }
}
