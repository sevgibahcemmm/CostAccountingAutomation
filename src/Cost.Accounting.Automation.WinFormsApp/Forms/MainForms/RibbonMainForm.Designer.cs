using System.Drawing;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    partial class RibbonMainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RibbonMainForm));
            ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            barButtonItemCompanyName = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemUserName = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemRoleName = new DevExpress.XtraBars.BarButtonItem();
            barStaticItemVersion = new DevExpress.XtraBars.BarStaticItem();
            barButtonItemLiveMessaging = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemLongDate = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemTime = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemExpTime = new DevExpress.XtraBars.BarButtonItem();
            ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            skinRibbonGalleryBarItem1 = new DevExpress.XtraBars.SkinRibbonGalleryBarItem();
            accordionControl = new DevExpress.XtraBars.Navigation.AccordionControl();
            elmHome = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator31 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            grpStockMaster = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmProducts = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator6 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmUnitTypes = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator7 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmTaxRates = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator8 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmConsumptionUnits = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator9 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator32 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            grpStockOperations = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmStockInput = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator10 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmStockOutput = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator11 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmConsumption = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator12 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmWorkshopTransfer = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator13 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator33 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            grpInvoices = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmInvoices = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator14 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmInvoiceApproval = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator15 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator34 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            grpCurrentAccounts = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmCustomers = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator16 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmSuppliers = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator17 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmCurrentAccountMovements = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator18 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmPayments = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator19 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmCurrentAccountBalance = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator20 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator35 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            grpCosting = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmCostSlips = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator21 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmRecipes = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator22 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmWorkshopAnalysis = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator23 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator36 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            grpAccounting = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmChartOfAccounts = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator24 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmCarryForwardOperations = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator25 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator37 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            grpReports = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmStockMovements = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator26 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmPriceStockList = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator27 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmWorkshopStockReport = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator28 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator38 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            grpSystem = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmCompanies = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator2 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmUsers = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator3 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmRoles = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator4 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmEmployees = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator5 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmSigningRoles = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator1 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmChangePassword = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator41 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmCreateUpdate = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmUpdateHistory = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator39 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            grpMessages = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmMessages = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator29 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            grpDirectProcurement = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmDirectProcurementHome = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmDirectProcurementFiles = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmDirectProcurementNew = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator44 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator45 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator46 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator47 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator40 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            elmExit = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            accordionControlSeparator30 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            xtraTabbedMdiManager = new DevExpress.XtraTabbedMdi.XtraTabbedMdiManager(components);
            accordionControlSeparator42 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            accordionControlSeparator43 = new DevExpress.XtraBars.Navigation.AccordionControlSeparator();
            ((System.ComponentModel.ISupportInitialize)ribbon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)accordionControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)xtraTabbedMdiManager).BeginInit();
            SuspendLayout();
            // 
            // ribbonStatusBar
            // 
            ribbonStatusBar.ItemLinks.Add(barButtonItemCompanyName);
            ribbonStatusBar.ItemLinks.Add(barButtonItemUserName);
            ribbonStatusBar.ItemLinks.Add(barButtonItemRoleName);
            ribbonStatusBar.ItemLinks.Add(barStaticItemVersion);
            ribbonStatusBar.ItemLinks.Add(barButtonItemLiveMessaging);
            ribbonStatusBar.ItemLinks.Add(barButtonItemLongDate);
            ribbonStatusBar.ItemLinks.Add(barButtonItemTime);
            ribbonStatusBar.ItemLinks.Add(barButtonItemExpTime);
            ribbonStatusBar.Location = new Point(0, 734);
            ribbonStatusBar.Margin = new Padding(3, 2, 3, 2);
            ribbonStatusBar.Name = "ribbonStatusBar";
            ribbonStatusBar.Ribbon = ribbon;
            ribbonStatusBar.Size = new Size(1411, 24);
            // 
            // barButtonItemCompanyName
            // 
            barButtonItemCompanyName.Caption = " ";
            barButtonItemCompanyName.Id = 1;
            barButtonItemCompanyName.Name = "barButtonItemCompanyName";
            // 
            // barButtonItemUserName
            // 
            barButtonItemUserName.Caption = " ";
            barButtonItemUserName.Id = 2;
            barButtonItemUserName.Name = "barButtonItemUserName";
            // 
            // barButtonItemRoleName
            // 
            barButtonItemRoleName.Caption = " ";
            barButtonItemRoleName.Id = 3;
            barButtonItemRoleName.Name = "barButtonItemRoleName";
            // 
            // barStaticItemVersion
            // 
            barStaticItemVersion.Caption = " ";
            barStaticItemVersion.Id = 8;
            barStaticItemVersion.Name = "barStaticItemVersion";
            // 
            // barButtonItemLiveMessaging
            // 
            barButtonItemLiveMessaging.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            barButtonItemLiveMessaging.Caption = " ";
            barButtonItemLiveMessaging.Id = 7;
            barButtonItemLiveMessaging.Name = "barButtonItemLiveMessaging";
            // 
            // barButtonItemLongDate
            // 
            barButtonItemLongDate.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            barButtonItemLongDate.Caption = " ";
            barButtonItemLongDate.Id = 4;
            barButtonItemLongDate.Name = "barButtonItemLongDate";
            // 
            // barButtonItemTime
            // 
            barButtonItemTime.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            barButtonItemTime.Caption = " ";
            barButtonItemTime.Id = 5;
            barButtonItemTime.Name = "barButtonItemTime";
            // 
            // barButtonItemExpTime
            // 
            barButtonItemExpTime.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            barButtonItemExpTime.Caption = " ";
            barButtonItemExpTime.Id = 6;
            barButtonItemExpTime.Name = "barButtonItemExpTime";
            // 
            // ribbon
            // 
            ribbon.ApplicationButtonImageOptions.Image = (Image)resources.GetObject("ribbon.ApplicationButtonImageOptions.Image");
            ribbon.CaptionBarItemLinks.Add(skinRibbonGalleryBarItem1);
            ribbon.EmptyAreaImageOptions.ImagePadding = new Padding(26, 24, 26, 24);
            ribbon.ExpandCollapseItem.Id = 0;
            ribbon.ImageAlignment = DevExpress.Utils.HorzAlignment.Center;
            ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] { skinRibbonGalleryBarItem1, ribbon.ExpandCollapseItem, barButtonItemCompanyName, barButtonItemUserName, barButtonItemRoleName, barStaticItemVersion, barButtonItemLiveMessaging, barButtonItemLongDate, barButtonItemTime, barButtonItemExpTime });
            ribbon.Location = new Point(0, 0);
            ribbon.Margin = new Padding(3, 2, 3, 2);
            ribbon.MaxItemId = 10;
            ribbon.Name = "ribbon";
            ribbon.OptionsMenuMinWidth = 283;
            ribbon.OptionsSearchMenu.SearchItemPosition = DevExpress.XtraBars.Ribbon.SearchItemPosition.PageHeader;
            ribbon.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.Office2007;
            ribbon.Size = new Size(1411, 58);
            ribbon.StatusBar = ribbonStatusBar;
            // 
            // skinRibbonGalleryBarItem1
            // 
            skinRibbonGalleryBarItem1.Caption = "Tema";
            skinRibbonGalleryBarItem1.Description = "Tema";
            skinRibbonGalleryBarItem1.Id = 7;
            skinRibbonGalleryBarItem1.Name = "skinRibbonGalleryBarItem1";
            // 
            // accordionControl
            // 
            accordionControl.Dock = DockStyle.Left;
            accordionControl.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmHome, accordionControlSeparator31, grpStockMaster, accordionControlSeparator32, grpStockOperations, accordionControlSeparator33, grpInvoices, accordionControlSeparator34, grpCurrentAccounts, accordionControlSeparator35, grpCosting, accordionControlSeparator36, grpAccounting, accordionControlSeparator37, grpReports, accordionControlSeparator38, grpSystem, accordionControlSeparator39, grpMessages, accordionControlSeparator40, grpDirectProcurement, accordionControlSeparator47, elmExit, accordionControlSeparator30 });
            accordionControl.Location = new Point(0, 58);
            accordionControl.Margin = new Padding(3, 2, 3, 2);
            accordionControl.Name = "accordionControl";
            accordionControl.Size = new Size(250, 676);
            accordionControl.TabIndex = 2;
            // 
            // elmHome
            // 
            elmHome.Expanded = true;
            elmHome.Name = "elmHome";
            elmHome.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmHome.Tag = 0;
            elmHome.Text = "Ana Sayfa";
            // 
            // accordionControlSeparator31
            // 
            accordionControlSeparator31.Name = "accordionControlSeparator31";
            // 
            // grpStockMaster
            // 
            grpStockMaster.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmProducts, accordionControlSeparator6, elmUnitTypes, accordionControlSeparator7, elmTaxRates, accordionControlSeparator8, elmConsumptionUnits, accordionControlSeparator9 });
            grpStockMaster.Name = "grpStockMaster";
            grpStockMaster.Tag = 1;
            grpStockMaster.Text = "Stok Yönetimi";
            // 
            // elmProducts
            // 
            elmProducts.Name = "elmProducts";
            elmProducts.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmProducts.Tag = 1;
            elmProducts.Text = "Ürünler";
            // 
            // accordionControlSeparator6
            // 
            accordionControlSeparator6.Name = "accordionControlSeparator6";
            // 
            // elmUnitTypes
            // 
            elmUnitTypes.Name = "elmUnitTypes";
            elmUnitTypes.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmUnitTypes.Tag = 1;
            elmUnitTypes.Text = "Birim Cinsleri";
            // 
            // accordionControlSeparator7
            // 
            accordionControlSeparator7.Name = "accordionControlSeparator7";
            // 
            // elmTaxRates
            // 
            elmTaxRates.Name = "elmTaxRates";
            elmTaxRates.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmTaxRates.Tag = 1;
            elmTaxRates.Text = "KDV Oranları";
            // 
            // accordionControlSeparator8
            // 
            accordionControlSeparator8.Name = "accordionControlSeparator8";
            // 
            // elmConsumptionUnits
            // 
            elmConsumptionUnits.Name = "elmConsumptionUnits";
            elmConsumptionUnits.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmConsumptionUnits.Tag = 1;
            elmConsumptionUnits.Text = "Tüketim Birimleri";
            // 
            // accordionControlSeparator9
            // 
            accordionControlSeparator9.Name = "accordionControlSeparator9";
            // 
            // accordionControlSeparator32
            // 
            accordionControlSeparator32.Name = "accordionControlSeparator32";
            // 
            // grpStockOperations
            // 
            grpStockOperations.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmStockInput, accordionControlSeparator10, elmStockOutput, accordionControlSeparator11, elmConsumption, accordionControlSeparator12, elmWorkshopTransfer, accordionControlSeparator13 });
            grpStockOperations.Name = "grpStockOperations";
            grpStockOperations.Tag = 2;
            grpStockOperations.Text = "Stok İşlemleri";
            // 
            // elmStockInput
            // 
            elmStockInput.Name = "elmStockInput";
            elmStockInput.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmStockInput.Tag = 2;
            elmStockInput.Text = "Stok Girişi";
            // 
            // accordionControlSeparator10
            // 
            accordionControlSeparator10.Name = "accordionControlSeparator10";
            // 
            // elmStockOutput
            // 
            elmStockOutput.Name = "elmStockOutput";
            elmStockOutput.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmStockOutput.Tag = 2;
            elmStockOutput.Text = "Stok Çıkışı";
            // 
            // accordionControlSeparator11
            // 
            accordionControlSeparator11.Name = "accordionControlSeparator11";
            // 
            // elmConsumption
            // 
            elmConsumption.Name = "elmConsumption";
            elmConsumption.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmConsumption.Tag = 2;
            elmConsumption.Text = "Tüketim";
            // 
            // accordionControlSeparator12
            // 
            accordionControlSeparator12.Name = "accordionControlSeparator12";
            // 
            // elmWorkshopTransfer
            // 
            elmWorkshopTransfer.Name = "elmWorkshopTransfer";
            elmWorkshopTransfer.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmWorkshopTransfer.Tag = 2;
            elmWorkshopTransfer.Text = "Atölye Transferi";
            // 
            // accordionControlSeparator13
            // 
            accordionControlSeparator13.Name = "accordionControlSeparator13";
            // 
            // accordionControlSeparator33
            // 
            accordionControlSeparator33.Name = "accordionControlSeparator33";
            // 
            // grpInvoices
            // 
            grpInvoices.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmInvoices, accordionControlSeparator14, elmInvoiceApproval, accordionControlSeparator15 });
            grpInvoices.Name = "grpInvoices";
            grpInvoices.Tag = 3;
            grpInvoices.Text = "Faturalar";
            // 
            // elmInvoices
            // 
            elmInvoices.Name = "elmInvoices";
            elmInvoices.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmInvoices.Tag = 3;
            elmInvoices.Text = "Faturalar (Tümü)";
            // 
            // accordionControlSeparator14
            // 
            accordionControlSeparator14.Name = "accordionControlSeparator14";
            // 
            // elmInvoiceApproval
            // 
            elmInvoiceApproval.Name = "elmInvoiceApproval";
            elmInvoiceApproval.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmInvoiceApproval.Tag = 3;
            elmInvoiceApproval.Text = "Fatura Onaylama";
            // 
            // accordionControlSeparator15
            // 
            accordionControlSeparator15.Name = "accordionControlSeparator15";
            // 
            // accordionControlSeparator34
            // 
            accordionControlSeparator34.Name = "accordionControlSeparator34";
            // 
            // grpCurrentAccounts
            // 
            grpCurrentAccounts.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmCustomers, accordionControlSeparator16, elmSuppliers, accordionControlSeparator17, elmCurrentAccountMovements, accordionControlSeparator18, elmPayments, accordionControlSeparator19, elmCurrentAccountBalance, accordionControlSeparator20 });
            grpCurrentAccounts.Name = "grpCurrentAccounts";
            grpCurrentAccounts.Tag = 4;
            grpCurrentAccounts.Text = "Cari Yönetimi";
            // 
            // elmCustomers
            // 
            elmCustomers.Name = "elmCustomers";
            elmCustomers.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmCustomers.Tag = 4;
            elmCustomers.Text = "Müşteriler";
            // 
            // accordionControlSeparator16
            // 
            accordionControlSeparator16.Name = "accordionControlSeparator16";
            // 
            // elmSuppliers
            // 
            elmSuppliers.Name = "elmSuppliers";
            elmSuppliers.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmSuppliers.Tag = 4;
            elmSuppliers.Text = "Tedarikçiler";
            // 
            // accordionControlSeparator17
            // 
            accordionControlSeparator17.Name = "accordionControlSeparator17";
            // 
            // elmCurrentAccountMovements
            // 
            elmCurrentAccountMovements.Name = "elmCurrentAccountMovements";
            elmCurrentAccountMovements.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmCurrentAccountMovements.Tag = 4;
            elmCurrentAccountMovements.Text = "Cari Hareketler";
            // 
            // accordionControlSeparator18
            // 
            accordionControlSeparator18.Name = "accordionControlSeparator18";
            // 
            // elmPayments
            // 
            elmPayments.Name = "elmPayments";
            elmPayments.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmPayments.Tag = 4;
            elmPayments.Text = "Ödeme / Tahsilat";
            // 
            // accordionControlSeparator19
            // 
            accordionControlSeparator19.Name = "accordionControlSeparator19";
            // 
            // elmCurrentAccountBalance
            // 
            elmCurrentAccountBalance.Name = "elmCurrentAccountBalance";
            elmCurrentAccountBalance.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmCurrentAccountBalance.Tag = 4;
            elmCurrentAccountBalance.Text = "Cari Borç/Alacak Özeti";
            // 
            // accordionControlSeparator20
            // 
            accordionControlSeparator20.Name = "accordionControlSeparator20";
            // 
            // accordionControlSeparator35
            // 
            accordionControlSeparator35.Name = "accordionControlSeparator35";
            // 
            // grpCosting
            // 
            grpCosting.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmCostSlips, accordionControlSeparator21, elmRecipes, accordionControlSeparator22, elmWorkshopAnalysis, accordionControlSeparator23 });
            grpCosting.Name = "grpCosting";
            grpCosting.Tag = 5;
            grpCosting.Text = "Maliyet & Üretim";
            // 
            // elmCostSlips
            // 
            elmCostSlips.Name = "elmCostSlips";
            elmCostSlips.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmCostSlips.Tag = 5;
            elmCostSlips.Text = "Maliyet Pusulası";
            // 
            // accordionControlSeparator21
            // 
            accordionControlSeparator21.Name = "accordionControlSeparator21";
            // 
            // elmRecipes
            // 
            elmRecipes.Name = "elmRecipes";
            elmRecipes.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmRecipes.Tag = 5;
            elmRecipes.Text = "Reçeteler";
            // 
            // accordionControlSeparator22
            // 
            accordionControlSeparator22.Name = "accordionControlSeparator22";
            // 
            // elmWorkshopAnalysis
            // 
            elmWorkshopAnalysis.Name = "elmWorkshopAnalysis";
            elmWorkshopAnalysis.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmWorkshopAnalysis.Tag = 5;
            elmWorkshopAnalysis.Text = "Atölye Gelir/Gider Analizi";
            // 
            // accordionControlSeparator23
            // 
            accordionControlSeparator23.Name = "accordionControlSeparator23";
            // 
            // accordionControlSeparator36
            // 
            accordionControlSeparator36.Name = "accordionControlSeparator36";
            // 
            // grpAccounting
            // 
            grpAccounting.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmChartOfAccounts, accordionControlSeparator24, elmCarryForwardOperations, accordionControlSeparator25 });
            grpAccounting.Name = "grpAccounting";
            grpAccounting.Tag = 6;
            grpAccounting.Text = "Muhasebe";
            // 
            // elmChartOfAccounts
            // 
            elmChartOfAccounts.Name = "elmChartOfAccounts";
            elmChartOfAccounts.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmChartOfAccounts.Tag = 6;
            elmChartOfAccounts.Text = "Hesap Planı";
            // 
            // accordionControlSeparator24
            // 
            accordionControlSeparator24.Name = "accordionControlSeparator24";
            // 
            // elmCarryForwardOperations
            // 
            elmCarryForwardOperations.Name = "elmCarryForwardOperations";
            elmCarryForwardOperations.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmCarryForwardOperations.Tag = 6;
            elmCarryForwardOperations.Text = "Devir İşlemleri";
            // 
            // accordionControlSeparator25
            // 
            accordionControlSeparator25.Name = "accordionControlSeparator25";
            // 
            // accordionControlSeparator37
            // 
            accordionControlSeparator37.Name = "accordionControlSeparator37";
            // 
            // grpReports
            // 
            grpReports.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmStockMovements, accordionControlSeparator26, elmPriceStockList, accordionControlSeparator27, elmWorkshopStockReport, accordionControlSeparator28 });
            grpReports.Name = "grpReports";
            grpReports.Tag = 7;
            grpReports.Text = "Raporlar";
            // 
            // elmStockMovements
            // 
            elmStockMovements.Name = "elmStockMovements";
            elmStockMovements.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmStockMovements.Tag = 7;
            elmStockMovements.Text = "Stok Hareketleri";
            // 
            // accordionControlSeparator26
            // 
            accordionControlSeparator26.Name = "accordionControlSeparator26";
            // 
            // elmPriceStockList
            // 
            elmPriceStockList.Name = "elmPriceStockList";
            elmPriceStockList.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmPriceStockList.Tag = 7;
            elmPriceStockList.Text = "Fiyat & Stok Listesi";
            // 
            // accordionControlSeparator27
            // 
            accordionControlSeparator27.Name = "accordionControlSeparator27";
            // 
            // elmWorkshopStockReport
            // 
            elmWorkshopStockReport.Name = "elmWorkshopStockReport";
            elmWorkshopStockReport.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmWorkshopStockReport.Tag = 7;
            elmWorkshopStockReport.Text = "Atölye Stok Raporu";
            // 
            // accordionControlSeparator28
            // 
            accordionControlSeparator28.Name = "accordionControlSeparator28";
            // 
            // accordionControlSeparator38
            // 
            accordionControlSeparator38.Name = "accordionControlSeparator38";
            // 
            // grpSystem
            // 
            grpSystem.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmCompanies, accordionControlSeparator2, elmUsers, accordionControlSeparator3, elmRoles, accordionControlSeparator4, elmEmployees, accordionControlSeparator5, elmSigningRoles, accordionControlSeparator1, elmChangePassword, accordionControlSeparator41, elmCreateUpdate, accordionControlSeparator42, elmUpdateHistory, accordionControlSeparator43 });
            grpSystem.Expanded = true;
            grpSystem.Name = "grpSystem";
            grpSystem.Tag = 8;
            grpSystem.Text = "Sistem Yönetimi";
            // 
            // elmCompanies
            // 
            elmCompanies.Name = "elmCompanies";
            elmCompanies.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmCompanies.Tag = 8;
            elmCompanies.Text = "Şirket Ayarları";
            // 
            // accordionControlSeparator2
            // 
            accordionControlSeparator2.Name = "accordionControlSeparator2";
            // 
            // elmUsers
            // 
            elmUsers.Name = "elmUsers";
            elmUsers.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmUsers.Tag = 8;
            elmUsers.Text = "Kullanıcılar";
            // 
            // accordionControlSeparator3
            // 
            accordionControlSeparator3.Name = "accordionControlSeparator3";
            // 
            // elmRoles
            // 
            elmRoles.Name = "elmRoles";
            elmRoles.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmRoles.Tag = 8;
            elmRoles.Text = "Roller ve Yetkiler";
            // 
            // accordionControlSeparator4
            // 
            accordionControlSeparator4.Name = "accordionControlSeparator4";
            // 
            // elmEmployees
            // 
            elmEmployees.Name = "elmEmployees";
            elmEmployees.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmEmployees.Tag = 8;
            elmEmployees.Text = "Personel";
            // 
            // accordionControlSeparator5
            // 
            accordionControlSeparator5.Name = "accordionControlSeparator5";
            // 
            // elmSigningRoles
            // 
            elmSigningRoles.Name = "elmSigningRoles";
            elmSigningRoles.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmSigningRoles.Tag = 8;
            elmSigningRoles.Text = "Yetkili Görevler";
            // 
            // accordionControlSeparator1
            // 
            accordionControlSeparator1.Name = "accordionControlSeparator1";
            // 
            // elmChangePassword
            // 
            elmChangePassword.Name = "elmChangePassword";
            elmChangePassword.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmChangePassword.Tag = 8;
            elmChangePassword.Text = "Şifre Yenile";
            // 
            // accordionControlSeparator41
            // 
            accordionControlSeparator41.Name = "accordionControlSeparator41";
            // 
            // elmCreateUpdate
            // 
            elmCreateUpdate.Hint = "Kaynak koddan kurulum paketi üretir ve veritabanına yeni sürüm olarak yayınlar.";
            elmCreateUpdate.Name = "elmCreateUpdate";
            elmCreateUpdate.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmCreateUpdate.Tag = 8;
            elmCreateUpdate.Text = "Güncelleme Oluştur";
            // 
            // elmUpdateHistory
            // 
            elmUpdateHistory.Hint = "Yayınlanmış tüm sürümlerin listesi.";
            elmUpdateHistory.Name = "elmUpdateHistory";
            elmUpdateHistory.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmUpdateHistory.Tag = 8;
            elmUpdateHistory.Text = "Sürüm Geçmişi";
            // 
            // accordionControlSeparator39
            // 
            accordionControlSeparator39.Name = "accordionControlSeparator39";
            // 
            // grpMessages
            // 
            grpMessages.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmMessages, accordionControlSeparator29 });
            grpMessages.Name = "grpMessages";
            grpMessages.Tag = 9;
            grpMessages.Text = "Mesajlaşma";
            // 
            // elmMessages
            // 
            elmMessages.Name = "elmMessages";
            elmMessages.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmMessages.Tag = 9;
            elmMessages.Text = "Mesajlar";
            // 
            // accordionControlSeparator29
            // 
            accordionControlSeparator29.Name = "accordionControlSeparator29";
            // 
            // accordionControlSeparator40
            // 
            accordionControlSeparator40.Name = "accordionControlSeparator40";
            // 
            // elmExit
            // 
            elmExit.Name = "elmExit";
            elmExit.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmExit.Tag = 99;
            elmExit.Text = "Çıkış";
            // 
            // accordionControlSeparator30
            // 
            accordionControlSeparator30.Name = "accordionControlSeparator30";
            // 
            // xtraTabbedMdiManager
            // 
            xtraTabbedMdiManager.MdiParent = this;
            xtraTabbedMdiManager.UseFormIconAsPageImage = DevExpress.Utils.DefaultBoolean.True;
            // 
            // accordionControlSeparator42
            // 
            accordionControlSeparator42.Name = "accordionControlSeparator42";
            // 
            // accordionControlSeparator43
            // 
            accordionControlSeparator43.Name = "accordionControlSeparator43";
            //
            // grpDirectProcurement
            //
            grpDirectProcurement.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmDirectProcurementHome, accordionControlSeparator44, elmDirectProcurementFiles, accordionControlSeparator45, elmDirectProcurementNew, accordionControlSeparator46 });
            grpDirectProcurement.Expanded = false;
            grpDirectProcurement.Name = "grpDirectProcurement";
            grpDirectProcurement.Tag = 10;
            grpDirectProcurement.Text = "Doğrudan Temin";
            //
            // elmDirectProcurementHome
            //
            elmDirectProcurementHome.Name = "elmDirectProcurementHome";
            elmDirectProcurementHome.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmDirectProcurementHome.Tag = 10;
            elmDirectProcurementHome.Text = "Doğrudan Temin Ana Sayfa";
            //
            // accordionControlSeparator44
            //
            accordionControlSeparator44.Name = "accordionControlSeparator44";
            //
            // elmDirectProcurementFiles
            //
            elmDirectProcurementFiles.Name = "elmDirectProcurementFiles";
            elmDirectProcurementFiles.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmDirectProcurementFiles.Tag = 10;
            elmDirectProcurementFiles.Text = "Doğrudan Temin Listesi";
            //
            // accordionControlSeparator45
            //
            accordionControlSeparator45.Name = "accordionControlSeparator45";
            //
            // elmDirectProcurementNew
            //
            elmDirectProcurementNew.Name = "elmDirectProcurementNew";
            elmDirectProcurementNew.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmDirectProcurementNew.Tag = 10;
            elmDirectProcurementNew.Text = "Yeni Doğrudan Temin Girişi";
            //
            // accordionControlSeparator46
            //
            accordionControlSeparator46.Name = "accordionControlSeparator46";
            //
            // accordionControlSeparator47
            //
            accordionControlSeparator47.Name = "accordionControlSeparator47";
            //
            // RibbonMainForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1411, 758);
            Controls.Add(accordionControl);
            Controls.Add(ribbon);
            Controls.Add(ribbonStatusBar);
            IsMdiContainer = true;
            Margin = new Padding(3, 2, 3, 2);
            Name = "RibbonMainForm";
            Ribbon = ribbon;
            StatusBar = ribbonStatusBar;
            Text = "Maliyet Muhasebesi Otomasyonu";
            WindowState = FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)ribbon).EndInit();
            ((System.ComponentModel.ISupportInitialize)accordionControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)xtraTabbedMdiManager).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        private DevExpress.XtraBars.SkinRibbonGalleryBarItem skinRibbonGalleryBarItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonItemCompanyName;
        private DevExpress.XtraBars.BarButtonItem barButtonItemUserName;
        private DevExpress.XtraBars.BarButtonItem barButtonItemRoleName;
        private DevExpress.XtraBars.BarButtonItem barButtonItemLiveMessaging;
        private DevExpress.XtraBars.BarButtonItem barButtonItemLongDate;
        private DevExpress.XtraBars.BarButtonItem barButtonItemTime;
        private DevExpress.XtraBars.BarButtonItem barButtonItemExpTime;
        private DevExpress.XtraBars.BarStaticItem barStaticItemVersion;

        private DevExpress.XtraBars.Navigation.AccordionControl accordionControl;

        // Ana Sayfa
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmHome;

        // Stok Yönetimi (tanımlar)
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpStockMaster;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmProducts;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmUnitTypes;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmTaxRates;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmConsumptionUnits;

        // Stok İşlemleri
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpStockOperations;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmStockInput;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmStockOutput;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmConsumption;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmWorkshopTransfer;

        // Faturalar
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpInvoices;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmInvoices;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmInvoiceApproval;

        // Cari Yönetimi
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpCurrentAccounts;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmCustomers;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmSuppliers;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmCurrentAccountMovements;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmPayments;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmCurrentAccountBalance;

        // Maliyet & Üretim
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpCosting;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmCostSlips;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmRecipes;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmWorkshopAnalysis;

        // Muhasebe
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpAccounting;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmChartOfAccounts;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmCarryForwardOperations;

        // Raporlar
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpReports;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmStockMovements;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmPriceStockList;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmWorkshopStockReport;

        // Sistem Yönetimi
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpSystem;
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpMessages;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmMessages;
        // Doğrudan Temin modülü
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpDirectProcurement;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmDirectProcurementHome;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmDirectProcurementFiles;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmDirectProcurementNew;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmCompanies;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmUsers;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmChangePassword;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmRoles;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmEmployees;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmSigningRoles;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmCreateUpdate;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmUpdateHistory;

        private DevExpress.XtraBars.Navigation.AccordionControlElement elmExit;

        private DevExpress.XtraTabbedMdi.XtraTabbedMdiManager xtraTabbedMdiManager;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator2;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator3;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator4;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator5;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator1;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator31;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator6;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator7;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator8;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator9;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator32;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator10;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator11;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator12;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator13;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator33;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator14;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator15;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator34;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator16;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator17;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator18;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator19;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator20;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator35;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator21;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator22;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator23;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator36;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator24;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator25;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator37;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator26;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator27;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator28;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator38;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator39;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator29;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator40;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator30;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator41;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator42;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator43;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator44;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator45;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator46;
        private DevExpress.XtraBars.Navigation.AccordionControlSeparator accordionControlSeparator47;
    }
}
