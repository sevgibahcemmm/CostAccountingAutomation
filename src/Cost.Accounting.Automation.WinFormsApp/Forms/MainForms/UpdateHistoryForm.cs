using Cost.Accounting.Automation.Application.Updates;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraGrid.Columns;
using Microsoft.Extensions.DependencyInjection;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    /// <summary>
    /// Yayınlanmış sürümlerin geçmişini listeler. Veri, merkezi master
    /// veritabanındaki <c>AppReleases</c> tablosundan okunur (kurulum dosyası
    /// içeriği dahil edilmez); en yeni sürüm en üstte gösterilir.
    /// </summary>
    public sealed partial class UpdateHistoryForm : XtraFormMdiBase
    {
        public UpdateHistoryForm() : base("Sürüm Geçmişi")
        {
            InitializeComponent();
            IconOptions.SvgImage = DxIcon.Restore;
            picIcon.SvgImage = DxIcon.Restore;
            ConfigureColumns();
        }

        private void ConfigureColumns()
        {
            gridView.Columns.Clear();

            GridColumn colVersion = new()
            {
                Caption = "Sürüm",
                FieldName = nameof(AppReleaseHistoryInfo.Version),
                Visible = true,
                Width = 110
            };
            colVersion.OptionsColumn.FixedWidth = true;

            GridColumn colIsPublished = new()
            {
                Caption = "Yayın",
                FieldName = nameof(AppReleaseHistoryInfo.IsPublished),
                Visible = true,
                Width = 70,
                UnboundType = DevExpress.Data.UnboundColumnType.Boolean
            };
            colIsPublished.OptionsColumn.FixedWidth = true;

            GridColumn colIsMandatory = new()
            {
                Caption = "Zorunlu",
                FieldName = nameof(AppReleaseHistoryInfo.IsMandatory),
                Visible = true,
                Width = 70,
                UnboundType = DevExpress.Data.UnboundColumnType.Boolean
            };
            colIsMandatory.OptionsColumn.FixedWidth = true;

            GridColumn colPublishedAt = new()
            {
                Caption = "Yayın Tarihi",
                FieldName = nameof(AppReleaseHistoryInfo.PublishedAt),
                Visible = true,
                Width = 120
            };
            colPublishedAt.DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
            colPublishedAt.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;

            GridColumn colFileName = new()
            {
                Caption = "Dosya",
                FieldName = nameof(AppReleaseHistoryInfo.FileName),
                Visible = true,
                Width = 260
            };

            GridColumn colFileSize = new()
            {
                Caption = "Boyut",
                FieldName = nameof(AppReleaseHistoryInfo.FileSizeBytes),
                Visible = true,
                Width = 100
            };
            colFileSize.DisplayFormat.FormatString = "{0:N0}";
            colFileSize.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;

            GridColumn colNotes = new()
            {
                Caption = "Sürüm Notları",
                FieldName = nameof(AppReleaseHistoryInfo.Notes),
                Visible = true,
                Width = 420
            };

            gridView.Columns.AddRange(new GridColumn[]
            {
                colVersion, colIsPublished, colIsMandatory, colPublishedAt, colFileName, colFileSize, colNotes
            });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ = LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                gridControl.DataSource = null;

                using IServiceScope scope = Program.Services.CreateScope();
                IAppReleaseService service = scope.ServiceProvider.GetRequiredService<IAppReleaseService>();

                IReadOnlyList<AppReleaseHistoryInfo> history = await service.GetHistoryAsync();

                gridControl.DataSource = history;
                lblSub.Text = history.Count > 0
                    ? $"{history.Count} sürüm kaydı bulunuyor"
                    : "Henüz yayınlanmış sürüm bulunmuyor";
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("UpdateHistoryForm.LoadHistory", ex);
                lblSub.Text = "Sürüm geçmişi okunamadı (veritabanına ulaşılamadı)";
                ToastHelper.Show("Sürüm geçmişi okunamadı.", ToastType.Error);
            }
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e)
        {
            await LoadHistoryAsync();
        }
    }
}