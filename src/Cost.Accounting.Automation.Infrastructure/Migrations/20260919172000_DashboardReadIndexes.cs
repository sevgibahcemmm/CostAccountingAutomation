using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DashboardReadIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProductMovements_Date_MovementType",
                table: "ProductMovements",
                columns: new[] { "Date", "MovementType" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductMovements_ProductId_Date",
                table: "ProductMovements",
                columns: new[] { "ProductId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Date_Status",
                table: "Invoices",
                columns: new[] { "Date", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CurrentAccountMovements_CurrentAccountType_CustomerId",
                table: "CurrentAccountMovements",
                columns: new[] { "CurrentAccountType", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_CurrentAccountMovements_CurrentAccountType_SupplierId",
                table: "CurrentAccountMovements",
                columns: new[] { "CurrentAccountType", "SupplierId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductMovements_Date_MovementType",
                table: "ProductMovements");

            migrationBuilder.DropIndex(
                name: "IX_ProductMovements_ProductId_Date",
                table: "ProductMovements");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_Date_Status",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_CurrentAccountMovements_CurrentAccountType_CustomerId",
                table: "CurrentAccountMovements");

            migrationBuilder.DropIndex(
                name: "IX_CurrentAccountMovements_CurrentAccountType_SupplierId",
                table: "CurrentAccountMovements");
        }
    }
}
