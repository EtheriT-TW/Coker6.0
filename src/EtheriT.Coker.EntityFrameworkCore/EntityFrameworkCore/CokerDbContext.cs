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
    public class CokerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Website> Websites { get; set; }
        public DbSet<MappingUserAndWebsite> MappingUserAndWebsites { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Prod> Prods { get; set; }
        public DbSet<Marquee> Marquees { get; set; }
        public DbSet<WebMenu> WebMenus { get; set; }
        public DbSet<Order_Header> Order_Headers { get; set; }
        public DbSet<Order_Details> Order_Details { get; set; }
        public DbSet<LogisticsSetting> LogisticsSettings { get; set; }
        public DbSet<Logisticstype> Logisticstypes { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<LogisticsType_PaymentType> LogisticsType_Payments { get; set; }
        public DbSet<ThirdParty> ThirdParties { get; set; }
        public DbSet<ThirdPartyKeypair> ThirdPartyKeypairs { get; set; }
        public DbSet<Prod_Spec> Prod_Specs { get; set; }
        public DbSet<Prod_Spec_Type> Prod_Spec_Types { get; set; }
        public DbSet<Prod_Stock> Prod_Stocks { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<Prod_Log> Prod_Logs { get; set; }
        public DbSet<Html_Content> Html_Contents { get; set; }
        public DbSet<TechnicalCertificate> TechnicalCertificates { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Prod_Price> Prod_Prices { get; set; }

        public CokerDbContext(DbContextOptions<CokerDbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MappingUserAndWebsite>(o =>
            {
                o.HasOne(u => u.User).WithMany(u => u.Webs).HasForeignKey(f => f.UserId);
                o.HasOne(w => w.Website).WithMany(w => w.Users).HasForeignKey(f => f.WebsiteId);
            });
            modelBuilder.Entity<Marquee>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.Marquees).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Token>(o =>
            {
                o.Property(t => t.id).HasDefaultValueSql("newid()").Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
            });
            modelBuilder.Entity<WebMenu>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.WebMenus).HasForeignKey(f => f.FK_WebsiteId);
                o.HasOne(t => t.FK_TopNode).WithMany(u => u.FK_ChildNodes).HasForeignKey(f => f.FK_TopNodeId).IsRequired(false);
            });
            modelBuilder.Entity<Order_Details>(o =>
            {
                o.HasOne(u => u.Order_Header).WithMany(u => u.Order_Details).HasForeignKey(f => f.FK_OId);
                o.HasOne(u => u.ShoppingCart).WithMany(u => u.Order_Details).HasForeignKey(f => f.FK_SCId);
            });
            modelBuilder.Entity<LogisticsSetting>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.LogisticsSettings).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<LogisticsType_PaymentType>(o =>
            {
                o.HasOne(u => u.Logisticstype).WithMany(u => u.LogisticsType_Payments).HasForeignKey(f => f.FK_Lid);
                o.HasOne(u => u.PaymentType).WithMany(u => u.LogisticsType_Payments).HasForeignKey(f => f.FK_Pid);
            });
            modelBuilder.Entity<ThirdPartyKeypair>(o =>
            {
                o.HasOne(u => u.ThirdParty).WithMany(u => u.ThirdPartyKeypair).HasForeignKey(f => f.FK_TPid);
            });
            modelBuilder.Entity<Prod>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.Prods).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Prod_Log>(o =>
            {
                o.HasOne(u => u.Prod).WithMany(u => u.Prod_Logs).HasForeignKey(f => f.FK_Pid);
                o.HasOne(u => u.User).WithMany(u => u.Prod_Logs).HasForeignKey(f => f.FK_Uid);
                o.HasOne(u => u.Token).WithMany(u => u.Prod_Logs).HasForeignKey(f => f.FK_Tid);
            });
            modelBuilder.Entity<Prod_Spec>(o =>
            {
                o.HasOne(u => u.Prod_Spec_Type).WithMany(u => u.Prod_Specs).HasForeignKey(f => f.FK_Tid);
            });
            modelBuilder.Entity<Prod_Spec_Type>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.Prod_Spec_Types).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Prod_Stock>(o =>
            {
                o.HasOne(u => u.Prod).WithMany(u => u.Prod_Stocks).HasForeignKey(f => f.FK_Pid);
            });
            modelBuilder.Entity<ShoppingCart>(o =>
            {
                o.HasOne(u => u.Prod_Stock).WithMany(u => u.ShoppingCarts).HasForeignKey(f => f.FK_PSid);
                o.HasOne(u => u.Token).WithMany(u => u.ShoppingCarts).HasForeignKey(f => f.FK_Tid);
            });
            modelBuilder.Entity<Html_Content>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.Html_Contents).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<TechnicalCertificate>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.TechnicalCertificates).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Prod_Price>(o =>
            {
                o.HasOne(u => u.Prod_Stock).WithMany(u => u.Prod_Prices).HasForeignKey(f => f.FK_PSId);
                o.HasOne(u => u.Role).WithMany(u => u.Prod_Prices).HasForeignKey(f => f.FK_RId);
            });
            base.OnModelCreating(modelBuilder);
            new SeedHelper(modelBuilder).SeedHost();
        }
    }
}
