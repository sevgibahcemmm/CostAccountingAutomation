using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cost.Accounting.Automation.Application.DirectProcurement;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.DirectProcurement;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.DirectProcurementForms
{
    /// <summary>
    /// Kaynak sistemdeki <c>TfrmMain</c> ("AMP Doğrudan Temin") penceresinin
    /// karşılığı: üstteki <c>dxBarManager1Bar2</c> ("Ihale") arac çubuğu,
    /// soldaki dört <c>TRzGroup</c> grubu ve ortadaki büyük AMP logosu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Form, entegre uygulamanın ana kabuğunun bir <b>MDI çocuğu</b> olarak
    /// açılır (bkz. <see cref="MdiFormManager"/>); üst menü çubuğu
    /// (<c>dxBarManager1Bar1</c>) burada yoktur, ribbon ile temsil edilir.
    /// </para>
    /// <para>
    /// Sol gruplar kaynaktaki gibi çalışır: temin türü simgesi + açıklama,
    /// seçili dosyanın işin adı, işin bilgileri ve incelenen son 5 dosya.
    /// Kaynakta bu alanlar <c>dmData.DSQIhale</c> master sorgusuna bağlıdır;
    /// burada aynı bilgi en son güncellenen dosyadan beslenir.
    /// </para>
    /// </remarks>
    public sealed partial class DirectProcurementHomeForm : XtraFormMdiBase
    {
        /// <summary>Kaynak grupta tam olarak beş kayıt gösterilir.</summary>
        private const int RecentFileLimit = 5;

        private static readonly Dictionary<ProcurementCategoryType, string> TeminTypeImages = new()
        {
            [ProcurementCategoryType.Construction] = "temin-nav-yapim-48.png",
            [ProcurementCategoryType.Service] = "temin-nav-hizmet-48.png",
            [ProcurementCategoryType.Goods] = "temin-nav-mal-48.png",
            [ProcurementCategoryType.Consultancy] = "temin-nav-danismanlik-48.png"
        };

        private static readonly Dictionary<ProcurementCategoryType, string> TeminTypeDescriptions = new()
        {
            [ProcurementCategoryType.Construction] = "Yapım İşi",
            [ProcurementCategoryType.Service] = "Hizmet",
            [ProcurementCategoryType.Goods] = "Mal Alımı",
            [ProcurementCategoryType.Consultancy] = "Danışmanlık"
        };

        private List<DirectProcurementFileDto> _recentFiles = [];

        public DirectProcurementHomeForm() : base("Doğrudan Temin")
        {
            InitializeComponent();

            LoadImages();
            WireToolbar();

            viewRecent.DoubleClick += (_, _) => OpenSelectedRecentFile();
            lblWorkName.Click += (_, _) => OpenFiles();

            Load += HomeForm_Load;
        }

        /// <summary>
        /// Kaynak formdaki görselleri gömülü kaynaklardan yükler.
        /// </summary>
        private void LoadImages()
        {
            AssignButtonImage(btnNew, "tb_new.png");
            AssignButtonImage(btnList, "tb_list.png");
            AssignButtonImage(btnApproxCost, "tb_approx_cost.png");
            AssignButtonImage(btnApproval, "tb_approval.png");
            AssignButtonImage(btnOfferDecision, "tb_offer_decision.png");
            AssignButtonImage(btnOrderAccept, "tb_order_accept.png");
            AssignButtonImage(btnContract, "tb_contract.png");
            AssignButtonImage(btnStatistics, "tb_statistics.png");
            AssignButtonImage(btnExtraContract, "tb_extra_contract.png");
            AssignButtonImage(btnExit, "tb_exit.png");

            picLogo.Image = EmbeddedImageLoader.Load("amp-logo.png");
            picBrand.Image = EmbeddedImageLoader.Load("amp-brand.png");

            lblStatusUser.Text = $"Kullanıcı: {Environment.UserName}";
        }

        private static void AssignButtonImage(SimpleButton button, string fileName)
            => button.ImageOptions.Image = EmbeddedImageLoader.Load(fileName);

        /// <summary>
        /// Kaynak <c>ActionListIhale</c> aksiyonlarını taklit eder.
        /// </summary>
        private void WireToolbar()
        {
            btnNew.Click += (_, _) => OpenNewFile();
            btnList.Click += (_, _) => OpenFiles();

            // Süreç adımı ekranları ikinci fazda ayrı pencere olarak eklenecek;
            // kaynakta da her adım dosya seçildikten sonra açılır.
            btnApproxCost.Click += (_, _) => ShowPhaseTwoInfo("Yaklaşık Maliyet");
            btnApproval.Click += (_, _) => ShowPhaseTwoInfo("Onay Belgesi");
            btnOfferDecision.Click += (_, _) => ShowPhaseTwoInfo("Tekliflerin Alınması ve Karar");
            btnOrderAccept.Click += (_, _) => ShowPhaseTwoInfo("Sipariş ve Kabul");
            btnContract.Click += (_, _) => ShowPhaseTwoInfo("Sözleşme Tasarısı");
            btnExtraContract.Click += (_, _) => ShowPhaseTwoInfo("Ek Sözleşme Uygulamaları");
            btnStatistics.Click += (_, _) => ShowPhaseTwoInfo("İstatistik");

            btnExit.Click += (_, _) => Close();
        }

        private static void ShowPhaseTwoInfo(string step)
            => ToastHelper.Show($"{step} ekranı ikinci fazda eklenecek.", ToastType.Info, 3000);

        private async void HomeForm_Load(object? sender, EventArgs e)
        {
            lblStatusInfo.Text = "Yükleniyor...";

            await LoadRecentFilesAsync();
        }

        /// <summary>
        /// Kaynak <c>RzGroupSon5Dosya</c> grubunu doldurur ve seçili dosyayı
        /// günceller.
        /// </summary>
        private async Task LoadRecentFilesAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

                IQueryable<DirectProcurementFileDto> files = await sender.Send(
                    new DirectProcurementFileGetAllQuery(),
                    CancellationToken.None);

                _recentFiles = files
                    .OrderByDescending(f => f.UpdatedAt ?? f.CreatedAt)
                    .Take(RecentFileLimit)
                    .ToList();

                gridRecent.DataSource = _recentFiles;
                lblStatusInfo.Text = $"{_recentFiles.Count} dosya";

                UpdateSelection(_recentFiles.FirstOrDefault());
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("DirectProcurementHome.Recent", ex);
                lblStatusInfo.Text = "Dosya listesi alınamadı";
                UpdateSelection(null);
            }
        }

        /// <summary>
        /// Kaynaktaki <c>dmData.DSQIhale</c> master kaydının ekrandaki karşılığı:
        /// sol gruplardaki temin türü, işin adı ve işin bilgileri.
        /// </summary>
        private void UpdateSelection(DirectProcurementFileDto? file)
        {
            picTeminType.Image = file == null
                ? null
                : EmbeddedImageLoader.Load(TeminTypeImages[file.Category]);

            lblTeminTypeDesc.Text = file == null
                ? "Dosya seçiniz"
                : TeminTypeDescriptions[file.Category];

            lblWorkName.Text = file?.WorkName ?? "Dosya bulunamadı";
            lblWorkInfo.Text = BuildWorkInfo(file);
            lblStatusCompany.Text = file?.CompanyName ?? string.Empty;
        }

        private static string BuildWorkInfo(DirectProcurementFileDto? file)
        {
            if (file == null)
            {
                return "Detaylı bilgi için soldaki listeden bir dosya açınız.";
            }

            return
                $"Dosya No: {file.FileNumber}\r\n" +
                $"Yıl: {file.WorkingYear}\r\n" +
                $"Birim: {file.CompanyName}\r\n" +
                $"İş Türü: {file.CategoryName}\r\n" +
                $"Temin Şekli: {file.FlowName}\r\n" +
                $"Aşama: {file.StageName}\r\n" +
                $"Firma: {file.SupplierName}\r\n" +
                $"Yaklaşık Maliyet: {file.ApproximateCost:N2}";
        }

        private void OpenNewFile()
        {
            using var scope = Program.Services.CreateScope();
            IMasterCompanyNameLookup? lookup = scope.ServiceProvider.GetService<IMasterCompanyNameLookup>();

            using var form = new DirectProcurementFileEditForm(null, lookup);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _ = LoadRecentFilesAsync();
            }
        }

        /// <summary>
        /// Dosya listesini açar.
        /// </summary>
        /// <remarks>
        /// Başlık, liste formunun kendi <c>Text</c> değeriyle birebir aynı olmalıdır:
        /// <see cref="MdiFormManager.OpenForm{TForm}"/> açık sekmeyi
        /// <c>form.Text == formTitle</c> karşılaştırmasıyla bulur.
        /// </remarks>
        private void OpenFiles() => Open<DirectProcurementFilesListForm>(ListFormTitle);

        /// <summary>
        /// Liste formunun başlığı. Ribbon ve ana pencere aynı sabiti kullanır.
        /// </summary>
        private const string ListFormTitle = "Doğrudan Temin Dosyaları";

        /// <summary>
        /// "İncelenen Son 5 Dosya" tablosunda seçili dosyayı düzenleme
        /// penceresinde açar (kaynak: <c>GRsonGezdiklerimDBBandedTableView1CellDblClick</c>).
        /// </summary>
        private void OpenSelectedRecentFile()
        {
            if (viewRecent.GetFocusedRow() is not DirectProcurementFileDto file)
            {
                return;
            }

            UpdateSelection(file);

            using var scope = Program.Services.CreateScope();
            IMasterCompanyNameLookup? lookup = scope.ServiceProvider.GetService<IMasterCompanyNameLookup>();

            using var form = new DirectProcurementFileEditForm(file, lookup);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _ = LoadRecentFilesAsync();
            }
        }

        private void Open<TForm>(string title) where TForm : XtraForm
        {
            // MdiParent tipi Form olarak bildirilmiştir; MdiFormManager bir
            // XtraForm bekler, bu yüzden daraltma burada yapılır.
            if (MdiParent is not XtraForm parent)
            {
                return;
            }

            MdiFormManager.Instance.OpenForm<TForm>(parent, title);
        }
    }
}
