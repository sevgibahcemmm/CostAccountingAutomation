using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StockIssueId",
                table: "ProductMovements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StockIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueType = table.Column<int>(type: "int", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    SourceWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostingMethod = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(MAX)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockIssues_ChartOfAccounts_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockIssues_ChartOfAccounts_TargetAccountId",
                        column: x => x.TargetAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockIssueLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockIssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "money", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockIssueLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockIssueLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockIssueLines_StockIssues_StockIssueId",
                        column: x => x.StockIssueId,
                        principalTable: "StockIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductMovements_StockIssueId",
                table: "ProductMovements",
                column: "StockIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueLines_ProductId",
                table: "StockIssueLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueLines_StockIssueId",
                table: "StockIssueLines",
                column: "StockIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssues_Date",
                table: "StockIssues",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssues_DocumentNumber",
                table: "StockIssues",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssues_IssueType",
                table: "StockIssues",
                column: "IssueType");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssues_SourceWarehouseId",
                table: "StockIssues",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssues_TargetAccountId",
                table: "StockIssues",
                column: "TargetAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockIssueLines");

            migrationBuilder.DropTable(
                name: "StockIssues");

            migrationBuilder.DropIndex(
                name: "IX_ProductMovements_StockIssueId",
                table: "ProductMovements");

            migrationBuilder.DropColumn(
                name: "StockIssueId",
                table: "ProductMovements");
        }
    }
}
