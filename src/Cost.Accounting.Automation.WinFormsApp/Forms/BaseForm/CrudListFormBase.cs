using System.Drawing;
using Cost.Accounting.Automation.Application.Behaviors;
using DomainEntityDto = Cost.Accounting.Automation.Domain.Abstractions.EntityDto;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    public abstract partial class CrudListFormBase<TListQuery, TDto, TEditForm> : XtraFormMdiBase
        where TListQuery : class, IRequest<IQueryable<TDto>>, new()
        where TDto : DomainEntityDto
        where TEditForm : XtraForm
    {
        protected List<TDto> _allItems = [];

        private bool _showDeleted;
        private bool _isFilterSetting;

        protected CrudListFormBase(string formTitle) : base(formTitle)
        {
            InitializeComponent();
            lblTitle.Text = formTitle;
            IconOptions.SvgImage = ModuleIcon;
            WireEvents();
            SetupGrid();
            SetupButtonIcons();
            AddHeaderIcon();
        }

        protected GridView View => gridView;

        protected DevExpress.XtraGrid.GridControl BaseGrid => gridControl;

        protected DevExpress.XtraEditors.PanelControl HeaderPanel => pnlHeader;

        protected DevExpress.XtraEditors.PanelControl ToolbarPanel => pnlToolbar;

        protected virtual SvgImage ModuleIcon => SvgIcons.Modules[5];

        protected virtual bool AllowDelete => true;

        protected virtual bool AllowsEdit(TDto item) => true;

        protected virtual bool AllowsDelete(TDto item) => true;

        protected bool ShowDeleted => _showDeleted;

        protected virtual bool SupportsRestore => false;

        protected virtual bool SupportsApprove => false;

        protected virtual IRequest<Result<string>>? BuildApproveCommand(TDto item) => null;

        protected virtual TListQuery BuildListQuery() => new();

        protected virtual IRequest<Result<string>>? BuildRestoreCommand(TDto item) => null;

        protected virtual string[] SearchFieldNames => [];

        protected abstract void ConfigureColumns();

        protected abstract IRequest<Result<string>> BuildDeleteCommand(TDto item);

        protected virtual string GetDeleteSummary(TDto item) => item.Id.ToString();

        protected virtual string GetSubtitle(int count)
            => _showDeleted ? $"{count} silinen kayıt" : $"{count} kayıt listeleniyor";

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            IconOptions.SvgImage = ModuleIcon;
            btnDeleted.Visible = SupportsRestore;
            if (!AllowDelete)
            {
                btnDelete.Visible = false;
            }
            btnApprove.Visible = SupportsApprove;
            ConfigureColumns();
            _ = ReloadAsync();
        }

        private void WireEvents()
        {
            btnClosePage.Click += (_, _) => Close();
            btnNew.Click += async (_, _) => await RunEditorAsync(null);
            btnEdit.Click += async (_, _) =>
            {
                int[] rows = gridView.GetSelectedRows();
                if (rows.Length == 1 && gridView.GetRow(rows[0]) is TDto dto)
                {
                    if (!AllowsEdit(dto))
                    {
                        ToastHelper.Show("Bu kayıt düzenlenemez.", ToastType.Warning);
                        return;
                    }

                    await RunEditorAsync(dto);
                }
            };
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += async (_, _) => await ReloadAsync();
            btnApprove.Click += BtnApprove_Click;
            btnDeleted.CheckedChanged += BtnDeleted_CheckedChanged;
            btnRestore.Click += BtnRestore_Click;
            txtSearch.EditValueChanged += TxtSearch_EditValueChanged;
            cmbFilter.EditValueChanged += CmbFilter_EditValueChanged;
            gridView.DoubleClick += GridView_DoubleClick;
            gridView.SelectionChanged += GridView_SelectionChanged;
        }

        private void SetupGrid()
        {
            gridView.OptionsBehavior.AutoPopulateColumns = false;
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.OptionsSelection.MultiSelect = true;
            gridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
            gridView.OptionsBehavior.Editable = false;
            gridView.OptionsView.EnableAppearanceEvenRow = true;
            gridView.OptionsView.EnableAppearanceOddRow = true;
        }

        private void SetupButtonIcons()
        {
            SetButtonIcon(btnNew, SvgIcons.PlusIcon, 18);
            SetButtonIcon(btnEdit, SvgIcons.EditIcon, 18);
            SetButtonIcon(btnDelete, SvgIcons.TrashIcon, 18);
            SetButtonIcon(btnRefresh, SvgIcons.RefreshIcon, 18);
            SetButtonIcon(btnDeleted, SvgIcons.TrashIcon, 18);
            SetButtonIcon(btnRestore, SvgIcons.RestoreIcon, 18);
            SetButtonIcon(btnApprove, SvgIcons.CheckIcon, 18);
            SetButtonIcon(btnClosePage, SvgIcons.CloseIcon, 16);
        }

        private static void SetButtonIcon(SimpleButton button, SvgImage icon, int size)
        {
            button.ImageOptions.SvgImage = icon;
            button.ImageOptions.SvgImageSize = new Size(size, size);
            button.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
        }

        /// <summary>
        /// Sağ üst arama çubuğunun soluna depo/cari gibi filtre amaçlı bir lookup ekler.
        /// </summary>
        protected void ConfigureFilter(object? dataSource, string valueMember, string displayMember, string caption)
        {
            bool hasItems = dataSource is System.Collections.IEnumerable { } items && items.Cast<object?>().Any();

            if (!hasItems)
            {
                lblFilter.Visible = false;
                cmbFilter.Visible = false;
                return;
            }

            _isFilterSetting = true;
            try
            {
                cmbFilter.Properties.DataSource = dataSource;
                cmbFilter.Properties.ValueMember = valueMember;
                cmbFilter.Properties.DisplayMember = displayMember;
                cmbFilter.Properties.PopupFilterMode = PopupFilterMode.Contains;
                cmbFilter.Properties.BestFitMode = BestFitMode.BestFit;

                cmbFilterView.Columns.Clear();
                GridColumn column = cmbFilterView.Columns.AddField(displayMember);
                column.Caption = caption;
                column.VisibleIndex = 0;
                column.Width = 160;
                cmbFilterView.BestFitColumns();

                lblFilter.Text = caption + ":";
                lblFilter.Visible = true;
                cmbFilter.Visible = true;
            }
            finally
            {
                _isFilterSetting = false;
            }
        }

        protected Guid? SelectedFilterGuid => cmbFilter.EditValue is Guid filterId ? filterId : null;

        private static void SetButtonIcon(CheckButton button, SvgImage icon, int size)
        {
            button.ImageOptions.SvgImage = icon;
            button.ImageOptions.SvgImageSize = new Size(size, size);
            button.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
        }

        private void AddHeaderIcon()
        {
            PictureEdit pic = new()
            {
                Location = new Point(28, 31),
                Size = new Size(42, 42),
                BackColor = Color.Transparent
            };
            pic.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            pic.Properties.SvgImageColorizationMode = DevExpress.Utils.SvgImageColorizationMode.Default;
            pic.SvgImage = ModuleIcon;
            pic.Properties.Appearance.BackColor = Color.Transparent;
            pic.Properties.Appearance.Options.UseBackColor = true;
            pnlHeader.Controls.Add(pic);
            lblTitle.Location = new Point(82, 16);
            lblSub.Location = new Point(84, 66);
        }

        protected GridColumn CreateBooleanColumn(string caption, string fieldName)
            => new()
            {
                Caption = caption,
                FieldName = fieldName,
                Visible = true
            };

        protected void AddColumnsFromAttributes()
            => GridColumnFactory.ConfigureFromAttributes(View, typeof(TDto));

        private void GridView_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            int selected = gridView.GetSelectedRows().Length;
            bool showDeleted = _showDeleted;
            bool allEditable = true;
            bool allDeletable = true;

            foreach (int row in gridView.GetSelectedRows())
            {
                if (gridView.GetRow(row) is not TDto dto)
                {
                    continue;
                }

                if (!AllowsEdit(dto))
                {
                    allEditable = false;
                }

                if (!AllowsDelete(dto))
                {
                    allDeletable = false;
                }
            }

            btnNew.Enabled = !showDeleted;
            btnEdit.Enabled = !showDeleted && selected == 1 && allEditable;
            btnDelete.Enabled = !showDeleted && selected >= 1 && allDeletable;
            btnDelete.Visible = AllowDelete && !showDeleted;
            btnApprove.Enabled = SupportsApprove && !showDeleted && selected >= 1;
            btnApprove.Visible = SupportsApprove && !showDeleted;
            btnRestore.Enabled = showDeleted && selected >= 1;
            btnRestore.Visible = showDeleted && SupportsRestore;
            btnDeleted.Checked = showDeleted;
        }

        protected virtual async Task ReloadAsync()
        {
            try
            {
                lblSub.Text = "Yenileniyor...";
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                TListQuery query = BuildListQuery();
                CrashLog.Write("Reload", $"{typeof(TListQuery).Name} query built");
                gridControl.DataSource = null;
                List<TDto> items = (await mediator.Send(query, CancellationToken.None)).ToList();
                CrashLog.Write("Reload", $"{typeof(TListQuery).Name} returned {items.Count} items");
                _allItems = items;
                gridControl.DataSource = items;
                gridView.BestFitColumns();
                lblSub.Text = GetSubtitle(items.Count);
            }
            catch (AuthorizationException ex)
            {
                CrashLog.WriteException("Reload.Auth", ex);
                ToastHelper.Show(ex.Message, ToastType.Warning, 4000);
                lblSub.Text = "Yetkiniz yok";
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Reload", ex);
                ToastHelper.Show("Liste yüklenemedi: " + ex.Message, ToastType.Error, 4000);
                lblSub.Text = "Yükleme hatası";
            }
            finally
            {
                UpdateButtonStates();
            }
        }

        private async Task RunEditorAsync(TDto? item)
        {
            TEditForm form;
            try
            {
                CrashLog.Write("Editor", $"Creating edit form for {typeof(TDto).Name}. New:{(item is null)}");
                form = item is null
                    ? Activator.CreateInstance<TEditForm>()
                    : (TEditForm)Activator.CreateInstance(typeof(TEditForm), item)!;
                CrashLog.Write("Editor", "Edit form created");
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Editor.Create", ex);
                ToastHelper.Show("Form açılamadı: " + ex.Message, ToastType.Error, 8000);
                return;
            }

            using (form)
            {
                try
                {
                    CrashLog.Write("Editor", "Showing edit dialog");
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        await ReloadAsync();
                    }
                    CrashLog.Write("Editor", "Edit dialog closed");
                }
                catch (Exception ex)
                {
                    CrashLog.WriteException("Editor.Show", ex);
                    ToastHelper.Show("Form hatası: " + ex.Message, ToastType.Error, 8000);
                }
            }
        }

        private void TxtSearch_EditValueChanged(object? sender, EventArgs e)
        {
            string term = (txtSearch.Text ?? string.Empty).Trim();
            if (term.Length == 0 || SearchFieldNames.Length == 0)
            {
                gridView.ActiveFilterString = "";
                return;
            }

            string safe = term.Replace("'", "''");
            string filter = string.Join(" OR ", SearchFieldNames.Select(field => $"[{field}] LIKE '%{safe}%'"));
            gridView.ActiveFilterString = filter;
        }

        private async void GridView_DoubleClick(object? sender, EventArgs e)
        {
            if (_showDeleted)
            {
                return;
            }

            if (gridView.GetFocusedRow() is TDto dto)
            {
                if (!AllowsEdit(dto))
                {
                    ToastHelper.Show("Bu kayıt düzenlenemez.", ToastType.Warning);
                    return;
                }

                await RunEditorAsync(dto);
            }
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (!AllowDelete)
            {
                return;
            }

            List<TDto> selected = gridView.GetSelectedRows()
                .Select(i => gridView.GetRow(i) as TDto)
                .Where(x => x is not null)
                .Cast<TDto>()
                .ToList();

            if (selected.Count == 0)
            {
                ToastHelper.Show("Silinecek kayıtları işaretleyin", ToastType.Warning);
                return;
            }

            if (selected.Any(item => !AllowsDelete(item)))
            {
                ToastHelper.Show("Silinemeyen kayıt(lar) seçildi. İşlem iptal edildi.", ToastType.Warning);
                return;
            }

            string preview = string.Join(", ", selected.Take(3).Select(GetDeleteSummary));
            if (selected.Count > 3)
            {
                preview += $" ve {selected.Count - 3} kayıt daha";
            }

            if (MsgBox.Confirm($"{selected.Count} kayıt silinecek.\n{preview}\n\nEmin misiniz?", $"{Text} - Silme Onayı") != DialogResult.Yes)
            {
                return;
            }

            btnDelete.Enabled = false;
            try
            {
                foreach (TDto item in selected)
                {
                    bool ok = await CrudExecutor.ExecuteAsync(BuildDeleteCommand(item));
                    if (!ok) break;
                }
                await ReloadAsync();
            }
            finally
            {
                btnDelete.Enabled = true;
            }
        }

        private async void CmbFilter_EditValueChanged(object? sender, EventArgs e)
        {
            if (_isFilterSetting)
            {
                return;
            }

            await ReloadAsync();
        }

        private async void BtnApprove_Click(object? sender, EventArgs e)
        {
            if (!SupportsApprove)
            {
                return;
            }

            List<TDto> selected = gridView.GetSelectedRows()
                .Select(i => gridView.GetRow(i) as TDto)
                .Where(x => x is not null)
                .Cast<TDto>()
                .ToList();

            if (selected.Count == 0)
            {
                ToastHelper.Show("Onaylanacak kayıtları işaretleyin", ToastType.Warning);
                return;
            }

            btnApprove.Enabled = false;
            try
            {
                foreach (TDto item in selected)
                {
                    IRequest<Result<string>>? command = BuildApproveCommand(item);
                    if (command is null)
                    {
                        ToastHelper.Show($"'{GetDeleteSummary(item)}' zaten onaylanmış durumda.", ToastType.Warning);
                        continue;
                    }

                    if (!await CrudExecutor.ExecuteAsync(command))
                    {
                        break;
                    }
                }
                await ReloadAsync();
            }
            finally
            {
                btnApprove.Enabled = true;
            }
        }

        private async void BtnDeleted_CheckedChanged(object? sender, EventArgs e)
        {
            _showDeleted = btnDeleted.Checked;
            UpdateButtonStates();
            await ReloadAsync();
        }

        private async void BtnRestore_Click(object? sender, EventArgs e)
        {
            if (!_showDeleted || !SupportsRestore)
            {
                return;
            }

            List<TDto> selected = gridView.GetSelectedRows()
                .Select(i => gridView.GetRow(i) as TDto)
                .Where(x => x is not null)
                .Cast<TDto>()
                .ToList();

            if (selected.Count == 0)
            {
                ToastHelper.Show("Geri yüklenecek kayıtları işaretleyin", ToastType.Warning);
                return;
            }

            string preview = string.Join(", ", selected.Take(3).Select(GetDeleteSummary));
            if (selected.Count > 3)
            {
                preview += $" ve {selected.Count - 3} kayıt daha";
            }

            if (MsgBox.Confirm($"{selected.Count} kayıt geri yüklenecek.\n{preview}\n\nEmin misiniz?", $"{Text} - Geri Yükleme Onayı") != DialogResult.Yes)
            {
                return;
            }

            btnRestore.Enabled = false;
            bool restored = false;
            try
            {
                foreach (TDto item in selected)
                {
                    IRequest<Result<string>>? command = BuildRestoreCommand(item);
                    if (command is null)
                    {
                        ToastHelper.Show($"{typeof(TDto).Name} için geri yükleme desteklenmiyor", ToastType.Warning);
                        return;
                    }

                    if (!await CrudExecutor.ExecuteAsync(command))
                    {
                        break;
                    }

                    restored = true;
                }
            }
            finally
            {
                btnRestore.Enabled = true;
            }

            if (restored && btnDeleted.Checked)
            {
                btnDeleted.Checked = false;
                return;
            }

            await ReloadAsync();
        }
    }
}