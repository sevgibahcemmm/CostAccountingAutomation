using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
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
    IEmployeeRepository employeeRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<EmployeeGetAllQuery, IQueryable<EmployeeDto>>
{
    public async Task<IQueryable<EmployeeDto>> Handle(
        EmployeeGetAllQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Employee> source = request.OnlyDeleted
            ? employeeRepository.GetAllIncludingDeleted().Where(e => e.IsDeleted)
            : employeeRepository.GetAllActive();

        if (request.WorkshopId is Guid workshopId)
        {
            IdentityId id = new(workshopId);
            source = source.Where(e => e.Duties.Any(d => d.WorkshopId == id && !d.IsDeleted));
        }

        return await source
            .MapTo()
            .ToAuditedListAsync(auditUserNames, cancellationToken);
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
    public async Task<Result<EmployeeDto>> Handle(
        EmployeeGetQuery request,
        CancellationToken cancellationToken)
    {
        // Kimlik veritabanında filtrelenir. Daha önce liste sorgusu belleğe
        // alınıp bellekte FirstOrDefault ile aranıyordu; "tek personel getir"
        // isteği tüm personel tablosunu okuyor ve SENKRON çalışıyordu.
        Employee? employee = await employeeRepository
            .GetAllIncludingDeleted()
            .Include(e => e.Duties)
            .ThenInclude(d => d.SigningRole)
            .Include(e => e.Duties)
            .ThenInclude(d => d.Workshop)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == new IdentityId(request.Id), cancellationToken);

        if (employee is null)
        {
            return Result<EmployeeDto>.Failure(EmployeeMessages.NotFound);
        }

        return employee.ToDto();
    }
}
