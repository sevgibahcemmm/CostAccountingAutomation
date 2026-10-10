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
            barButtonItemLiveMessaging = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemLongDate = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemTime = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemExpTime = new DevExpress.XtraBars.BarButtonItem();
            barStaticItemVersion = new DevExpress.XtraBars.BarStaticItem();
            ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            skinRibbonGalleryBarItem1 = new DevExpress.XtraBars.SkinRibbonGalleryBarItem();
            accordionControl = new DevExpress.XtraBars.Navigation.AccordionControl();
            elmHome = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpStockMaster = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmProducts = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmUnitTypes = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmTaxRates = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmConsumptionUnits = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpStockOperations = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmStockInput = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmStockOutput = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmConsumption = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmWorkshopTransfer = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpInvoices = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmInvoices = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmInvoiceApproval = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpCurrentAccounts = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmCustomers = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmSuppliers = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmCurrentAccountMovements = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmPayments = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmCurrentAccountBalance = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpCosting = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmCostSlips = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmRecipes = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmWorkshopAnalysis = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpAccounting = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmChartOfAccounts = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmCarryForwardOperations = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpReports = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmStockMovements = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmPriceStockList = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmWorkshopStockReport = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpSystem = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmCompanies = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmUsers = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmChangePassword = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmRoles = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmEmployees = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmSigningRoles = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpMessages = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmMessages = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmExit = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            xtraTabbedMdiManager = new DevExpress.XtraTabbedMdi.XtraTabbedMdiManager(components);
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
            // barStaticItemVersion
            // 
            barStaticItemVersion.Caption = " ";
            barStaticItemVersion.Id = 8;
            barStaticItemVersion.Name = "barStaticItemVersion";
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
            accordionControl.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmHome, grpStockMaster, grpStockOperations, grpInvoices, grpCurrentAccounts, grpCosting, grpAccounting, grpReports, grpSystem, grpMessages, elmExit });
            accordionControl.Location = new Point(0, 58);
            accordionControl.Margin = new Padding(3, 2, 3, 2);
            accordionControl.Name = "accordionControl";
            accordionControl.OptionsMinimizing.State = DevExpress.XtraBars.Navigation.AccordionControlState.Minimized;
            accordionControl.Size = new Size(48, 676);
            accordionControl.TabIndex = 2;
            accordionControl.ViewType = DevExpress.XtraBars.Navigation.AccordionControlViewType.HamburgerMenu;
            // 
            // elmHome
            // 
            elmHome.Expanded = true;
            elmHome.Name = "elmHome";
            elmHome.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmHome.Tag = 0;
            elmHome.Text = "Ana Sayfa";
            // 
            // grpStockMaster
            // 
            grpStockMaster.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmProducts, elmUnitTypes, elmTaxRates, elmConsumptionUnits });
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
            // elmUnitTypes
            // 
            elmUnitTypes.Name = "elmUnitTypes";
            elmUnitTypes.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmUnitTypes.Tag = 1;
            elmUnitTypes.Text = "Birim Cinsleri";
            // 
            // elmTaxRates
            // 
            elmTaxRates.Name = "elmTaxRates";
            elmTaxRates.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmTaxRates.Tag = 1;
            elmTaxRates.Text = "KDV Oranları";
            // 
            // elmConsumptionUnits
            // 
            elmConsumptionUnits.Name = "elmConsumptionUnits";
            elmConsumptionUnits.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmConsumptionUnits.Tag = 1;
            elmConsumptionUnits.Text = "Tüketim Birimleri";
            // 
            // grpStockOperations
            // 
            grpStockOperations.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmStockInput, elmStockOutput, elmConsumption, elmWorkshopTransfer });
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
            // elmStockOutput
            // 
            elmStockOutput.Name = "elmStockOutput";
            elmStockOutput.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmStockOutput.Tag = 2;
            elmStockOutput.Text = "Stok Çıkışı";
            // 
            // elmConsumption
            // 
            elmConsumption.Name = "elmConsumption";
            elmConsumption.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmConsumption.Tag = 2;
            elmConsumption.Text = "Tüketim";
            // 
            // elmWorkshopTransfer
            // 
            elmWorkshopTransfer.Name = "elmWorkshopTransfer";
            elmWorkshopTransfer.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmWorkshopTransfer.Tag = 2;
            elmWorkshopTransfer.Text = "Atölye Transferi";
            // 
            // grpInvoices
            // 
            grpInvoices.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmInvoices, elmInvoiceApproval });
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
            // elmInvoiceApproval
            // 
            elmInvoiceApproval.Name = "elmInvoiceApproval";
            elmInvoiceApproval.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmInvoiceApproval.Tag = 3;
            elmInvoiceApproval.Text = "Fatura Onaylama";
            // 
            // grpCurrentAccounts
            // 
            grpCurrentAccounts.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmCustomers, elmSuppliers, elmCurrentAccountMovements, elmPayments, elmCurrentAccountBalance });
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
            // elmSuppliers
            // 
            elmSuppliers.Name = "elmSuppliers";
            elmSuppliers.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmSuppliers.Tag = 4;
            elmSuppliers.Text = "Tedarikçiler";
            // 
            // elmCurrentAccountMovements
            // 
            elmCurrentAccountMovements.Name = "elmCurrentAccountMovements";
            elmCurrentAccountMovements.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmCurrentAccountMovements.Tag = 4;
            elmCurrentAccountMovements.Text = "Cari Hareketler";
            // 
            // elmPayments
            // 
            elmPayments.Name = "elmPayments";
            elmPayments.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmPayments.Tag = 4;
            elmPayments.Text = "Ödeme / Tahsilat";
            // 
            // elmCurrentAccountBalance
            // 
            elmCurrentAccountBalance.Name = "elmCurrentAccountBalance";
            elmCurrentAccountBalance.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmCurrentAccountBalance.Tag = 4;
            elmCurrentAccountBalance.Text = "Cari Borç/Alacak Özeti";
            // 
            // grpCosting
            // 
            grpCosting.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmCostSlips, elmRecipes, elmWorkshopAnalysis });
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
            // elmRecipes
            // 
            elmRecipes.Name = "elmRecipes";
            elmRecipes.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmRecipes.Tag = 5;
            elmRecipes.Text = "Reçeteler";
            // 
            // elmWorkshopAnalysis
            // 
            elmWorkshopAnalysis.Name = "elmWorkshopAnalysis";
            elmWorkshopAnalysis.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmWorkshopAnalysis.Tag = 5;
            elmWorkshopAnalysis.Text = "Atölye Gelir/Gider Analizi";
            // 
            // grpAccounting
            // 
            grpAccounting.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmChartOfAccounts, elmCarryForwardOperations });
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
            // elmCarryForwardOperations
            // 
            elmCarryForwardOperations.Name = "elmCarryForwardOperations";
            elmCarryForwardOperations.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmCarryForwardOperations.Tag = 6;
            elmCarryForwardOperations.Text = "Devir İşlemleri";
            // 
            // grpReports
            // 
            grpReports.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmStockMovements, elmPriceStockList, elmWorkshopStockReport });
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
            // elmPriceStockList
            // 
            elmPriceStockList.Name = "elmPriceStockList";
            elmPriceStockList.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmPriceStockList.Tag = 7;
            elmPriceStockList.Text = "Fiyat & Stok Listesi";
            // 
            // elmWorkshopStockReport
            // 
            elmWorkshopStockReport.Name = "elmWorkshopStockReport";
            elmWorkshopStockReport.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmWorkshopStockReport.Tag = 7;
            elmWorkshopStockReport.Text = "Atölye Stok Raporu";
            // 
            // grpSystem
            // 
            grpSystem.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmCompanies, elmUsers, elmChangePassword, elmRoles, elmEmployees, elmSigningRoles });
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
            // elmUsers
            // 
            elmUsers.Name = "elmUsers";
            elmUsers.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmUsers.Tag = 8;
            elmUsers.Text = "Kullanıcılar";
            // 
            // elmChangePassword
            // 
            elmChangePassword.Name = "elmChangePassword";
            elmChangePassword.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmChangePassword.Tag = 8;
            elmChangePassword.Text = "Şifre Yenile";
            // 
            // elmRoles
            // 
            elmRoles.Name = "elmRoles";
            elmRoles.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmRoles.Tag = 8;
            elmRoles.Text = "Roller ve Yetkiler";
            // 
            // elmEmployees
            // 
            elmEmployees.Name = "elmEmployees";
            elmEmployees.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmEmployees.Tag = 8;
            elmEmployees.Text = "Personel";
            // 
            // elmSigningRoles
            // 
            elmSigningRoles.Name = "elmSigningRoles";
            elmSigningRoles.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmSigningRoles.Tag = 8;
            elmSigningRoles.Text = "Yetkili Görevler";
            // 
            // grpMessages
            // 
            grpMessages.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmMessages });
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
            // elmExit
            // 
            elmExit.Name = "elmExit";
            elmExit.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmExit.Tag = 99;
            elmExit.Text = "Çıkış";
            // 
            // xtraTabbedMdiManager
            // 
            xtraTabbedMdiManager.MdiParent = this;
            xtraTabbedMdiManager.UseFormIconAsPageImage = DevExpress.Utils.DefaultBoolean.True;
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
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmCompanies;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmUsers;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmChangePassword;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmRoles;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmEmployees;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmSigningRoles;

        private DevExpress.XtraBars.Navigation.AccordionControlElement elmExit;

        private DevExpress.XtraTabbedMdi.XtraTabbedMdiManager xtraTabbedMdiManager;
    }
}
