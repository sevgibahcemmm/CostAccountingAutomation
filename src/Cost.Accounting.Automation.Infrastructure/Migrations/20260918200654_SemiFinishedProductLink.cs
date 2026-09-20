using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SemiFinishedProductLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SemiFinishedProductId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SemiFinishedProductId",
                table: "Products",
                column: "SemiFinishedProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Products_SemiFinishedProductId",
                table: "Products",
                column: "SemiFinishedProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Products_SemiFinishedProductId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SemiFinishedProductId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SemiFinishedProductId",
                table: "Products");
        }
    }
}
