using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations.Year
{
    /// <summary>
    /// Yetkili görev tanımlarını enum sütunundan ayrı bir tabloya taşır.
    ///
    /// <para>
    /// Görevler artık kodda sabit değil, veritabanında kayıt olduğu için imza
    /// bloklarında kullanılacak yeni görevler kod değiştirmeden eklenebilir.
    /// Mevcut görevlendirmeler kaybolmaması için eski <c>SigningRole</c>
    /// (tinyint) değerleri yeni tabloya göre <c>SigningRoleId</c> olarak
    /// yazılır.
    /// </para>
    ///
    /// <para>
    /// <c>DuplicateKey</c> bilerek boş bırakılır: bu değer
    /// <c>DuplicateKeyRule.Normalize</c> ile üretilir ve .NET'in büyük harf
    /// dönüşümü SQL <c>UPPER()</c> fonksiyonundan farklıdır (Türkçe collation).
    /// Anahtarlar yıl veritabanı açılırken
    /// <c>YearDatabaseProvisioner</c> tarafından uygulama kuralıyla doldurulur.
    /// </para>
    /// </summary>
    public partial class AddEmployeeSigningRoles : Migration
    {
        /// <summary>
        /// Eski enum (<c>EmployeeSigningRole : byte</c>) değerleri.
        /// Migration'lar donmuş bir geçmiş olduğu için burada isimler tek tek
        /// yazılmıştır; yeni görev tanımları eklenirse bu eşleme değişmez.
        /// </summary>
        private const string SeedSigningRolesSql = @"
INSERT INTO [EmployeeSigningRoles] ([Id], [Name], [Description], [RequiresWorkshop], [SortOrder], [IsActive], [DuplicateKey], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    ('A1000000-0000-0000-0000-000000000001', N'İşyurdu Müdürü',        N'Kurumun en üst yetkilisi; maliyet pusulası onay imzası',                                  0, 10, 1, NULL, SYSUTCDATETIME(), '00000000-0000-0000-0000-000000000000', 0),
    ('A1000000-0000-0000-0000-000000000002', N'Atölye Şefi',          N'Üretimin yapıldığı atölyenin şefi; mamül beyanı ve maliyet pusulası imzası',           1, 20, 1, NULL, SYSUTCDATETIME(), '00000000-0000-0000-0000-000000000000', 0),
    ('A1000000-0000-0000-0000-000000000003', N'Taşınır Kayıt Yetkilisi', N'Taşınır işlem fişi giriş/çıkış kaydını yapan yetkili',                                 0, 30, 1, NULL, SYSUTCDATETIME(), '00000000-0000-0000-0000-000000000000', 0),
    ('A1000000-0000-0000-0000-000000000004', N'Muhasebe Yetkilisi',   N'Mali kayıtları tutan ve onaylayan muhasebe yetkilisi',                                    0, 40, 1, NULL, SYSUTCDATETIME(), '00000000-0000-0000-0000-000000000000', 0),
    ('A1000000-0000-0000-0000-000000000005', N'Harcama Yetkilisi',    N'Harcama onaylayan yetkili',                                                               0, 50, 1, NULL, SYSUTCDATETIME(), '00000000-0000-0000-0000-000000000000', 0),
    ('A1000000-0000-0000-0000-000000000006', N'Sabit Görevli',        N'Belgeyi düzenleyen ve teslim eden sabit kadro görevlisi',                                 0, 60, 1, NULL, SYSUTCDATETIME(), '00000000-0000-0000-0000-000000000000', 0),
    ('A1000000-0000-0000-0000-000000000007', N'Sayım Yapan',         N'Stok sayımını fiilen gerçekleştiren personel',                                            0, 70, 1, NULL, SYSUTCDATETIME(), '00000000-0000-0000-0000-000000000000', 0),
    ('A1000000-0000-0000-0000-000000000008', N'Kontrol Eden',         N'Sayım sonucunu kontrol edip onaylayan personel',                                           0, 80, 1, NULL, SYSUTCDATETIME(), '00000000-0000-0000-0000-000000000000', 0);
";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeSigningRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(MAX)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(MAX)", maxLength: 500, nullable: false),
                    RequiresWorkshop = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EmployeeSigningRoles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSigningRoles_DuplicateKey",
                table: "EmployeeSigningRoles",
                column: "DuplicateKey");

            migrationBuilder.Sql(SeedSigningRolesSql);

            // Yeni sütun geçiş sırasında boş olabilir; görevlendirmeler
            // doldurulduktan sonra zorunluya çevrilir.
            migrationBuilder.AddColumn<Guid>(
                name: "SigningRoleId",
                table: "EmployeeDuties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(BuildBackfillSql());

            migrationBuilder.AlterColumn<Guid>(
                name: "SigningRoleId",
                table: "EmployeeDuties",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDuties_SigningRoleId",
                table: "EmployeeDuties",
                column: "SigningRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDuties_EmployeeSigningRoles_SigningRoleId",
                table: "EmployeeDuties",
                column: "SigningRoleId",
                principalTable: "EmployeeSigningRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "SigningRole",
                table: "EmployeeDuties");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDuties_EmployeeSigningRoles_SigningRoleId",
                table: "EmployeeDuties");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDuties_SigningRoleId",
                table: "EmployeeDuties");

            migrationBuilder.AddColumn<byte>(
                name: "SigningRole",
                table: "EmployeeDuties",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.Sql(BuildRestoreLegacySql());

            migrationBuilder.DropColumn(
                name: "SigningRoleId",
                table: "EmployeeDuties");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSigningRoles_DuplicateKey",
                table: "EmployeeSigningRoles");

            migrationBuilder.DropTable(
                name: "EmployeeSigningRoles");
        }

        /// <summary>
        /// Eski enum değerlerini yeni görev kayıtlarına bağlar. Rol adı üzerinden
        /// eşleme yapılır; böylece kimlik değerleri veritabanı tarafından
        /// üretilmiş olsa da dönüşüm doğru çalışır.
        /// </summary>
        private static string BuildBackfillSql()
        {
            (byte LegacyValue, string RoleName)[] map =
            [
                (1, "Sabit Görevli"),
                (2, "Atölye Şefi"),
                (3, "Taşınır Kayıt Yetkilisi"),
                (4, "İşyurdu Müdürü"),
                (5, "Muhasebe Yetkilisi"),
                (6, "Harcama Yetkilisi"),
                (7, "Sayım Yapan"),
                (8, "Kontrol Eden")
            ];

            var sql = new System.Text.StringBuilder();

            sql.AppendLine(@"
IF EXISTS (SELECT 1 FROM [EmployeeDuties] WHERE [SigningRole] IS NULL OR [SigningRole] NOT IN (1, 2, 3, 4, 5, 6, 7, 8))
BEGIN
    ;THROW 51000, 'EmployeeDuties tablosunda tanimsiz bir SigningRole degeri var; gorev gecisi yapilamadi.', 1;
END;
");

            foreach ((byte legacyValue, string roleName) in map)
            {
                sql.AppendLine($@"
UPDATE d
   SET d.[SigningRoleId] = r.[Id]
  FROM [EmployeeDuties] AS d
  INNER JOIN [EmployeeSigningRoles] AS r ON r.[Name] = N'{roleName}'
 WHERE d.[SigningRole] = {legacyValue};
");
            }

            return sql.ToString();
        }

        /// <summary>
        /// Geri alma: yeni görev tanımı eski enum değerine eşlenemiyorsa satır
        /// silinmez, <c>SigningRole = 0</c> (tanımsız) olarak yazılır. Böylece
        /// veri kaybı olmaz; eski sürümde de tanımsız görev imza satırına
        /// basamazdı.
        /// </summary>
        private static string BuildRestoreLegacySql()
        {
            (byte LegacyValue, string RoleName)[] map =
            [
                (1, "Sabit Görevli"),
                (2, "Atölye Şefi"),
                (3, "Taşınır Kayıt Yetkilisi"),
                (4, "İşyurdu Müdürü"),
                (5, "Muhasebe Yetkilisi"),
                (6, "Harcama Yetkilisi"),
                (7, "Sayım Yapan"),
                (8, "Kontrol Eden")
            ];

            var sql = new System.Text.StringBuilder();

            foreach ((byte legacyValue, string roleName) in map)
            {
                sql.AppendLine($@"
UPDATE d
   SET d.[SigningRole] = {legacyValue}
  FROM [EmployeeDuties] AS d
  INNER JOIN [EmployeeSigningRoles] AS r ON r.[Id] = d.[SigningRoleId]
 WHERE r.[Name] = N'{roleName}';
");
            }

            return sql.ToString();
        }
    }
}
