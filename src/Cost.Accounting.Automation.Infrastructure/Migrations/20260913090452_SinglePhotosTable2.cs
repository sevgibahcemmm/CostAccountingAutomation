using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SinglePhotosTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "Photos");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Photos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "OwnerType",
                table: "Photos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Photos",
                type: "nvarchar(MAX)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "Photos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("UPDATE Photos SET OwnerType = 'User' WHERE OwnerType = '' OR OwnerType IS NULL;");

            migrationBuilder.Sql(@"
INSERT INTO Photos (Id, ProductId, OwnerType, FileName, ContentType, Path, IsDefault, CreatedAt, CreatedBy, IsActive, UpdatedAt, UpdatedBy, IsDeleted, DeletedAt, DeletedBy)
SELECT
    Id,
    ProductId,
    'Product',
    RIGHT(Path, CHARINDEX('\', REVERSE(Path)) - 1),
    CASE LOWER(RIGHT(Path, CHARINDEX('.', REVERSE(Path))))
        WHEN '.jpg' THEN 'image/jpeg'
        WHEN '.jpeg' THEN 'image/jpeg'
        WHEN '.png' THEN 'image/png'
        WHEN '.gif' THEN 'image/gif'
        WHEN '.bmp' THEN 'image/bmp'
        WHEN '.webp' THEN 'image/webp'
        ELSE 'application/octet-stream'
    END,
    Path,
    IsPrimary,
    CreatedAt,
    CreatedBy,
    IsActive,
    UpdatedAt,
    UpdatedBy,
    IsDeleted,
    DeletedAt,
    DeletedBy
FROM ProductImages;");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_ProductId",
                table: "Photos",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Products_ProductId",
                table: "Photos",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Products_ProductId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_ProductId",
                table: "Photos");

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
INSERT INTO ProductImages (Id, ProductId, CreatedAt, CreatedBy, DeletedAt, DeletedBy, IsActive, IsDeleted, IsPrimary, Path, UpdatedAt, UpdatedBy)
SELECT Id, ProductId, CreatedAt, CreatedBy, DeletedAt, DeletedBy, IsActive, IsDeleted, IsDefault, Path, UpdatedAt, UpdatedBy
FROM Photos
WHERE OwnerType = 'Product' AND ProductId IS NOT NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.Sql("DELETE FROM Photos WHERE OwnerType = 'Product';");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Photos");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Photos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "Photos",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}