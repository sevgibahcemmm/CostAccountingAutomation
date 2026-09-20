using Cost.Accounting.Automation.WinFormsApp.Reports;
using DevExpress.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.Reports
{
    public partial class MovableAssetTransactionSlipReport : DevExpress.XtraReports.UI.XtraReport
    {
        private const float LayoutWidth = 731.329F;
        private const float RowHeight = 15.5F;

        private readonly DXFont _small = new("Arial", 6.4F);
        private readonly DXFont _smallBold = new("Arial", 6.4F, DXFontStyle.Bold);
        private readonly DXFont _title = new("Arial", 12.8F, DXFontStyle.Bold);
        private readonly DXFont _section = new("Arial", 9.6F, DXFontStyle.Bold);

        public MovableAssetTransactionSlipReport()
        {
            InitializeComponent();
        }

        public MovableAssetTransactionSlipReport(MovableAssetTransactionSlipData data)
            : this()
        {
            ApplyData(data);
        }

        private void BuildLayout()
        {
            TopMargin = new TopMarginBand { HeightF = 22F, Name = "TopMargin" };
            ReportHeader = new ReportHeaderBand { HeightF = 22F, Name = "ReportHeader" };
            PageHeader = new PageHeaderBand { HeightF = 46F, Name = "PageHeader" };
            GroupHeader = new GroupHeaderBand { HeightF = 0F, Name = "GroupHeader" };
            Detail = new DetailBand { HeightF = RowHeight, Name = "Detail" };
            GroupFooter = new GroupFooterBand { HeightF = RowHeight, Name = "GroupFooter" };
            ReportFooter = new ReportFooterBand { HeightF = 1F, Name = "ReportFooter" };
            BottomMargin = new BottomMarginBand { HeightF = 23F, Name = "BottomMargin" };

            ReportHeader.Controls.Add(MakeTitle());
            PageHeader.Controls.Add(BuildColumnHeaderTable());
            Detail.Controls.Add(BuildDetailTable());
            GroupFooter.Controls.Add(BuildGroupFooterTable());
            BottomMargin.Controls.Add(MakeFormNoLabel());

            GroupHeader.GroupFields.Add(
                new GroupField(nameof(MovableAssetTransactionSlipRow.GroupCode), XRColumnSortOrder.Ascending));

            ReportHeader.SubBands.AddRange(
            [
                MakeSubBand("subFisNo", 19F, BuildFisNoTable()),
                MakeSubBand("subParties", 49F, BuildPartiesTable()),
                MakeSubBand("subDocumentRef", 33F, BuildDocumentRefTable()),
                MakeSubBand("subOperation", 33F, BuildOperationTable()),
                MakeSubBand("subUnits", 66F, BuildUnitsTable())
            ]);

            ReportFooter.SubBands.AddRange(
            [
                MakeSubBand("subSummary", 17F, MakeSummaryLabel()),
                MakeSubBand("subSignatures", 150F, BuildSignatureTable())
            ]);

            Bands.AddRange(
            [
                TopMargin,
                ReportHeader,
                PageHeader,
                GroupHeader,
                Detail,
                GroupFooter,
                ReportFooter,
                BottomMargin
            ]);

            Font = new DXFont("Arial", 7.8F);
            Margins = new DXMargins(47.72F, 47.72F, 22F, 23F);
            PageHeightF = 1169.291F;
            PageWidthF = 826.7717F;
            PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A4;
            Version = "25.2";

            CreateParameters();
        }

        private void CreateParameters()
        {
            string[] names =
            [
                "FisSiraNo",
                "Tarih",
                "IlIlceAdi",
                "IlIlceKodu",
                "HarcamaBirimiAdi",
                "HarcamaBirimiKodu",
                "AmbarAdi",
                "AmbarKodu",
                "MuhasebeBirimiAdi",
                "MuhasebeBirimiKodu",
                "DayanakTarihi",
                "DayanakKodu",
                "IslemCesidi",
                "NeredenGeldigi",
                "KimeVerildigi",
                "NereyeVerildigi",
                "Ozet"
            ];

            foreach (string name in names)
            {
                Parameters.Add(new Parameter
                {
                    Name = name,
                    Type = typeof(string),
                    Visible = false,
                    AllowNull = true,
                    Value = string.Empty
                });
            }
        }

        private void ApplyData(MovableAssetTransactionSlipData data)
        {
            SetParameter("FisSiraNo", data.DocumentNumber);
            SetParameter("Tarih", data.Date.ToString("dd.MM.yyyy"));
            SetParameter("IlIlceAdi", data.IlIlceAdi);
            SetParameter("IlIlceKodu", data.IlIlceKodu);
            SetParameter("HarcamaBirimiAdi", data.HarcamaBirimiAdi);
            SetParameter("HarcamaBirimiKodu", data.HarcamaBirimiKodu);
            SetParameter("AmbarAdi", data.AmbarAdi);
            SetParameter("AmbarKodu", data.AmbarKodu);
            SetParameter("MuhasebeBirimiAdi", data.MuhasebeBirimiAdi);
            SetParameter("MuhasebeBirimiKodu", data.MuhasebeBirimiKodu);
            SetParameter("DayanakTarihi", data.DayanakTarihi?.ToString("dd.MM.yyyy") ?? string.Empty);
            SetParameter("DayanakKodu", data.DayanakKodu);
            SetParameter("IslemCesidi", data.IslemCesidi);
            SetParameter("NeredenGeldigi", data.NeredenGeldigi);
            SetParameter("KimeVerildigi", data.KimeVerildigi);
            SetParameter("NereyeVerildigi", data.NereyeVerildigi);
            SetParameter(
                "Ozet",
                $"Yukarıda gösterilen {data.DetailLineCount} kalem, toplam {data.TotalQuantity:N0} adet taşınırın");

            BuildDepotTotals(data);

            DataSource = data.Rows;
        }

        private void SetParameter(string name, string? value)
        {
            Parameters[name].Value = value ?? string.Empty;
        }

        private void BuildDepotTotals(MovableAssetTransactionSlipData data)
        {
            ReportFooter.Controls.Clear();

            List<XRTableRow> rows = [];

            foreach (MovableAssetTransactionSlipDepotTotal depot in data.DepotTotals)
            {
                string label = string.IsNullOrWhiteSpace(depot.DepoAdi)
                    ? "DEPO TOPLAMI"
                    : $"{depot.DepoAdi} DEPO TOPLAMI";

                rows.Add(MakeRow(
                    1D,
                    MakeCell(5D, label, bold: true, header: true),
                    MakeCell(1D, depot.Miktari.ToString("N2"), bold: true, header: true, align: TextAlignment.MiddleRight),
                    MakeCell(1D, string.Empty, bold: true, header: true),
                    MakeCell(1D, depot.Tutari.ToString("N2"), bold: true, header: true, align: TextAlignment.MiddleRight)));
            }

            rows.Add(MakeRow(
                1D,
                MakeCell(5D, "GENEL TOPLAM", bold: true, header: true),
                MakeCell(1D, data.GrandMiktar.ToString("N2"), bold: true, header: true, align: TextAlignment.MiddleRight),
                MakeCell(1D, string.Empty, bold: true, header: true),
                MakeCell(1D, data.GrandTutar.ToString("N2"), bold: true, header: true, align: TextAlignment.MiddleRight)));

            float height = RowHeight * rows.Count;
            XRTable table = MakeTable(height, [.. rows]);

            ReportFooter.Controls.Add(table);
            ReportFooter.HeightF = height;
        }

        private XRLabel MakeTitle()
        {
            XRLabel label = new()
            {
                Text = "T A Ş I N I R   İ Ş L E M   F İ Ş İ",
                Multiline = true,
                CanGrow = true,
                Font = _title,
                TextAlignment = TextAlignment.MiddleCenter,
                WidthF = LayoutWidth,
                HeightF = 22F,
                LocationF = new System.Drawing.PointF(0F, 0F)
            };
            label.StylePriority.UseFont = false;
            label.StylePriority.UseTextAlignment = false;
            return label;
        }

        private XRLabel MakeSummaryLabel()
        {
            XRLabel label = new()
            {
                Multiline = true,
                CanGrow = true,
                Font = _small,
                TextAlignment = TextAlignment.MiddleLeft,
                WidthF = LayoutWidth,
                HeightF = 16F,
                LocationF = new System.Drawing.PointF(0F, 0F)
            };
            label.StylePriority.UseFont = false;
            label.StylePriority.UseTextAlignment = false;
            label.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "?Ozet"));
            return label;
        }

        private XRLabel MakeFormNoLabel()
        {
            XRLabel label = new()
            {
                Text = "T.M.Y. Örnek No:5",
                Multiline = true,
                Font = _small,
                TextAlignment = TextAlignment.MiddleLeft,
                WidthF = 186.878F,
                HeightF = 23F,
                LocationF = new System.Drawing.PointF(0F, 0F)
            };
            label.StylePriority.UseFont = false;
            label.StylePriority.UseTextAlignment = false;
            return label;
        }

        private XRTable BuildFisNoTable()
        {
            return MakeTable(
                RowHeight,
                MakeRow(
                    1D,
                    MakeCell(1D, bold: true, align: TextAlignment.MiddleLeft, expression: "'FİŞ SIRA NO: ' + ?FisSiraNo"),
                    MakeCell(1D, bold: false, align: TextAlignment.MiddleRight, expression: "'Tarih: ' + ?Tarih")));
        }

        private XRTable BuildPartiesTable()
        {
            return MakeTable(
                RowHeight * 3F,
                MakeRow(1D, RowLabel("İL VE İLÇENİN (1)"), NameCaption(), NameValue("IlIlceAdi"), CodeCaption(), CodeValue("IlIlceKodu")),
                MakeRow(1D, RowLabel("HARCAMA BİRİMİNİN (2)"), NameCaption(), NameValue("HarcamaBirimiAdi"), CodeCaption(), CodeValue("HarcamaBirimiKodu")),
                MakeRow(1D, RowLabel("MUHASEBE BİRİMİNİN (3)"), NameCaption(), NameValue("MuhasebeBirimiAdi"), CodeCaption(), CodeValue("MuhasebeBirimiKodu")));
        }

        private XRTable BuildDocumentRefTable()
        {
            return MakeTable(
                RowHeight * 2F,
                MakeRow(1D, RowLabel("MUAYENE VE KABUL KOMİSYONU TUTANAĞININ (4)"), DateCaption(), MakeCell(2.052D), CodeCaption(), MakeCell(0.625D)),
                MakeRow(1D, RowLabel("DAYANAĞI BELGENİN (5)"), DateCaption(), NameValue("DayanakTarihi"), CodeCaption(), CodeValue("DayanakKodu")));
        }

        private XRTable BuildOperationTable()
        {
            return MakeTable(
                RowHeight * 2F,
                MakeRow(
                    1D,
                    MakeCell(0.975D, "İŞLEM ÇEŞİDİ (6)", bold: true, header: true, align: TextAlignment.MiddleCenter),
                    MakeCell(1.769D, "NEREDEN GELDİĞİ (7)", bold: true, header: true, align: TextAlignment.MiddleCenter),
                    MakeCell(1.256D, "KİME VERİLDİĞİ (8)", bold: true, header: true, align: TextAlignment.MiddleCenter),
                    MakeCell(1D, "NEREYE VERİLDİĞİ (9)", bold: true, header: true, align: TextAlignment.MiddleCenter)),
                MakeRow(
                    1D,
                    MakeCell(0.975D, expression: "?IslemCesidi", align: TextAlignment.MiddleCenter),
                    MakeCell(1.769D, expression: "?NeredenGeldigi", align: TextAlignment.MiddleCenter),
                    MakeCell(1.256D, expression: "?KimeVerildigi", align: TextAlignment.MiddleCenter),
                    MakeCell(1D, expression: "?NereyeVerildigi", align: TextAlignment.MiddleCenter)));
        }

        private XRTable BuildUnitsTable()
        {
            return MakeTable(
                RowHeight * 4F,
                MakeRow(1.4D, MakeCell(5D, "BİRİMLER VE AMBARLAR ARASI TAŞINIR HAREKETLERİNDE", bold: true, header: true, align: TextAlignment.MiddleCenter)),
                MakeRow(1D, RowLabel("GÖNDERİLEN HARCAMA BİRİMİ (10)"), NameCaption(), NameValue("HarcamaBirimiAdi"), CodeCaption(), CodeValue("HarcamaBirimiKodu")),
                MakeRow(1D, RowLabel("GÖNDERİLEN TAŞINIR AMBARI (11)"), NameCaption(), NameValue("AmbarAdi"), CodeCaption(), CodeValue("AmbarKodu")),
                MakeRow(1D, RowLabel("MUHASEBE BİRİMİ (12)"), NameCaption(), NameValue("MuhasebeBirimiAdi"), CodeCaption(), CodeValue("MuhasebeBirimiKodu")));
        }

        private XRTable BuildColumnHeaderTable()
        {
            return MakeTable(
                46F,
                MakeRow(
                    1.2D,
                    MakeCell(8D, "T   A    Ş   I   N   I    R    I    N", bold: true, header: true, align: TextAlignment.MiddleCenter, font: _section)),
                MakeRow(
                    1.4D,
                    MakeCell(0.41099476439790572D, "SIRA NO", bold: true, header: true, align: TextAlignment.MiddleCenter),
                    MakeCell(1.1487780665852014D, "KODU\r\n(13)", bold: true, header: true, align: TextAlignment.MiddleCenter),
                    MakeCell(0.77486934961448806D, "BARKODU\r\n(14)", bold: true, header: true, align: TextAlignment.MiddleCenter),
                    MakeCell(2.0554101754233476D, "ADI", bold: true, header: true, align: TextAlignment.MiddleCenter),
                    MakeCell(0.60994764397905765D, "ÖLÇÜ BİRİMİ", bold: true, header: true, align: TextAlignment.MiddleCenter),
                    MakeCell(1D, "MİKTARI", bold: true, header: true, align: TextAlignment.MiddleCenter),
                    MakeCell(1D, "BİRİM FİYATI", bold: true, header: true, align: TextAlignment.MiddleCenter),
                    MakeCell(1D, "TUTARI", bold: true, header: true, align: TextAlignment.MiddleCenter)));
        }

        private XRTable BuildDetailTable()
        {
            return MakeTable(
                RowHeight,
                MakeRow(
                    1D,
                    MakeCell(0.41099476439790572D, bindMember: nameof(MovableAssetTransactionSlipRow.SiraNo), align: TextAlignment.MiddleCenter),
                    MakeCell(1.1487780665852014D, bindMember: nameof(MovableAssetTransactionSlipRow.Kodu), align: TextAlignment.MiddleCenter),
                    MakeCell(0.77486934961448806D, bindMember: nameof(MovableAssetTransactionSlipRow.BarkodNo), align: TextAlignment.MiddleCenter),
                    MakeCell(2.0554101754233476D, bindMember: nameof(MovableAssetTransactionSlipRow.Adi)),
                    MakeCell(0.60994764397905765D, bindMember: nameof(MovableAssetTransactionSlipRow.OlcuBirimi), align: TextAlignment.MiddleCenter),
                    MakeCell(1D, bindMember: nameof(MovableAssetTransactionSlipRow.Miktari), bindFormat: "{0:n2}", align: TextAlignment.MiddleRight),
                    MakeCell(1D, bindMember: nameof(MovableAssetTransactionSlipRow.BirimFiyati), bindFormat: "{0:n2}", align: TextAlignment.MiddleRight),
                    MakeCell(1D, bindMember: nameof(MovableAssetTransactionSlipRow.Tutari), bindFormat: "{0:n2}", align: TextAlignment.MiddleRight)));
        }

        private XRTable BuildGroupFooterTable()
        {
            return MakeTable(
                RowHeight,
                MakeRow(
                    1D,
                    MakeCell(5D, bold: true, header: true, bindMember: nameof(MovableAssetTransactionSlipRow.GroupDisplay)),
                    MakeCell(1D, bold: true, header: true, align: TextAlignment.MiddleRight, bindMember: nameof(MovableAssetTransactionSlipRow.Miktari), bindFormat: "{0:n2}", summary: new XRSummary(SummaryRunning.Group, SummaryFunc.Sum, "{0:n2}")),
                    MakeCell(1D, "TOPLAM", bold: true, header: true, align: TextAlignment.MiddleCenter),
                    MakeCell(1D, bold: true, header: true, align: TextAlignment.MiddleRight, bindMember: nameof(MovableAssetTransactionSlipRow.Tutari), bindFormat: "{0:n2}", summary: new XRSummary(SummaryRunning.Group, SummaryFunc.Sum, "{0:n2}"))));
        }

        private XRTable BuildSignatureTable()
        {
            const string kayit = "\r\n\r\n\r\n  Taşınır Kayıt ve Yetkilisinin\r\n\r\nAdı Soyadı :\r\nÜnvanı :\r\nİmzası :";

            return MakeTable(
                150F,
                MakeRow(
                    1D,
                    MakeCell(1.51D, "GİRİŞ KAYDI YAPILMIŞTIR" + kayit, align: TextAlignment.TopCenter),
                    MakeCell(1.49D, "ÇIKIŞ KAYDI YAPILMIŞTIR" + kayit, align: TextAlignment.TopCenter)),
                MakeRow(
                    1D,
                    MakeCell(1.51D, "TESLİM EDEN (15)\r\n\r\n\r\n\r\nAdı Soyadı :\r\nÜnvanı :\r\nİmzası :", align: TextAlignment.TopCenter),
                    MakeCell(1.49D, "TESLİM ALAN (16)\r\n\r\n\r\n\r\nAdı Soyadı :\r\nÜnvanı :\r\nİmzası :", align: TextAlignment.TopCenter)));
        }

        private XRTableCell RowLabel(string text)
        {
            return MakeCell(1.649D, text, bold: true);
        }

        private XRTableCell NameCaption()
        {
            return MakeCell(0.299D, "ADI", bold: true, align: TextAlignment.MiddleCenter);
        }

        private XRTableCell CodeCaption()
        {
            return MakeCell(0.375D, "KODU", bold: true, align: TextAlignment.MiddleCenter);
        }

        private XRTableCell DateCaption()
        {
            return MakeCell(0.299D, "TARİH", bold: true, align: TextAlignment.MiddleCenter);
        }

        private XRTableCell NameValue(string parameterName)
        {
            return MakeCell(2.052D, expression: "?" + parameterName);
        }

        private XRTableCell CodeValue(string parameterName)
        {
            return MakeCell(0.625D, expression: "?" + parameterName);
        }

        private static XRTableRow MakeRow(double weight, params XRTableCell[] cells)
        {
            XRTableRow row = new() { Weight = weight };
            row.Cells.AddRange(cells);
            return row;
        }

        private XRTable MakeTable(float height, params XRTableRow[] rows)
        {
            XRTable table = new()
            {
                Borders = BorderSide.All,
                Font = _small,
                TextAlignment = TextAlignment.MiddleLeft,
                Padding = new PaddingInfo(2F, 2F, 2F, 2F, 100F),
                CanGrow = true,
                LocationF = new System.Drawing.PointF(0F, 0F),
                SizeF = new System.Drawing.SizeF(LayoutWidth, height)
            };
            table.StylePriority.UseBorders = false;
            table.StylePriority.UseFont = false;
            table.StylePriority.UseTextAlignment = false;
            table.StylePriority.UsePadding = false;
            table.Rows.AddRange(rows);
            return table;
        }

        private static SubBand MakeSubBand(string name, float height, XRControl content)
        {
            SubBand band = new() { Name = name, HeightF = height, CanGrow = true };
            band.Controls.Add(content);
            return band;
        }

        private XRTableCell MakeCell(
            double weight,
            string? text = null,
            bool bold = false,
            bool header = false,
            TextAlignment align = TextAlignment.MiddleLeft,
            string? expression = null,
            string? bindMember = null,
            string? bindFormat = null,
            XRSummary? summary = null,
            DXFont? font = null)
        {
            XRTableCell cell = new()
            {
                Weight = weight,
                Multiline = true,
                CanGrow = true,
                Borders = BorderSide.All,
                Font = font ?? (bold ? _smallBold : _small),
                TextAlignment = align,
                Padding = new PaddingInfo(2F, 2F, 2F, 2F, 100F)
            };

            if (!string.IsNullOrEmpty(text))
            {
                cell.Text = text;
            }

            cell.StylePriority.UseBorders = false;
            cell.StylePriority.UseFont = false;
            cell.StylePriority.UseTextAlignment = false;
            cell.StylePriority.UsePadding = false;

            if (header)
            {
                cell.BackColor = System.Drawing.Color.Silver;
                cell.StylePriority.UseBackColor = false;
            }

            if (!string.IsNullOrEmpty(expression))
            {
                cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", expression));
            }

            if (!string.IsNullOrEmpty(bindMember))
            {
                if (string.IsNullOrEmpty(bindFormat))
                {
                    cell.DataBindings.Add("Text", null, bindMember!);
                }
                else
                {
                    cell.DataBindings.Add("Text", null, bindMember!, bindFormat!);
                }
            }

            if (summary is not null)
            {
                cell.Summary = summary;
            }

            return cell;
        }
    }
}
