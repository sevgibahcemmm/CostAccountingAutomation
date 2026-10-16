using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraNavBar;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.DirectProcurementForms
{
    /// <summary>
    /// Kaynak sistemdeki <c>TfrmMain</c> ("AMP Doğrudan Temin" ana penceresi)
    /// penceresinin istemci alanını birebir taşır.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kaynak DFM karşılıkları: üstteki <c>dxBarManager1Bar2</c> ("Ihale") arac
    /// cubugu → <c>pnlToolbar</c> icindeki buyuk <c>SimpleButton</c>'lar; soldaki
    /// <c>RzGroupBar1</c> → <c>navGroups</c> (Temin Turu, Isin Adi, Isin Bilgileri,
    /// Incelenen Son 5 Dosya); orta <c>PNOrta</c> → <c>pnlMain</c> (buyuk Logo +
    /// marka gorseli); alt <c>SB</c> (TdxStatusBar) → <c>pnlStatus</c>.
    /// </para>
    /// <para>
    /// Ust menu cubugu (<c>dxBarManager1Bar1</c>) bu formda yoktur; entegre
    /// uygulamanin ana kabugundaki ribbon ile temsil edilir.
    /// </para>
    /// </remarks>
    sealed partial class DirectProcurementHomeForm
    {
        private PanelControl pnlToolbar = null!;
        private FlowLayoutPanel flpToolbar = null!;
        private SimpleButton btnNew = null!;
        private SimpleButton btnList = null!;
        private PanelControl sep1 = null!;
        private SimpleButton btnApproxCost = null!;
        private PanelControl sep2 = null!;
        private SimpleButton btnApproval = null!;
        private PanelControl sep3 = null!;
        private SimpleButton btnOfferDecision = null!;
        private PanelControl sep4 = null!;
        private SimpleButton btnOrderAccept = null!;
        private PanelControl sep5 = null!;
        private SimpleButton btnContract = null!;
        private SimpleButton btnStatistics = null!;
        private PanelControl sep6 = null!;
        private SimpleButton btnExtraContract = null!;
        private SimpleButton btnExit = null!;

        private NavBarControl navGroups = null!;
        private NavBarGroup grpTeminTuru = null!;
        private NavBarGroupControlContainer ccTeminTuru = null!;
        private PictureEdit picTeminType = null!;
        private LabelControl lblTeminTypeDesc = null!;
        private NavBarGroup grpIsinAdi = null!;
        private NavBarGroupControlContainer ccIsinAdi = null!;
        private LabelControl lblWorkName = null!;
        private NavBarGroup grpIsinBilgileri = null!;
        private NavBarGroupControlContainer ccIsinBilgileri = null!;
        private LabelControl lblWorkInfo = null!;
        private NavBarGroup grpSon5Dosya = null!;
        private NavBarGroupControlContainer ccSon5Dosya = null!;
        private GridControl gridRecent = null!;
        private GridView viewRecent = null!;
        private GridColumn colRecentWorkName = null!;

        private PanelControl pnlMain = null!;
        private PictureEdit picLogo = null!;
        private PictureEdit picBrand = null!;

        private PanelControl pnlStatus = null!;
        private LabelControl lblStatusKeyboard = null!;
        private LabelControl lblStatusUser = null!;
        private LabelControl lblStatusCompany = null!;
        private LabelControl lblStatusInfo = null!;

        private void InitializeComponent()
        {
            this.pnlToolbar = new DevExpress.XtraEditors.PanelControl();
            this.flpToolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.btnNew = new DevExpress.XtraEditors.SimpleButton();
            this.btnList = new DevExpress.XtraEditors.SimpleButton();
            this.sep1 = new DevExpress.XtraEditors.PanelControl();
            this.btnApproxCost = new DevExpress.XtraEditors.SimpleButton();
            this.sep2 = new DevExpress.XtraEditors.PanelControl();
            this.btnApproval = new DevExpress.XtraEditors.SimpleButton();
            this.sep3 = new DevExpress.XtraEditors.PanelControl();
            this.btnOfferDecision = new DevExpress.XtraEditors.SimpleButton();
            this.sep4 = new DevExpress.XtraEditors.PanelControl();
            this.btnOrderAccept = new DevExpress.XtraEditors.SimpleButton();
            this.sep5 = new DevExpress.XtraEditors.PanelControl();
            this.btnContract = new DevExpress.XtraEditors.SimpleButton();
            this.btnStatistics = new DevExpress.XtraEditors.SimpleButton();
            this.sep6 = new DevExpress.XtraEditors.PanelControl();
            this.btnExtraContract = new DevExpress.XtraEditors.SimpleButton();
            this.btnExit = new DevExpress.XtraEditors.SimpleButton();
            this.navGroups = new DevExpress.XtraNavBar.NavBarControl();
            this.grpTeminTuru = new DevExpress.XtraNavBar.NavBarGroup();
            this.ccTeminTuru = new DevExpress.XtraNavBar.NavBarGroupControlContainer();
            this.picTeminType = new DevExpress.XtraEditors.PictureEdit();
            this.lblTeminTypeDesc = new DevExpress.XtraEditors.LabelControl();
            this.grpIsinAdi = new DevExpress.XtraNavBar.NavBarGroup();
            this.ccIsinAdi = new DevExpress.XtraNavBar.NavBarGroupControlContainer();
            this.lblWorkName = new DevExpress.XtraEditors.LabelControl();
            this.grpIsinBilgileri = new DevExpress.XtraNavBar.NavBarGroup();
            this.ccIsinBilgileri = new DevExpress.XtraNavBar.NavBarGroupControlContainer();
            this.lblWorkInfo = new DevExpress.XtraEditors.LabelControl();
            this.grpSon5Dosya = new DevExpress.XtraNavBar.NavBarGroup();
            this.ccSon5Dosya = new DevExpress.XtraNavBar.NavBarGroupControlContainer();
            this.gridRecent = new DevExpress.XtraGrid.GridControl();
            this.viewRecent = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colRecentWorkName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.pnlMain = new DevExpress.XtraEditors.PanelControl();
            this.picLogo = new DevExpress.XtraEditors.PictureEdit();
            this.picBrand = new DevExpress.XtraEditors.PictureEdit();
            this.pnlStatus = new DevExpress.XtraEditors.PanelControl();
            this.lblStatusKeyboard = new DevExpress.XtraEditors.LabelControl();
            this.lblStatusUser = new DevExpress.XtraEditors.LabelControl();
            this.lblStatusCompany = new DevExpress.XtraEditors.LabelControl();
            this.lblStatusInfo = new DevExpress.XtraEditors.LabelControl();
            this.pnlToolbar.SuspendLayout();
            this.flpToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.pnlToolbar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.sep1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.sep2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.sep3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.sep4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.sep5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.sep6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.navGroups).BeginInit();
            this.ccTeminTuru.SuspendLayout();
            this.ccIsinAdi.SuspendLayout();
            this.ccIsinBilgileri.SuspendLayout();
            this.ccSon5Dosya.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.picTeminType.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.gridRecent).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.viewRecent).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.pnlMain).BeginInit();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.picLogo.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.picBrand.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.pnlStatus).BeginInit();
            this.pnlStatus.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlToolbar  (kaynak: dxBarManager1Bar2 "Ihale" arac cubugu)
            //
            this.pnlToolbar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlToolbar.Controls.Add(this.flpToolbar);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 0);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1200, 68);
            this.pnlToolbar.TabIndex = 0;
            //
            // flpToolbar
            //
            this.flpToolbar.AutoScroll = true;
            this.flpToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpToolbar.Location = new System.Drawing.Point(0, 0);
            this.flpToolbar.Name = "flpToolbar";
            this.flpToolbar.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.flpToolbar.Size = new System.Drawing.Size(1200, 68);
            this.flpToolbar.TabIndex = 0;
            this.flpToolbar.WrapContents = false;
            //
            // btnNew  (kaynak: dxBarLargeButton2 "Yeni Temin")
            //
            this.btnNew.Location = new System.Drawing.Point(4, 4);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(80, 68);
            this.btnNew.TabIndex = 0;
            this.btnNew.Text = "Yeni Temin";
            StyleToolbarButton(this.btnNew);
            //
            // btnList  (kaynak: dxBarLargeButton3 "Doğrudan Temin Listesi")
            //
            this.btnList.Location = new System.Drawing.Point(88, 4);
            this.btnList.Name = "btnList";
            this.btnList.Size = new System.Drawing.Size(116, 68);
            this.btnList.TabIndex = 1;
            this.btnList.Text = "Doğrudan Temin Listesi";
            StyleToolbarButton(this.btnList);
            //
            // sep1
            //
            StyleSeparator(this.sep1, "sep1", 2);
            //
            // btnApproxCost  (kaynak: dxBarLargeButtonYaklasikMaliyet "Yaklaşık Maliyet")
            //
            this.btnApproxCost.Location = new System.Drawing.Point(210, 4);
            this.btnApproxCost.Name = "btnApproxCost";
            this.btnApproxCost.Size = new System.Drawing.Size(96, 68);
            this.btnApproxCost.TabIndex = 2;
            this.btnApproxCost.Text = "Yaklaşık Maliyet";
            StyleToolbarButton(this.btnApproxCost);
            //
            // sep2
            //
            StyleSeparator(this.sep2, "sep2", 2);
            //
            // btnApproval  (kaynak: dxBarLargeButtonOnayBelgesi "Onay Belgesi")
            //
            this.btnApproval.Location = new System.Drawing.Point(312, 4);
            this.btnApproval.Name = "btnApproval";
            this.btnApproval.Size = new System.Drawing.Size(88, 68);
            this.btnApproval.TabIndex = 3;
            this.btnApproval.Text = "Onay Belgesi";
            StyleToolbarButton(this.btnApproval);
            //
            // sep3
            //
            StyleSeparator(this.sep3, "sep3", 2);
            //
            // btnOfferDecision  (kaynak: dxBarLargeButtonIhtiyacListesi "Tekliflerin Alınması ve Karar")
            //
            this.btnOfferDecision.Location = new System.Drawing.Point(406, 4);
            this.btnOfferDecision.Name = "btnOfferDecision";
            this.btnOfferDecision.Size = new System.Drawing.Size(150, 68);
            this.btnOfferDecision.TabIndex = 4;
            this.btnOfferDecision.Text = "Tekliflerin Alınması ve Karar";
            StyleToolbarButton(this.btnOfferDecision);
            //
            // sep4
            //
            StyleSeparator(this.sep4, "sep4", 2);
            //
            // btnOrderAccept  (kaynak: dxBarLargeButtonKabulAsamasi "Sipariş ve Kabul")
            //
            this.btnOrderAccept.Location = new System.Drawing.Point(562, 4);
            this.btnOrderAccept.Name = "btnOrderAccept";
            this.btnOrderAccept.Size = new System.Drawing.Size(104, 68);
            this.btnOrderAccept.TabIndex = 5;
            this.btnOrderAccept.Text = "Sipariş ve Kabul";
            StyleToolbarButton(this.btnOrderAccept);
            //
            // sep5
            //
            StyleSeparator(this.sep5, "sep5", 2);
            //
            // btnContract  (kaynak: dxBarLargeButtonSozlesmeTasarisi "Sözleşme Tasarısı")
            //
            this.btnContract.Location = new System.Drawing.Point(672, 4);
            this.btnContract.Name = "btnContract";
            this.btnContract.Size = new System.Drawing.Size(110, 68);
            this.btnContract.TabIndex = 6;
            this.btnContract.Text = "Sözleşme Tasarısı";
            StyleToolbarButton(this.btnContract);
            //
            // btnStatistics  (kaynak: dxBarLargeButton1 "  İstatistik  ")
            //
            this.btnStatistics.Location = new System.Drawing.Point(786, 4);
            this.btnStatistics.Name = "btnStatistics";
            this.btnStatistics.Size = new System.Drawing.Size(80, 68);
            this.btnStatistics.TabIndex = 7;
            this.btnStatistics.Text = "İstatistik";
            StyleToolbarButton(this.btnStatistics);
            //
            // sep6
            //
            StyleSeparator(this.sep6, "sep6", 2);
            //
            // btnExtraContract  (kaynak: dxBarLargeButton5 "Ek Söz. Uygulamaları")
            //
            this.btnExtraContract.Location = new System.Drawing.Point(872, 4);
            this.btnExtraContract.Name = "btnExtraContract";
            this.btnExtraContract.Size = new System.Drawing.Size(128, 68);
            this.btnExtraContract.TabIndex = 8;
            this.btnExtraContract.Text = "Ek Söz. Uygulamaları";
            StyleToolbarButton(this.btnExtraContract);
            //
            // btnExit  (kaynak: dxBarLargeButtonCikis "Çıkış")
            //
            this.btnExit.Location = new System.Drawing.Point(1004, 4);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(80, 68);
            this.btnExit.TabIndex = 9;
            this.btnExit.Text = "Çıkış";
            StyleToolbarButton(this.btnExit);
            //
            // flpToolbar icerigi
            //
            this.flpToolbar.Controls.Add(this.btnNew);
            this.flpToolbar.Controls.Add(this.btnList);
            this.flpToolbar.Controls.Add(this.sep1);
            this.flpToolbar.Controls.Add(this.btnApproxCost);
            this.flpToolbar.Controls.Add(this.sep2);
            this.flpToolbar.Controls.Add(this.btnApproval);
            this.flpToolbar.Controls.Add(this.sep3);
            this.flpToolbar.Controls.Add(this.btnOfferDecision);
            this.flpToolbar.Controls.Add(this.sep4);
            this.flpToolbar.Controls.Add(this.btnOrderAccept);
            this.flpToolbar.Controls.Add(this.sep5);
            this.flpToolbar.Controls.Add(this.btnContract);
            this.flpToolbar.Controls.Add(this.btnStatistics);
            this.flpToolbar.Controls.Add(this.sep6);
            this.flpToolbar.Controls.Add(this.btnExtraContract);
            this.flpToolbar.Controls.Add(this.btnExit);
            //
            // navGroups  (kaynak: RzGroupBar1)
            //
            this.navGroups.Controls.Add(this.ccTeminTuru);
            this.navGroups.Controls.Add(this.ccIsinAdi);
            this.navGroups.Controls.Add(this.ccIsinBilgileri);
            this.navGroups.Controls.Add(this.ccSon5Dosya);
            this.navGroups.Dock = System.Windows.Forms.DockStyle.Left;
            this.navGroups.Location = new System.Drawing.Point(0, 68);
            this.navGroups.Name = "navGroups";
            this.navGroups.Size = new System.Drawing.Size(302, 588);
            this.navGroups.TabIndex = 1;
            this.navGroups.Text = "navGroups";
            //
            // ccTeminTuru  (kaynak: bsSkinPanel2 280x65)
            //
            this.ccTeminTuru.Controls.Add(this.picTeminType);
            this.ccTeminTuru.Controls.Add(this.lblTeminTypeDesc);
            this.ccTeminTuru.Name = "ccTeminTuru";
            this.ccTeminTuru.Size = new System.Drawing.Size(280, 65);
            this.ccTeminTuru.TabIndex = 0;
            //
            // picTeminType  (kaynak: ImYapim/ImHizmet/ImMalAlimi/ImDanismanlik 39x39 @16,12)
            //
            this.picTeminType.Location = new System.Drawing.Point(16, 12);
            this.picTeminType.Name = "picTeminType";
            this.picTeminType.Properties.NullText = "";
            this.picTeminType.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;
            this.picTeminType.Size = new System.Drawing.Size(39, 39);
            this.picTeminType.TabIndex = 0;
            //
            // lblTeminTypeDesc  (kaynak: LbTeminTuru 62,13 206x32, -17 bold, tlCenter)
            //
            this.lblTeminTypeDesc.Appearance.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Bold);
            this.lblTeminTypeDesc.Appearance.ForeColor = System.Drawing.Color.FromArgb(63, 86, 120);
            this.lblTeminTypeDesc.Appearance.Options.UseFont = true;
            this.lblTeminTypeDesc.Appearance.Options.UseForeColor = true;
            this.lblTeminTypeDesc.Appearance.Options.UseTextOptions = true;
            this.lblTeminTypeDesc.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblTeminTypeDesc.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblTeminTypeDesc.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTeminTypeDesc.Location = new System.Drawing.Point(62, 13);
            this.lblTeminTypeDesc.Name = "lblTeminTypeDesc";
            this.lblTeminTypeDesc.Size = new System.Drawing.Size(206, 32);
            this.lblTeminTypeDesc.TabIndex = 1;
            //
            // grpTeminTuru
            //
            this.grpTeminTuru.Caption = "Temin Türü";
            this.grpTeminTuru.ControlContainer = this.ccTeminTuru;
            this.grpTeminTuru.Expanded = true;
            this.grpTeminTuru.GroupClientHeight = 65;
            this.grpTeminTuru.GroupStyle = DevExpress.XtraNavBar.NavBarGroupStyle.ControlContainer;
            this.grpTeminTuru.Name = "grpTeminTuru";
            //
            // ccIsinAdi  (kaynak: bsSkinPanel1 280x84)
            //
            this.ccIsinAdi.Controls.Add(this.lblWorkName);
            this.ccIsinAdi.Name = "ccIsinAdi";
            this.ccIsinAdi.Size = new System.Drawing.Size(280, 84);
            this.ccIsinAdi.TabIndex = 1;
            //
            // lblWorkName  (kaynak: DBText1 11,11 258x63, ISINADI, -12 bold, wordwrap)
            //
            this.lblWorkName.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblWorkName.Appearance.ForeColor = System.Drawing.Color.FromArgb(63, 86, 120);
            this.lblWorkName.Appearance.Options.UseFont = true;
            this.lblWorkName.Appearance.Options.UseForeColor = true;
            this.lblWorkName.Appearance.Options.UseTextOptions = true;
            this.lblWorkName.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblWorkName.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblWorkName.Location = new System.Drawing.Point(11, 11);
            this.lblWorkName.Name = "lblWorkName";
            this.lblWorkName.Size = new System.Drawing.Size(258, 63);
            this.lblWorkName.TabIndex = 0;
            //
            // grpIsinAdi
            //
            this.grpIsinAdi.Caption = "İşin Adı";
            this.grpIsinAdi.ControlContainer = this.ccIsinAdi;
            this.grpIsinAdi.Expanded = true;
            this.grpIsinAdi.GroupClientHeight = 84;
            this.grpIsinAdi.GroupStyle = DevExpress.XtraNavBar.NavBarGroupStyle.ControlContainer;
            this.grpIsinAdi.Name = "grpIsinAdi";
            //
            // ccIsinBilgileri  (kaynak: bsSkinPanel3 280x151)
            //
            this.ccIsinBilgileri.Controls.Add(this.lblWorkInfo);
            this.ccIsinBilgileri.Name = "ccIsinBilgileri";
            this.ccIsinBilgileri.Size = new System.Drawing.Size(280, 151);
            this.ccIsinBilgileri.TabIndex = 2;
            //
            // lblWorkInfo  (kaynak: LBDosyaBilgileri 11,11 258x129, -12 bold, wordwrap)
            //
            this.lblWorkInfo.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblWorkInfo.Appearance.ForeColor = System.Drawing.Color.FromArgb(63, 86, 120);
            this.lblWorkInfo.Appearance.Options.UseFont = true;
            this.lblWorkInfo.Appearance.Options.UseForeColor = true;
            this.lblWorkInfo.Appearance.Options.UseTextOptions = true;
            this.lblWorkInfo.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblWorkInfo.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblWorkInfo.Location = new System.Drawing.Point(11, 11);
            this.lblWorkInfo.Name = "lblWorkInfo";
            this.lblWorkInfo.Size = new System.Drawing.Size(258, 129);
            this.lblWorkInfo.TabIndex = 0;
            //
            // grpIsinBilgileri
            //
            this.grpIsinBilgileri.Caption = "İşin Bilgileri";
            this.grpIsinBilgileri.ControlContainer = this.ccIsinBilgileri;
            this.grpIsinBilgileri.Expanded = true;
            this.grpIsinBilgileri.GroupClientHeight = 151;
            this.grpIsinBilgileri.GroupStyle = DevExpress.XtraNavBar.NavBarGroupStyle.ControlContainer;
            this.grpIsinBilgileri.Name = "grpIsinBilgileri";
            //
            // ccSon5Dosya  (kaynak: bsSkinPanel4 280x97)
            //
            this.ccSon5Dosya.Controls.Add(this.gridRecent);
            this.ccSon5Dosya.Name = "ccSon5Dosya";
            this.ccSon5Dosya.Size = new System.Drawing.Size(280, 97);
            this.ccSon5Dosya.TabIndex = 3;
            //
            // gridRecent  (kaynak: GRsonGezdiklerim 11,11 258x75)
            //
            this.gridRecent.Location = new System.Drawing.Point(11, 11);
            this.gridRecent.MainView = this.viewRecent;
            this.gridRecent.Name = "gridRecent";
            this.gridRecent.Size = new System.Drawing.Size(258, 75);
            this.gridRecent.TabIndex = 0;
            this.gridRecent.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { this.viewRecent });
            //
            // viewRecent  (kaynak: GRsonGezdiklerimDBBandedTableView1)
            //
            this.viewRecent.Appearance.Row.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.viewRecent.Appearance.Row.Options.UseFont = true;
            this.viewRecent.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { this.colRecentWorkName });
            this.viewRecent.GridControl = this.gridRecent;
            this.viewRecent.Name = "viewRecent";
            this.viewRecent.OptionsBehavior.Editable = false;
            this.viewRecent.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.viewRecent.OptionsView.ColumnAutoWidth = true;
            this.viewRecent.OptionsView.ShowColumnHeaders = false;
            this.viewRecent.OptionsView.ShowGroupPanel = false;
            this.viewRecent.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;
            this.viewRecent.OptionsView.ShowIndicator = false;
            this.viewRecent.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.False;
            //
            // colRecentWorkName  (kaynak: GRsonGezdiklerimDBBandedTableView1ISINADI1)
            //
            this.colRecentWorkName.Caption = "İşin Adı";
            this.colRecentWorkName.FieldName = "WorkName";
            this.colRecentWorkName.Name = "colRecentWorkName";
            this.colRecentWorkName.OptionsColumn.AllowEdit = false;
            this.colRecentWorkName.Visible = true;
            this.colRecentWorkName.VisibleIndex = 0;
            //
            // grpSon5Dosya
            //
            this.grpSon5Dosya.Caption = "İncelenen Son 5 Dosya";
            this.grpSon5Dosya.ControlContainer = this.ccSon5Dosya;
            this.grpSon5Dosya.Expanded = true;
            this.grpSon5Dosya.GroupClientHeight = 97;
            this.grpSon5Dosya.GroupStyle = DevExpress.XtraNavBar.NavBarGroupStyle.ControlContainer;
            this.grpSon5Dosya.Name = "grpSon5Dosya";
            //
            // navGroups gruplari
            //
            this.navGroups.Groups.AddRange(new DevExpress.XtraNavBar.NavBarGroup[] { this.grpTeminTuru, this.grpIsinAdi, this.grpIsinBilgileri, this.grpSon5Dosya });
            //
            // pnlMain  (kaynak: PNOrta, gradient 15451044 -> 16641763)
            //
            this.pnlMain.Appearance.BackColor = System.Drawing.Color.FromArgb(253, 238, 227);
            this.pnlMain.Appearance.BackColor2 = System.Drawing.Color.FromArgb(235, 195, 164);
            this.pnlMain.Appearance.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.pnlMain.Appearance.Options.UseBackColor = true;
            this.pnlMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlMain.Controls.Add(this.picBrand);
            this.pnlMain.Controls.Add(this.picLogo);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(302, 68);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(898, 588);
            this.pnlMain.TabIndex = 2;
            //
            // picLogo  (kaynak: Logo 240,187 512x119)
            //
            this.picLogo.Location = new System.Drawing.Point(240, 187);
            this.picLogo.Name = "picLogo";
            this.picLogo.Properties.NullText = "";
            this.picLogo.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;
            this.picLogo.Size = new System.Drawing.Size(512, 119);
            this.picLogo.TabIndex = 0;
            //
            // picBrand  (kaynak: Image1 138x24, sag alt ~32/28 kenar bosluk)
            //
            this.picBrand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.picBrand.Location = new System.Drawing.Point(728, 532);
            this.picBrand.Name = "picBrand";
            this.picBrand.Properties.NullText = "";
            this.picBrand.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;
            this.picBrand.Size = new System.Drawing.Size(138, 24);
            this.picBrand.TabIndex = 1;
            //
            // pnlStatus  (kaynak: SB / TdxStatusBar)
            //
            this.pnlStatus.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlStatus.Controls.Add(this.lblStatusInfo);
            this.pnlStatus.Controls.Add(this.lblStatusCompany);
            this.pnlStatus.Controls.Add(this.lblStatusUser);
            this.pnlStatus.Controls.Add(this.lblStatusKeyboard);
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatus.Location = new System.Drawing.Point(0, 656);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(1200, 24);
            this.pnlStatus.TabIndex = 3;
            //
            // lblStatusKeyboard
            //
            this.lblStatusKeyboard.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.lblStatusKeyboard.Appearance.Options.UseForeColor = true;
            this.lblStatusKeyboard.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblStatusKeyboard.Location = new System.Drawing.Point(6, 5);
            this.lblStatusKeyboard.Name = "lblStatusKeyboard";
            this.lblStatusKeyboard.Size = new System.Drawing.Size(160, 14);
            this.lblStatusKeyboard.TabIndex = 0;
            this.lblStatusKeyboard.Text = "CAPS  NUM  SCRL  INS";
            //
            // lblStatusUser
            //
            this.lblStatusUser.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.lblStatusUser.Appearance.Options.UseForeColor = true;
            this.lblStatusUser.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblStatusUser.Location = new System.Drawing.Point(180, 5);
            this.lblStatusUser.Name = "lblStatusUser";
            this.lblStatusUser.Size = new System.Drawing.Size(120, 14);
            this.lblStatusUser.TabIndex = 1;
            this.lblStatusUser.Text = "Kullanıcı";
            //
            // lblStatusCompany
            //
            this.lblStatusCompany.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.lblStatusCompany.Appearance.Options.UseForeColor = true;
            this.lblStatusCompany.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblStatusCompany.Location = new System.Drawing.Point(320, 5);
            this.lblStatusCompany.Name = "lblStatusCompany";
            this.lblStatusCompany.Size = new System.Drawing.Size(200, 14);
            this.lblStatusCompany.TabIndex = 2;
            //
            // lblStatusInfo
            //
            this.lblStatusInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatusInfo.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.lblStatusInfo.Appearance.Options.UseForeColor = true;
            this.lblStatusInfo.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblStatusInfo.Location = new System.Drawing.Point(940, 5);
            this.lblStatusInfo.Name = "lblStatusInfo";
            this.lblStatusInfo.Size = new System.Drawing.Size(250, 14);
            this.lblStatusInfo.TabIndex = 3;
            this.lblStatusInfo.Text = "Hazır";
            //
            // DirectProcurementHomeForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 680);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.navGroups);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.pnlToolbar);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Name = "DirectProcurementHomeForm";
            this.Text = "Doğrudan Temin";
            this.pnlStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.pnlStatus).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.picBrand.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.picLogo.Properties).EndInit();
            this.pnlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.pnlMain).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.viewRecent).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.gridRecent).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.picTeminType.Properties).EndInit();
            this.ccSon5Dosya.ResumeLayout(false);
            this.ccIsinBilgileri.ResumeLayout(false);
            this.ccIsinAdi.ResumeLayout(false);
            this.ccTeminTuru.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.navGroups).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.sep6).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.sep5).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.sep4).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.sep3).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.sep2).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.sep1).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.pnlToolbar).EndInit();
            this.flpToolbar.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Kaynaktaki <c>TdxBarLargeButton</c> gorunumunu taklit eder: gorsel
        /// ustte, aciklama altta.
        /// </summary>
        private static void StyleToolbarButton(SimpleButton button)
        {
            button.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.TopCenter;
            button.Appearance.BackColor = System.Drawing.Color.FromArgb(240, 244, 249);
            button.Appearance.BorderColor = System.Drawing.Color.FromArgb(198, 214, 235);
            button.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            button.Appearance.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseBorderColor = true;
            button.Appearance.Options.UseFont = true;
            button.Appearance.Options.UseForeColor = true;
            button.ImageOptions.ImageToTextIndent = 4;
            button.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        }

        /// <summary>Arac cubugu ayirici cizgisi.</summary>
        private static void StyleSeparator(PanelControl separator, string name, int width)
        {
            separator.Appearance.BackColor = System.Drawing.Color.FromArgb(198, 214, 235);
            separator.Appearance.Options.UseBackColor = true;
            separator.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            separator.Name = name;
            separator.Size = new System.Drawing.Size(width, 44);
            separator.Margin = new System.Windows.Forms.Padding(4, 12, 4, 0);
        }
    }
}
