using Cost.Accounting.Automation.Application.Roles;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.RoleForms
{
    public sealed partial class RolesListForm : CrudListFormBase<RoleGetAllQuery, RoleDto, RoleEditForm>
    {
        public RolesListForm() : base("Roller ve Yetkiler")
        {
            InitializeDetailView();
        }

        protected override SvgImage ModuleIcon => SvgIcons.ShieldIcon;

        protected override string[] SearchFieldNames =>
        [
            nameof(RoleDto.Name)
        ];

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            foreach (Control ctrl in Controls)
            {
                if (ctrl is Panel panel && panel.Dock == DockStyle.Top)
                {
                    panel.Height = 100;
                    break;
                }
            }
        }

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();

            GridColumn colName = View.Columns[nameof(RoleDto.Name)]!;
            colName.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Near;

            GridColumn colCreatedAt = View.Columns[nameof(RoleDto.CreatedAt)]!;
            colCreatedAt.Caption = "Kayıt Tarihi";
            colCreatedAt.DisplayFormat.FormatType = FormatType.Custom;
            colCreatedAt.DisplayFormat.FormatString = "dd/MMMMM/yyyy";
            colCreatedAt.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
            colCreatedAt.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;

            View.Columns[nameof(RoleDto.IsActive)]!.Caption = "Durum";
        }

        private void InitializeDetailView()
        {
            GridView detailView = new()
            {
                Name = "RolePermissionsDetailView",
                GridControl = BaseGrid
            };
            detailView.OptionsBehavior.Editable = false;
            detailView.OptionsDetail.AllowOnlyOneMasterRowExpanded = true;
            detailView.OptionsView.ShowGroupPanel = false;
            detailView.OptionsView.ShowHorizontalLines = DefaultBoolean.False;
            detailView.RowHeight = 26;
            detailView.Appearance.HeaderPanel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            detailView.Appearance.HeaderPanel.Options.UseFont = true;

            GridColumn colPermission = new()
            {
                Caption = "Tanımlanan Yetkiler",
                FieldName = nameof(RolePermissionDto.Key),
                Visible = true,
                Width = 460
            };
            colPermission.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Near;
            detailView.Columns.Add(colPermission);

            detailView.CustomColumnDisplayText += (_, e) =>
            {
                if (e.Column?.FieldName == nameof(RolePermissionDto.Key) && e.Value is string key && !string.IsNullOrEmpty(key))
                {
                    e.DisplayText = RolePermissionLabels.GetLabel(key);
                }
            };

            View.MasterRowEmpty += (_, e) =>
            {
                if (View.GetRow(e.RowHandle) is RoleDto role)
                {
                    e.IsEmpty = role.PermissionCount == 0;
                }
            };

            GridLevelNode levelNode = new()
            {
                RelationName = nameof(RoleDto.PermissionDetails),
                LevelTemplate = detailView
            };
            BaseGrid.LevelTree.Nodes.Add(levelNode);
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(RoleDto item)
            => new RoleDeleteCommand(item.Id);

        protected override string GetDeleteSummary(RoleDto item) => item.Name;
    }
}