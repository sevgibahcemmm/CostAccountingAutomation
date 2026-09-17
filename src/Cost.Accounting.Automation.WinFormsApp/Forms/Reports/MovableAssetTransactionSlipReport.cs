using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using System.Collections.Generic;
using System.Linq;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.Reports
{
    public partial class MovableAssetTransactionSlipReport : DevExpress.XtraReports.UI.XtraReport
    {
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
            xrLabel43.Text = data.DocumentNumber;

            xrLabel3.Text = data.Date.ToString("dd.MM.yyyy");

            xrTableCell23.Text = data.IlIlceAdi;
            xrTableCell25.Text = data.IlIlceKodu;
            xrTableCell28.Text = data.HarcamaBirimiAdi;
            xrTableCell30.Text = data.HarcamaBirimiKodu;
            xrTableCell33.Text = data.AmbarAdi;
            xrTableCell35.Text = data.AmbarKodu;
            xrTableCell38.Text = data.MuhasebeBirimiAdi;
            xrTableCell40.Text = data.MuhasebeBirimiKodu;

            xrTableCell8.Text = data.DayanakTarihi.HasValue ? data.DayanakTarihi.Value.ToString("dd.MM.yyyy") : string.Empty;
            xrTableCell10.Text = data.DayanakKodu;

            xrTableCell16.Text = data.IslemCesidi;
            xrTableCell17.Text = data.NeredenGeldigi;
            xrTableCell18.Text = data.KimeVerildigi;
            xrTableCell19.Text = data.NereyeVerildigi;

            xrTableCell48.Text = data.HarcamaBirimiAdi;
            xrTableCell50.Text = data.HarcamaBirimiKodu;
            xrTableCell53.Text = data.AmbarAdi;
            xrTableCell55.Text = data.AmbarKodu;
            xrTableCell58.Text = data.MuhasebeBirimiAdi;
            xrTableCell60.Text = data.MuhasebeBirimiKodu;

            string footerText = $"Yukarıda gösterilen {data.DetailLineCount} kalem, toplam {data.TotalQuantity:N0} adet taşınırın";
            xrLabel5.Text = footerText;
            xrLabel15.Text = footerText;

            DataSource = data.Rows;
            DataSourceRowChanged += Slip_DataSourceRowChanged;
            BindDetailRow();
        }

        private void Slip_DataSourceRowChanged(object? sender, DataSourceRowEventArgs e)
        {
            if (_detailCells is null)
            {
                return;
            }

            if (GetCurrentRow() is not MovableAssetTransactionSlipRow row)
            {
                return;
            }

            bool isTotal = row.IsSubtotal || row.IsGrandTotal;
            System.Drawing.Color back = isTotal
                ? System.Drawing.Color.Silver
                : System.Drawing.Color.Transparent;

            foreach (XRTableCell cell in _detailCells)
            {
                if (isTotal)
                {
                    cell.Font = new DevExpress.Drawing.DXFont("Arial", cell.Font.Size, DevExpress.Drawing.DXFontStyle.Bold);
                    cell.StylePriority.UseFont = true;
                }
                else
                {
                    cell.StylePriority.UseFont = false;
                }

                cell.BackColor = back;
                cell.StylePriority.UseBackColor = isTotal;
            }
        }

        private XRTableCell[]? _detailCells;

        private void BindDetailRow()
        {
            _detailCells =
            [
                xrTableCell42,
                xrTableCell61,
                xrTableCell63,
                xrTableCell65,
                xrTableCell67,
                xrTableCell69,
                xrTableCell71,
                xrTableCell72
            ];

            BindCell(xrTableCell42, nameof(MovableAssetTransactionSlipRow.SiraNo), null);
            BindCell(xrTableCell61, nameof(MovableAssetTransactionSlipRow.Kodu), null);
            BindCell(xrTableCell63, nameof(MovableAssetTransactionSlipRow.BarkodNo), null);
            BindCell(xrTableCell65, nameof(MovableAssetTransactionSlipRow.Adi), null);
            BindCell(xrTableCell67, nameof(MovableAssetTransactionSlipRow.OlcuBirimi), null);
            BindCell(xrTableCell69, nameof(MovableAssetTransactionSlipRow.Miktari), "{0:n2}");
            BindCell(xrTableCell71, nameof(MovableAssetTransactionSlipRow.BirimFiyati), "{0:n2}");
            BindCell(xrTableCell72, nameof(MovableAssetTransactionSlipRow.Tutari), "{0:n2}");
        }

        private void BindCell(XRTableCell cell, string dataMember, string? formatString)
        {
            cell.Text = string.Empty;
            cell.DataBindings.Clear();

            if (formatString is null)
            {
                cell.DataBindings.Add("Text", null, dataMember);
            }
            else
            {
                cell.DataBindings.Add("Text", null, dataMember, formatString);
            }
        }
    }
}