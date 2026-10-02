namespace Cost.Accounting.Automation.Domain.Employees.ValueObjects;

/// <summary>
/// Personelin rütbesi / görev ünvanı (örn. "Bakanlık Müdürü", "Atölye Şefi").
/// Rapor imza bloklarındaki "Ünvanı" satırına bu değer basılır.
/// </summary>
public sealed record EmployeeTitle(string Value);