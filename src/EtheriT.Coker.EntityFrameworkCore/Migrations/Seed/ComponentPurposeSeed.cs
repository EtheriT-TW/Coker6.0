using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.EntityFrameworkCore.Migrations.Seed
{
    public static class ComponentPurposeSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ComponentPurpose>().HasData(
                new ComponentPurpose
                {
                    Id = 1,
                    Code = "product-import-directory",
                    Name = "商品匯入目錄",
                    SerNo = 10,
                    Visible = true,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2026, 7, 17, 0, 0, 0, DateTimeKind.Local),
                }
            );
        }
    }
}
