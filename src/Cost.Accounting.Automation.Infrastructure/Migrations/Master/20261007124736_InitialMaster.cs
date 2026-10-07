using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cost.Accounting.Automation.Infrastructure.Migrations.Master
{
    /// <inheritdoc />
    public partial class InitialMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    TaxOffice_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    TaxNumber_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Description_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Invoiceinformation_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Letterhead_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    CompanyPrefix_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Address_City = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Address_District = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Address_FullAddress = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Contact_PhoneNumber1 = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Contact_PhoneNumber2 = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Contact_Email = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    ExpenditureUnit_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    ExpenditureUnit_Code = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    AccountingUnit_Name = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    AccountingUnit_Code = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyYears",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    DatabaseName = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    OpeningDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoginTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive_Value = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresDate_Value = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPresences",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionStartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastHeartbeatAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LoggedOutAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPresences", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    LastName_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    FullName_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Email_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    UserName_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Password_PasswordHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Password_PasswordSalt = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Password_HashIterations = table.Column<int>(type: "int", nullable: false),
                    ForgotPasswordCode_Value = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ForgotPasswordDate_Value = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsForgotPasswordCompleted_Value = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TcNo_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    RegistryNumber = table.Column<string>(type: "nvarchar(MAX)", maxLength: 50, nullable: true),
                    AvatarPath = table.Column<string>(type: "nvarchar(MAX)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => new { x.RoleId, x.Id });
                    table.ForeignKey(
                        name: "FK_Permission_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationClears",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CounterpartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsAnnouncementChannel = table.Column<bool>(type: "bit", nullable: false),
                    ClearedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationClears", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationClears_Users_CounterpartId",
                        column: x => x.CounterpartId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConversationClears_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(MAX)", maxLength: 400, nullable: true),
                    AttachmentFileName = table.Column<string>(type: "nvarchar(MAX)", maxLength: 260, nullable: true),
                    AttachmentSize = table.Column<long>(type: "bigint", nullable: true),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Body_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    IsAnnouncement = table.Column<bool>(type: "bit", nullable: false),
                    ReadState_Value = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EditedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", maxLength: 8, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_DuplicateKey",
                table: "Companies",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyYears_CompanyId_Year",
                table: "CompanyYears",
                columns: new[] { "CompanyId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyYears_DatabaseName",
                table: "CompanyYears",
                column: "DatabaseName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyYears_DuplicateKey",
                table: "CompanyYears",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationClears_CounterpartId",
                table: "ConversationClears",
                column: "CounterpartId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationClears_DuplicateKey",
                table: "ConversationClears",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationClears_User_Counterpart_Channel",
                table: "ConversationClears",
                columns: new[] { "UserId", "CounterpartId", "IsAnnouncementChannel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_DuplicateKey",
                table: "Messages",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Recipient_Sender_CreatedAt",
                table: "Messages",
                columns: new[] { "RecipientId", "SenderId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RecipientId",
                table: "Messages",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Sender_Recipient_CreatedAt",
                table: "Messages",
                columns: new[] { "SenderId", "RecipientId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_DuplicateKey",
                table: "Roles",
                column: "DuplicateKey");

            migrationBuilder.CreateIndex(
                name: "IX_UserPresences_LastHeartbeatAt",
                table: "UserPresences",
                column: "LastHeartbeatAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DuplicateKey",
                table: "Users",
                column: "DuplicateKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyYears");

            migrationBuilder.DropTable(
                name: "ConversationClears");

            migrationBuilder.DropTable(
                name: "LoginTokens");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "UserPresences");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
