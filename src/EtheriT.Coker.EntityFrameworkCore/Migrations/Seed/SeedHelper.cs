using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;

namespace EtheriT.Coker.EntityFrameworkCore.Migrations.Seed
{
    public class SeedHelper
    {
        private readonly ModelBuilder modelBuilder;
        public SeedHelper(ModelBuilder modelBuilder)
        {
            this.modelBuilder = modelBuilder;
        }
        public void SeedHost()
        {
            UserSeed.Seed(modelBuilder);
            WebsiteSeed.Seed(modelBuilder);
            ProdSeed.Seed(modelBuilder);
            ObjectTypeSeed.Seed(modelBuilder);
            StoreSetSeed.Seed(modelBuilder);
            RoleSeed.Seed(modelBuilder);
            ThirdPartySeed.Seed(modelBuilder);
        }
    }
}
