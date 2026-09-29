using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.EntityFrameworkCore.Migrations.Seed;

public static class PlatformRoleSeed
{
    private static readonly DateTime SeedTime =
        new(2026, 9, 29, 0, 0, 0, DateTimeKind.Local);

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlatformRole>().HasData(
            new
            {
                Id = 1L,
                Code = PlatformRoleCodes.Administrator,
                Name = "Platform 總管理者",
                Description = "可管理 Platform 所有功能、角色與伺服器操作。",
                IsEnabled = true,
                Sort = 10,
                CreatorUserId = 1L,
                CreationTime = SeedTime,
                IsDeleted = false
            },
            new
            {
                Id = 2L,
                Code = PlatformRoleCodes.DataManager,
                Name = "網站資料管理人員",
                Description = "可管理客戶、網站與網域資料，不可操作伺服器或管理 Platform 角色。",
                IsEnabled = true,
                Sort = 20,
                CreatorUserId = 1L,
                CreationTime = SeedTime,
                IsDeleted = false
            },
            new
            {
                Id = 3L,
                Code = PlatformRoleCodes.ServerOperator,
                Name = "伺服器操作管理員",
                Description = "可檢視監控並執行經確認的主機、IIS、DNS 與 SSL 操作。",
                IsEnabled = true,
                Sort = 30,
                CreatorUserId = 1L,
                CreationTime = SeedTime,
                IsDeleted = false
            });

        // 新環境必須先有一位可分配 Platform 角色的總管理者。
        // UserId = 1 為 UserSeed 的 EtheriT 系統帳號。
        modelBuilder.Entity<MappingUserAndPlatformRole>().HasData(
            new
            {
                Id = 1L,
                UserId = 1L,
                PlatformRoleId = 1L,
                CreatorUserId = 1L,
                CreationTime = SeedTime,
                IsDeleted = false
            });
    }
}
