using System.ComponentModel;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public sealed class AtelierTransferStockForm : XtraForm
    {
        private readonly BindingList<AtelierTransferStockMasterDto> _masters = [];
        private readonly BindingList<AtelierTransferStockDetailDto> _details = [];

        private List<AtelierTransferStockMasterDto> _loaded = [];
        private Label lblTitle = default!;
        private Label lblMasterCaption = default!;
        private Label lblDetailCaption = default!;
        private Label lblSummary = default!;
        private GridControl gridMasters = default!;
        private GridView gridMastersView = default!;
        private GridControl gridDetails = default!;
        private GridView gridDetailsView = default!;
        private SimpleButton btnRefresh = default!;
        private SimpleButton btnClose = default!;

        public AtelierTransferStockForm()
        {
            BuildLayout();
            Load += AtelierTransferStockForm_Load;
        }

        private void BuildLayout()
        {
            SuspendLayout();

            Text = "Atölyeye Transfer Edilen Ürünler";
            IconOptions.SvgImage = DxIcon.AtelierTransfer;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(1080, 720);
            Font = new Font("Segoe UI", 9F);

            lblTitle = new Label
            {
                Text = "Atölyeye Transfer Edilen Ürünler",
                Location = new Point(24, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold)
            };

            lblSummary = new Label
            {
                Location = new Point(26, 46),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            lblMasterCaption = MakeCaption("Ürünler (Atölyeye transfer edilen)", 24, 76);

            gridMasters = new GridControl { Location = new Point(24, 100), Size = new Size(1032, 280), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            gridMastersView = new GridView();
            gridMasters.MainView = gridMastersView;
            gridMasters.ViewCollection.Add(gridMastersView);

            lblDetailCaption = MakeCaption("Transfer Detayı", 24, 392);

            gridDetails = new GridControl
            {
                Location = new Point(24, 416),
                Size = new Size(1032, 220),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            gridDetailsView = new GridView();
            gridDetails.MainView = gridDetailsView;
            gridDetails.ViewCollection.Add(gridDetailsView);

            btnRefresh = new SimpleButton
            {
                Text = "Yenile",
                Location = new Point(24, 652),
                Size = new Size(110, 30),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            btnClose = new SimpleButton
            {
                Text = "Kapat",
                Location = new Point(946, 652),
                Size = new Size(110, 30),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };

            Controls.AddRange([
                lblTitle, lblSummary, lblMasterCaption, gridMasters, lblDetailCaption,
                gridDetails, btnRefresh, btnClose
            ]);

            ConfigureMasterGrid();
            ConfigureDetailGrid();

            btnRefresh.Click += async (_, _) => await LoadDataAsync();
            btnClose.Click += (_, _) => Close();
            gridMastersView.FocusedRowChanged += GridMastersView_FocusedRowChanged;

            ResumeLayout(false);
        }

        private static Label MakeCaption(string text, int x, int y)
            => new() { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };

        private void ConfigureMasterGrid()
        {
            gridMastersView.OptionsBehavior.AutoPopulateColumns = false;
            gridMastersView.OptionsView.ColumnAutoWidth = false;
            gridMastersView.OptionsView.ShowGroupPanel = false;
            gridMastersView.OptionsSelection.MultiSelect = false;
            gridMastersView.RowHeight = 26;
            gridMasters.DataSource = _masters;

            gridMastersView.Columns.AddRange(
            [
                MakeColumn("Ürün Kodu", nameof(AtelierTransferStockMasterDto.ProductCode), 120, nearText: true),
                MakeColumn("Ürün Adı", nameof(AtelierTransferStockMasterDto.ProductName), 260),
                MakeColumn("Birim", nameof(AtelierTransferStockMasterDto.UnitTypeName), 70, center: true),
                MakeMoneyColumn("Giren Miktar (Toplam)", nameof(AtelierTransferStockMasterDto.TotalQuantity), 150),
                MakeMoneyColumn("Toplam Tutar", nameof(AtelierTransferStockMasterDto.TotalAmount), 150),
                MakeMoneyColumn("Bakiye (Güncel Stok)", nameof(AtelierTransferStockMasterDto.CurrentStock), 150)
            ]);

            CenterHeaders();
        }

        private void ConfigureDetailGrid()
        {
            gridDetailsView.OptionsBehavior.AutoPopulateColumns = false;
            gridDetailsView.OptionsView.ColumnAutoWidth = false;
            gridDetailsView.OptionsView.ShowGroupPanel = false;
            gridDetailsView.RowHeight = 26;
            gridDetails.DataSource = _details;

            gridDetailsView.Columns.AddRange(
            [
                MakeDateColumn("Tarih", nameof(AtelierTransferStockDetailDto.Date), 110),
                MakeColumn("Belge No", nameof(AtelierTransferStockDetailDto.DocumentNumber), 130),
                MakeColumn("Hedef Hesap Kodu", nameof(AtelierTransferStockDetailDto.TargetAccountCode), 130, nearText: true),
                MakeColumn("Atölye", nameof(AtelierTransferStockDetailDto.TargetAccountName), 260),
                MakeMoneyColumn("Çıkan Miktar", nameof(AtelierTransferStockDetailDto.Quantity), 130),
                MakeMoneyColumn("Birim Maliyet", nameof(AtelierTransferStockDetailDto.UnitCost), 130),
                MakeMoneyColumn("Toplam Tutar", nameof(AtelierTransferStockDetailDto.TotalAmount), 140)
            ]);

            CenterHeaders();
        }

        private static GridColumn MakeColumn(string caption, string field, int width, bool nearText = false, bool center = false)
        {
            GridColumn column = CreateBaseColumn(caption, field, width);
            if (nearText)
            {
                column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            }
            else if (center)
            {
                column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
            }
            return column;
        }

        private static GridColumn MakeMoneyColumn(string caption, string field, int width)
        {
            GridColumn column = CreateBaseColumn(caption, field, width);
            column.DisplayFormat.FormatType = FormatType.Numeric;
            column.DisplayFormat.FormatString = "n2";
            column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            return column;
        }

        private static GridColumn MakeDateColumn(string caption, string field, int width)
        {
            GridColumn column = CreateBaseColumn(caption, field, width);
            column.DisplayFormat.FormatType = FormatType.Custom;
            column.DisplayFormat.FormatString = "dd.MM.yyyy";
            column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
            return column;
        }

        private static GridColumn CreateBaseColumn(string caption, string field, int width)
            => new() { Caption = caption, FieldName = field, Visible = true, Width = width };

        private void CenterHeaders()
        {
            foreach (GridColumn col in gridMastersView.Columns)
            {
                col.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
            }
            foreach (GridColumn col in gridDetailsView.Columns)
            {
                col.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
            }
        }

        private async void AtelierTransferStockForm_Load(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            btnRefresh.Enabled = false;
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(new AtelierTransferStockQuery(), CancellationToken.None);

                _loaded = result.Data ?? [];
                _masters.Clear();
                foreach (AtelierTransferStockMasterDto master in _loaded)
                {
                    _masters.Add(master);
                }

                lblSummary.Text = _loaded.Count == 0
                    ? "Henüz atölyeye transfer edilmiş ürün bulunamadı."
                    : $"Toplam {_loaded.Count} ürün atölyeye transfer edildi.";

                ShowSelectedDetail();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Rapor yüklenirken hata oluştu: " + ex.Message, ToastType.Error, 6000);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private void GridMastersView_FocusedRowChanged(object? sender, FocusedRowChangedEventArgs e)
        {
            ShowSelectedDetail();
        }

        private void ShowSelectedDetail()
        {
            _details.Clear();

            if (gridMastersView.FocusedRowHandle < 0 || gridMastersView.FocusedRowHandle >= _masters.Count)
            {
                return;
            }

            AtelierTransferStockMasterDto? master = _masters[gridMastersView.FocusedRowHandle];
            if (master is null)
            {
                return;
            }

            foreach (AtelierTransferStockDetailDto detail in master.Transfers)
            {
                _details.Add(detail);
            }

            gridDetailsView.RefreshData();
        }
    }
}