using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Reports;
using Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable;
using Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Data;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using DevExpress.XtraReports.UI;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms
{
    public sealed partial class CostSlipsListForm : CrudListFormBase<CostSlipGetAllQuery, CostSlipListDto, CostSlipEditForm>
    {
public CostSlipsListForm() : base("Maliyet Pusulası")
        {
        }

protected override SvgImage ModuleIcon => DxIcon.Module;

        /// <summary>
        /// Seçilen dönem ve atölyeler. Fiş verisi, bekleme penceresi içinde
        /// hazırlanır; bu yüzden kullanıcı soruları
        /// <see cref="PrepareSlipPrintAsync"/> içinde sorulup sonuçları burada
        /// saklanır.
        /// </summary>
        private DateOnly _slipStartDate;
        private DateOnly _slipEndDate;
        private List<Guid> _slipWorkshopIds = [];

        /// <summary>
        /// Taşınır işlem fişinin "NEREDEN GELDİĞİ" alanına yazılan açıklama.
        /// Mamül maliyet pusulasının belgelediği tek stok olayı üretimdir;
        /// malzeme mamül ambarına bu açıklamayla girer.
        /// </summary>
        private const string ProductionEntrySourceParty = "Mamül Üretimden Gelen";

        protected override string[] SearchFieldNames =>
        [
            nameof(CostSlipListDto.SlipNumber),
            nameof(CostSlipListDto.WorkshopName),
            nameof(CostSlipListDto.ProducedProductName),
            nameof(CostSlipListDto.Description)
        ];

protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            ConfigureMasterSummary();
            ConfigureItemsDetail();
        }

        private void ConfigureMasterSummary()
        {
            View.OptionsView.ShowFooter = true;

            GridColumn quantityColumn = View.Columns[nameof(CostSlipListDto.Quantity)];
            quantityColumn.Summary.Add(SummaryItemType.Sum, nameof(CostSlipListDto.Quantity), "Toplam: {0:n0}");

            GridColumn grandTotalColumn = View.Columns[nameof(CostSlipListDto.GrandTotal)];
            grandTotalColumn.Summary.Add(SummaryItemType.Sum, nameof(CostSlipListDto.GrandTotal), "Toplam: {0:n2}");
        }

        private void ConfigureItemsDetail()
        {
            View.OptionsDetail.EnableMasterViewMode = true;
            View.OptionsDetail.ShowDetailTabs = false;
            View.OptionsDetail.AllowOnlyOneMasterRowExpanded = false;

            GridView itemsView = new(BaseGrid)
            {
                Name = "CostSlipItemsView"
            };
            itemsView.OptionsBehavior.Editable = false;
            itemsView.OptionsView.ShowGroupPanel = false;
            itemsView.OptionsView.EnableAppearanceEvenRow = true;
            itemsView.OptionsView.EnableAppearanceOddRow = true;

            AddDetailColumn(itemsView, nameof(CostSlipItemDto.ProductName), "Ürün / Masraf", 220);
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.ProductUnitTypeName), "Birim", 80, alignment: "Center");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.ExpenseAccountTypeName), "Hesap", 250);
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.Quantity), "Miktar", 90, "n2", "Right");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.UnitPrice), "Birim Fiyat", 100, "n2", "Right");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.TotalAmount), "Tutar", 110, "n2", "Right");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.Description), "Açıklama", 200);

            itemsView.OptionsView.ShowFooter = true;
            itemsView.Columns[nameof(CostSlipItemDto.Quantity)].Summary.Add(
                SummaryItemType.Sum, nameof(CostSlipItemDto.Quantity), "Toplam: {0:n2}");
            itemsView.Columns[nameof(CostSlipItemDto.TotalAmount)].Summary.Add(
                SummaryItemType.Sum, nameof(CostSlipItemDto.TotalAmount), "Toplam: {0:n2}");

            BaseGrid.LevelTree.Nodes.Add(new GridLevelNode
            {
                RelationName = "CostSlipItems",
                LevelTemplate = itemsView
            });

            View.MasterRowGetRelationName += (_, e) => e.RelationName = "CostSlipItems";
            View.MasterRowGetChildList += (_, e) =>
                e.ChildList = (View.GetRow(e.RowHandle) as CostSlipListDto)?.CostSlipItems;
        }

        private static void AddDetailColumn(GridView view, string fieldName, string caption, int width,
            string? format = null, string? alignment = null)
        {
            GridColumn column = new()
            {
                Caption = caption,
                FieldName = fieldName,
                Width = width,
                Visible = true,
                OptionsColumn = { AllowEdit = false }
            };

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                column.DisplayFormat.FormatString = format;
            }

            if (alignment == "Right")
            {
                column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            }
            else if (alignment == "Center")
            {
                column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            }

            view.Columns.Add(column);
        }

