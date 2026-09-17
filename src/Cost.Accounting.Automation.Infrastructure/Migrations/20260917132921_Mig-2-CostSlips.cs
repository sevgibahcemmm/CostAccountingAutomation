using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig2CostSlips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CostSlips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlipNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CostSlipType = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CostDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProducedProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(MAX)", maxLength: 500, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "money", nullable: false),
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
                    table.PrimaryKey("PK_CostSlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostSlips_ChartOfAccounts_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CostSlips_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CostSlips_Products_ProducedProductId",
                        column: x => x.ProducedProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CostSlipItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostSlipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductUnitTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpenseAccountType = table.Column<byte>(type: "tinyint", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "money", nullable: false),
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
                    table.PrimaryKey("PK_CostSlipItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostSlipItems_CostSlips_CostSlipId",
                        column: x => x.CostSlipId,
                        principalTable: "CostSlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CostSlipItems_ProductUnitTypes_ProductUnitTypeId",
                        column: x => x.ProductUnitTypeId,
                        principalTable: "ProductUnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CostSlipItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostSlipItems_CostSlipId",
                table: "CostSlipItems",
                column: "CostSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_CostSlipItems_ProductId",
                table: "CostSlipItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CostSlipItems_ProductUnitTypeId",
                table: "CostSlipItems",
                column: "ProductUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CostSlips_CostDate",
                table: "CostSlips",
                column: "CostDate");

            migrationBuilder.CreateIndex(
                name: "IX_CostSlips_CostSlipType",
                table: "CostSlips",
                column: "CostSlipType");

            migrationBuilder.CreateIndex(
                name: "IX_CostSlips_CustomerId",
                table: "CostSlips",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CostSlips_ProducedProductId",
                table: "CostSlips",
                column: "ProducedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CostSlips_SlipNumber",
                table: "CostSlips",
                column: "SlipNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CostSlips_Status",
                table: "CostSlips",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CostSlips_WorkshopId",
                table: "CostSlips",
                column: "WorkshopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostSlipItems");

            migrationBuilder.DropTable(
                name: "CostSlips");
        }
    }
}
