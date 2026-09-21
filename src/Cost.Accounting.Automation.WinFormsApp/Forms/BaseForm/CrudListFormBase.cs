using System.Drawing;
using Cost.Accounting.Automation.Application.Behaviors;
using DomainEntityDto = Cost.Accounting.Automation.Domain.Abstractions.EntityDto;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips;

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
            picModuleIcon.SvgImage = ModuleIcon;
        }

        protected GridView View => gridView;

        protected DevExpress.XtraGrid.GridControl BaseGrid => gridControl;

        protected DevExpress.XtraEditors.PanelControl HeaderPanel => pnlHeader;

        protected DevExpress.XtraEditors.PanelControl ToolbarPanel => pnlToolbar;

protected virtual SvgImage ModuleIcon => DxIcon.Module;

        protected virtual bool AllowDelete => true;

        protected virtual bool AllowsEdit(TDto item) => true;

        protected virtual bool AllowsDelete(TDto item) => true;

        protected bool ShowDeleted => _showDeleted;

        protected virtual bool SupportsRestore => false;

        protected virtual bool SupportsApprove => false;

        protected virtual bool AllowsApprove(TDto item) => true;

        /// <summary>
        /// Liste ekranında "Maliyet Pusulası Yazdır" gibi bir rapor butonunun gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsSlipReport => false;

        /// <summary>
        /// Liste ekranında "Gider Dağıtım Tablosu Yazdır" butonunun gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsDistributionReport => false;

        /// <summary>
        /// Liste ekranında "Mamül Beyan Yazdır" butonunun gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsProductDeclarationReport => false;

        /// <summary>
        /// Liste ekranında "Stok Hareket Listesi Yazdır" butonunun gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsStockMovementsListReport => false;

        /// <summary>
        /// Liste ekranında "Stok Sayım Listesi Yazdır" butonunun gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsStockCountListReport => false;

        protected virtual IRequest<Result<string>>? BuildApproveCommand(TDto item) => null;

        /// <summary>
        /// Liste ekranında "TIF Yazdır" butonu ve sağ tık menüsünün gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsSlipPrint => false;

        /// <summary>
        /// Belirli bir kaydın taşınır işlem fişi olarak yazdırılıp yazdırılamayacağı.
        /// </summary>
        protected virtual bool CanPrintSlip(TDto item) => SupportsSlipPrint;

        /// <summary>
        /// Kaydın taşınır işlem fişi verisini üretir. Desteklenmiyorsa null döner.
        /// </summary>
        protected virtual Task<MovableAssetTransactionSlipData?> BuildSlipDataAsync(TDto item)
            => Task.FromResult<MovableAssetTransactionSlipData?>(null);

        /// <summary>
        /// Seçili kaydın raporu (ör. maliyet pusulası) açılır. Desteklenmiyorsa hiçbir işlem yapılmaz.
        /// </summary>
        protected virtual Task ShowSlipReportAsync(TDto item) => Task.CompletedTask;

        /// <summary>
        /// Seçili kaydın gider dağıtım tablosu raporu açılır. Desteklenmiyorsa hiçbir işlem yapılmaz.
        /// </summary>
        protected virtual Task ShowDistributionReportAsync(TDto item) => Task.CompletedTask;

        /// <summary>
        /// Mamül üretim beyanı raporu açılır. Desteklenmiyorsa hiçbir işlem yapılmaz.
        /// </summary>
        protected virtual Task ShowProductDeclarationReportAsync(TDto? item) => Task.CompletedTask;

        /// <summary>
        /// Stok hareket listesi raporu açılır. Desteklenmiyorsa hiçbir işlem yapılmaz.
        /// </summary>
        protected virtual Task ShowStockMovementsListReportAsync(TDto? item) => Task.CompletedTask;

        /// <summary>
        /// Stok sayım listesi raporu açılır. Desteklenmiyorsa hiçbir işlem yapılmaz.
        /// </summary>
        protected virtual Task ShowStockCountListReportAsync(TDto? item) => Task.CompletedTask;

        protected virtual TListQuery BuildListQuery() => new();

        protected virtual IRequest<Result<string>>? BuildRestoreCommand(TDto item) => null;

        protected virtual string[] SearchFieldNames => [];

        protected abstract void ConfigureColumns();

        protected abstract IRequest<Result<string>> BuildDeleteCommand(TDto item);

        protected virtual string GetDeleteSummary(TDto item) => item.Id.ToString();

        /// <summary>
        /// Hareket görüldüğü vb. nedenle silinemeyen kayıtları döndürür. Boş değilse silme
        /// onayı hiç sorulmaz; yalnızca bir bilgi toast'ı gösterilir ve silme çalıştırılmaz.
        /// </summary>
        protected virtual Task<List<TDto>> GetUndeletableAsync(List<TDto> selected, CancellationToken cancellationToken)
            => Task.FromResult(new List<TDto>());

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
            btnSlipPrint.Visible = SupportsSlipPrint;
            btnSlipReport.Visible = SupportsSlipReport;
            btnDistributionReport.Visible = SupportsDistributionReport;
            btnProductDeclaration.Visible = SupportsProductDeclarationReport;
            btnStockMovementsList.Visible = SupportsStockMovementsListReport;
            btnStockCountList.Visible = SupportsStockCountListReport;
            SetupSlipContextMenu();
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
btnSlipPrint.Click += async (_, _) => await ShowSelectedSlipAsync();
            btnSlipReport.Click += async (_, _) => await ShowSelectedSlipReportAsync();
            btnDistributionReport.Click += async (_, _) => await ShowSelectedDistributionReportAsync();
            btnProductDeclaration.Click += async (_, _) => await ShowSelectedProductDeclarationReportAsync();
            btnStockMovementsList.Click += async (_, _) => await ShowSelectedStockMovementsListReportAsync();
            btnStockCountList.Click += async (_, _) => await ShowSelectedStockCountListReportAsync();
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
            gridView.OptionsView.ColumnAutoWidth = false;
        }

        private void SetupButtonIcons()
        {
            SetButtonIcon(btnNew, DxIcon.Add, 18);
            SetButtonIcon(btnEdit, DxIcon.Edit, 18);
            SetButtonIcon(btnDelete, DxIcon.Delete, 18);
            SetButtonIcon(btnRefresh, DxIcon.Refresh, 18);
            SetButtonIcon(btnSlipPrint, DxIcon.Receipt, 18);
            SetButtonIcon(btnSlipReport, DxIcon.Receipt, 18);
            SetButtonIcon(btnDistributionReport, DxIcon.Receipt, 18);
            SetButtonIcon(btnProductDeclaration, DxIcon.Receipt, 18);
            SetButtonIcon(btnStockMovementsList, DxIcon.Receipt, 18);
            SetButtonIcon(btnStockCountList, DxIcon.StockBox, 18);
            SetButtonIcon(btnDeleted, DxIcon.Delete, 18);
            SetButtonIcon(btnRestore, DxIcon.Restore, 18);
SetButtonIcon(btnApprove, DxIcon.Check, 18);
            SetButtonIcon(btnClosePage, DxIcon.Close, 16);
            AutoSizeToolbarButtons();
        }

        private void AutoSizeToolbarButtons()
        {
            foreach (SimpleButton button in flpToolbar.Controls.OfType<SimpleButton>())
            {
                button.Width = MeasureButtonWidth(button);
            }
        }

        private static int MeasureButtonWidth(SimpleButton button)
        {
            bool hasIcon = button.ImageOptions.SvgImage is not null;
            int textWidth = string.IsNullOrEmpty(button.Text)
                ? 0
                : System.Windows.Forms.TextRenderer.MeasureText(button.Text, button.Appearance.Font).Width;
            int iconWidth = hasIcon ? button.ImageOptions.SvgImageSize.Width + 6 : 0;
            int padding = string.IsNullOrEmpty(button.Text) ? 16 : 56;
            return textWidth + iconWidth + padding;
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
            bool allPrintable = true;
            bool allApprovable = true;

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

                if (!CanPrintSlip(dto))
                {
                    allPrintable = false;
                }

                if (!AllowsApprove(dto))
                {
                    allApprovable = false;
                }
            }

            btnNew.Enabled = !showDeleted;
            btnEdit.Enabled = !showDeleted && selected == 1 && allEditable;
            btnDelete.Enabled = !showDeleted && selected >= 1 && allDeletable;
            btnDelete.Visible = AllowDelete && !showDeleted;
            btnSlipPrint.Visible = SupportsSlipPrint;
            btnSlipPrint.Enabled = !showDeleted && selected == 1 && allPrintable;
