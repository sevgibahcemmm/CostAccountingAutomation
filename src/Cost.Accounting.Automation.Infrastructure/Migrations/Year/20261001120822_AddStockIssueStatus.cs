using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations.Year
{
    /// <inheritdoc />
    public partial class AddStockIssueStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut kayitlar TASLAK kabul edilir; enum'da Draft = 1. Varsayilan
            // deger 0 olsaydi mevcut tum belgeler gecersiz bir duruma dusardi.
            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "StockIssues",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "StockIssues");
        }
    }
}
