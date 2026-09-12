using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.Roles;
using Cost.Accounting.Automation.Application.Users;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
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
        private readonly List<PhotoInput> _photos = [];

        public UserEditForm() : this(null)
        {
        }

        public UserEditForm(UserDto? existing)
        {
            InitializeComponent();
            _editing = existing;
            IconOptions.SvgImage = SvgIcons.Modules[5];

            Text = _editing is null ? "Yeni Kullanıcı" : "Kullanıcı Düzenle";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Yeni kullanıcı oluşturmak için bilgileri doldurun"
                : "Kullanıcı bilgilerini güncelleyin";
            lblPasswordNote.Visible = _editing is null;
            chkActive.Checked = _editing?.IsActive ?? true;

            lblHeaderIcon.ImageOptions.SvgImage = SvgIcons.HeaderUserIcon;
            lblHeaderIcon.ImageOptions.SvgImageSize = new Size(32, 32);

            StyleTabs();
            StyleButtons();

            AddFieldIcon(tabPersonal, txtFirstName, SvgIcons.UserIcon);
            AddFieldIcon(tabPersonal, txtLastName, SvgIcons.UserIcon);
            AddFieldIcon(tabPersonal, txtUserName, SvgIcons.AtIcon);
            AddFieldIcon(tabPersonal, txtEmail, SvgIcons.MailIcon);
            AddFieldIcon(tabAccount, txtTcNo, SvgIcons.IdCardIcon);
            AddFieldIcon(tabAccount, cmbCompany, SvgIcons.BuildingIcon);
            AddFieldIcon(tabAccount, cmbRole, SvgIcons.KeyIcon);

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            btnAddPhoto.Click += BtnAddPhoto_Click;
            btnSetDefault.Click += BtnSetDefault_Click;
            btnRemovePhoto.Click += BtnRemovePhoto_Click;
            Load += UserEditForm_Load;
        }

        private static void AddFieldIcon(Control parent, TextEdit editor, SvgImage icon)
        {
            editor.Properties.Padding = new Padding(26, 2, 2, 2);
            AddFieldIconCore(parent, editor, icon);
        }

        private static void AddFieldIcon(Control parent, SearchLookUpEdit editor, SvgImage icon)
        {
            editor.Properties.Padding = new Padding(26, 2, 2, 2);
            AddFieldIconCore(parent, editor, icon);
        }

        private static void AddFieldIconCore(Control parent, Control editor, SvgImage icon)
        {
            var label = new LabelControl
            {
                Parent = parent,
                Name = "Icon_" + editor.Name,
                Location = new Point(editor.Left + 4, editor.Top + 4),
                Size = new Size(18, 18),
                Cursor = Cursors.Default,
            };
            label.ImageOptions.SvgImage = icon;
            label.ImageOptions.SvgImageSize = new Size(18, 18);
            label.MouseDown += (_, _) => editor.Focus();
        }

        private void StyleTabs()
        {
            tabPersonal.ImageOptions.SvgImage = SvgIcons.UserIcon;
            tabPersonal.ImageOptions.SvgImageSize = new Size(16, 16);
            tabAccount.ImageOptions.SvgImage = SvgIcons.ShieldIcon;
            tabAccount.ImageOptions.SvgImageSize = new Size(16, 16);
            tabPhotos.ImageOptions.SvgImage = SvgIcons.PhotoIcon;
            tabPhotos.ImageOptions.SvgImageSize = new Size(16, 16);
        }

        private void StyleButtons()
        {
            btnSave.ImageOptions.SvgImage = SvgIcons.CheckIcon;
            btnSave.ImageOptions.SvgImageSize = new Size(20, 20);
            btnSave.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnCancel.ImageOptions.SvgImage = SvgIcons.CloseIcon;
            btnCancel.ImageOptions.SvgImageSize = new Size(16, 16);
            btnCancel.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnAddPhoto.ImageOptions.SvgImage = SvgIcons.PlusIcon;
            btnAddPhoto.ImageOptions.SvgImageSize = new Size(16, 16);
            btnAddPhoto.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnSetDefault.ImageOptions.SvgImage = SvgIcons.StarIcon;
            btnSetDefault.ImageOptions.SvgImageSize = new Size(16, 16);
            btnSetDefault.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnRemovePhoto.ImageOptions.SvgImage = SvgIcons.TrashIcon;
            btnRemovePhoto.ImageOptions.SvgImageSize = new Size(16, 16);
            btnRemovePhoto.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
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
                    await LoadExistingPhotosAsync();
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

        private async Task LoadExistingPhotosAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            var photos = await mediator.Send(new UserGetPhotosQuery(_editing!.Id), CancellationToken.None);
            if (photos.Data is null)
            {
                return;
            }

            foreach (PhotoDto photo in photos.Data)
            {
                _photos.Add(new PhotoInput(photo.FileName, photo.ContentType, photo.Data, photo.IsDefault));
            }
            RefreshPhotoList();
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
                Multiselect = true,
                Title = "Fotoğraf Seç",
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            foreach (string file in dialog.FileNames)
            {
                byte[] data = File.ReadAllBytes(file);
                string fileName = Path.GetFileName(file);
                string contentType = GetContentType(fileName);
                bool isDefault = _photos.Count == 0;
                _photos.Add(new PhotoInput(fileName, contentType, data, isDefault));
            }

            RefreshPhotoList();
        }

        private void BtnSetDefault_Click(object? sender, EventArgs e)
        {
            if (lstPhotos.SelectedIndex < 0)
            {
                ToastHelper.Show("Önce bir fotoğraf seçin", ToastType.Warning);
                return;
            }

            for (int i = 0; i < _photos.Count; i++)
            {
                _photos[i] = _photos[i] with { IsDefault = i == lstPhotos.SelectedIndex };
            }

            RefreshPhotoList();
            ToastHelper.Show("Varsayılan fotoğraf güncellendi", ToastType.Success, 2000);
        }

        private void BtnRemovePhoto_Click(object? sender, EventArgs e)
        {
            int index = lstPhotos.SelectedIndex;
            if (index < 0)
            {
                ToastHelper.Show("Önce bir fotoğraf seçin", ToastType.Warning);
                return;
            }

            _photos.RemoveAt(index);
            if (_photos.Count > 0 && !_photos.Any(p => p.IsDefault))
            {
                _photos[0] = _photos[0] with { IsDefault = true };
            }

            RefreshPhotoList();
        }

        private void LstPhotos_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void RefreshPhotoList()
        {
            int previous = lstPhotos.SelectedIndex;
            lstPhotos.Items.Clear();
            foreach (PhotoInput photo in _photos)
            {
                lstPhotos.Items.Add((photo.IsDefault ? "★ " : "") + photo.FileName);
            }

            if (_photos.Count == 0)
            {
                picPhoto.Image = null;
                return;
            }

            lstPhotos.SelectedIndex = previous >= 0 && previous < lstPhotos.Items.Count ? previous : 0;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (_photos.Count == 0)
            {
                picPhoto.Image = null;
                return;
            }

            int index = lstPhotos.SelectedIndex;
            if (index < 0 || index >= _photos.Count)
            {
                index = _photos.FindIndex(p => p.IsDefault);
                if (index < 0)
                {
                    index = 0;
                }
            }

            PhotoInput toShow = _photos[index];
            using var ms = new MemoryStream(toShow.Data);
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
            List<PhotoInput> photos = _photos.ToList();

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