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
            View.Columns.Clear();

            GridColumn colName = new()
            {
                Caption = "Rol Adı",
                FieldName = nameof(RoleDto.Name),
                Visible = true,
                Width = 220
            };
            colName.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Near;

            GridColumn colPermissionCount = new()
            {
                Caption = "Yetki Sayısı",
                FieldName = nameof(RoleDto.PermissionCount),
                Visible = true,
                Width = 120
            };
            colPermissionCount.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
            colPermissionCount.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;

            GridColumn colIsActive = CreateBooleanColumn("Durum", nameof(RoleDto.IsActive));

            GridColumn colCreatedAt = new()
            {
                Caption = "Kayıt Tarihi",
                FieldName = nameof(RoleDto.CreatedAt),
                Visible = true,
                Width = 140
            };
            colCreatedAt.DisplayFormat.FormatType = FormatType.Custom;
            colCreatedAt.DisplayFormat.FormatString = "dd/MMMMM/yyyy";
            colCreatedAt.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
            colCreatedAt.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;

            View.Columns.AddRange([colName, colPermissionCount, colIsActive, colCreatedAt]);
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