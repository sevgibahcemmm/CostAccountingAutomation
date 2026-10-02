using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>Rapor imza satırına basılacak yetkili.</summary>
public sealed record ReportSignatory(
    EmployeeSigningRole Role,
    string RoleName,
    string FullName,
    string Title,
    string? PhotoPath);

/// <summary>Bir belgenin ihtiyaç duyduğu imza yuvası.</summary>
/// <param name="Role">Yuvanın karşılığı olan görev.</param>
/// <param name="Caption">Raporda yuvanın üstünde yazacak satır başlığı.</param>
public sealed record SignatorySlot(EmployeeSigningRole Role, string Caption);

/// <summary>
/// Bir belge için imza yetkililerini çözer.
///
/// Raporların imza blokları bugün tasarım dosyasında sabit metin taşıyor
/// ("Adı Soyadı :", "© Yazılımcı Emrullah AKPINAR / Muhasebe Yetkilisi" gibi).
/// Bu sorgu, belgenin atölyesine ve kurumuna göre doğru personeli bulup
/// adı-soyadı ve ünvanıyla döndürür.
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
    IReadOnlyList<SignatorySlot> Slots) : IRequest<Result<IReadOnlyDictionary<EmployeeSigningRole, ReportSignatory>>>;

internal sealed class ReportSignatoryQueryHandler(
    IEmployeeDutyRepository employeeDutyRepository) : IRequestHandler<ReportSignatoryQuery, Result<IReadOnlyDictionary<EmployeeSigningRole, ReportSignatory>>>
{
    public Task<Result<IReadOnlyDictionary<EmployeeSigningRole, ReportSignatory>>> Handle(
        ReportSignatoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Slots.Count == 0)
        {
            return Task.FromResult(
                Result<IReadOnlyDictionary<EmployeeSigningRole, ReportSignatory>>.Succeed(
                    new Dictionary<EmployeeSigningRole, ReportSignatory>()));
        }

        List<EmployeeSigningRole> roles = [.. request.Slots.Select(s => s.Role)];

        IdentityId? workshopId = request.WorkshopId is Guid id ? new IdentityId(id) : null;

        List<EmployeeDuty> duties = employeeDutyRepository
            .GetAll()
            .Include(d => d.Employee)
            .Where(d => roles.Contains(d.SigningRole)
                && !d.IsDeleted
                && d.Employee!.IsActive
                && !d.Employee.IsDeleted)
            .ToList();

        var result = new Dictionary<EmployeeSigningRole, ReportSignatory>();

        foreach (SignatorySlot slot in request.Slots)
        {
            ReportSignatory? signatory = Resolve(duties, slot.Role, workshopId);

            if (signatory is not null)
            {
                result[slot.Role] = signatory;
            }
        }

        return Task.FromResult(
            Result<IReadOnlyDictionary<EmployeeSigningRole, ReportSignatory>>.Succeed(result));
    }

    /// <summary>
    /// Önce atölyeye bağlı kayıt, yoksa kurum geneli kayıt aranır. Bu sıra
    /// raporun doğruluğu için önemlidir: maliyet pusulası bir atölyeye aittir
    /// ve o atölyenin şefi imzalamalıdır; atölye şefi tanımlı değilse
    /// kurum genelindeki yetkiliye düşülür.
    /// </summary>
    private static ReportSignatory? Resolve(
        List<EmployeeDuty> duties,
        EmployeeSigningRole role,
        IdentityId? workshopId)
    {
        EmployeeDuty? match = workshopId is null
            ? duties.FirstOrDefault(d =>
                d.SigningRole == role && d.WorkshopId is null && d.IsActive)
            : duties.FirstOrDefault(d =>
                d.SigningRole == role && d.IsActive
                && (d.WorkshopId == workshopId
                    || (workshopId is IdentityId w && d.WorkshopId is not null && d.WorkshopId == w)));

        match ??= duties.FirstOrDefault(d =>
            d.SigningRole == role && d.WorkshopId is null && d.IsActive);

        if (match?.Employee is null)
        {
            return null;
        }

        return new ReportSignatory(
            match.SigningRole,
            EnumDisplay.GetDisplayName(match.SigningRole),
            match.Employee.FullName,
            match.Employee.Title.Value,
            match.Employee.PhotoPath);
    }
}