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
            barButtonItemLongDate = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemTime = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemExpTime = new DevExpress.XtraBars.BarButtonItem();
            ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            skinRibbonGalleryBarItem1 = new DevExpress.XtraBars.SkinRibbonGalleryBarItem();
            accordionControl = new DevExpress.XtraBars.Navigation.AccordionControl();
            elmAnaSayfa = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpMaliyet = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmMaliyetMerkezleri = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmMaliyetHesaplama = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmMaliyetRaporlari = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpStok = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmUrunler = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmBirimCinsleri = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmKdvOranlari = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmStokGirisi = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmStokCikisi = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpSatinAlma = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmSatinAlmaSiparisleri = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmSatinAlmaFaturalari = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpSatis = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmSatisSiparisleri = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmSatisFaturalari = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpCari = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmMusteriler = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmTedarikciler = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpMuhasebe = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmHesapPlani = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmOdemeTahsilat = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmBankaIslemleri = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmMizan = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpFinans = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmGelirGider = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmKasa = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmRaporlar = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            grpSistem = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmSirketAyarlari = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmKullanicilar = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmRoller = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            elmCikis = new DevExpress.XtraBars.Navigation.AccordionControlElement();
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
            ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] { skinRibbonGalleryBarItem1, ribbon.ExpandCollapseItem, barButtonItemCompanyName, barButtonItemUserName, barButtonItemRoleName, barButtonItemLongDate, barButtonItemTime, barButtonItemExpTime });
            ribbon.Location = new Point(0, 0);
            ribbon.Margin = new Padding(3, 2, 3, 2);
            ribbon.MaxItemId = 8;
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
            accordionControl.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmAnaSayfa, grpMaliyet, grpStok, grpSatinAlma, grpSatis, grpCari, grpMuhasebe, grpFinans, grpSistem, elmCikis });
            accordionControl.Location = new Point(0, 58);
            accordionControl.Margin = new Padding(3, 2, 3, 2);
            accordionControl.Name = "accordionControl";
            accordionControl.Size = new Size(223, 676);
            accordionControl.TabIndex = 2;
            accordionControl.ViewType = DevExpress.XtraBars.Navigation.AccordionControlViewType.HamburgerMenu;
            // 
            // elmAnaSayfa
            // 
            elmAnaSayfa.Expanded = true;
            elmAnaSayfa.Name = "elmAnaSayfa";
            elmAnaSayfa.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmAnaSayfa.Tag = 0;
            elmAnaSayfa.Text = "Ana Sayfa";
            // 
            // grpMaliyet
            // 
            grpMaliyet.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmMaliyetMerkezleri, elmMaliyetHesaplama, elmMaliyetRaporlari });
            grpMaliyet.Expanded = true;
            grpMaliyet.Name = "grpMaliyet";
            grpMaliyet.Tag = 1;
            grpMaliyet.Text = "Maliyet Yönetimi";
            // 
            // elmMaliyetMerkezleri
            // 
            elmMaliyetMerkezleri.Name = "elmMaliyetMerkezleri";
            elmMaliyetMerkezleri.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmMaliyetMerkezleri.Tag = 1;
            elmMaliyetMerkezleri.Text = "Maliyet Merkezleri";
            // 
            // elmMaliyetHesaplama
            // 
            elmMaliyetHesaplama.Name = "elmMaliyetHesaplama";
            elmMaliyetHesaplama.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmMaliyetHesaplama.Tag = 1;
            elmMaliyetHesaplama.Text = "Maliyet Hesaplama";
            // 
            // elmMaliyetRaporlari
            // 
            elmMaliyetRaporlari.Name = "elmMaliyetRaporlari";
            elmMaliyetRaporlari.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmMaliyetRaporlari.Tag = 1;
            elmMaliyetRaporlari.Text = "Maliyet Raporları";
            // 
            // grpStok
            // 
            grpStok.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmUrunler, elmBirimCinsleri, elmKdvOranlari, elmStokGirisi, elmStokCikisi });
            grpStok.Expanded = true;
            grpStok.Name = "grpStok";
            grpStok.Tag = 2;
            grpStok.Text = "Stok Yönetimi";
            // 
            // elmUrunler
            // 
            elmUrunler.Name = "elmUrunler";
            elmUrunler.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmUrunler.Tag = 2;
            elmUrunler.Text = "Ürünler";
            // 
            // elmBirimCinsleri
            // 
            elmBirimCinsleri.Name = "elmBirimCinsleri";
            elmBirimCinsleri.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmBirimCinsleri.Tag = 2;
            elmBirimCinsleri.Text = "Birim Cinsleri";
            // 
            // elmKdvOranlari
            // 
            elmKdvOranlari.Name = "elmKdvOranlari";
            elmKdvOranlari.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmKdvOranlari.Tag = 2;
            elmKdvOranlari.Text = "KDV Oranları";
            // 
            // elmStokGirisi
            // 
            elmStokGirisi.Name = "elmStokGirisi";
            elmStokGirisi.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmStokGirisi.Tag = 2;
            elmStokGirisi.Text = "Stok Girişi";
            // 
            // elmStokCikisi
            // 
            elmStokCikisi.Name = "elmStokCikisi";
            elmStokCikisi.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmStokCikisi.Tag = 2;
            elmStokCikisi.Text = "Stok Çıkışı";
            // 
            // grpSatinAlma
            // 
            grpSatinAlma.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmSatinAlmaSiparisleri, elmSatinAlmaFaturalari });
            grpSatinAlma.Expanded = true;
            grpSatinAlma.Name = "grpSatinAlma";
            grpSatinAlma.Tag = 3;
            grpSatinAlma.Text = "Satın Alma";
            // 
            // elmSatinAlmaSiparisleri
            // 
            elmSatinAlmaSiparisleri.Name = "elmSatinAlmaSiparisleri";
            elmSatinAlmaSiparisleri.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmSatinAlmaSiparisleri.Tag = 3;
            elmSatinAlmaSiparisleri.Text = "Satın Alma Siparişleri";
            // 
            // elmSatinAlmaFaturalari
            // 
            elmSatinAlmaFaturalari.Name = "elmSatinAlmaFaturalari";
            elmSatinAlmaFaturalari.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmSatinAlmaFaturalari.Tag = 3;
            elmSatinAlmaFaturalari.Text = "Satın Alma Faturaları";
            // 
            // grpSatis
            // 
            grpSatis.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmSatisSiparisleri, elmSatisFaturalari });
            grpSatis.Expanded = true;
            grpSatis.Name = "grpSatis";
            grpSatis.Tag = 4;
            grpSatis.Text = "Satış";
            // 
            // elmSatisSiparisleri
            // 
            elmSatisSiparisleri.Name = "elmSatisSiparisleri";
            elmSatisSiparisleri.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmSatisSiparisleri.Tag = 4;
            elmSatisSiparisleri.Text = "Satış Siparişleri";
            // 
            // elmSatisFaturalari
            // 
            elmSatisFaturalari.Name = "elmSatisFaturalari";
            elmSatisFaturalari.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmSatisFaturalari.Tag = 4;
            elmSatisFaturalari.Text = "Satış Faturaları";
            // 
            // grpCari
            // 
            grpCari.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmMusteriler, elmTedarikciler });
            grpCari.Expanded = true;
            grpCari.Name = "grpCari";
            grpCari.Tag = 5;
            grpCari.Text = "Cari Yönetimi";
            // 
            // elmMusteriler
            // 
            elmMusteriler.Name = "elmMusteriler";
            elmMusteriler.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmMusteriler.Tag = 5;
            elmMusteriler.Text = "Müşteriler";
            // 
            // elmTedarikciler
            // 
            elmTedarikciler.Name = "elmTedarikciler";
            elmTedarikciler.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmTedarikciler.Tag = 5;
            elmTedarikciler.Text = "Tedarikçiler";
            // 
            // grpMuhasebe
            // 
            grpMuhasebe.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmHesapPlani, elmOdemeTahsilat, elmBankaIslemleri, elmMizan });
            grpMuhasebe.Expanded = true;
            grpMuhasebe.Name = "grpMuhasebe";
            grpMuhasebe.Tag = 6;
            grpMuhasebe.Text = "Muhasebe";
            // 
            // elmHesapPlani
            // 
            elmHesapPlani.Name = "elmHesapPlani";
            elmHesapPlani.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmHesapPlani.Tag = 6;
            elmHesapPlani.Text = "Hesap Planı";
            // 
            // elmOdemeTahsilat
            // 
            elmOdemeTahsilat.Name = "elmOdemeTahsilat";
            elmOdemeTahsilat.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmOdemeTahsilat.Tag = 6;
            elmOdemeTahsilat.Text = "Ödeme / Tahsilat";
            // 
            // elmBankaIslemleri
            // 
            elmBankaIslemleri.Name = "elmBankaIslemleri";
            elmBankaIslemleri.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmBankaIslemleri.Tag = 6;
            elmBankaIslemleri.Text = "Banka İşlemleri";
            // 
            // elmMizan
            // 
            elmMizan.Name = "elmMizan";
            elmMizan.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmMizan.Tag = 6;
            elmMizan.Text = "Mizan";
            // 
            // grpFinans
            // 
            grpFinans.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmGelirGider, elmKasa, elmRaporlar });
            grpFinans.Expanded = true;
            grpFinans.Name = "grpFinans";
            grpFinans.Tag = 7;
            grpFinans.Text = "Finans";
            // 
            // elmGelirGider
            // 
            elmGelirGider.Name = "elmGelirGider";
            elmGelirGider.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmGelirGider.Tag = 7;
            elmGelirGider.Text = "Gelir / Gider";
            // 
            // elmKasa
            // 
            elmKasa.Name = "elmKasa";
            elmKasa.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmKasa.Tag = 7;
            elmKasa.Text = "Kasa";
            // 
            // elmRaporlar
            // 
            elmRaporlar.Name = "elmRaporlar";
            elmRaporlar.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmRaporlar.Tag = 7;
            elmRaporlar.Text = "Finansal Raporlar";
            // 
            // grpSistem
            // 
            grpSistem.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] { elmSirketAyarlari, elmKullanicilar, elmRoller });
            grpSistem.Expanded = true;
            grpSistem.Name = "grpSistem";
            grpSistem.Tag = 8;
            grpSistem.Text = "Sistem";
            // 
            // elmSirketAyarlari
            // 
            elmSirketAyarlari.Name = "elmSirketAyarlari";
            elmSirketAyarlari.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmSirketAyarlari.Tag = 8;
            elmSirketAyarlari.Text = "Şirket Ayarları";
            // 
            // elmKullanicilar
            // 
            elmKullanicilar.Name = "elmKullanicilar";
            elmKullanicilar.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmKullanicilar.Tag = 8;
            elmKullanicilar.Text = "Kullanıcılar";
            // 
            // elmRoller
            // 
            elmRoller.Name = "elmRoller";
            elmRoller.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmRoller.Tag = 8;
            elmRoller.Text = "Roller ve Yetkiler";
            // 
            // elmCikis
            // 
            elmCikis.Name = "elmCikis";
            elmCikis.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            elmCikis.Tag = 99;
            elmCikis.Text = "Çıkış";
            // 
            // RibbonMainForm
            // 
            AllowFormGlass = DevExpress.Utils.DefaultBoolean.False;
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
            Text = "Cost Accounting Automation V.01";
            WindowState = FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)ribbon).EndInit();
            ((System.ComponentModel.ISupportInitialize)accordionControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)xtraTabbedMdiManager).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private DevExpress.XtraBars.Navigation.AccordionControl accordionControl;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmAnaSayfa;
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpMaliyet;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmMaliyetMerkezleri;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmMaliyetHesaplama;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmMaliyetRaporlari;
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpStok;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmUrunler;
private DevExpress.XtraBars.Navigation.AccordionControlElement elmBirimCinsleri;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmKdvOranlari;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmStokGirisi;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmStokCikisi;
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpSatinAlma;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmSatinAlmaSiparisleri;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmSatinAlmaFaturalari;
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpSatis;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmSatisSiparisleri;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmSatisFaturalari;
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpCari;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmMusteriler;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmTedarikciler;
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpMuhasebe;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmHesapPlani;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmOdemeTahsilat;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmBankaIslemleri;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmMizan;
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpFinans;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmGelirGider;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmKasa;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmRaporlar;
        private DevExpress.XtraBars.Navigation.AccordionControlElement grpSistem;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmSirketAyarlari;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmKullanicilar;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmRoller;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elmCikis;
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        private DevExpress.XtraBars.SkinRibbonGalleryBarItem skinRibbonGalleryBarItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonItemCompanyName;
        private DevExpress.XtraBars.BarButtonItem barButtonItemUserName;
        private DevExpress.XtraBars.BarButtonItem barButtonItemRoleName;
        private DevExpress.XtraBars.BarButtonItem barButtonItemLongDate;
        private DevExpress.XtraBars.BarButtonItem barButtonItemTime;
        private DevExpress.XtraBars.BarButtonItem barButtonItemExpTime;
        private DevExpress.XtraTabbedMdi.XtraTabbedMdiManager xtraTabbedMdiManager;
    }
}