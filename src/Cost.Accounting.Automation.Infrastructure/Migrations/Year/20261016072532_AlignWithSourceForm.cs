using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations.Year
{
    /// <inheritdoc />
    public partial class AlignWithSourceForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequireApprovedOffer",
                table: "DirectProcurementFiles",
                newName: "RequireFinalOffer");

            migrationBuilder.AddColumn<int>(
                name: "DecimalPlaces",
                table: "DirectProcurementFiles",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "UnitPriceDecimalPlaces",
                table: "DirectProcurementFiles",
                type: "int",
                nullable: false,
                defaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecimalPlaces",
                table: "DirectProcurementFiles");

            migrationBuilder.DropColumn(
                name: "UnitPriceDecimalPlaces",
                table: "DirectProcurementFiles");

            migrationBuilder.RenameColumn(
                name: "RequireFinalOffer",
                table: "DirectProcurementFiles",
                newName: "RequireApprovedOffer");
        }
    }
}
