using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.Roles;
using Cost.Accounting.Automation.Application.Users;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.UserForms
{
    public partial class UserEditForm : XtraForm
    {
        private readonly UserDto? _editing;
        private List<RoleDto> _roles = [];
        private PhotoInput? _avatar;

        public UserEditForm() : this(null)
        {
        }

        public UserEditForm(UserDto? existing)
        {
            InitializeComponent();
            _editing = existing;

            Text = _editing is null ? "Yeni Kullanıcı" : "Kullanıcı Düzenle";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Yeni kullanıcı oluşturmak için bilgileri doldurun"
                : "Kullanıcı bilgilerini güncelleyin";
            lblPasswordNote.Visible = _editing is null;
            chkActive.Checked = _editing?.IsActive ?? true;

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            btnAddPhoto.Click += BtnAddPhoto_Click;
            btnRemovePhoto.Click += BtnRemovePhoto_Click;
            Load += UserEditForm_Load;
        }

        private void FieldIcon_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is LabelControl icon && icon.Tag is Control editor)
            {
                editor.Focus();
            }
        }

        private async void UserEditForm_Load(object? sender, EventArgs e)
        {
            btnSave.Enabled = false;
            try
            {
                await LoadLookupsAsync();
                if (_editing is not null)
                {
                    Populate(_editing);
                    if (!string.IsNullOrWhiteSpace(_editing.DefaultPhotoPath))
                    {
                        LoadAvatarFromPath(_editing.DefaultPhotoPath);
                    }
                }
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Şirket/rol listesi yüklenemedi: " + ex.Message, ToastType.Error, 4000);
                Close();
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private void LoadAvatarFromPath(string relativePath)
        {
            string fullPath = StorageRoot.Resolve(relativePath);
            if (!File.Exists(fullPath))
            {
                return;
            }

            byte[] data = File.ReadAllBytes(fullPath);
            string fileName = Path.GetFileName(relativePath);
            string contentType = GetContentType(relativePath);
            _avatar = new PhotoInput(Path.GetFileName(relativePath), contentType, data, true);
            picPhoto.Image = Image.FromStream(new MemoryStream(_avatar.Data));
        }

        private async Task LoadLookupsAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            _roles = (await mediator.Send(new RoleGetAllQuery(), CancellationToken.None)).ToList();
            List<CompanyDto> companies = (await mediator.Send(new CompanyGetAllQuery(), CancellationToken.None)).ToList();

            ConfigureLookUp(cmbRole, _roles, nameof(RoleDto.Id), nameof(RoleDto.Name), "Rol");
            ConfigureLookUp(cmbCompany, companies, nameof(CompanyDto.Id), nameof(CompanyDto.Name), "Şirket");
        }

        private static void ConfigureLookUp(SearchLookUpEdit editor, object dataSource, string valueMember, string displayMember, string caption)
        {
            editor.Properties.DataSource = dataSource;
            editor.Properties.ValueMember = valueMember;
            editor.Properties.DisplayMember = displayMember;
            editor.Properties.PopupFilterMode = PopupFilterMode.Contains;
            editor.Properties.BestFitMode = BestFitMode.BestFit;

            GridView view = editor.Properties.View;
            view.Columns.Clear();
            GridColumn column = view.Columns.AddField(displayMember);
            column.Caption = caption;
            column.VisibleIndex = 0;
            column.Width = 260;
            view.BestFitColumns();
        }

        private void Populate(UserDto user)
        {
            txtFirstName.Text = user.FirstName;
            txtLastName.Text = user.LastName;
            txtUserName.Text = user.UserName;
            txtEmail.Text = user.Email;
            txtTcNo.Text = user.TRIdentityNumber ?? string.Empty;
            cmbCompany.EditValue = user.CompanyId;
            cmbRole.EditValue = user.RoleId;
            chkActive.Checked = user.IsActive;
        }

        private void BtnAddPhoto_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Görseller (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Multiselect = false,
                Title = "Avatar Seç",
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string file = dialog.FileName;
            byte[] data = File.ReadAllBytes(file);
            string fileName = Path.GetFileName(file);
            string contentType = GetContentType(fileName);
            _avatar = new PhotoInput(fileName, contentType, data, true);

            using var ms = new MemoryStream(_avatar.Data);
            picPhoto.Image = Image.FromStream(ms);
        }

        private void BtnRemovePhoto_Click(object? sender, EventArgs e)
        {
            if (_avatar is null)
            {
                ToastHelper.Show("Kaldırılacak avatar yok", ToastType.Warning);
                return;
            }

            _avatar = null;
            picPhoto.Image = null;
            ToastHelper.Show("Avatar kaldırıldı", ToastType.Success, 2000);
        }

        private void RefreshPhotoList()
        {
            if (_avatar is null)
            {
                picPhoto.Image = null;
                return;
            }

            using var ms = new MemoryStream(_avatar.Data);
            picPhoto.Image = Image.FromStream(ms);
        }

        private static string GetContentType(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".jpeg" or ".jpg" => "image/jpeg",
                _ => "application/octet-stream",
            };
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string userName = txtUserName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string tcNo = (txtTcNo.EditValue as string ?? txtTcNo.Text).Trim().Replace("-", "");
            Guid? companyId = cmbCompany.EditValue as Guid?;
            Guid roleId = cmbRole.EditValue is Guid r ? r : Guid.Empty;
            bool isActive = chkActive.Checked;
            List<PhotoInput> photos = _avatar is not null ? [_avatar] : [];

            IRequest<Result<string>> command = _editing is null
                ? new UserCreateCommand(firstName, lastName, email, userName, companyId, roleId, isActive, tcNo, photos)
                : new UserUpdateCommand(_editing.Id, firstName, lastName, email, userName, companyId, roleId, isActive, tcNo, photos);

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
            ClearFieldErrors();

            ValidationResult? result = command switch
            {
                UserCreateCommand create => new UserCreateCommandValidator().Validate(create),
                UserUpdateCommand update => new UserUpdateCommandValidator().Validate(update),
                _ => null,
            };

            if (result is null || result.IsValid)
            {
                return true;
            }

            (string Property, BaseEdit Editor)[] map =
            [
                (nameof(UserCommandFields.FirstName), txtFirstName),
                (nameof(UserCommandFields.LastName), txtLastName),
                (nameof(UserCommandFields.UserName), txtUserName),
                (nameof(UserCommandFields.Email), txtEmail),
                (nameof(UserCommandFields.TRIdentityNumber), txtTcNo),
                (nameof(UserCommandFields.RoleId), cmbRole),
            ];

            var messages = new List<string>();
            foreach (ValidationFailure failure in result.Errors)
            {
                (string, BaseEdit) entry = map.FirstOrDefault(m => m.Property == failure.PropertyName);
                if (entry.Item2 is not null)
                {
                    entry.Item2.ErrorText = failure.ErrorMessage;
                    messages.Add(failure.ErrorMessage);
                }
            }

            if (messages.Count == 0)
            {
                messages.AddRange(result.Errors.Select(e => e.ErrorMessage));
            }

            (string, BaseEdit) firstInvalid = map.FirstOrDefault(m => result.Errors.Any(e => e.PropertyName == m.Property));
            firstInvalid.Item2?.Focus();

            ToastHelper.Show(string.Join(Environment.NewLine, messages), ToastType.Warning, 6000);
            return false;
        }

        private static class UserCommandFields
        {
            public const string FirstName = "FirstName";
            public const string LastName = "LastName";
            public const string UserName = "UserName";
            public const string Email = "Email";
            public const string TRIdentityNumber = "TRIdentityNumber";
            public const string RoleId = "RoleId";
        }

        private void ClearFieldErrors()
        {
            txtFirstName.ErrorText = string.Empty;
            txtLastName.ErrorText = string.Empty;
            txtUserName.ErrorText = string.Empty;
            txtEmail.ErrorText = string.Empty;
            txtTcNo.ErrorText = string.Empty;
            cmbCompany.ErrorText = string.Empty;
            cmbRole.ErrorText = string.Empty;
        }
    }
}