btnApprove.Enabled = SupportsApprove && !showDeleted && selected >= 1 && allApprovable;
            btnApprove.Visible = SupportsApprove && !showDeleted;
            btnSlipReport.Visible = SupportsSlipReport;
            btnSlipReport.Enabled = !showDeleted && selected == 1;
            btnDistributionReport.Visible = SupportsDistributionReport;
            btnDistributionReport.Enabled = !showDeleted;
            btnProductDeclaration.Visible = SupportsProductDeclarationReport;
            btnProductDeclaration.Enabled = !showDeleted;
            btnStockMovementsList.Visible = SupportsStockMovementsListReport;
            btnStockMovementsList.Enabled = !showDeleted;
            btnStockCountList.Visible = SupportsStockCountListReport;
            btnStockCountList.Enabled = !showDeleted;
            btnRestore.Enabled = showDeleted && selected >= 1;
            btnRestore.Visible = showDeleted && SupportsRestore;
            btnDeleted.Checked = showDeleted;
        }

        private void SetupSlipContextMenu()
        {
            if (!SupportsSlipPrint)
            {
                gridControl.ContextMenuStrip = null;
                return;
            }

            slipMenuItem.Click += async (_, _) => await ShowFocusedSlipAsync();
            slipMenu.Opening += (_, _) =>
                slipMenuItem.Enabled = CanPrintSlipItem(gridView.GetFocusedRow() as TDto);
        }

        private bool CanPrintSlipItem(TDto? item)
            => !_showDeleted && item is not null && CanPrintSlip(item);

        private async Task ShowSelectedSlipAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            if (rows.Length != 1 || gridView.GetRow(rows[0]) is not TDto dto)
            {
                ToastHelper.Show("Taşınır işlem fişi için tek bir kayıt seçin.", ToastType.Warning);
                return;
            }

            await PrintSlipAsync(dto);
        }

        private async Task ShowSelectedSlipReportAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            if (rows.Length != 1 || gridView.GetRow(rows[0]) is not TDto dto)
            {
                ToastHelper.Show("Rapor için tek bir kayıt seçin.", ToastType.Warning);
                return;
            }

            try
            {
                await ShowSlipReportAsync(dto);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("SlipReport", ex);
                ToastHelper.Show("Rapor açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private async Task ShowSelectedDistributionReportAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            TDto? dto = rows.Length == 1 ? gridView.GetRow(rows[0]) as TDto : null;

            try
            {
                await ShowDistributionReportAsync(dto!);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("DistributionReport", ex);
                ToastHelper.Show("Rapor açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private async Task ShowSelectedProductDeclarationReportAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            TDto? dto = rows.Length == 1 ? gridView.GetRow(rows[0]) as TDto : null;

            try
            {
                await ShowProductDeclarationReportAsync(dto);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("ProductDeclarationReport", ex);
                ToastHelper.Show("Rapor açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private async Task ShowSelectedStockMovementsListReportAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            TDto? dto = rows.Length == 1 ? gridView.GetRow(rows[0]) as TDto : null;

            try
            {
                await ShowStockMovementsListReportAsync(dto);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("StockMovementsListReport", ex);
                ToastHelper.Show("Rapor açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private async Task ShowSelectedStockCountListReportAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            TDto? dto = rows.Length == 1 ? gridView.GetRow(rows[0]) as TDto : null;

            try
            {
                await ShowStockCountListReportAsync(dto);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("StockCountListReport", ex);
                ToastHelper.Show("Rapor açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private async Task ShowFocusedSlipAsync()
        {
            if (gridView.GetFocusedRow() is not TDto dto)
            {
                return;
            }

            await PrintSlipAsync(dto);
        }

        private async Task PrintSlipAsync(TDto item)
        {
            if (!CanPrintSlip(item))
            {
                ToastHelper.Show("Bu kayıt için taşınır işlem fişi oluşturulamaz.", ToastType.Warning);
                return;
            }

            try
            {
                MovableAssetTransactionSlipData? data = await BuildSlipDataAsync(item);
                if (data is null)
                {
                    return;
                }

                await MovableAssetTransactionSlipPresenter.ShowAsync(data);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("SlipPrint", ex);
                ToastHelper.Show("Taşınır işlem fişi açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
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
                List<TDto> items = (await mediator.Send(query, CancellationToken.None))
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.Id)
                    .ToList();
                CrashLog.Write("Reload", $"{typeof(TListQuery).Name} returned {items.Count} items");
                _allItems = items;
gridControl.DataSource = items;
                FitColumnsToContent();
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

        /// <summary>
        /// Kolon genişliklerini içeriklere göre ayarlar; metin kolonları devasa büyümesin diye
        /// tavan değer uygulanır. Sabit genişlikli (görsel) kolonlar etkilenmez.
        /// </summary>
        private void FitColumnsToContent()
        {
            gridView.BeginUpdate();
            try
            {
                foreach (GridColumn column in gridView.Columns)
                {
                    if (column.MaxWidth == 0)
                    {
                        column.MaxWidth = Math.Max(280, column.Width * 2);
                    }
                }

                gridView.BestFitColumns();
            }
            finally
            {
                gridView.EndUpdate();
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
                    form.ShowDialog(this);
                    await ReloadAsync();
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

            List<TDto> blocked;
            try
            {
                blocked = await GetUndeletableAsync(selected, CancellationToken.None);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Delete.PreCheck", ex);
                ToastHelper.Show("Silme kontrolü yapılamadı: " + ex.Message, ToastType.Error, 6000);
                return;
            }

            if (blocked.Count > 0)
            {
                string blockedPreview = string.Join(", ", blocked.Take(3).Select(GetDeleteSummary));
                if (blocked.Count > 3)
                {
                    blockedPreview += $" ve {blocked.Count - 3} kayıt daha";
                }

                ToastHelper.Show($"{blocked.Count} kayıt hareket gördüğü için silinemez: {blockedPreview}", ToastType.Warning, 6000);
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