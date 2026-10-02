using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>Rapor imza satırına basılacak yetkili.</summary>
/// <param name="SigningRoleId">Görevin kimliği (<c>EmployeeSigningRoles</c>).</param>
public sealed record ReportSignatory(
    Guid SigningRoleId,
    string RoleName,
    string FullName,
    string Title,
    string? PhotoPath);

/// <summary>Bir belgenin ihtiyaç duyduğu imza yuvası.</summary>
/// <param name="SigningRoleId">Yuvanın karşılığı olan görev tanımının kimliği.</param>
/// <param name="Caption">Raporda yuvanın üstünde yazacak satır başlığı.</param>
public sealed record SignatorySlot(Guid SigningRoleId, string Caption);

/// <summary>
/// Bir belge için imza yetkililerini çözer.
///
/// Görev tanımları veritabanında tutulduğu için yuva bir enum değeri değil,
/// <see cref="EmployeeSigningRole"/> kimliğiyle tanımlanır. Böylece kurum
/// kendi imza görevini tanımlayıp rapor şablonuna ekleyebilir.
///
/// <para>
/// Görev çözümlemesi iki basamakta olur: <c>WorkshopId</c> verilmişse önce
/// o atölyeye bağlı görev, verilmemişse kurum geneli görev aranır. Bir kişi
/// hem atölyeye bağlı hem kurum geneli görev taşıyabildiği için atölyeye
/// özgü kayıt bulunamazsa kurum geneline düşülür.
/// </para>
/// </summary>
[Permission("employee:view")]
public sealed record ReportSignatoryQuery(
    Guid? WorkshopId,
    IReadOnlyList<SignatorySlot> Slots) : IRequest<Result<IReadOnlyDictionary<Guid, ReportSignatory>>>;

internal sealed class ReportSignatoryQueryHandler(
    IEmployeeDutyRepository employeeDutyRepository) : IRequestHandler<ReportSignatoryQuery, Result<IReadOnlyDictionary<Guid, ReportSignatory>>>
{
    public Task<Result<IReadOnlyDictionary<Guid, ReportSignatory>>> Handle(
        ReportSignatoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Slots.Count == 0)
        {
            return Task.FromResult(
                Result<IReadOnlyDictionary<Guid, ReportSignatory>>.Succeed(
                    new Dictionary<Guid, ReportSignatory>()));
        }

        List<Guid> roles = [.. request.Slots.Select(s => s.SigningRoleId)];

        IdentityId? workshopId = request.WorkshopId is Guid id ? new IdentityId(id) : null;

        // SigningRoleId bir value-converter alanıdır; sorgu `.Value` özelliğine
        // değil nesnenin kendisine karşı yazılmalıdır, aksi hâlde LINQ ifadesi
        // SQL'e çevrilemez. Bu yüzden liste Guid değil IdentityId tutar ve
        // karşılaştırma alanın kendisiyle yapılır.
        List<IdentityId?> roleIds = [.. roles.Select(r => new IdentityId(r))];

        List<EmployeeDuty> duties = employeeDutyRepository
            .GetAll()
            .Include(d => d.Employee)
            .Where(d => roleIds.Contains(d.SigningRoleId)
                && !d.IsDeleted
                && d.Employee!.IsActive
                && !d.Employee.IsDeleted)
            .ToList();

        var result = new Dictionary<Guid, ReportSignatory>();

        foreach (SignatorySlot slot in request.Slots)
        {
            ReportSignatory? signatory = Resolve(duties, slot.SigningRoleId, workshopId);

            if (signatory is not null)
            {
                result[slot.SigningRoleId] = signatory;
            }
        }

        return Task.FromResult(
            Result<IReadOnlyDictionary<Guid, ReportSignatory>>.Succeed(result));
    }

    /// <summary>
    /// Önce atölyeye bağlı kayıt, yoksa kurum geneli kayıt aranır. Bu sıra
    /// raporun doğruluğu için önemlidir: maliyet pusulası bir atölyeye aittir
    /// ve o atölyenin şefi imzalamalıdır; atölye şefi tanımlı değilse
    /// kurum genelindeki yetkiliye düşülür.
    /// </summary>
    private static ReportSignatory? Resolve(
        List<EmployeeDuty> duties,
        Guid signingRoleId,
        IdentityId? workshopId)
    {
        EmployeeDuty? match = workshopId is null
            ? duties.FirstOrDefault(d =>
                d.SigningRoleId == new IdentityId(signingRoleId) && d.WorkshopId is null && d.IsActive)
            : duties.FirstOrDefault(d =>
                d.SigningRoleId == new IdentityId(signingRoleId) && d.IsActive
                && (d.WorkshopId == workshopId
                    || (workshopId is IdentityId w && d.WorkshopId is not null && d.WorkshopId == w)));

        match ??= duties.FirstOrDefault(d =>
            d.SigningRoleId == new IdentityId(signingRoleId) && d.WorkshopId is null && d.IsActive);

        if (match?.Employee is null)
        {
            return null;
        }

        return new ReportSignatory(
            signingRoleId,
            match.SigningRole?.Name.Value ?? string.Empty,
            match.Employee.FullName,
            match.Employee.Title.Value,
            match.Employee.PhotoPath);
    }
}