protected override CostSlipGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        /// <summary>En yeni maliyet pusulası üstte (maliyet tarihine göre).</summary>
        protected override IEnumerable<CostSlipListDto> ApplyDefaultOrder(IEnumerable<CostSlipListDto> items)
            => Utils.ListOrder.NewestDocumentFirst(items, x => x.CostDate, x => x.CreatedAt, x => x.Id);

        protected override IRequest<Result<string>> BuildDeleteCommand(CostSlipListDto item)
            => new CostSlipDeleteCommand(item.Id);

        protected override string GetDeleteSummary(CostSlipListDto item)
            => item.SlipNumber;

        protected override bool SupportsRestore => true;

        protected override bool SupportsApprove => true;

        protected override bool SupportsSlipReport => true;

        protected override bool SupportsDistributionReport => true;

        protected override bool SupportsProductDeclarationReport => true;

        protected override bool SupportsSlipPrint => true;

        /// <summary>
        /// Taşınır işlem fişi MAMÜL üretimini belgeler; yarı mamül ve hizmet
        /// pusulalarının stok girişi olmadığı için bu listede fiş basılamaz.
        /// </summary>
        protected override bool CanPrintSlip(CostSlipListDto item)
            => item.CostSlipType == CostSlipType.Product;

        /// <summary>
        /// Fiş bir kayda değil bir döneme bağlıdır; bu yüzden buton kayıt
        /// seçilmeden de çalışır ve dönem sorusu fiş penceresinde sorulur.
        /// </summary>
        protected override bool AllowsSlipPrintWithoutSelection => true;

        /// <summary>
        /// Fiş, tek bir mamül girişini değil bir ATÖLYENİN dönem içindeki tüm
        /// mamül üretimlerini belgeler. Bu yüzden önce dönem, sonra atölye
        /// seçilir; seçilen her atölye için bir fiş basılır.
        ///
        /// <para>
        /// Sorular bekleme penceresi açılmadan sorulur: pencere açıkken
        /// modal soru penceresi açılırsa ekranda iki kalıcı pencere üst üste
        /// birikir.
        /// </para>
        /// </summary>
        protected override async Task<bool> PrepareSlipPrintAsync(CostSlipListDto? item)
        {
            // Varsayılan dönem, seçili kaydın maliyet ayıdır: "o ay üretilen
            // tüm ürünler" bu ayın üretimini kasteder. Kayıt seçilmediyse
            // (buton seçimsiz de çalışır) içinde bulunulan ay önerilir.
            DateOnly anchor = item?.CostDate ?? DateOnly.FromDateTime(DateTime.Today);

            using var dateForm = new DateRangePromptForm(
                defaultStart: new DateOnly(anchor.Year, anchor.Month, 1),
                defaultEnd: new DateOnly(anchor.Year, anchor.Month, DateTime.DaysInMonth(anchor.Year, anchor.Month)),
                showTypeSelector: false,
                headerTitle: "Mamül Üretimi Taşınır İşlem Fişi");

            if (dateForm.ShowDialog(this) != DialogResult.OK)
            {
                return false;
            }

            List<CostSlipProductionEntryDto> entries = await LoadProductionEntriesAsync(dateForm.StartDate, dateForm.EndDate);

            if (entries.Count == 0)
            {
                ToastHelper.Show(
                    "Seçilen dönemde onaylı mamül maliyet pusulası bulunamadı.",
                    ToastType.Warning);
                return false;
            }

            List<(Guid Id, string Name)> workshops = entries
                .GroupBy(e => e.WorkshopId)
                .Select(g => (Id: g.Key, Name: g.First().WorkshopName))
                .OrderBy(w => w.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (workshops.Count > 1)
            {
                using var prompt = new WorkshopSelectionPromptForm(
                    workshops.Select(w => w.Name).ToList(),
                    "Mamül Üretimi Taşınır İşlem Fişi");

                if (prompt.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }

                List<string> selected = prompt.SelectedWorkshops;
                workshops = workshops.Where(w => selected.Contains(w.Name)).ToList();
            }

            _slipStartDate = dateForm.StartDate;
            _slipEndDate = dateForm.EndDate;
            _slipWorkshopIds = workshops.Select(w => w.Id).ToList();

            return _slipWorkshopIds.Count > 0;
        }

        private async Task<List<CostSlipProductionEntryDto>> LoadProductionEntriesAsync(
            DateOnly startDate,
            DateOnly endDate)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                return await mediator.Send(
                    new CostSlipProductionEntriesQuery(startDate, endDate),
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("SlipPrint.ProductionEntries", ex);
                ToastHelper.Show("Mamül üretim kayıtları okunamadı: " + ex.Message, ToastType.Error, 6000);
                return [];
            }
        }

        /// <summary>
        /// Seçilen her atölye için bir taşınır işlem fişi üretir. Fiş atölye
        /// bazlıdır: dönem içinde o atölyenin ürettiği TÜM mamüller tek
        /// fişe girer ve fişin ambari atölyenin kendi 152 hesabıdır
        /// (ör. "152.10.01 Mobilya ve Ağaç İşleri"), genel "152 Mamüller
        /// Hesabı" değil.
        /// </summary>
        protected override async Task<IReadOnlyList<MovableAssetTransactionSlipData>> BuildSlipDataListAsync(CostSlipListDto? item)
        {
            if (_slipWorkshopIds.Count == 0)
            {
                return [];
            }

            List<CostSlipProductionEntryDto> entries = await LoadProductionEntriesAsync(_slipStartDate, _slipEndDate);

            if (entries.Count == 0)
            {
                ToastHelper.Show("Seçilen dönemde onaylı mamül maliyet pusulası bulunamadı.", ToastType.Warning);
                return [];
            }

            CompanyDto company = await MovableAssetTransactionSlipPresenter.LoadCompanyAsync();
            List<ChartOfAccountLookUpDto> accounts = await MovableAssetTransactionSlipPresenter.LoadAccountsAsync();
            Dictionary<Guid, ProductCatalogDto> productsById =
                await MovableAssetTransactionSlipPresenter.LoadProductsByIdAsync();

            List<MovableAssetTransactionSlipData> slips = [];

            foreach (IGrouping<Guid, CostSlipProductionEntryDto> group in entries
                .Where(e => _slipWorkshopIds.Contains(e.WorkshopId))
                .GroupBy(e => e.WorkshopId))
            {
                List<CostSlipProductionEntryDto> groupEntries = group
                    .OrderBy(e => e.CostDate)
                    .ThenBy(e => e.SlipNumber)
                    .ToList();

                if (groupEntries.Count == 0)
                {
                    continue;
                }

                (string Code, string Name) warehouse = ResolveWorkshopWarehouse(groupEntries, accounts, productsById);

                if (string.IsNullOrWhiteSpace(warehouse.Code))
                {
                    // Ambarı olmayan kalem için fiş üretmek, 152 girişini
                    // belgesiz bırakmaktan iyidir ama yine de yanıltıcıdır:
                    // atölye bu dönemde atlanır, kullanıcı uyarılır.
                    CrashLog.Write(
                        "SlipPrint.WorkshopWarehouse",
                        $"'{groupEntries[0].WorkshopName}' atölyesi için 152 mamül hesabı çözülemedi.");
                    continue;
                }

                slips.Add(BuildWorkshopSlip(groupEntries, groupEntries[0].WorkshopName, warehouse, company, accounts, productsById));
            }

            if (slips.Count == 0)
            {
                ToastHelper.Show(
                    "Seçilen atölyeler için 152 mamül hesabı bulunamadı; ürün kartındaki stok hesabını kontrol edin.",
                    ToastType.Warning);
            }

            return slips;
        }

        private static MovableAssetTransactionSlipData BuildWorkshopSlip(
            List<CostSlipProductionEntryDto> entries,
            string workshopName,
            (string Code, string Name) warehouse,
            CompanyDto company,
            List<ChartOfAccountLookUpDto> accounts,
            Dictionary<Guid, ProductCatalogDto> productsById)
        {
            string city = string.IsNullOrWhiteSpace(company.City) ? string.Empty : company.City.Trim();
            string district = string.IsNullOrWhiteSpace(company.District) ? string.Empty : company.District.Trim();
            string ilIlce = city.Length > 0 && district.Length > 0 ? $"{city} / {district}" : city + district;

            DateOnly lastDate = entries[^1].CostDate;
            DateTime date = lastDate.ToDateTime(TimeOnly.MinValue);

            // Fiş pusula numarasıyla değil, kapsadığı pusulaların numara
            // aralığıyla belgelenir: tek pusula varsa o numara, çoklusunda
            // ilk - sonkiler.
            string documentNumber = entries.Count == 1
                ? entries[0].SlipNumber
                : $"{entries[0].SlipNumber} - {entries[^1].SlipNumber}";

            MovableAssetTransactionSlipData data = new()
            {
                DocumentNumber = documentNumber,
                Date = date,
                OperationType = "Giriş",

                // "NEREDEN GELDİĞİ": malzeme üretimden mamül ambarına girer.
                SourceParty = ProductionEntrySourceParty,
                RecipientParty = workshopName,
                DestinationParty = $"{warehouse.Code} - {warehouse.Name}",
                ProvinceDistrictName = ilIlce,
                ProvinceDistrictCode = string.Empty,
                ExpenditureUnitName = company.ExpenditureUnitName ?? string.Empty,
                ExpenditureUnitCode = company.ExpenditureUnitCode ?? string.Empty,
                StoreName = warehouse.Name,
                StoreCode = warehouse.Code,
                AccountingUnitName = company.AccountingUnitName ?? string.Empty,
                AccountingUnitCode = company.AccountingUnitCode ?? string.Empty,
                ReferenceDate = date,
                ReferenceCode = documentNumber,
                AccountNames = MovableAssetTransactionSlipPresenter.BuildAccountNameMap(accounts)
            };

            foreach (CostSlipProductionEntryDto entry in entries)
            {
                ProductCatalogDto? product = productsById.GetValueOrDefault(entry.ProducedProductId);

                data.Rows.Add(new MovableAssetTransactionSlipRow
                {
                    Code = MovableAssetTransactionSlipPresenter.ResolveItemCode(product, entry.ProducedProductAccountCode),

                    // Ambar, satırda ürünün kendi ambarı değil atölyenin 152
                    // hesabıdır: fişin tamamı tek atölyenin üretimini belgeler.
                    WarehouseCode = warehouse.Code,
                    WarehouseName = warehouse.Name,
                    Barcode = product?.Barcode ?? string.Empty,
                    Adi = product?.Name ?? entry.ProducedProductName,
                    UnitOfMeasure = product?.ProductUnitTypeName ?? string.Empty,
                    Quantity = entry.Quantity,
                    UnitPrice = entry.UnitCost,
                    Amount = Math.Round(entry.UnitCost * entry.Quantity, 2)
                });
            }

            data.Prepare();

            return data;
        }

        /// <summary>
        /// Atölyenin 152 mamül hesabını çözer.
        ///
        /// <para>
        /// Sıra şöyledir:
        /// 1) Atölye hesabında üretim bağlantısı kurulmuşsa
        ///    (<c>FinishedAccountId</c>) o hesap,
        /// 2) bağ kurulmamışsa ürünlerin kendi stok hesabından türetilen
        ///    hesap (ör. "152.10.01 Mobilya ve Ağaç İşleri"),
        /// 3) o da yoksa genel 152 mamül ambarı.
        /// </para>
        /// </summary>
        private static (string Code, string Name) ResolveWorkshopWarehouse(
            List<CostSlipProductionEntryDto> entries,
            List<ChartOfAccountLookUpDto> accounts,
            Dictionary<Guid, ProductCatalogDto> productsById)
        {
            ChartOfAccountLookUpDto? workshop = accounts.FirstOrDefault(
                a => a.Id == entries[0].WorkshopId);

            if (workshop?.FinishedAccountId is Guid finishedAccountId)
            {
                ChartOfAccountLookUpDto? finished = accounts.FirstOrDefault(a => a.Id == finishedAccountId);

                if (finished is not null && !string.IsNullOrWhiteSpace(finished.Code))
                {
                    return (finished.Code, finished.Name);
                }
            }

            // En çok geçen üst hesap: bir atölye tek bir 152 dalına üretir;
            // dal bilgisi hesap planında kurulmamışsa kalemler yine de
            // doğru grupta toplanır.
            string? mostCommonCode = entries
                .Select(e => ProductAccountGroupCode(e.ProducedProductAccountCode))
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .GroupBy(code => code, StringComparer.Ordinal)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            if (mostCommonCode is not null)
            {
                string name = accounts.FirstOrDefault(a => a.Code == mostCommonCode)?.Name ?? mostCommonCode;

                return (mostCommonCode, name);
            }

            ProductCatalogDto? anyProduct = productsById.GetValueOrDefault(entries[0].ProducedProductId);

            return (anyProduct?.WarehouseCode ?? string.Empty, anyProduct?.WarehouseName ?? string.Empty);
        }

        /// <summary>Ürün hesabından atölye düzeyindeki 152 hesabı türetir.</summary>
        private static string ProductAccountGroupCode(string productAccountCode)
            => MovableAssetTransactionSlipData.GetLevelCode(productAccountCode, 3);

        protected override bool AllowsApprove(CostSlipListDto item) => item.Status == CostSlipStatus.Draft;

        /// <summary>Maliyet pusulası listesi onaylı kayıtları da içerir.</summary>
        protected override string PendingItemLabel => "maliyet pusulası";

        protected override bool IsPendingApproval(CostSlipListDto item) => item.Status == CostSlipStatus.Draft;

        /// <summary>Maliyet pusulası onayı ayrı bir ekranda değil, bu listede yapılır.</summary>
        protected override Type? PendingApprovalFormType => null;

        protected override bool AllowsEdit(CostSlipListDto item) => item.Status != CostSlipStatus.Approved;

        protected override bool AllowsDelete(CostSlipListDto item) => item.Status != CostSlipStatus.Approved;

