using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig2_AddDuplicateKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "Users",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "TaxRates",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "Suppliers",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "StockIssues",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "StockIssueLines",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "Roles",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "ProductUnitTypes",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "Products",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "ProductPrices",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "ProductMovements",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "Photos",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "Invoices",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "InvoiceLines",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "Customers",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "CurrentAccountMovements",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "CostSlips",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "CostSlipItems",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "Companies",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "ChartOfAccounts",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuplicateKey",
                table: "ChartOfAccountLedger",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DuplicateKey",
                table: "Users",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_DuplicateKey",
                table: "TaxRates",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_DuplicateKey",
                table: "Suppliers",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssues_DuplicateKey",
                table: "StockIssues",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueLines_DuplicateKey",
                table: "StockIssueLines",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_DuplicateKey",
                table: "Roles",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnitTypes_DuplicateKey",
                table: "ProductUnitTypes",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DuplicateKey",
                table: "Products",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_DuplicateKey",
                table: "ProductPrices",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMovements_DuplicateKey",
                table: "ProductMovements",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_DuplicateKey",
                table: "Photos",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_DuplicateKey",
                table: "Invoices",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_DuplicateKey",
                table: "InvoiceLines",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_DuplicateKey",
                table: "Customers",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentAccountMovements_DuplicateKey",
                table: "CurrentAccountMovements",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_CostSlips_DuplicateKey",
                table: "CostSlips",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_CostSlipItems_DuplicateKey",
                table: "CostSlipItems",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_DuplicateKey",
                table: "Companies",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_DuplicateKey",
                table: "ChartOfAccounts",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccountLedger_DuplicateKey",
                table: "ChartOfAccountLedger",
                column: "DuplicateKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_DuplicateKey",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TaxRates_DuplicateKey",
                table: "TaxRates");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_DuplicateKey",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_StockIssues_DuplicateKey",
                table: "StockIssues");

            migrationBuilder.DropIndex(
                name: "IX_StockIssueLines_DuplicateKey",
                table: "StockIssueLines");

            migrationBuilder.DropIndex(
                name: "IX_Roles_DuplicateKey",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnitTypes_DuplicateKey",
                table: "ProductUnitTypes");

            migrationBuilder.DropIndex(
                name: "IX_Products_DuplicateKey",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductPrices_DuplicateKey",
                table: "ProductPrices");

            migrationBuilder.DropIndex(
                name: "IX_ProductMovements_DuplicateKey",
                table: "ProductMovements");

            migrationBuilder.DropIndex(
                name: "IX_Photos_DuplicateKey",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_DuplicateKey",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLines_DuplicateKey",
                table: "InvoiceLines");

            migrationBuilder.DropIndex(
                name: "IX_Customers_DuplicateKey",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CurrentAccountMovements_DuplicateKey",
                table: "CurrentAccountMovements");

            migrationBuilder.DropIndex(
                name: "IX_CostSlips_DuplicateKey",
                table: "CostSlips");

            migrationBuilder.DropIndex(
                name: "IX_CostSlipItems_DuplicateKey",
                table: "CostSlipItems");

            migrationBuilder.DropIndex(
                name: "IX_Companies_DuplicateKey",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_ChartOfAccounts_DuplicateKey",
                table: "ChartOfAccounts");

            migrationBuilder.DropIndex(
                name: "IX_ChartOfAccountLedger_DuplicateKey",
                table: "ChartOfAccountLedger");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "StockIssues");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "StockIssueLines");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "ProductUnitTypes");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "ProductPrices");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "ProductMovements");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "CurrentAccountMovements");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "CostSlips");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "CostSlipItems");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "DuplicateKey",
                table: "ChartOfAccountLedger");
        }
    }
}
