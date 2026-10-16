using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations.Year
{
    /// <inheritdoc />
    public partial class AddDirectProcurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DirectProcurementFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    FileType = table.Column<int>(type: "int", nullable: false),
                    WorkingYear = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Flow = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChartOfAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApproximateCost = table.Column<decimal>(type: "money", nullable: true),
                    Appropriation = table.Column<decimal>(type: "money", nullable: true),
                    TenderPrice = table.Column<decimal>(type: "money", nullable: true),
                    CostEstimationMethod = table.Column<int>(type: "int", nullable: false),
                    ProjectNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DecisionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DecisionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    WorkNature = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    WorkQuantity = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    DecisionNote = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    OtherExplanations = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    SingleSourceExplanation = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    SingleSourceExplanation2 = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    SingleSourceOtherExplanation = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    AdvanceExplanation = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    PriceDifferenceExplanation = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    PurchasingUnit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RelatedPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeliveryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DemandDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OrderDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AcceptanceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ApprovalOffice = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApprovalNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApprovalDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ApprovingPersonnel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApprovingPersonnelTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApprovingPersonnelDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ConformityPersonnel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ConformityPersonnelTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ConformityPersonnelDate = table.Column<DateOnly>(type: "date", nullable: true),
                    HasContract = table.Column<bool>(type: "bit", nullable: false),
                    HasSpecification = table.Column<bool>(type: "bit", nullable: false),
                    HasCommission = table.Column<bool>(type: "bit", nullable: false),
                    HasAwardDocumentation = table.Column<bool>(type: "bit", nullable: false),
                    HasApproximateCost = table.Column<bool>(type: "bit", nullable: false),
                    HasCostEstimateTeam = table.Column<bool>(type: "bit", nullable: false),
                    HasMarketSurveyTeam = table.Column<bool>(type: "bit", nullable: false),
                    UseHistoryPurchases = table.Column<bool>(type: "bit", nullable: false),
                    UseRequirementList = table.Column<bool>(type: "bit", nullable: false),
                    RequireApprovedOffer = table.Column<bool>(type: "bit", nullable: false),
                    RequireApprovedOffer2 = table.Column<bool>(type: "bit", nullable: false),
                    IsBudgetApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    IsDemo = table.Column<bool>(type: "bit", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectProcurementFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectProcurementFiles_ChartOfAccounts_ChartOfAccountId",
                        column: x => x.ChartOfAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DirectProcurementFiles_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DirectProcurementBudgetSelections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectProcurementFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChartOfAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    VoucherType = table.Column<int>(type: "int", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "money", nullable: true),
                    SpentAmount = table.Column<decimal>(type: "money", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectProcurementBudgetSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectProcurementBudgetSelections_ChartOfAccounts_ChartOfAccountId",
                        column: x => x.ChartOfAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DirectProcurementBudgetSelections_DirectProcurementFiles_DirectProcurementFileId",
                        column: x => x.DirectProcurementFileId,
                        principalTable: "DirectProcurementFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DirectProcurementItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectProcurementFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "money", nullable: true),
                    TaxRatePercent = table.Column<int>(type: "int", nullable: true),
                    ApproximateCost = table.Column<decimal>(type: "money", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaterialGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectProcurementItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectProcurementItems_DirectProcurementFiles_DirectProcurementFileId",
                        column: x => x.DirectProcurementFileId,
                        principalTable: "DirectProcurementFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementAwardedFirms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectProcurementFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "money", nullable: true),
                    ContractDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InvoiceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeliveryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AcceptanceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    VatAmount = table.Column<decimal>(type: "money", nullable: true),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PaymentOrderDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PaymentOrderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentOrderNo2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ChequeNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Memorandum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Attachment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BudgetExpense = table.Column<decimal>(type: "money", nullable: true),
                    SpecialExpenseReduction = table.Column<decimal>(type: "money", nullable: true),
                    PayableAmount = table.Column<decimal>(type: "money", nullable: true),
                    DeductionTotal = table.Column<decimal>(type: "money", nullable: true),
                    NegotiationNote = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementAwardedFirms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementAwardedFirms_DirectProcurementFiles_DirectProcurementFileId",
                        column: x => x.DirectProcurementFileId,
                        principalTable: "DirectProcurementFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcurementAwardedFirms_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementTeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectProcurementFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    PersonnelName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementTeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementTeamMembers_DirectProcurementFiles_DirectProcurementFileId",
                        column: x => x.DirectProcurementFileId,
                        principalTable: "DirectProcurementFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementApprovedFirms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectProcurementFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectProcurementItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Price = table.Column<decimal>(type: "money", nullable: true),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    IsEligible = table.Column<bool>(type: "bit", nullable: false),
                    LetterDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementApprovedFirms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementApprovedFirms_DirectProcurementFiles_DirectProcurementFileId",
                        column: x => x.DirectProcurementFileId,
                        principalTable: "DirectProcurementFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcurementApprovedFirms_DirectProcurementItems_DirectProcurementItemId",
                        column: x => x.DirectProcurementItemId,
                        principalTable: "DirectProcurementItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProcurementApprovedFirms_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementHistoryPurchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectProcurementFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectProcurementItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "money", nullable: true),
                    TaxRatePercent = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "money", nullable: true),
                    CurrentPrice = table.Column<decimal>(type: "money", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IndexDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CurrentIndex = table.Column<decimal>(type: "money", nullable: true),
                    Coefficient = table.Column<decimal>(type: "money", nullable: true),
                    BaseIndex = table.Column<decimal>(type: "money", nullable: true),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementHistoryPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementHistoryPurchases_DirectProcurementFiles_DirectProcurementFileId",
                        column: x => x.DirectProcurementFileId,
                        principalTable: "DirectProcurementFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcurementHistoryPurchases_DirectProcurementItems_DirectProcurementItemId",
                        column: x => x.DirectProcurementItemId,
                        principalTable: "DirectProcurementItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProcurementQuotedFirms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectProcurementFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectProcurementItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Price = table.Column<decimal>(type: "money", nullable: true),
                    FinalPrice = table.Column<decimal>(type: "money", nullable: true),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    IsEligible = table.Column<bool>(type: "bit", nullable: false),
                    LetterDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalOffer = table.Column<decimal>(type: "money", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementQuotedFirms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementQuotedFirms_DirectProcurementFiles_DirectProcurementFileId",
                        column: x => x.DirectProcurementFileId,
                        principalTable: "DirectProcurementFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcurementQuotedFirms_DirectProcurementItems_DirectProcurementItemId",
                        column: x => x.DirectProcurementItemId,
                        principalTable: "DirectProcurementItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProcurementQuotedFirms_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementAwardedFirmAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcurementAwardedFirmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Debit = table.Column<decimal>(type: "money", nullable: true),
                    Credit = table.Column<decimal>(type: "money", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementAwardedFirmAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementAwardedFirmAccounts_ProcurementAwardedFirms_ProcurementAwardedFirmId",
                        column: x => x.ProcurementAwardedFirmId,
                        principalTable: "ProcurementAwardedFirms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementBudgetSelections_ChartOfAccountId",
                table: "DirectProcurementBudgetSelections",
                column: "ChartOfAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementBudgetSelections_DirectProcurementFileId_Year",
                table: "DirectProcurementBudgetSelections",
                columns: new[] { "DirectProcurementFileId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementBudgetSelections_DuplicateKey",
                table: "DirectProcurementBudgetSelections",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementFiles_ChartOfAccountId",
                table: "DirectProcurementFiles",
                column: "ChartOfAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementFiles_CompanyId",
                table: "DirectProcurementFiles",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementFiles_Date",
                table: "DirectProcurementFiles",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementFiles_DuplicateKey",
                table: "DirectProcurementFiles",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementFiles_SupplierId",
                table: "DirectProcurementFiles",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementFiles_WorkingYear_Stage",
                table: "DirectProcurementFiles",
                columns: new[] { "WorkingYear", "Stage" });

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementItems_DirectProcurementFileId_LineNo",
                table: "DirectProcurementItems",
                columns: new[] { "DirectProcurementFileId", "LineNo" });

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementItems_DuplicateKey",
                table: "DirectProcurementItems",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_DirectProcurementItems_ProductId",
                table: "DirectProcurementItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementApprovedFirms_DirectProcurementFileId_DirectProcurementItemId",
                table: "ProcurementApprovedFirms",
                columns: new[] { "DirectProcurementFileId", "DirectProcurementItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementApprovedFirms_DirectProcurementItemId",
                table: "ProcurementApprovedFirms",
                column: "DirectProcurementItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementApprovedFirms_DuplicateKey",
                table: "ProcurementApprovedFirms",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementApprovedFirms_SupplierId",
                table: "ProcurementApprovedFirms",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementAwardedFirmAccounts_DuplicateKey",
                table: "ProcurementAwardedFirmAccounts",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementAwardedFirmAccounts_ProcurementAwardedFirmId",
                table: "ProcurementAwardedFirmAccounts",
                column: "ProcurementAwardedFirmId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementAwardedFirms_DirectProcurementFileId",
                table: "ProcurementAwardedFirms",
                column: "DirectProcurementFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementAwardedFirms_DuplicateKey",
                table: "ProcurementAwardedFirms",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementAwardedFirms_SupplierId",
                table: "ProcurementAwardedFirms",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementHistoryPurchases_DirectProcurementFileId",
                table: "ProcurementHistoryPurchases",
                column: "DirectProcurementFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementHistoryPurchases_DirectProcurementItemId",
                table: "ProcurementHistoryPurchases",
                column: "DirectProcurementItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementHistoryPurchases_DuplicateKey",
                table: "ProcurementHistoryPurchases",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementHistoryPurchases_ProductId",
                table: "ProcurementHistoryPurchases",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementQuotedFirms_DirectProcurementFileId_DirectProcurementItemId",
                table: "ProcurementQuotedFirms",
                columns: new[] { "DirectProcurementFileId", "DirectProcurementItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementQuotedFirms_DirectProcurementItemId",
                table: "ProcurementQuotedFirms",
                column: "DirectProcurementItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementQuotedFirms_DuplicateKey",
                table: "ProcurementQuotedFirms",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementQuotedFirms_SupplierId",
                table: "ProcurementQuotedFirms",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementTeamMembers_DirectProcurementFileId_Role_Sequence",
                table: "ProcurementTeamMembers",
                columns: new[] { "DirectProcurementFileId", "Role", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementTeamMembers_DuplicateKey",
                table: "ProcurementTeamMembers",
                column: "DuplicateKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectProcurementBudgetSelections");

            migrationBuilder.DropTable(
                name: "ProcurementApprovedFirms");

            migrationBuilder.DropTable(
                name: "ProcurementAwardedFirmAccounts");

            migrationBuilder.DropTable(
                name: "ProcurementHistoryPurchases");

            migrationBuilder.DropTable(
                name: "ProcurementQuotedFirms");

            migrationBuilder.DropTable(
                name: "ProcurementTeamMembers");

            migrationBuilder.DropTable(
                name: "ProcurementAwardedFirms");

            migrationBuilder.DropTable(
                name: "DirectProcurementItems");

            migrationBuilder.DropTable(
                name: "DirectProcurementFiles");
        }
    }
}
