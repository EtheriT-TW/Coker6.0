using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.EntityFrameworkCore.Migrations.Seed
{
    public static class RoleSeed
    {
        public static void Seed(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Role>().HasData(
                   new Role
                   {
                       Id = 1,
                       Name = "系統總管理者",
                       Type = 0,
                       IsDeleted = false,
                       CreatorUserId = 1,
                       CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                   }
               );
            modelBuilder.Entity<MappingUserAndRole>().HasData(
                new MappingUserAndRole
                {
                    Id = 1,
                    RoleId = 1,
                    UserId = 1,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }
            );
        }
    }
}
