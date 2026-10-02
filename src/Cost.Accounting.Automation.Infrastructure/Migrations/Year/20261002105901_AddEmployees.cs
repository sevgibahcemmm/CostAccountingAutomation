using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations.Year
{
    /// <inheritdoc />
    public partial class AddEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(MAX)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(MAX)", maxLength: 100, nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(MAX)", maxLength: 200, nullable: false),
                    PhoneNumber1 = table.Column<string>(type: "nvarchar(MAX)", maxLength: 50, nullable: false),
                    PhoneNumber2 = table.Column<string>(type: "nvarchar(MAX)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(MAX)", maxLength: 200, nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(MAX)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDuties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SigningRole = table.Column<byte>(type: "tinyint", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeDuties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDuties_ChartOfAccounts_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeDuties_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDuties_DuplicateKey",
                table: "EmployeeDuties",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDuties_EmployeeId",
                table: "EmployeeDuties",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDuties_WorkshopId",
                table: "EmployeeDuties",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CreatedBy",
                table: "Employees",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DuplicateKey",
                table: "Employees",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IdentityNumber",
                table: "Employees",
                column: "IdentityNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UpdatedBy",
                table: "Employees",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDuties");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
