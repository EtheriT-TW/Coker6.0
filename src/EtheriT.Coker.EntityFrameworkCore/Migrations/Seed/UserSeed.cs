using EtheriT.Coker.Web.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.EntityFrameworkCore.Migrations.Seed
{
    public static class UserSeed
    {
        public static void Seed(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>().HasData(
                   new User()
                   {
                       Id = 1,
                       Name = "易碩網際科技科技股份有限公司",
                       Account = "EtheriT",
                       CellPhone = "0906801568",
                       Email = "service@ether.com.tw",
                       Password = "AQAAAAEAACcQAAAAEE3X/SrNcUs6zaH9K+51XEMp8G2z3r9d/5SYuLJpKy3TlYNX7DdHF6PDW8NxWk7CWg==", //123qwe
                       CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1328)
                   }, new User()
                   {
                       Id = 2,
                       Name = "隆昌窯業",
                       Account = "lcb",
                       CellPhone = "0920497649",
                       Email = "lienmienchou@evergreen.com.tw",
                       Password = "AQAAAAEAACcQAAAAEE3X/SrNcUs6zaH9K+51XEMp8G2z3r9d/5SYuLJpKy3TlYNX7DdHF6PDW8NxWk7CWg==", //123qwe
                       CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1338)
                   }
                );
        }
    }
}
