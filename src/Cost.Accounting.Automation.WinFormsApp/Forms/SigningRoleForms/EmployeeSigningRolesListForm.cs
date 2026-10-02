using Cost.Accounting.Automation.Application.Employees.SigningRoles;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.SigningRoleForms
{
    /// <summary>
    /// Rapor imza bloklarında kullanılacak yetkili görevlerin tanım listesi.
    /// Görevler enum değil veritabanı kaydı olduğu için buradan yeni görev
    /// eklenebilir; tanım eklendiğinde personel kaydındaki görev seçim kutusu
    /// da yeni görevi gösterir.
    /// </summary>
    public sealed partial class EmployeeSigningRolesListForm
        : CrudListFormBase<EmployeeSigningRoleGetAllQuery, EmployeeSigningRoleDto, EmployeeSigningRoleEditForm>
    {
        public EmployeeSigningRolesListForm() : base("Yetkili Görevler")
        {
            InitializeComponent();
        }

        protected override SvgImage ModuleIcon => DxIcon.Roles;

        protected override string[] SearchFieldNames =>
        [
            nameof(EmployeeSigningRoleDto.Name),
            nameof(EmployeeSigningRoleDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(EmployeeSigningRoleDto item)
            => new EmployeeSigningRoleDeleteCommand(item.Id);

        protected override string GetDeleteSummary(EmployeeSigningRoleDto item)
            => item.Name;

        protected override bool SupportsRestore => true;

        protected override EmployeeSigningRoleGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(EmployeeSigningRoleDto item)
            => new EmployeeSigningRoleRestoreCommand(item.Id);
    }
}
