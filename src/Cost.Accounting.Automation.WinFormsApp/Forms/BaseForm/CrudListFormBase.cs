using System.Reflection;
using System.Text;
using System.Drawing;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using DomainEntityDto = Cost.Accounting.Automation.Domain.Abstractions.EntityDto;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
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
        private readonly RepositoryItemCheckEdit _riActive = new();
        private readonly RepositoryItemTextEdit _riBoolText = new();
        private readonly Dictionary<string, ColumnAttribute> _boolTextColumns = new();
        private readonly Dictionary<string, string> _stringFormatColumns = new();

        protected List<TDto> _allItems = [];

        private bool _showDeleted;

        protected CrudListFormBase(string formTitle) : base(formTitle)
        {
            InitializeComponent();
            lblTitle.Text = formTitle;
            pnlHeader.Paint += PnlHeader_Paint;
            IconOptions.SvgImage = ModuleIcon;
            WireEvents();
            SetupGrid();
            SetupButtonIcons();
            AddHeaderIcon();
        }

        protected GridView View => gridView;

        protected DevExpress.XtraGrid.GridControl BaseGrid => gridControl;

        protected System.Windows.Forms.Panel HeaderPanel => pnlHeader;

        protected DevExpress.XtraEditors.PanelControl ToolbarPanel => pnlToolbar;

        protected virtual SvgImage ModuleIcon => SvgIcons.Modules[5];

        protected virtual bool AllowDelete => true;

        protected bool ShowDeleted => _showDeleted;

        protected virtual bool SupportsRestore => false;

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
                    await RunEditorAsync(dto);
                }
            };
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += async (_, _) => await ReloadAsync();
            btnDeleted.CheckedChanged += BtnDeleted_CheckedChanged;
            btnRestore.Click += BtnRestore_Click;
            txtSearch.EditValueChanged += TxtSearch_EditValueChanged;
            gridView.DoubleClick += GridView_DoubleClick;
            gridView.SelectionChanged += GridView_SelectionChanged;
        }

        private void SetupGrid()
        {
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.OptionsSelection.MultiSelect = true;
            gridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
            gridView.OptionsBehavior.Editable = false;
            gridView.OptionsView.EnableAppearanceEvenRow = false;
            gridView.OptionsView.EnableAppearanceOddRow = false;
            _riActive.ReadOnly = true;
            _riBoolText.ReadOnly = true;
            gridControl.RepositoryItems.Add(_riActive);
            gridControl.RepositoryItems.Add(_riBoolText);
            gridView.CustomColumnDisplayText += GridView_CustomColumnDisplayText;
        }

        private void SetupButtonIcons()
        {
            SetButtonIcon(btnNew, SvgIcons.PlusIcon, 18);
            SetButtonIcon(btnEdit, SvgIcons.EditIcon, 18);
            SetButtonIcon(btnDelete, SvgIcons.TrashIcon, 18);
            SetButtonIcon(btnRefresh, SvgIcons.RefreshIcon, 18);
            SetButtonIcon(btnDeleted, SvgIcons.TrashIcon, 18);
            SetButtonIcon(btnRestore, SvgIcons.RestoreIcon, 18);
            SetButtonIcon(btnClosePage, SvgIcons.CloseIcon, 16);
        }

        private static void SetButtonIcon(SimpleButton button, SvgImage icon, int size)
        {
            button.ImageOptions.SvgImage = icon;
            button.ImageOptions.SvgImageSize = new Size(size, size);
            button.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
        }

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
                Location = new Point(28, 24),
                Size = new Size(42, 42),
                BackColor = Color.Transparent
            };
            pic.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            pic.Properties.SvgImageColorizationMode = DevExpress.Utils.SvgImageColorizationMode.None;
            pic.SvgImage = ModuleIcon;
            pic.Properties.Appearance.BackColor = Color.Transparent;
            pic.Properties.Appearance.Options.UseBackColor = true;
            pnlHeader.Controls.Add(pic);
            lblTitle.Location = new Point(82, 20);
            lblSub.Location = new Point(84, 62);
        }

        protected GridColumn CreateBooleanColumn(string caption, string fieldName)
            => new()
            {
                Caption = caption,
                FieldName = fieldName,
                Visible = true,
                ColumnEdit = _riActive
            };

        protected void AddColumnsFromAttributes()
        {
            PropertyInfo[] properties = typeof(TDto).GetProperties();
            List<(GridColumn Column, int Order)> columns = new();

            foreach (PropertyInfo property in properties)
            {
                ColumnAttribute? attr = property.GetCustomAttribute<ColumnAttribute>();
                if (attr is null || !attr.IsVisible)
                {
                    continue;
                }

                GridColumn column = new()
                {
                    Caption = attr.Title,
                    FieldName = property.Name,
                    Visible = true,
                    Width = attr.Width
                };

                ApplyColumnFormat(column, property, attr);
                columns.Add((column, attr.Order));
            }

            foreach ((GridColumn column, int order) in columns.OrderBy(c => c.Order))
            {
                View.Columns.Add(column);
            }
        }

        private void ApplyColumnFormat(GridColumn column, PropertyInfo property, ColumnAttribute attr)
        {
            switch (attr.Alignment?.ToLowerInvariant())
            {
                case "center":
                    column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                    column.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                    break;
                case "right":
                    column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                    column.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                    break;
            }

            if (property.PropertyType == typeof(bool))
            {
                if (attr.TrueText is null && attr.FalseText is null)
                {
                    column.ColumnEdit = _riActive;
                }
                else
                {
                    column.ColumnEdit = _riBoolText;
                    _boolTextColumns[property.Name] = attr;
                }
                return;
            }

            if (property.PropertyType == typeof(string))
            {
                if (!string.IsNullOrWhiteSpace(attr.Format) && attr.Format != "G")
                {
                    _stringFormatColumns[property.Name] = attr.Format;
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(attr.Format) && attr.Format != "G")
            {
                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
                column.DisplayFormat.FormatString = attr.Format;
            }
        }

        private void GridView_CustomColumnDisplayText(object? sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column is null)
            {
                return;
            }

            if (_boolTextColumns.TryGetValue(e.Column.FieldName, out ColumnAttribute? attr))
            {
                e.DisplayText = e.Value is bool b && b ? attr.TrueText : attr.FalseText;
                return;
            }

            if (_stringFormatColumns.TryGetValue(e.Column.FieldName, out string? format)
                && e.Value is string raw)
            {
                e.DisplayText = ApplyStringFormat(raw, format);
            }
        }

        private static string ApplyStringFormat(string value, string format)
        {
            StringBuilder sb = new(value.Length + 4);
            int valueIndex = 0;

            foreach (char c in format)
            {
                if (c == '#')
                {
                    if (valueIndex < value.Length)
                    {
                        sb.Append(value[valueIndex++]);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private void GridView_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            int selected = gridView.GetSelectedRows().Length;
            bool showDeleted = _showDeleted;

            btnNew.Enabled = !showDeleted;
            btnEdit.Enabled = !showDeleted && selected == 1;
            btnDelete.Enabled = !showDeleted && selected >= 1;
            btnDelete.Visible = AllowDelete && !showDeleted;
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
                gridControl.DataSource = null;
                List<TDto> items = (await mediator.Send(query, CancellationToken.None)).ToList();
                _allItems = items;
                gridControl.DataSource = items;
                gridView.BestFitColumns();
                lblSub.Text = GetSubtitle(items.Count);
            }
            catch (AuthorizationException ex)
            {
                ToastHelper.Show(ex.Message, ToastType.Warning, 4000);
                lblSub.Text = "Yetkiniz yok";
            }
            catch (Exception ex)
            {
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