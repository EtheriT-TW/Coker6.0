using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtheriT.Coker.Web.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.Migrations.Seed;
using EtheriT.Coker.Core.Models;

namespace EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore
{
    public class CokerDbContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Website> Websites { get; set; }
        public DbSet<MappingUserAndWebsite> MappingUserAndWebsites { get; set; }
        public DbSet<Prod> Prods { get; set; }
        public DbSet<Marquee> Marquees { get; set; }
        
        public CokerDbContext(DbContextOptions<CokerDbContext> options)
        : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<MappingUserAndWebsite>(o => {
                o.HasOne(u => u.User).WithMany(u => u.Webs).HasForeignKey(f => f.UserId);
                o.HasOne(w => w.Website).WithMany(w => w.Users).HasForeignKey(f => f.WebsiteId);
            });
            modelBuilder.Entity<Marquee>(o => {
                o.HasOne(u => u.Website).WithMany(u => u.Marquees).HasForeignKey(f => f.WebsiteId);
            });
            base.OnModelCreating(modelBuilder);
            new SeedHelper(modelBuilder).SeedHost();
        }
    }
}
