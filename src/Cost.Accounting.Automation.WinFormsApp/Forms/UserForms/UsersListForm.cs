using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Cost.Accounting.Automation.Application.Users;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.UserForms
{
    public sealed partial class UsersListForm : CrudListFormBase<UserGetAllQuery, UserDto, UserEditForm>
    {
        public UsersListForm() : base("Kullanıcılar")
        {
            View.CustomUnboundColumnData += UsersListForm_CustomUnboundColumnData;
        }

        protected override SvgImage ModuleIcon => DxIcon.Users;

        protected override string[] SearchFieldNames =>
        [
            nameof(UserDto.TRIdentityNumber),
            nameof(UserDto.UserName),
            nameof(UserDto.FullName),
            nameof(UserDto.Email),
            nameof(UserDto.CompanyName),
            nameof(UserDto.RoleName)
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

            if (View.GridControl != null)
            {
                ToolTipController toolTipController = new ToolTipController();
                toolTipController.GetActiveObjectInfo += ToolTipController_GetActiveObjectInfo;
                View.GridControl.ToolTipController = toolTipController;
            }
        }

        private void ToolTipController_GetActiveObjectInfo(object? sender, ToolTipControllerGetActiveObjectInfoEventArgs e)
        {
            if (e.SelectedControl is DevExpress.XtraGrid.GridControl grid)
            {
                if (grid.MainView is GridView view)
                {
                    Point pt = grid.PointToClient(Control.MousePosition);
                    GridHitInfo hi = view.CalcHitInfo(pt);
                    if (hi.InRowCell && hi.Column.FieldName == "PhotoPreview")
                    {
                        if (view.GetRow(hi.RowHandle) is UserDto user)
                        {
                            SuperToolTip superTip = new();
                            ToolTipTitleItem titleItem = new() { Text = user.FullName };
                            superTip.Items.Add(titleItem);

                            e.Info = new ToolTipControlInfo(hi.RowHandle.ToString() + hi.Column.FieldName, string.Empty);
                            e.Info.SuperTip = superTip;
                        }
                    }
                }
            }
        }

        protected override void ConfigureColumns()
        {
            View.Columns.Clear();
            AddColumnsFromAttributes();
            GridColumn colPhoto = new()
            {
                Caption = "Foto",
                FieldName = "PhotoPreview",
                UnboundType = UnboundColumnType.Object,
                Visible = true,
                Width = 60,
                ColumnEdit = new RepositoryItemPictureEdit
                {
                    SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom,
                    AllowZoom = DefaultBoolean.True,
                    ShowZoomSubMenu = DefaultBoolean.True
                }
            };
            View.Columns.AddRange([colPhoto]);
            colPhoto.VisibleIndex = 0;
            colPhoto.OptionsColumn.FixedWidth = true;
        }

        private static readonly Dictionary<Guid, Image> _imageCache = new();
        private static readonly object _imageCacheLock = new();

        private void UsersListForm_CustomUnboundColumnData(object? sender, CustomColumnDataEventArgs e)
        {
            if (e.Column.FieldName != "PhotoPreview")
            {
                return;
            }

            if (e.ListSourceRowIndex < 0 || View is null)
            {
                return;
            }

            if (View.GetRow(e.ListSourceRowIndex) is not UserDto user)
            {
                return;
            }

            if (e.IsGetData)
            {
                e.Value = BuildUserImage(user);
            }
        }

        private static Image BuildUserImage(UserDto user)
        {
            lock (_imageCacheLock)
            {
                if (_imageCache.TryGetValue(user.Id, out Image? cached))
                {
                    return cached;
                }

                Image image = TryLoadPhoto(user) ?? CreateAvatar(GetInitials(user));
                _imageCache[user.Id] = image;
                return image;
            }
        }

        private static Image? TryLoadPhoto(UserDto user)
        {
            if (string.IsNullOrWhiteSpace(user.DefaultPhotoPath))
            {
                return null;
            }

            try
            {
                string fullPath = StorageRoot.Resolve(user.DefaultPhotoPath);
                if (!File.Exists(fullPath))
                {
                    return null;
                }

                using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                using var ms = new MemoryStream();
                fs.CopyTo(ms);
                return new Bitmap(ms);
            }
            catch
            {
                return null;
            }
        }

        private static Bitmap CreateAvatar(string initials)
        {
            Bitmap bmp = new(60, 60);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(SkinTheme.Primary);
                g.FillEllipse(brush, 0, 0, 60, 60);
                using var textBrush = new SolidBrush(SkinTheme.GetContrastText(SkinTheme.Primary));
                using var font = new Font("Segoe UI", 20, FontStyle.Bold);
                SizeF size = g.MeasureString(initials, font);
                g.DrawString(initials, font, textBrush, (60 - size.Width) / 2, (60 - size.Height) / 2);
            }

            return bmp;
        }

        private static string GetInitials(UserDto user)
        {
            string first = user.FirstName.Length > 0 ? user.FirstName[..1].ToUpperInvariant() : "";
            string last = user.LastName.Length > 0 ? user.LastName[..1].ToUpperInvariant() : "";
            string result = first + last;
            return result.Length > 0 ? result : "?";
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(UserDto item)
            => new UserDeleteCommand(item.Id);

        protected override string GetDeleteSummary(UserDto item) => item.UserName;

        protected override bool SupportsRestore => true;

        protected override UserGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(UserDto item)
            => new UserRestoreCommand(item.Id);

        protected override async Task ReloadAsync()
        {
            ClearImageCache();
            await base.ReloadAsync();
        }

        private static void ClearImageCache()
        {
            lock (_imageCacheLock)
            {
                _imageCache.Clear();
            }
        }
    }
}