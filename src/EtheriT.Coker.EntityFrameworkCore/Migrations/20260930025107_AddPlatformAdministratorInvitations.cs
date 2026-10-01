using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformAdministratorInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformAdministratorInvitations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    MvcRoleId = table.Column<long>(type: "bigint", nullable: false),
                    PlatformRoleId = table.Column<long>(type: "bigint", nullable: false),
                    InvitedEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailVerifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformAdministratorInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformAdministratorInvitations_PlatformRoles_PlatformRoleId",
                        column: x => x.PlatformRoleId,
                        principalTable: "PlatformRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlatformAdministratorInvitations_Roles_MvcRoleId",
                        column: x => x.MvcRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlatformAdministratorInvitations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAdministratorInvitations_InvitedEmail_ApprovedAtUtc_RevokedAtUtc",
                table: "PlatformAdministratorInvitations",
                columns: new[] { "InvitedEmail", "ApprovedAtUtc", "RevokedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAdministratorInvitations_MvcRoleId",
                table: "PlatformAdministratorInvitations",
                column: "MvcRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAdministratorInvitations_PlatformRoleId",
                table: "PlatformAdministratorInvitations",
                column: "PlatformRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAdministratorInvitations_UserId",
                table: "PlatformAdministratorInvitations",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformAdministratorInvitations");
        }
    }
}
