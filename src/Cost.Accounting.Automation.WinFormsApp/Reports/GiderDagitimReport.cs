using Cost.Accounting.Automation.WinFormsApp.Reports;
using DevExpress.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.Reports
{
    public partial class GiderDagitimReport : DevExpress.XtraReports.UI.XtraReport
    {
        private const float LayoutWidth = 731.329F;
        private const float RowHeight = 15.5F;

        private readonly DXFont _small = new("Arial", 6.4F);
        private readonly DXFont _smallBold = new("Arial", 6.4F, DXFontStyle.Bold);
        private readonly DXFont _title = new("Arial", 12.8F, DXFontStyle.Bold);
        private readonly DXFont _section = new("Arial", 9.6F, DXFontStyle.Bold);

        public GiderDagitimReport()
        {
            InitializeComponent();
        }

        private void BuildLayout()
        {
            TopMargin = new TopMarginBand { HeightF = 22F, Name = "TopMargin" };
            ReportHeader = new ReportHeaderBand { HeightF = 1F, Name = "ReportHeader" };
            PageHeader = new PageHeaderBand { HeightF = RowHeight, Name = "PageHeader" };
            Detail = new DetailBand { HeightF = RowHeight, Name = "Detail" };
            ReportFooter = new ReportFooterBand { HeightF = 1F, Name = "ReportFooter" };
            BottomMargin = new BottomMarginBand { HeightF = 23F, Name = "BottomMargin" };

            ReportHeader.SubBands.AddRange(
            [
                MakeSubBand("subTitle", 22F, MakeTitle()),
                MakeSubBand("subSlipInfo", 62F, BuildSlipInfoTable())
            ]);

            PageHeader.Controls.Add(BuildColumnHeaderTable());
            Detail.Controls.Add(BuildDetailTable());

            ReportFooter.SubBands.AddRange(
            [
                MakeSubBand("subTotals", RowHeight * 2F, BuildTotalsTable())
            ]);

            Bands.AddRange(
            [
                TopMargin,
                ReportHeader,
                PageHeader,
                Detail,
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
                "PusulaNo",
                "Tarih",
                "Turu",
                "Isyurdu",
                "Atolye",
                "MamulAdi",
                "Miktari",
                "CiltNo",
                "SayfaNo",
                "SiparisNo",
                "GenelToplam",
                "BirimMaliyet"
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

        private XRLabel MakeTitle()
        {
            XRLabel label = new()
            {
                Text = "GİDER DAĞITIM TABLOSU",
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

        private XRTable BuildSlipInfoTable()
        {
            return MakeTable(
                RowHeight * 4F,
                MakeRow(
                    1D,
                    Caption("MALİYET PUSULASI NO"), ValueCell("?PusulaNo"),
                    Caption("TARİH"), ValueCell("?Tarih"),
                    Caption("TÜRÜ"), ValueCell("?Turu")),
                MakeRow(
                    1D,
                    Caption("İŞYERİ"), ValueCell("?Isyurdu"),
                    Caption("ATÖLYE"), ValueCell("?Atolye"),
                    MakeCell(1D, bold: true),
                    MakeCell(1D)),
                MakeRow(
                    1D,
                    Caption("MAMUL"), ValueCell("?MamulAdi"),
                    Caption("MİKTAR"), ValueCell("?Miktari"),
                    MakeCell(1D, bold: true),
                    MakeCell(1D)),
                MakeRow(
                    1D,
                    Caption("CİLT NO"), ValueCell("?CiltNo"),
                    Caption("SAYFA NO"), ValueCell("?SayfaNo"),
                    Caption("SİPARİŞ FİŞİ NO"), ValueCell("?SiparisNo")));
        }

        private XRTable BuildColumnHeaderTable()
        {
            return MakeTable(
                RowHeight,
                MakeRow(
                    1D,
                    MakeCell(1D, "SIRA NO", bold: true, header: true, align: TextAlignment.MiddleCenter, font: _section),
                    MakeCell(10D, "HESAP", bold: true, header: true, align: TextAlignment.MiddleCenter, font: _section),
                    MakeCell(3D, "TUTAR (₺)", bold: true, header: true, align: TextAlignment.MiddleCenter, font: _section),
                    MakeCell(2D, "ORAN (%)", bold: true, header: true, align: TextAlignment.MiddleCenter, font: _section)));
        }

        private XRTable BuildDetailTable()
        {
            return MakeTable(
                RowHeight,
                MakeRow(
                    1D,
                    MakeCell(1D, bindMember: nameof(GiderDagitimRow.SiraNo), align: TextAlignment.MiddleCenter),
                    MakeCell(10D, bindMember: nameof(GiderDagitimRow.HesapAdi)),
                    MakeCell(3D, bindMember: nameof(GiderDagitimRow.Tutar), bindFormat: "{0:n2}", align: TextAlignment.MiddleRight),
                    MakeCell(2D, bindMember: nameof(GiderDagitimRow.Oran), bindFormat: "{0:n2}", align: TextAlignment.MiddleRight)));
        }

        private XRTable BuildTotalsTable()
        {
            return MakeTable(
                RowHeight * 2F,
                MakeRow(
                    1D,
                    MakeCell(11D, "GENEL TOPLAM", bold: true, header: true),
                    MakeCell(3D, bold: true, header: true, align: TextAlignment.MiddleRight, expression: "?GenelToplam"),
                    MakeCell(2D, string.Empty, bold: true, header: true)),
                MakeRow(
                    1D,
                    MakeCell(11D, "BİRİM MALİYET (Genel Toplam / Miktar)", bold: true, header: true),
                    MakeCell(3D, bold: true, header: true, align: TextAlignment.MiddleRight, expression: "?BirimMaliyet"),
                    MakeCell(2D, string.Empty, bold: true, header: true)));
        }

        private XRTableCell Caption(string text)
        {
            return MakeCell(1D, text, bold: true);
        }

        private XRTableCell ValueCell(string expression)
        {
            return MakeCell(2D, expression: expression);
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

            return cell;
        }
    }
}