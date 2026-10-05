using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations.Year
{
    /// <inheritdoc />
    public partial class AddRowVersionConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TaxRates",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Suppliers",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StockIssues",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StockIssueLines",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Recipes",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RecipeItems",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ProductUnitTypes",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Products",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ProductPrices",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ProductMovements",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Photos",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Invoices",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "InvoiceLines",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EmployeeSigningRoles",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Employees",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EmployeeDuties",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DevirLogs",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Customers",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CurrentAccountMovements",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CostSlips",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CostSlipItems",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ChartOfAccounts",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ChartOfAccountLedger",
                type: "rowversion",
                maxLength: 8,
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StockIssues");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StockIssueLines");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RecipeItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ProductUnitTypes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ProductPrices");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ProductMovements");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EmployeeSigningRoles");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EmployeeDuties");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DevirLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CurrentAccountMovements");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CostSlips");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CostSlipItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ChartOfAccountLedger");
        }
    }
}
