using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations.Year
{
    /// <inheritdoc />
    public partial class AddRecipeWorkshop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkshopId",
                table: "Recipes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_WorkshopId",
                table: "Recipes",
                column: "WorkshopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_ChartOfAccounts_WorkshopId",
                table: "Recipes",
                column: "WorkshopId",
                principalTable: "ChartOfAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_ChartOfAccounts_WorkshopId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_WorkshopId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "Recipes");
        }
    }
}
