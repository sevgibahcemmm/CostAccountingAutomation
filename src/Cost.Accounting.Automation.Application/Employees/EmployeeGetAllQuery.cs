using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>
/// Personel listesi. <c>SigningRoles</c> ve <c>Workshops</c> sütunları liste
/// ekranında tek satırda özet gösterir; ayrıntılı görev-atölye eşleşmesi
/// düzenleme formundan okunur.
/// </summary>
[Permission("employee:view")]
public sealed record EmployeeGetAllQuery(
    bool OnlyDeleted = false,
    Guid? WorkshopId = null) : IRequest<IQueryable<EmployeeDto>>
{
    public EmployeeGetAllQuery() : this(false, null) { }
}

internal sealed class EmployeeGetAllQueryHandler(
    IEmployeeRepository employeeRepository) : IRequestHandler<EmployeeGetAllQuery, IQueryable<EmployeeDto>>
{
    public Task<IQueryable<EmployeeDto>> Handle(
        EmployeeGetAllQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<Employee>> source = request.OnlyDeleted
            ? employeeRepository.GetAllWithAuditIncludingDeleted().Where(e => e.Entity.IsDeleted)
            : employeeRepository.GetAllWithAudit();

        if (request.WorkshopId is Guid workshopId)
        {
            source = source.Where(e => e.Entity.Duties
                .Any(d => d.WorkshopId == new IdentityId(workshopId) && !d.IsDeleted));
        }

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}

/// <summary>
/// Tek bir personel kaydını görevleri ve atölyeleriyle birlikte getirir.
/// Düzenleme formu görev tablosunu bu sorgudan doldurur.
/// </summary>
[Permission("employee:view")]
public sealed record EmployeeGetQuery(Guid Id) : IRequest<Result<EmployeeDto>>;

internal sealed class EmployeeGetQueryHandler(
    IEmployeeRepository employeeRepository) : IRequestHandler<EmployeeGetQuery, Result<EmployeeDto>>
{
    public Task<Result<EmployeeDto>> Handle(
        EmployeeGetQuery request,
        CancellationToken cancellationToken)
    {
        // GetAllWithAuditIncludingDeleted sonucu belleğe materyalize edilip
        // LINQ-to-Objects olarak döner; bu yüzden FirstOrDefaultAsync yerine
        // FirstOrDefault kullanılır.
        Employee? employee = employeeRepository
            .GetAllWithAuditIncludingDeleted()
            .Where(e => e.Entity.Id == new IdentityId(request.Id))
            .Select(e => e.Entity)
            .FirstOrDefault();

        if (employee is null)
        {
            return Task.FromResult(Result<EmployeeDto>.Failure(EmployeeMessages.NotFound));
        }

        return Task.FromResult(Result<EmployeeDto>.Succeed(employee.ToDto()));
    }
}