using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInvalidCompanyMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 正式機站台 20（drkinglin，林嘉祈婦產科）同時綁了公司 7 與公司 8，
            // 公司 8 已不存在，導致 Platform 網站存檔被擋（SyncCompanyMappingAsync）。
            // 只軟刪除「公司 8 ↔ 站台 20」這一筆；用 OrgName 確認是同一個站台，
            // 其他環境 Id 對不上時不會動到任何資料。可重跑。
            migrationBuilder.Sql(@"
                UPDATE m
                SET m.IsDeleted = 1,
                    m.DeletionTime = GETDATE()
                FROM MappingCompanyAndWebsites m
                INNER JOIN Websites w ON w.Id = m.FK_WebsiteId AND w.OrgName = 'drkinglin'
                LEFT JOIN Companies c ON c.Id = m.FK_CompanyId
                WHERE m.FK_CompanyId = 8
                  AND m.FK_WebsiteId = 20
                  AND m.IsDeleted = 0
                  AND (c.Id IS NULL OR c.IsDeleted = 1)
                  AND EXISTS (SELECT 1
                              FROM MappingCompanyAndWebsites other
                              INNER JOIN Companies oc ON oc.Id = other.FK_CompanyId AND oc.IsDeleted = 0
                              WHERE other.FK_WebsiteId = m.FK_WebsiteId
                                AND other.Id <> m.Id
                                AND other.IsDeleted = 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 資料修正不還原：被刪的綁定指向不存在或已刪除的公司，還原只會讓 Platform 存檔再次被擋。
        }
    }
}