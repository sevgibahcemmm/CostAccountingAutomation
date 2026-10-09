using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddAppReleases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppReleases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    VersionSort = table.Column<long>(type: "bigint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppReleases", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppReleases_Version",
                table: "AppReleases",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppReleases_VersionSort",
                table: "AppReleases",
                column: "VersionSort");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppReleases");
        }
    }
}
