using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations.Year
{
    /// <inheritdoc />
    public partial class AddDevirLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevirLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceYear = table.Column<int>(type: "int", nullable: false),
                    TargetYear = table.Column<int>(type: "int", nullable: false),
                    SourceDatabaseName = table.Column<string>(type: "nvarchar(MAX)", maxLength: 128, nullable: false),
                    TargetDatabaseName = table.Column<string>(type: "nvarchar(MAX)", maxLength: 128, nullable: false),
                    ChartOfAccountsAdded = table.Column<int>(type: "int", nullable: false),
                    ChartOfAccountsSkipped = table.Column<int>(type: "int", nullable: false),
                    CustomersAdded = table.Column<int>(type: "int", nullable: false),
                    SuppliersAdded = table.Column<int>(type: "int", nullable: false),
                    ProductsAdded = table.Column<int>(type: "int", nullable: false),
                    ProductPricesAdded = table.Column<int>(type: "int", nullable: false),
                    ProductPhotosAdded = table.Column<int>(type: "int", nullable: false),
                    RecipesAdded = table.Column<int>(type: "int", nullable: false),
                    CurrentAccountBalancesAdded = table.Column<int>(type: "int", nullable: false),
                    StockBalancesAdded = table.Column<int>(type: "int", nullable: false),
                    ChartBalancesAdded = table.Column<int>(type: "int", nullable: false),
                    TotalAdded = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
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
                    table.PrimaryKey("PK_DevirLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevirLogs_DuplicateKey",
                table: "DevirLogs",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_DevirLogs_TargetYear",
                table: "DevirLogs",
                column: "TargetYear");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevirLogs");
        }
    }
}