protected override IRequest<Result<string>>? BuildApproveCommand(CostSlipListDto item)
            => item.Status == CostSlipStatus.Draft ? new CostSlipApproveCommand(item.Id) : null;

        /// <summary>
        /// Seçili pusulaları tek transaction'da toplu onaylar. Aynı ambalaj
        /// gibi malzeme birden çok pusulada kullanılıyorsa her tekil onay
        /// stoğu sırayla düşürür ve sonrakiler reddedilirdi; toplu onay
        /// kümülatif stoğu maliyet tarihine göre hesaplayarak ya hep birlikte
        /// ya hiç onaylar.
        /// </summary>
        protected override IRequest<Result<string>>? BuildBulkApproveCommand(IReadOnlyList<CostSlipListDto> items)
        {
            List<Guid> draftIds = items
                .Where(i => i.Status == CostSlipStatus.Draft)
                .Select(i => i.Id)
                .ToList();

            return draftIds.Count > 0
                ? new BulkApproveCostSlipsCommand(draftIds)
                : null;
        }

        protected override IRequest<Result<string>> BuildRestoreCommand(CostSlipListDto item)
            => new CostSlipRestoreCommand(item.Id);

protected override Task ShowSlipReportAsync(CostSlipListDto item)
            => CostSlipReportPresenter.ShowAsync(item);


        protected override async Task ShowDistributionReportAsync(CostSlipListDto? item)
        {
            using var dateForm = new DateRangePromptForm(item?.CostDate, item?.CostDate);
            if (dateForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // Bekleme penceresi yalnızca veri hazırlığını kapsar; önizleme
            // modal bir pencere olduğu için bekleme kapandıktan sonra açılır.
            ICostAllocationTableReport? report = await LoadingHelper.RunAsync(
                async () =>
                {
                    using var scope = Program.Services.CreateScope();
                    ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                    ExpenseDistributionReportResult result = await mediator.Send(
                        new ExpenseDistributionReportQuery(dateForm.StartDate, dateForm.EndDate, dateForm.CostSlipType));

                    if (result.Rows.Count == 0)
                    {
                        ToastHelper.Show("Seçilen tarih aralığında atölye kaydı bulunamadı.", ToastType.Warning);
                        return null;
                    }

                    CompanyDto company = await LoadCompanyAsync();

            ICostAllocationTableReport built = dateForm.CostSlipType is CostSlipType.Service or CostSlipType.SemiFinishedService
                ? new ServiceCostAllocationTable()
                : new ProductCostAllocationTableReport();

            built.SetData(dateForm.StartDate, dateForm.EndDate, result, company.Letterhead, dateForm.CostSlipType);

            // Dağıtım tablosu kurum genelidir; muhasebe imza yuvası atölyeye
            // bağlı olmadığı için atölye kimliği verilmez.
            IReadOnlyDictionary<string, string> accounting =
                await CostSlipSignatoryHelper.ResolveAccountingSignatoriesAsync();

            built.SetSignatoryNames(
                accounting[CostSlipSignatoryHelper.AccountingOfficerRoleName],
                accounting[CostSlipSignatoryHelper.AccountingClerkRoleName]);

            string missing = CostSlipSignatoryHelper.DescribeMissing(
                (CostSlipSignatoryHelper.AccountingOfficerRoleName, accounting[CostSlipSignatoryHelper.AccountingOfficerRoleName]),
                (CostSlipSignatoryHelper.AccountingClerkRoleName, accounting[CostSlipSignatoryHelper.AccountingClerkRoleName]));

            if (missing.Length > 0)
            {
                ToastHelper.Show(
                    $"Şu görevler için imza bulunamadı: {missing}. Personel ekranından görev atayın.",
                    ToastType.Warning);
            }

            return built;
                },
                caption: "Rapor hazırlanıyor...",
                description: "Lütfen bekleyin...");

            if (report is null)
            {
                return;
            }

            await ReportPreviewHelper.PrintAsync(
                (XtraReport)report,
                caption: "Mamül maliyet pusulasi hazırlanıyor...",
                description: "Lütfen bekleyin...");
        }

        protected override async Task ShowProductDeclarationReportAsync(CostSlipListDto? item)
        {
            using var dateForm = new DateRangePromptForm(
                item?.CostDate,
                item?.CostDate,
                showTypeSelector: false,
                headerTitle: "Mamül Üretim Beyanı");
            if (dateForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // Yalnızca veri çekilişi bekleme penceresinde bekler; sonrasındaki
            // atölye seçimi ve rapor önizlemesi modal pencereler oldukları için
            // bekleme penceresi kapalıyken çalışır.
            List<ProductDeclarationRowDto> rows = await LoadingHelper.RunAsync(
                async () =>
                {
                    using var scope = Program.Services.CreateScope();
                    ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                    return await mediator.Send(
                        new ProductDeclarationReportQuery(dateForm.StartDate, dateForm.EndDate),
                        CancellationToken.None);
                },
                caption: "Rapor hazırlanıyor...",
                description: "Lütfen bekleyin...");

            if (rows.Count == 0)
            {
                ToastHelper.Show("Seçilen tarih aralığında onaylı mamül fişi bulunamadı.", ToastType.Warning);
                return;
            }

            List<string> workshops = rows
                .Select(r => r.WorkshopName)
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Distinct()
                .OrderBy(w => w, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (workshops.Count > 1)
            {
                using var prompt = new WorkshopSelectionPromptForm(workshops);
                if (prompt.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<string> selected = prompt.SelectedWorkshops;
                rows = selected.Count == workshops.Count
                    ? rows
                    : rows.Where(r => selected.Contains(r.WorkshopName)).ToList();
            }

            CompanyDto company = await LoadCompanyAsync();

            var report = new ProductDeclarationReport();
            report.SetData(dateForm.StartDate, dateForm.EndDate, rows, company.Letterhead);

            // Atölye şefi, beyanın ait olduğu atölyeye özeldir; rapor atölyeye
            // göre gruplandığı için şef adı grup altbilgisinde basılır.
            var workshopIds = rows
                .Where(r => r.WorkshopId is not null)
                .Select(r => r.WorkshopId!.Value)
                .Distinct()
                .ToList();

            report.SetWorkshopChiefs(await CostSlipSignatoryHelper.ResolveWorkshopNamesAsync(
                workshopIds,
                CostSlipSignatoryHelper.WorkshopChiefRoleName));

            // Beyanda muhasebe yetkilisi yuvası yoktur; memur kurum genelinden gelir.
            IReadOnlyDictionary<string, string> accounting =
                await CostSlipSignatoryHelper.ResolveAccountingSignatoriesAsync();

            report.SetAccountingClerk(accounting[CostSlipSignatoryHelper.AccountingClerkRoleName]);

            var missingRoles = new List<(string Role, string Name)>
            {
                (CostSlipSignatoryHelper.AccountingClerkRoleName, accounting[CostSlipSignatoryHelper.AccountingClerkRoleName])
            };

            // Atölye şefi atölyeye özeldir; hiçbirinde şef yoksa uyarılır.
            if (workshopIds.Count > 0 && report.WorkshopChiefCount == 0)
            {
                missingRoles.Add((CostSlipSignatoryHelper.WorkshopChiefRoleName, string.Empty));
            }

            string missing = CostSlipSignatoryHelper.DescribeMissing([.. missingRoles]);

            if (missing.Length > 0)
            {
                ToastHelper.Show(
                    $"Şu görevler için imza bulunamadı: {missing}. Personel ekranından görev atayın.",
                    ToastType.Warning);
            }

            await ReportPreviewHelper.PrintAsync(report, caption: "Uretim beyanı hazırlanıyor...", description: "Lütfen bekleyin...");
        }

        private static async Task<CompanyDto> LoadCompanyAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                SessionClaimContext session = Program.Services.GetRequiredService<SessionClaimContext>();

                var result = await mediator.Send(new CompanyGetQuery(session.GetCompanyId()), CancellationToken.None);
                if (result.IsSuccessful && result.Data is not null)
                {
                    return result.Data;
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("DistributionReport.Company", ex);
            }

            return new CompanyDto();
        }
    }
}
