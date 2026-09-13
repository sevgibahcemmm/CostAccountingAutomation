using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxRateLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(MAX)", maxLength: 120, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_TaxRates", x => x.Id);
                });

            // Mevcut ürünlerdeki KDV oranlarını TaxRates tablosuna kayıt olarak taşı
            migrationBuilder.Sql(@"
INSERT INTO [TaxRates] (Id, Name, Rate, IsActive, CreatedAt, CreatedBy, IsDeleted, DeletedAt, DeletedBy)
SELECT NEWID(),
       'KDV %' + CAST(CAST(FLOOR(Rate * 100 + 0.5) AS int) AS varchar(10)),
       Rate, 1, SYSDATETIMEOFFSET(), (SELECT TOP 1 Id FROM [Users]), 0, NULL, NULL
FROM (SELECT DISTINCT TaxRate AS Rate FROM [Products]) AS r;");

            migrationBuilder.AddColumn<Guid>(
                name: "TaxRateId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Ürünleri taşınan KDV oranı kaydına bağla
            migrationBuilder.Sql(@"
UPDATE [Products] SET TaxRateId = (SELECT TOP 1 Id FROM [TaxRates] WHERE Rate = [Products].TaxRate);");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TaxRateId",
                table: "Products",
                column: "TaxRateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_TaxRates_TaxRateId",
                table: "Products",
                column: "TaxRateId",
                principalTable: "TaxRates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "Products",
                type: "money",
                nullable: false,
                defaultValue: 0m);

            // Oranı tekrar ürünün decimal alanına geri taşı
            migrationBuilder.Sql(@"
UPDATE [Products] SET TaxRate = (SELECT TOP 1 Rate FROM [TaxRates] WHERE Id = [Products].TaxRateId);");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_TaxRates_TaxRateId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TaxRateId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TaxRateId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "TaxRates");
        }
    }
}