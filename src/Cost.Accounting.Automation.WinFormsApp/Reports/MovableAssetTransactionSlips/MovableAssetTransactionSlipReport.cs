using Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips;
using DevExpress.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.Reports
{
    public partial class MovableAssetTransactionSlipReport : DevExpress.XtraReports.UI.XtraReport
    {
        private const float LayoutWidth = 731.329F;
        private const float RowHeight = 15.5F;

        private readonly DXFont _small = new("Arial", 6.4F);
        private readonly DXFont _smallBold = new("Arial", 6.4F, DXFontStyle.Bold);

        public MovableAssetTransactionSlipReport()
        {
            InitializeComponent();
        }

        public MovableAssetTransactionSlipReport(MovableAssetTransactionSlipData data)
            : this()
        {
            ApplyData(data);
        }

        private void ApplyData(MovableAssetTransactionSlipData data)
        {
            SetParameter("FisSiraNo", data.DocumentNumber);
            SetParameter("Tarih", data.Date.ToString("dd.MM.yyyy"));
            SetParameter("ProvinceDistrictName", data.ProvinceDistrictName);
            SetParameter("ProvinceDistrictCode", data.ProvinceDistrictCode);
            SetParameter("ExpenditureUnitName", data.ExpenditureUnitName);
            SetParameter("ExpenditureUnitCode", data.ExpenditureUnitCode);
            SetParameter("StoreName", data.StoreName);
            SetParameter("StoreCode", data.StoreCode);
            SetParameter("AccountingUnitName", data.AccountingUnitName);
            SetParameter("AccountingUnitCode", data.AccountingUnitCode);
            SetParameter("ReferenceDate", data.ReferenceDate?.ToString("dd.MM.yyyy") ?? string.Empty);
            SetParameter("ReferenceCode", data.ReferenceCode);
            SetParameter("OperationType", data.OperationType);
            SetParameter("SourceParty", data.SourceParty);
            SetParameter("RecipientParty", data.RecipientParty);
            SetParameter("DestinationParty", data.DestinationParty);
            SetParameter(
                "Ozet",
                $"Yukarıda gösterilen {data.DetailLineCount} kalem, toplam {data.TotalQuantity:N0} adet taşınırın");

            BuildDepotTotals(data);
            BuildSignatureBlocks(data);

            DataSource = data.Rows;
        }

        /// <summary>
        /// İmza kutularını fişin türüne göre doldurur.
        ///
        /// <para>
        /// Giriş fişinde yalnızca kayıt (giriş) kutusu dolar. Tüketim ve atölye
        /// transferi çıkış kaydıdır: çıkış kutusunu ve teslim eden kutusunu
        /// taşınırcı (kayıt yetkilisi) doldurur, teslim alan kutusunu ise
        /// malzemeyi teslim alan görevli (tüketimde muhasebe birimi, transferde
        /// atölye şefi).
        /// </para>
        ///
        /// <para>
        /// Fişe ait olmayan kutuların değerleri ve tarihleri boşaltılır.
        /// </para>
        ///
        /// <para>
        /// Etiketler ("Adı Soyadı :", ...) ve başlıklar şablonda hücreye
        /// eklenmiş, tasarımda konumlandırılmış kontrollerdir; burada yalnızca
        /// değer etiketlerine yazılır. Böylece etiketler yerinden oynamaz,
        /// iki noktalar tek dikey çizgide kalır ve değerler kendi sütununda
        /// hizalı basılır.
        /// </para>
        ///
        /// <para>
        /// Tarih, etiket sütununda değil kutunun başlık satırının sağ ucunda,
        /// başlığın hemen bitişiğinde durur.
        /// </para>
        /// </summary>
        private void BuildSignatureBlocks(MovableAssetTransactionSlipData data)
        {
            bool isEntry = data.Kind == MovableAssetTransactionSlipKind.Entry;
            bool isExit = data.Kind is MovableAssetTransactionSlipKind.Exit or MovableAssetTransactionSlipKind.AtelierTransfer;

            FillSignatureValues(
                isEntry,
                lblSig1GirisAdiSoyadi, lblSig1GirisUnvani,
                data.SignatureName, data.SignatureTitle);
            lblSig1GirisTarihi.Text = isEntry ? data.SignatureDate : string.Empty;

            FillSignatureValues(
                isExit,
                lblSig1ExitAdiSoyadi, lblSig1ExitUnvani,
                data.SignatureName, data.SignatureTitle);
            lblSig1ExitTarihi.Text = isExit ? data.SignatureDate : string.Empty;

            // Teslim eden taşınırcının kendisidir: malzemeyi fişi düzenleyen
            // kayıt yetkilisi elden eder.
            FillSignatureValues(
                isExit,
                lblSig2TeslimEdenAdiSoyadi, lblSig2TeslimEdenUnvani,
                data.SignatureName, data.SignatureTitle);
            lblSig2TeslimEdenTarihi.Text = isExit ? data.SignatureDate : string.Empty;

            FillSignatureValues(
                isExit,
                lblSig2TeslimAlanAdiSoyadi, lblSig2TeslimAlanUnvani,
                data.RecipientSignatureName, data.RecipientSignatureTitle);
            lblSig2TeslimAlanTarihi.Text = isExit ? data.SignatureDate : string.Empty;
        }

        /// <summary>
        /// Bir kutunun ad ve ünvan satırlarını yazar; kutu fişe ait değilse
        /// satırları boşaltır.
        /// </summary>
        private static void FillSignatureValues(
            bool isFilled,
            XRLabel nameLabel,
            XRLabel titleLabel,
            string name,
            string title)
        {
            nameLabel.Text = isFilled ? name : string.Empty;
            titleLabel.Text = isFilled ? title : string.Empty;
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
                string label = string.IsNullOrWhiteSpace(depot.WarehouseName)
                    ? "DEPO TOPLAMI"
                    : $"{depot.WarehouseName} DEPO TOPLAMI";

                // Etiket de sağa hizalı: rakamlar zaten sağa hizalı olduğu için
                // başlık ile tutarları aynı hizada buluşur, toplam satırı
                // okunur kalır.
                rows.Add(MakeRow(
                    1D,
                    MakeCell(5D, label, bold: true, header: true, align: TextAlignment.MiddleRight),
                    MakeCell(1D, depot.Quantity.ToString("N2"), bold: true, header: true, align: TextAlignment.MiddleRight),
                    MakeCell(1D, string.Empty, bold: true, header: true),
                    MakeCell(1D, depot.Amount.ToString("N2"), bold: true, header: true, align: TextAlignment.MiddleRight)));
            }

            rows.Add(MakeRow(
                1D,
                MakeCell(5D, "GENEL TOPLAM", bold: true, header: true, align: TextAlignment.MiddleRight),
                MakeCell(1D, data.GrandQuantity.ToString("N2"), bold: true, header: true, align: TextAlignment.MiddleRight),
                MakeCell(1D, string.Empty, bold: true, header: true),
                MakeCell(1D, data.GrandAmount.ToString("N2"), bold: true, header: true, align: TextAlignment.MiddleRight)));

            float height = RowHeight * rows.Count;
            XRTable table = MakeTable(height, [.. rows]);

            ReportFooter.Controls.Add(table);
            ReportFooter.HeightF = height;
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
                Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom,
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
                Font = font ?? (bold ? _smallBold : _small),
                TextAlignment = align,
                Padding = new PaddingInfo(2F, 2F, 2F, 2F, 100F)
            };

            if (!string.IsNullOrEmpty(text))
            {
                cell.Text = text;
            }

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
