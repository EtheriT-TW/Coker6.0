using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations;

public partial class AddAiMemoryStorage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AiMemoryEntries",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ScopeKind = table.Column<byte>(type: "tinyint", nullable: false),
                WebsiteId = table.Column<long>(type: "bigint", nullable: true),
                UserId = table.Column<long>(type: "bigint", nullable: true),
                Feature = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Category = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                ScopeKey = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                MemoryKey = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ContentHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                SourceHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                RuntimeHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                SchemaVersion = table.Column<int>(type: "int", nullable: false),
                State = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                Version = table.Column<long>(type: "bigint", nullable: false),
                ConcurrencyToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AiMemoryEntries", x => x.Id);
                table.CheckConstraint("CK_AiMemoryEntries_Scope", "([ScopeKind] = 0 AND [WebsiteId] IS NULL AND [UserId] IS NULL) OR ([ScopeKind] = 1 AND [WebsiteId] IS NOT NULL AND [UserId] IS NULL) OR ([ScopeKind] = 2 AND [WebsiteId] IS NOT NULL AND [UserId] IS NOT NULL)");
                table.ForeignKey("FK_AiMemoryEntries_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_AiMemoryEntries_Websites_WebsiteId", x => x.WebsiteId, "Websites", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateTable(
            name: "AiMemoryRevisions",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                EntryId = table.Column<long>(type: "bigint", nullable: false),
                Version = table.Column<long>(type: "bigint", nullable: false),
                Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ContentHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                SourceHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                RuntimeHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                SchemaVersion = table.Column<int>(type: "int", nullable: false),
                State = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                ActorUserId = table.Column<long>(type: "bigint", nullable: true),
                RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AiMemoryRevisions", x => x.Id);
                table.ForeignKey("FK_AiMemoryRevisions_AiMemoryEntries_EntryId", x => x.EntryId, "AiMemoryEntries", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_AiMemoryRevisions_Users_ActorUserId", x => x.ActorUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex("IX_AiMemoryEntries_ScopeKey_MemoryKey", "AiMemoryEntries", new[] { "ScopeKey", "MemoryKey" }, unique: true);
        migrationBuilder.CreateIndex("IX_AiMemoryEntries_ScopeKind_WebsiteId_UserId_UpdatedAtUtc", "AiMemoryEntries", new[] { "ScopeKind", "WebsiteId", "UserId", "UpdatedAtUtc" });
        migrationBuilder.CreateIndex("IX_AiMemoryEntries_UserId", "AiMemoryEntries", "UserId");
        migrationBuilder.CreateIndex("IX_AiMemoryEntries_WebsiteId", "AiMemoryEntries", "WebsiteId");
        migrationBuilder.CreateIndex("IX_AiMemoryRevisions_EntryId_Version", "AiMemoryRevisions", new[] { "EntryId", "Version" }, unique: true);
        migrationBuilder.CreateIndex("IX_AiMemoryRevisions_ActorUserId", "AiMemoryRevisions", "ActorUserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AiMemoryRevisions");
        migrationBuilder.DropTable("AiMemoryEntries");
    }
}
