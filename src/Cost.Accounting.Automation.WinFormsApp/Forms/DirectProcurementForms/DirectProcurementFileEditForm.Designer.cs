using System;
using System.Drawing;
using System.Windows.Forms;
using Cost.Accounting.Automation.Domain.DirectProcurement;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.DirectProcurementForms
{
    /// <summary>
    /// Kaynak sistemdeki <c>TFrmYeniIhale</c> penceresinin birebir düzenidir.
    /// Konum ve boyutlar <c>TFRMYENIIHALE.dfm</c> içindeki değerlerin aynısıdır.
    /// </summary>
    /// <remarks>
    /// Kaynak hiyerarşi: <c>Label1/Label2</c> (başlık bandının arkasında kalan,
    /// görünmeyen etiketler) → <c>bsSkinScrollBox1</c> (üst başlık bandı,
    /// y=0..44) → <c>PC</c> (<c>TbsSkinPageControl</c>, tek sekmesi
    /// <c>TabVisible=False</c> olduğu için sekme başlığı çizilmez; içeriği
    /// <c>pnlContent</c> taşır) → <c>bsSkinToolBar1</c> (alt düğme çubuğu,
    /// y=701..738).
    /// </remarks>
    partial class DirectProcurementFileEditForm
    {
        // --- Başlık bandının arkasında kalan (DFM z-sırasında geride) etiketler ---
        private LabelControl lblBehindTitle = null!;
        private LabelControl lblBehindSubtitle = null!;

        // --- Üst başlık bandı: bsSkinScrollBox1 ---
        private PanelControl pnlHeader = null!;
        private PictureEdit picLogo = null!;
        private LabelControl lblHeaderTitle = null!;

        // --- Sekme içeriği: PC / TSGenelBilgiler ---
        private PanelControl pnlContent = null!;

        // --- Birim seçim şeridi: bsSkinPanel2 ---
        private PanelControl pnlUnit = null!;
        private ComboBoxEdit cmbUnit = null!;
        private LabelControl lblUnitWarning = null!;
        private LabelControl lblEditDate = null!;
        private DateEdit deEditDate = null!;

        // --- "İşe Ait Bilgiler": GBGenelBilgileri ---
        private GroupControl grpWorkInfo = null!;
        private LabelControl lblFileNumber = null!;
        private TextEdit txtFileNumber = null!;
        private LabelControl lblWorkingYear = null!;
        private ComboBoxEdit cmbWorkingYear = null!;
        private LabelControl lblWorkName = null!;
        private MemoEdit memoWorkName = null!;
        private LabelControl lblTeminType = null!;
        private PanelControl pnlTeminButtons = null!;
        private SimpleButton btnMalAlimi = null!;
        private SimpleButton btnHizmet = null!;
        private SimpleButton btnYapim = null!;
        private SimpleButton btnDanismanlik = null!;
        private LabelControl lblTeminKind = null!;
        private ComboBoxEdit cmbTeminKind = null!;
        private LabelControl lblTeminKindDesc = null!;
        private MemoEdit memoTeminKindDesc = null!;

        // --- "Dosya Ayarları": GBDosyaAyatalri ---
        private GroupControl grpFileSettings = null!;
        private LabelControl lblMaterialProcurement = null!;
        private ComboBoxEdit cmbMaterialProcurement = null!;
        private LabelControl lblHasApproximateCost = null!;
        private RadioGroup rgHasApproximateCost = null!;
        private LabelControl lblCostEstimationMethod = null!;
        private RadioGroup rgCostEstimationMethod = null!;
        private LabelControl lblUseHistoryPurchases = null!;
        private RadioGroup rgUseHistoryPurchases = null!;
        private LabelControl lblRequireFinalOffer = null!;
        private RadioGroup rgRequireFinalOffer = null!;
        private LabelControl lblHasContract = null!;
        private RadioGroup rgHasContract = null!;
        private LabelControl lblDecimalPlaces = null!;
        private SpinEdit seDecimalPlaces = null!;
        private LabelControl lblUnitPriceDecimalPlaces = null!;
        private SpinEdit seUnitPriceDecimalPlaces = null!;

        // --- Alt düğme çubuğu: bsSkinToolBar1 ---
        private PanelControl pnlFooter = null!;
        private SimpleButton btnGerekce = null!;
        private SimpleButton btnSave = null!;
        private SimpleButton btnCancel = null!;

        private void InitializeComponent()
        {
            lblBehindTitle = new LabelControl();
            lblBehindSubtitle = new LabelControl();
            pnlHeader = new PanelControl();
            lblHeaderTitle = new LabelControl();
            picLogo = new PictureEdit();
            pnlContent = new PanelControl();
            pnlUnit = new PanelControl();
            cmbUnit = new ComboBoxEdit();
            lblUnitWarning = new LabelControl();
            lblEditDate = new LabelControl();
            deEditDate = new DateEdit();
            grpWorkInfo = new GroupControl();
            lblFileNumber = new LabelControl();
            txtFileNumber = new TextEdit();
            lblWorkingYear = new LabelControl();
            cmbWorkingYear = new ComboBoxEdit();
            lblWorkName = new LabelControl();
            memoWorkName = new MemoEdit();
            lblTeminType = new LabelControl();
            pnlTeminButtons = new PanelControl();
            btnMalAlimi = new SimpleButton();
            btnHizmet = new SimpleButton();
            btnYapim = new SimpleButton();
            btnDanismanlik = new SimpleButton();
            lblTeminKind = new LabelControl();
            cmbTeminKind = new ComboBoxEdit();
            lblTeminKindDesc = new LabelControl();
            memoTeminKindDesc = new MemoEdit();
            grpFileSettings = new GroupControl();
            lblMaterialProcurement = new LabelControl();
            cmbMaterialProcurement = new ComboBoxEdit();
            lblHasApproximateCost = new LabelControl();
            rgHasApproximateCost = new RadioGroup();
            lblCostEstimationMethod = new LabelControl();
            rgCostEstimationMethod = new RadioGroup();
            lblUseHistoryPurchases = new LabelControl();
            rgUseHistoryPurchases = new RadioGroup();
            lblRequireFinalOffer = new LabelControl();
            rgRequireFinalOffer = new RadioGroup();
            lblHasContract = new LabelControl();
            rgHasContract = new RadioGroup();
            lblDecimalPlaces = new LabelControl();
            seDecimalPlaces = new SpinEdit();
            lblUnitPriceDecimalPlaces = new LabelControl();
            seUnitPriceDecimalPlaces = new SpinEdit();
            pnlFooter = new PanelControl();
            btnGerekce = new SimpleButton();
            btnSave = new SimpleButton();
            btnCancel = new SimpleButton();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlContent).BeginInit();
            pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlUnit).BeginInit();
            pnlUnit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbUnit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)deEditDate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)deEditDate.Properties.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grpWorkInfo).BeginInit();
            grpWorkInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtFileNumber.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbWorkingYear.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memoWorkName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlTeminButtons).BeginInit();
            pnlTeminButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbTeminKind.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memoTeminKindDesc.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grpFileSettings).BeginInit();
            grpFileSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbMaterialProcurement.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rgHasApproximateCost.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rgCostEstimationMethod.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rgUseHistoryPurchases.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rgRequireFinalOffer.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rgHasContract.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)seDecimalPlaces.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)seUnitPriceDecimalPlaces.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            // 
            // lblBehindTitle
            // 
            lblBehindTitle.Appearance.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold);
            lblBehindTitle.Appearance.Options.UseFont = true;
            lblBehindTitle.Location = new Point(8, 8);
            lblBehindTitle.Name = "lblBehindTitle";
            lblBehindTitle.Size = new Size(136, 25);
            lblBehindTitle.TabIndex = 100;
            lblBehindTitle.Text = "Yeni İhale Girişi";
            // 
            // lblBehindSubtitle
            // 
            lblBehindSubtitle.Location = new Point(8, 40);
            lblBehindSubtitle.Name = "lblBehindSubtitle";
            lblBehindSubtitle.Size = new Size(158, 13);
            lblBehindSubtitle.TabIndex = 101;
            lblBehindSubtitle.Text = "Yeni açılacak ihale bilgilerini giriniz";
            // 
            // pnlHeader
            // 
            pnlHeader.BorderStyle = BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(picLogo);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(611, 44);
            pnlHeader.TabIndex = 0;
            // 
            // lblHeaderTitle
            // 
            lblHeaderTitle.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblHeaderTitle.Appearance.Options.UseFont = true;
            lblHeaderTitle.Location = new Point(59, 13);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new Size(210, 21);
            lblHeaderTitle.TabIndex = 1;
            lblHeaderTitle.Text = "Yeni Doğrudan Temin Girişi";
            // 
            // picLogo
            // 
            picLogo.Location = new Point(11, 6);
            picLogo.Name = "picLogo";
            picLogo.Properties.Appearance.BackColor = Color.Transparent;
            picLogo.Properties.Appearance.Options.UseBackColor = true;
            picLogo.Properties.BorderStyle = BorderStyles.NoBorder;
            picLogo.Properties.SizeMode = PictureSizeMode.Stretch;
            picLogo.Size = new Size(32, 32);
            picLogo.TabIndex = 0;
            // 
            // pnlContent
            // 
            pnlContent.BorderStyle = BorderStyles.NoBorder;
            pnlContent.Controls.Add(pnlUnit);
            pnlContent.Controls.Add(grpWorkInfo);
            pnlContent.Controls.Add(grpFileSettings);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 44);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(611, 657);
            pnlContent.TabIndex = 1;
            // 
            // pnlUnit
            // 
            pnlUnit.BorderStyle = BorderStyles.NoBorder;
            pnlUnit.Controls.Add(cmbUnit);
            pnlUnit.Controls.Add(lblUnitWarning);
            pnlUnit.Controls.Add(lblEditDate);
            pnlUnit.Controls.Add(deEditDate);
            pnlUnit.Location = new Point(7, 8);
            pnlUnit.Name = "pnlUnit";
            pnlUnit.Size = new Size(594, 36);
            pnlUnit.TabIndex = 0;
            // 
            // cmbUnit
            // 
            cmbUnit.Location = new Point(4, 6);
            cmbUnit.Name = "cmbUnit";
            cmbUnit.Properties.NullText = "(Seçiniz)";
            cmbUnit.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cmbUnit.Size = new Size(241, 20);
            cmbUnit.TabIndex = 0;
            // 
            // lblUnitWarning
            // 
            lblUnitWarning.Appearance.ForeColor = Color.Red;
            lblUnitWarning.Appearance.Options.UseForeColor = true;
            lblUnitWarning.Location = new Point(249, 11);
            lblUnitWarning.Name = "lblUnitWarning";
            lblUnitWarning.Size = new Size(103, 13);
            lblUnitWarning.TabIndex = 1;
            lblUnitWarning.Text = "Lütfen Birim Seçiniz..!";
            // 
            // lblEditDate
            // 
            lblEditDate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblEditDate.Location = new Point(370, 11);
            lblEditDate.Name = "lblEditDate";
            lblEditDate.Size = new Size(81, 13);
            lblEditDate.TabIndex = 2;
            lblEditDate.Text = "Düzenleme Tarihi";
            // 
            // deEditDate
            // 
            deEditDate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            deEditDate.EditValue = null;
            deEditDate.Location = new Point(461, 7);
            deEditDate.Name = "deEditDate";
            deEditDate.Properties.Buttons.AddRange(new EditorButton[] { new EditorButton(ButtonPredefines.Combo) });
            deEditDate.Properties.CalendarTimeProperties.Buttons.AddRange(new EditorButton[] { new EditorButton(ButtonPredefines.Combo) });
            deEditDate.Properties.DisplayFormat.FormatString = "g";
            deEditDate.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            deEditDate.Properties.EditFormat.FormatString = "g";
            deEditDate.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            deEditDate.Size = new Size(128, 20);
            deEditDate.TabIndex = 3;
            // 
            // grpWorkInfo
            // 
            grpWorkInfo.Controls.Add(lblFileNumber);
            grpWorkInfo.Controls.Add(txtFileNumber);
            grpWorkInfo.Controls.Add(lblWorkingYear);
            grpWorkInfo.Controls.Add(cmbWorkingYear);
            grpWorkInfo.Controls.Add(lblWorkName);
            grpWorkInfo.Controls.Add(memoWorkName);
            grpWorkInfo.Controls.Add(lblTeminType);
            grpWorkInfo.Controls.Add(pnlTeminButtons);
            grpWorkInfo.Controls.Add(lblTeminKind);
            grpWorkInfo.Controls.Add(cmbTeminKind);
            grpWorkInfo.Controls.Add(lblTeminKindDesc);
            grpWorkInfo.Controls.Add(memoTeminKindDesc);
            grpWorkInfo.Location = new Point(7, 51);
            grpWorkInfo.Name = "grpWorkInfo";
            grpWorkInfo.Size = new Size(594, 292);
            grpWorkInfo.TabIndex = 1;
            grpWorkInfo.Text = "İşe Ait Bilgiler";
            // 
            // lblFileNumber
            // 
            lblFileNumber.Location = new Point(16, 32);
            lblFileNumber.Name = "lblFileNumber";
            lblFileNumber.Size = new Size(46, 13);
            lblFileNumber.TabIndex = 0;
            lblFileNumber.Text = "Dosya No";
            // 
            // txtFileNumber
            // 
            txtFileNumber.Location = new Point(119, 32);
            txtFileNumber.Name = "txtFileNumber";
            txtFileNumber.Properties.MaxLength = 50;
            txtFileNumber.Size = new Size(232, 20);
            txtFileNumber.TabIndex = 1;
            // 
            // lblWorkingYear
            // 
            lblWorkingYear.Location = new Point(359, 36);
            lblWorkingYear.Name = "lblWorkingYear";
            lblWorkingYear.Size = new Size(51, 13);
            lblWorkingYear.TabIndex = 2;
            lblWorkingYear.Text = "Çalışma Yılı";
            // 
            // cmbWorkingYear
            // 
            cmbWorkingYear.Location = new Point(430, 32);
            cmbWorkingYear.Name = "cmbWorkingYear";
            cmbWorkingYear.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cmbWorkingYear.Size = new Size(89, 20);
            cmbWorkingYear.TabIndex = 3;
            // 
            // lblWorkName
            // 
            lblWorkName.Location = new Point(16, 57);
            lblWorkName.Name = "lblWorkName";
            lblWorkName.Size = new Size(35, 13);
            lblWorkName.TabIndex = 4;
            lblWorkName.Text = "İşin Adı";
            // 
            // memoWorkName
            // 
            memoWorkName.Location = new Point(119, 57);
            memoWorkName.Name = "memoWorkName";
            memoWorkName.Properties.MaxLength = 500;
            memoWorkName.Size = new Size(463, 47);
            memoWorkName.TabIndex = 5;
            // 
            // lblTeminType
            // 
            lblTeminType.Location = new Point(16, 110);
            lblTeminType.Name = "lblTeminType";
            lblTeminType.Size = new Size(53, 13);
            lblTeminType.TabIndex = 6;
            lblTeminType.Text = "Temin Türü";
            // 
            // pnlTeminButtons
            // 
            pnlTeminButtons.BorderStyle = BorderStyles.NoBorder;
            pnlTeminButtons.Controls.Add(btnMalAlimi);
            pnlTeminButtons.Controls.Add(btnHizmet);
            pnlTeminButtons.Controls.Add(btnYapim);
            pnlTeminButtons.Controls.Add(btnDanismanlik);
            pnlTeminButtons.Location = new Point(120, 110);
            pnlTeminButtons.Name = "pnlTeminButtons";
            pnlTeminButtons.Size = new Size(462, 75);
            pnlTeminButtons.TabIndex = 7;
            // 
            // btnMalAlimi
            // 
            btnMalAlimi.ImageOptions.ImageToTextAlignment = ImageAlignToText.TopCenter;
            btnMalAlimi.Location = new Point(0, 0);
            btnMalAlimi.Name = "btnMalAlimi";
            btnMalAlimi.Size = new Size(112, 75);
            btnMalAlimi.TabIndex = 0;
            btnMalAlimi.Text = "&Mal Alım";
            // 
            // btnHizmet
            // 
            btnHizmet.ImageOptions.ImageToTextAlignment = ImageAlignToText.TopCenter;
            btnHizmet.Location = new Point(112, 0);
            btnHizmet.Name = "btnHizmet";
            btnHizmet.Size = new Size(112, 75);
            btnHizmet.TabIndex = 1;
            btnHizmet.Text = "&Hizmet";
            // 
            // btnYapim
            // 
            btnYapim.ImageOptions.ImageToTextAlignment = ImageAlignToText.TopCenter;
            btnYapim.Location = new Point(224, 0);
            btnYapim.Name = "btnYapim";
            btnYapim.Size = new Size(112, 75);
            btnYapim.TabIndex = 2;
            btnYapim.Text = "&Yapım";
            // 
            // btnDanismanlik
            // 
            btnDanismanlik.ImageOptions.ImageToTextAlignment = ImageAlignToText.TopCenter;
            btnDanismanlik.Location = new Point(336, 0);
            btnDanismanlik.Name = "btnDanismanlik";
            btnDanismanlik.Size = new Size(112, 75);
            btnDanismanlik.TabIndex = 3;
            btnDanismanlik.Text = "&Danışmanlık";
            btnDanismanlik.Click += btnDanismanlik_Click;
            // 
            // lblTeminKind
            // 
            lblTeminKind.Location = new Point(16, 194);
            lblTeminKind.Name = "lblTeminKind";
            lblTeminKind.Size = new Size(47, 13);
            lblTeminKind.TabIndex = 8;
            lblTeminKind.Text = "Temin Tipi";
            // 
            // cmbTeminKind
            // 
            cmbTeminKind.Location = new Point(119, 190);
            cmbTeminKind.Name = "cmbTeminKind";
            cmbTeminKind.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cmbTeminKind.Size = new Size(449, 20);
            cmbTeminKind.TabIndex = 9;
            // 
            // lblTeminKindDesc
            // 
            lblTeminKindDesc.Location = new Point(16, 215);
            lblTeminKindDesc.Name = "lblTeminKindDesc";
            lblTeminKindDesc.Size = new Size(91, 13);
            lblTeminKindDesc.TabIndex = 10;
            lblTeminKindDesc.Text = "Temin Tipi Açıklama";
            // 
            // memoTeminKindDesc
            // 
            memoTeminKindDesc.Location = new Point(119, 215);
            memoTeminKindDesc.Name = "memoTeminKindDesc";
            memoTeminKindDesc.Properties.ReadOnly = true;
            memoTeminKindDesc.Size = new Size(466, 68);
            memoTeminKindDesc.TabIndex = 11;
            // 
            // grpFileSettings
            // 
            grpFileSettings.Controls.Add(lblMaterialProcurement);
            grpFileSettings.Controls.Add(cmbMaterialProcurement);
            grpFileSettings.Controls.Add(lblHasApproximateCost);
            grpFileSettings.Controls.Add(rgHasApproximateCost);
            grpFileSettings.Controls.Add(lblCostEstimationMethod);
            grpFileSettings.Controls.Add(rgCostEstimationMethod);
            grpFileSettings.Controls.Add(lblUseHistoryPurchases);
            grpFileSettings.Controls.Add(rgUseHistoryPurchases);
            grpFileSettings.Controls.Add(lblRequireFinalOffer);
            grpFileSettings.Controls.Add(rgRequireFinalOffer);
            grpFileSettings.Controls.Add(lblHasContract);
            grpFileSettings.Controls.Add(rgHasContract);
            grpFileSettings.Controls.Add(lblDecimalPlaces);
            grpFileSettings.Controls.Add(seDecimalPlaces);
            grpFileSettings.Controls.Add(lblUnitPriceDecimalPlaces);
            grpFileSettings.Controls.Add(seUnitPriceDecimalPlaces);
            grpFileSettings.Location = new Point(7, 358);
            grpFileSettings.Name = "grpFileSettings";
            grpFileSettings.Size = new Size(593, 283);
            grpFileSettings.TabIndex = 2;
            grpFileSettings.Text = "Dosya Ayarları";
            // 
            // lblMaterialProcurement
            // 
            lblMaterialProcurement.Location = new Point(16, 34);
            lblMaterialProcurement.Name = "lblMaterialProcurement";
            lblMaterialProcurement.Size = new Size(158, 13);
            lblMaterialProcurement.TabIndex = 0;
            lblMaterialProcurement.Text = "Malzeme Alım Şekli Nasıl Yapılsın ?";
            // 
            // cmbMaterialProcurement
            // 
            cmbMaterialProcurement.Location = new Point(199, 30);
            cmbMaterialProcurement.Name = "cmbMaterialProcurement";
            cmbMaterialProcurement.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cmbMaterialProcurement.Size = new Size(383, 20);
            cmbMaterialProcurement.TabIndex = 1;
            // 
            // lblHasApproximateCost
            // 
            lblHasApproximateCost.Location = new Point(16, 73);
            lblHasApproximateCost.Name = "lblHasApproximateCost";
            lblHasApproximateCost.Size = new Size(142, 13);
            lblHasApproximateCost.TabIndex = 2;
            lblHasApproximateCost.Text = "Yaklaşık Maliyet Yapılacak mı ?";
            // 
            // rgHasApproximateCost
            // 
            rgHasApproximateCost.Location = new Point(271, 73);
            rgHasApproximateCost.Name = "rgHasApproximateCost";
            rgHasApproximateCost.Properties.Appearance.BackColor = Color.Transparent;
            rgHasApproximateCost.Properties.Appearance.Options.UseBackColor = true;
            rgHasApproximateCost.Properties.BorderStyle = BorderStyles.NoBorder;
            rgHasApproximateCost.Properties.Columns = 2;
            rgHasApproximateCost.Properties.Items.AddRange(new RadioGroupItem[] { new RadioGroupItem(true, "Evet"), new RadioGroupItem(false, "Hayır") });
            rgHasApproximateCost.Size = new Size(357, 17);
            rgHasApproximateCost.TabIndex = 3;
            // 
            // lblCostEstimationMethod
            // 
            lblCostEstimationMethod.Location = new Point(16, 104);
            lblCostEstimationMethod.Name = "lblCostEstimationMethod";
            lblCostEstimationMethod.Size = new Size(170, 13);
            lblCostEstimationMethod.TabIndex = 4;
            lblCostEstimationMethod.Text = "Yaklaşık Maliyet Nasıl Hesaplansın ? ";
            // 
            // rgCostEstimationMethod
            // 
            rgCostEstimationMethod.Location = new Point(271, 102);
            rgCostEstimationMethod.Name = "rgCostEstimationMethod";
            rgCostEstimationMethod.Properties.Appearance.BackColor = Color.Transparent;
            rgCostEstimationMethod.Properties.Appearance.Options.UseBackColor = true;
            rgCostEstimationMethod.Properties.BorderStyle = BorderStyles.NoBorder;
            rgCostEstimationMethod.Properties.Columns = 2;
            rgCostEstimationMethod.Properties.Items.AddRange(new RadioGroupItem[] { new RadioGroupItem(CostEstimationMethodType.CommonUnitPrice, "Ortalama Birim Fiyat "), new RadioGroupItem(CostEstimationMethodType.LowestUnitPrice, "En Düşük Birim Fiyat") });
            rgCostEstimationMethod.Size = new Size(357, 17);
            rgCostEstimationMethod.TabIndex = 5;
            // 
            // lblUseHistoryPurchases
            // 
            lblUseHistoryPurchases.Location = new Point(16, 129);
            lblUseHistoryPurchases.Name = "lblUseHistoryPurchases";
            lblUseHistoryPurchases.Size = new Size(206, 13);
            lblUseHistoryPurchases.TabIndex = 6;
            lblUseHistoryPurchases.Text = "YM Hesabında Geçmiş Alımlar Kullanılsın mı ?";
            // 
            // rgUseHistoryPurchases
            // 
            rgUseHistoryPurchases.Location = new Point(271, 129);
            rgUseHistoryPurchases.Name = "rgUseHistoryPurchases";
            rgUseHistoryPurchases.Properties.Appearance.BackColor = Color.Transparent;
            rgUseHistoryPurchases.Properties.Appearance.Options.UseBackColor = true;
            rgUseHistoryPurchases.Properties.BorderStyle = BorderStyles.NoBorder;
            rgUseHistoryPurchases.Properties.Columns = 2;
            rgUseHistoryPurchases.Properties.Items.AddRange(new RadioGroupItem[] { new RadioGroupItem(true, "Evet"), new RadioGroupItem(false, "Hayır") });
            rgUseHistoryPurchases.Size = new Size(358, 17);
            rgUseHistoryPurchases.TabIndex = 7;
            // 
            // lblRequireFinalOffer
            // 
            lblRequireFinalOffer.Location = new Point(16, 165);
            lblRequireFinalOffer.Name = "lblRequireFinalOffer";
            lblRequireFinalOffer.Size = new Size(246, 13);
            lblRequireFinalOffer.TabIndex = 8;
            lblRequireFinalOffer.Text = "Temin Aşamasında Firmadan Son Teklif İstensin mi ?";
            // 
            // rgRequireFinalOffer
            // 
            rgRequireFinalOffer.Location = new Point(271, 165);
            rgRequireFinalOffer.Name = "rgRequireFinalOffer";
            rgRequireFinalOffer.Properties.Appearance.BackColor = Color.Transparent;
            rgRequireFinalOffer.Properties.Appearance.Options.UseBackColor = true;
            rgRequireFinalOffer.Properties.BorderStyle = BorderStyles.NoBorder;
            rgRequireFinalOffer.Properties.Columns = 2;
            rgRequireFinalOffer.Properties.Items.AddRange(new RadioGroupItem[] { new RadioGroupItem(true, "Evet"), new RadioGroupItem(false, "Hayır") });
            rgRequireFinalOffer.Size = new Size(358, 17);
            rgRequireFinalOffer.TabIndex = 9;
            // 
            // lblHasContract
            // 
            lblHasContract.Location = new Point(16, 200);
            lblHasContract.Name = "lblHasContract";
            lblHasContract.Size = new Size(127, 13);
            lblHasContract.TabIndex = 10;
            lblHasContract.Text = "Sözleşme İmzalanacak mı ?";
            // 
            // rgHasContract
            // 
            rgHasContract.Location = new Point(271, 200);
            rgHasContract.Name = "rgHasContract";
            rgHasContract.Properties.Appearance.BackColor = Color.Transparent;
            rgHasContract.Properties.Appearance.Options.UseBackColor = true;
            rgHasContract.Properties.BorderStyle = BorderStyles.NoBorder;
            rgHasContract.Properties.Columns = 2;
            rgHasContract.Properties.Items.AddRange(new RadioGroupItem[] { new RadioGroupItem(true, "Evet"), new RadioGroupItem(false, "Hayır") });
            rgHasContract.Size = new Size(358, 17);
            rgHasContract.TabIndex = 11;
            // 
            // lblDecimalPlaces
            // 
            lblDecimalPlaces.Location = new Point(16, 228);
            lblDecimalPlaces.Name = "lblDecimalPlaces";
            lblDecimalPlaces.Size = new Size(202, 13);
            lblDecimalPlaces.TabIndex = 12;
            lblDecimalPlaces.Text = "Tutarlarda Kullanılacak Ondalık Hassasiyeti";
            // 
            // seDecimalPlaces
            // 
            seDecimalPlaces.EditValue = new decimal(new int[] { 2, 0, 0, 0 });
            seDecimalPlaces.Location = new Point(271, 230);
            seDecimalPlaces.Name = "seDecimalPlaces";
            seDecimalPlaces.Properties.IsFloatValue = false;
            seDecimalPlaces.Properties.MaxValue = new decimal(new int[] { 10, 0, 0, 0 });
            seDecimalPlaces.Size = new Size(59, 20);
            seDecimalPlaces.TabIndex = 13;
            // 
            // lblUnitPriceDecimalPlaces
            // 
            lblUnitPriceDecimalPlaces.Location = new Point(16, 255);
            lblUnitPriceDecimalPlaces.Name = "lblUnitPriceDecimalPlaces";
            lblUnitPriceDecimalPlaces.Size = new Size(225, 13);
            lblUnitPriceDecimalPlaces.TabIndex = 14;
            lblUnitPriceDecimalPlaces.Text = "Birim Fiyatlarda Kullanılacak Ondalık Hassasiyeti";
            // 
            // seUnitPriceDecimalPlaces
            // 
            seUnitPriceDecimalPlaces.EditValue = new decimal(new int[] { 2, 0, 0, 0 });
            seUnitPriceDecimalPlaces.Location = new Point(271, 258);
            seUnitPriceDecimalPlaces.Name = "seUnitPriceDecimalPlaces";
            seUnitPriceDecimalPlaces.Properties.IsFloatValue = false;
            seUnitPriceDecimalPlaces.Properties.MaxValue = new decimal(new int[] { 10, 0, 0, 0 });
            seUnitPriceDecimalPlaces.Size = new Size(59, 20);
            seUnitPriceDecimalPlaces.TabIndex = 15;
            // 
            // pnlFooter
            // 
            pnlFooter.BorderStyle = BorderStyles.NoBorder;
            pnlFooter.Controls.Add(btnGerekce);
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 701);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(611, 37);
            pnlFooter.TabIndex = 2;
            // 
            // btnGerekce
            // 
            btnGerekce.Location = new Point(10, 6);
            btnGerekce.Name = "btnGerekce";
            btnGerekce.Size = new Size(217, 25);
            btnGerekce.TabIndex = 0;
            btnGerekce.Text = "Doğrudan Temin Gerekçe Tutanağı";
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Location = new Point(429, 6);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(83, 25);
            btnSave.TabIndex = 1;
            btnSave.Text = "Kaydet";
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Location = new Point(518, 6);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(83, 25);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Vazgeç";
            // 
            // DirectProcurementFileEditForm
            // 
            Appearance.Options.UseFont = true;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(611, 738);
            Controls.Add(pnlContent);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            Controls.Add(lblBehindSubtitle);
            Controls.Add(lblBehindTitle);
            Font = new Font("Segoe UI", 8.25F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DirectProcurementFileEditForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Doğrudan Temin";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlContent).EndInit();
            pnlContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlUnit).EndInit();
            pnlUnit.ResumeLayout(false);
            pnlUnit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cmbUnit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)deEditDate.Properties.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)deEditDate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)grpWorkInfo).EndInit();
            grpWorkInfo.ResumeLayout(false);
            grpWorkInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtFileNumber.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbWorkingYear.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)memoWorkName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlTeminButtons).EndInit();
            pnlTeminButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)cmbTeminKind.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)memoTeminKindDesc.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)grpFileSettings).EndInit();
            grpFileSettings.ResumeLayout(false);
            grpFileSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cmbMaterialProcurement.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)rgHasApproximateCost.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)rgCostEstimationMethod.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)rgUseHistoryPurchases.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)rgRequireFinalOffer.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)rgHasContract.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)seDecimalPlaces.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)seUnitPriceDecimalPlaces.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
