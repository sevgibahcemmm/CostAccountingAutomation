using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedUserTfaProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TFACode_Value",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TFAConfirmCode_Value",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TFAExpiresDate_Value",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TFAIsCompleted_Value",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TFAStatus_Value",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TFACode_Value",
                table: "Users",
                type: "nvarchar(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TFAConfirmCode_Value",
                table: "Users",
                type: "nvarchar(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TFAExpiresDate_Value",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TFAIsCompleted_Value",
                table: "Users",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TFAStatus_Value",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
