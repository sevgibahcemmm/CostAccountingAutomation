using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cost.Accounting.Automation.Application.Permissions;
using Cost.Accounting.Automation.Application.Roles;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.RoleForms
{
    public partial class RoleEditForm : XtraForm
    {
        private static readonly Font FontBold = new("Segoe UI", 9F, FontStyle.Bold);
        private static readonly Font FontStrikeout = new("Segoe UI", 9F, FontStyle.Strikeout);
        private static readonly Color CheckedBack = Color.FromArgb(225, 244, 230);
        private static readonly Color AddedBack = Color.FromArgb(222, 240, 255);
        private static readonly Color RemovedFore = Color.FromArgb(205, 92, 66);

        private readonly RoleDto? _editing;
        private readonly bool _isSysAdmin;
        private readonly Dictionary<string, TreeListNode> _permissionNodes = new();
        private readonly HashSet<string> _originalPermissions = new(StringComparer.Ordinal);

        public RoleEditForm() : this(null)
        {
        }

        public RoleEditForm(RoleDto? existing)
        {
            InitializeComponent();
            _editing = existing;
            _isSysAdmin = existing is not null && string.Equals(existing.Name, "sys_admin", StringComparison.OrdinalIgnoreCase);

            Text = _editing is null ? "Yeni Rol" : "Rol Düzenle";
            lblTitle.Text = Text;

            if (_isSysAdmin)
            {
                lblSubtitle.Text = "Tüm yetkilere sahiptir — yetki ayrıcalıkları kilitli";
                lblPermissions.Visible = false;
                btnGroupSelectAll.Visible = false;
                btnGroupClear.Visible = false;
                trePermissions.Visible = false;
                lblSysAdminNote.Visible = true;
                lblPermissionSummary.Visible = false;
            }
            else
            {
                lblSysAdminNote.Visible = false;
                lblSubtitle.Text = _editing is null
                    ? "Yeni rol tanımlamak için bilgileri doldurun"
                    : "Rol bilgilerini güncelleyin";
            }

            chkActive.Checked = _editing?.IsActive ?? true;

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            btnGroupSelectAll.Click += (_, _) => SetAllChecked(true);
            btnGroupClear.Click += (_, _) => SetAllChecked(false);
            trePermissions.AfterCheckNode += TrePermissions_AfterCheckNode;
            trePermissions.CustomDrawNodeCell += TrePermissions_CustomDrawNodeCell;
            trePermissions.Click += TrePermissions_Click;
            Load += RoleEditForm_Load;
        }

        private async void RoleEditForm_Load(object? sender, EventArgs e)
        {
            btnSave.Enabled = false;
            try
            {
                if (!_isSysAdmin)
                {
                    await LoadPermissionTreeAsync();

                    if (_editing is not null)
                    {
                        txtName.Text = _editing.Name;
                        chkActive.Checked = _editing.IsActive;
                        await CheckExistingPermissionsAsync(_editing.Id);
                    }
                }
                else
                {
                    txtName.Text = _editing!.Name;
                }
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Yetki listesi yüklenemedi: " + ex.Message, ToastType.Error, 4000);
                Close();
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private async Task LoadPermissionTreeAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await mediator.Send(new PermissionGetAllQuery(), CancellationToken.None);
            BuildPermissionTree(result.Data ?? []);
        }

        private void BuildPermissionTree(List<string> permissions)
        {
            trePermissions.Nodes.Clear();
            _permissionNodes.Clear();

            foreach (var group in permissions
                .GroupBy(RolePermissionLabels.GetGroup)
                .OrderBy(g => RolePermissionLabels.GroupOrder(g.Key)))
            {
                TreeListNode groupNode = trePermissions.AppendNode(new object[] { RolePermissionLabels.GetGroupCaption(group.Key) }, null);
                trePermissions.SetNodeCheckState(groupNode, CheckState.Unchecked);

                foreach (string permission in group.OrderBy(p => p))
                {
                    TreeListNode leaf = trePermissions.AppendNode(new object[] { RolePermissionLabels.GetLabel(permission) }, groupNode);
                    trePermissions.SetNodeCheckState(leaf, CheckState.Unchecked);
                    leaf.Tag = permission;
                    _permissionNodes[permission] = leaf;
                }
            }

            trePermissions.ExpandAll();
            UpdatePermissionSummary();
        }

        private async Task CheckExistingPermissionsAsync(Guid roleId)
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await mediator.Send(new RoleGetQuery(roleId), CancellationToken.None);
            if (result.Data is null)
            {
                return;
            }

            foreach (string permission in result.Data.Permissions)
            {
                _originalPermissions.Add(permission);

                if (_permissionNodes.TryGetValue(permission, out TreeListNode? node))
                {
                    trePermissions.SetNodeCheckState(node, CheckState.Checked);
                }
            }

            UpdatePermissionSummary();
        }

        private void TrePermissions_AfterCheckNode(object? sender, NodeEventArgs e) => UpdatePermissionSummary();

        private void TrePermissions_Click(object? sender, EventArgs e)
        {
            TreeListHitInfo hit = trePermissions.CalcHitInfo(trePermissions.PointToClient(Cursor.Position));

            if (hit.Node is null || hit.HitInfoType != HitInfoType.Cell)
            {
                return;
            }

            TreeListNode node = hit.Node;
            CheckState next = node.CheckState == CheckState.Checked ? CheckState.Unchecked : CheckState.Checked;
            trePermissions.SetNodeCheckState(node, next, true);
            UpdatePermissionSummary();
        }

        private void UpdatePermissionSummary()
        {
            if (trePermissions.Nodes.Count == 0)
            {
                return;
            }

            int selected = 0;
            int added = 0;
            int removed = 0;

            foreach (TreeListNode groupNode in trePermissions.Nodes)
            {
                foreach (TreeListNode leaf in groupNode.Nodes)
                {
                    if (leaf.Tag is not string permission)
                    {
                        continue;
                    }

                    bool isChecked = leaf.CheckState == CheckState.Checked;
                    bool wasSet = _originalPermissions.Contains(permission);

                    if (isChecked)
                    {
                        selected++;
                        if (!wasSet)
                        {
                            added++;
                        }
                    }
                    else if (wasSet)
                    {
                        removed++;
                    }
                }
            }

            lblPermissionSummary.Text = _editing is null
                ? $"{selected} yetki seçildi"
                : $"{selected} yetki seçildi · <color=#4A90D9>+{added} yeni</color> · <color=#CD5C42>-{removed} kaldırılacak</color>";

            trePermissions.Invalidate();
        }

        private void TrePermissions_CustomDrawNodeCell(object? sender, CustomDrawNodeCellEventArgs e)
        {
            if (e.Node is null || e.Column != colPermission)
            {
                return;
            }

            if (e.Node.HasChildren)
            {
                if (e.Node.CheckState == CheckState.Checked)
                {
                    e.Appearance.BackColor = CheckedBack;
                }

                e.Appearance.Font = FontBold;
                return;
            }

            if (e.Node.Tag is not string permission)
            {
                return;
            }

            bool isChecked = e.Node.CheckState == CheckState.Checked;
            bool wasSet = _originalPermissions.Contains(permission);

            if (isChecked)
            {
                e.Appearance.BackColor = wasSet ? CheckedBack : AddedBack;
                e.Appearance.ForeColor = wasSet ? SystemColors.ControlText : Color.FromArgb(35, 90, 160);
                e.Appearance.Font = wasSet ? FontBold : FontBold;
            }
            else if (wasSet)
            {
                e.Appearance.ForeColor = RemovedFore;
                e.Appearance.Font = FontStrikeout;
                e.Appearance.BackColor = Color.FromArgb(255, 242, 238);
            }
        }

        private void SetAllChecked(bool state)
        {
            if (state)
            {
                trePermissions.CheckAll();
            }
            else
            {
                trePermissions.UncheckAll();
            }

            UpdatePermissionSummary();
        }

        private static List<string> GetSelectedPermissions(TreeList tree)
        {
            var selected = new List<string>();
            foreach (TreeListNode groupNode in tree.Nodes)
            {
                foreach (TreeListNode leaf in groupNode.Nodes)
                {
                    if (leaf.CheckState == CheckState.Checked && leaf.Tag is string permission)
                    {
                        selected.Add(permission);
                    }
                }
            }

            return selected;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            bool isActive = chkActive.Checked;
            List<string> selectedPermissions = GetSelectedPermissions(trePermissions);

            IRequest<Result<string>> command = _editing is null
                ? new RoleCreateCommand(name, isActive, selectedPermissions.Count > 0 ? selectedPermissions : null)
                : new RoleUpdateCommand(_editing.Id, name, isActive, _isSysAdmin ? null : selectedPermissions);

            if (!RunApplicationValidator(command))
            {
                return;
            }

            btnSave.Enabled = false;
            try
            {
                bool ok = await CrudExecutor.ExecuteAsync(command);
                if (ok)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private bool RunApplicationValidator(object command)
        {
            txtName.ErrorText = string.Empty;

            ValidationResult? result = command switch
            {
                RoleCreateCommand create => new RoleCreateCommandValidator().Validate(create),
                RoleUpdateCommand update => new RoleUpdateCommandValidator().Validate(update),
                _ => null,
            };

            if (result is null || result.IsValid)
            {
                return true;
            }

            var messages = new List<string>();
            foreach (ValidationFailure failure in result.Errors)
            {
                if (failure.PropertyName is nameof(RoleCreateCommand.Name) or nameof(RoleUpdateCommand.Name))
                {
                    txtName.ErrorText = failure.ErrorMessage;
                }

                messages.Add(failure.ErrorMessage);
            }

            ToastHelper.Show(string.Join(Environment.NewLine, messages), ToastType.Warning, 6000);
            return false;
        }
    }
}