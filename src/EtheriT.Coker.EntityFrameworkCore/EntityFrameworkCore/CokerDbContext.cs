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
using Directory = EtheriT.Coker.Core.Models.Directory;
using System.Text.Json.Nodes;

namespace EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore
{
    public class CokerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<FrontUser> FrontUsers { get; set; }
        public DbSet<Account_Log> Account_Logs  { get; set; }
        public DbSet<Website> Websites { get; set; }
        public DbSet<MappingUserAndWebsite> MappingUserAndWebsites { get; set; }
        public DbSet<MappingFrontUserAndWebsite> MappingFrontUserAndWebsite { get; set; }
        public DbSet<MappingOldNewUUID> MappingOldNewUUID { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Prod> Prods { get; set; }
        public DbSet<Marquee> Marquees { get; set; }
        public DbSet<WebMenu> WebMenus { get; set; }
        public DbSet<Order_Header> Order_Headers { get; set; }
        public DbSet<Order_Details> Order_Details { get; set; }
        public DbSet<LogisticsSetting> LogisticsSettings { get; set; }
        public DbSet<Logisticstype> Logisticstypes { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<PaymentTypesValue> PaymentTypesValues { get; set; }
        public DbSet<LogisticsType_PaymentType> LogisticsType_Payments { get; set; }
        public DbSet<ThirdParty> ThirdParties { get; set; }
        public DbSet<ThirdPartyKeypair> ThirdPartyKeypairs { get; set; }
        public DbSet<ThirdPartyKeypairValue> ThirdPartyKeypairValues { get; set; }
        public DbSet<Prod_Spec> Prod_Specs { get; set; }
        public DbSet<Prod_Spec_Type> Prod_Spec_Types { get; set; }
        public DbSet<Prod_Stock> Prod_Stocks { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<SearchLog> SearchLogs { get; set; }
        public DbSet<Prod_Log> Prod_Logs { get; set; }
        public DbSet<Html_Content> Html_Contents { get; set; }
        public DbSet<TechnicalCertificate> TechnicalCertificates { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Prod_Price> Prod_Prices { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<MappingUserAndRole> MappingUserAndRoles { get; set; }
        public DbSet<Prod_TechCert> Prod_TechCerts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Tag_Associate> Tag_Associates { get; set; }
        public DbSet<Tag_Group> Tag_Groups { get; set; }
        public DbSet<Tag_TagGroup> Tag_TagGroups { get; set; }
        public DbSet<FileUpload> FileUploads { get; set; }
        public DbSet<FileBind> FileBinds { get; set; }
        public DbSet<FileBindMore> FileBindMores { get; set; }
        public DbSet<ObjectType> ObjectTypes { get; set; }
        public DbSet<MappingWebsiteRelationship> MappingWebsiteRelationship { get; set; }
        public DbSet<Advertise> Advertise { get; set; }
        public DbSet<Advertise_Log> Advertise_Logs { get; set; }
        public DbSet<Article> Article { get; set; }
        public DbSet<Directory> Directory { get; set; }
        public DbSet<StoreSetGroup> StoreSetGroup { get; set; }
        public DbSet<StoreSet> StoreSet { get; set; }
        public DbSet<StoreSetDetail> StoreSetDetail { get; set; }
        public DbSet<storeSetItem> StoreSetItems { get; set; }
        public DbSet<CustSearch> CustSearch { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<MappingCompanyAndWebsites> MappingCompanyAndWebsites { get; set; }
        public DbSet<Recipient> Recipients { get; set; }
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<PermissionDetail> PermissionDetail { get; set; }
        public DbSet<Remote> Remotes { get; set; }
        public DbSet<NotFoundImage> NotFoundImage { get; set; }
        public DbSet<Core.Models.JsonObject> JsonObjects { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        public CokerDbContext(DbContextOptions<CokerDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Website>(o =>
            {
                o.Property(w => w.Level).HasDefaultValue(1).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
            });
            modelBuilder.Entity<MappingUserAndWebsite>(o =>
            {
                o.HasOne(u => u.User).WithMany(u => u.Webs).HasForeignKey(f => f.UserId);
                o.HasOne(w => w.Website).WithMany(w => w.Users).HasForeignKey(f => f.WebsiteId);
            });
            modelBuilder.Entity<MappingFrontUserAndWebsite>(o =>
            {
                o.HasOne(u => u.User).WithMany(u => u.Websites).HasForeignKey(f => f.FK_UserId);
                o.HasOne(w => w.Website).WithMany(w => w.FrontUsers).HasForeignKey(f => f.FK_WebsiteId);
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
                o.Property(m => m.VisibleHeader).HasDefaultValue(true);
                o.Property(m => m.VisibleFooter).HasDefaultValue(true);
                o.Property(m => m.VisibleTitle).HasDefaultValue(true);
                o.Property(m => m.ShowToMenu).HasDefaultValue(true);
                o.Property(m => m.RemovedFromShelves).HasDefaultValue(false);
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
            modelBuilder.Entity<ThirdPartyKeypairValue>(o =>
            {
                o.HasOne(u => u.ThirdPartyKeypair).WithMany(u => u.thirdPartyKeypairValues).HasForeignKey(f => f.FK_ThirdPartyKeypairId);
            });
            modelBuilder.Entity<PaymentTypesValue>(o =>
            {
                o.HasOne(u => u.paymentType).WithMany(u => u.paymentTypesValues).HasForeignKey(f => f.FK_PaymentTypesId);
            });
            modelBuilder.Entity<Prod>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.Prods).HasForeignKey(f => f.FK_WebsiteId);
                o.Property(p => p.Visible).HasDefaultValue(true);
                o.Property(p => p.RemovedFromShelves).HasDefaultValue(false);
                o.Property(p => p.Status).HasDefaultValue(0);
            });

            modelBuilder.Entity<Prod_TechCert>(o =>
            {
                o.HasOne(u => u.Prod).WithMany(u => u.TechnicalCertificates).HasForeignKey(f => f.FK_PId);
                o.HasOne(u => u.TechnicalCertificate).WithMany(u => u.prods).HasForeignKey(f => f.FK_TCId);
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
            modelBuilder.Entity<SearchLog>(o =>
            {
                o.HasOne(s => s.Website).WithMany(w => w.SearchLogs).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Html_Content>(o =>
            {
                o.HasOne(c => c.Website).WithMany(u => u.Html_Contents).HasForeignKey(f => f.FK_WebsiteId);
                o.HasOne(c => c.ObjectClassify).WithMany(o => o.html_Contents).HasForeignKey(c => c.Type);
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
            modelBuilder.Entity<Role>(o =>
            {
                o.Property(w => w.Type).HasDefaultValue(1).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
            });
            modelBuilder.Entity<MappingUserAndRole>(o =>
            {
                o.HasOne(w => w.Role).WithMany(w => w.Users).HasForeignKey(f => f.RoleId);
            });
            modelBuilder.Entity<Tag>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.Tags).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Tag_Associate>(o =>
            {
                o.HasOne(u => u.Tag).WithMany(u => u.Tag_Associates).HasForeignKey(f => f.FK_TId);
            });
            modelBuilder.Entity<Tag_TagGroup>(o =>
            {
                o.HasOne(u => u.Tag).WithMany(u => u.Tag_TagGroups).HasForeignKey(f => f.FK_TId);
                o.HasOne(u => u.Tag_Group).WithMany(u => u.Tag_TagGroups).HasForeignKey(f => f.FK_TGId);
            });
            modelBuilder.Entity<FileUpload>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.Files).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<FileBind>(o =>
            {
                o.HasOne(b => b.fileUpload).WithMany(f => f.fileBinds).HasForeignKey(f => f.FK_FileUploadId);
                o.HasKey(b => b.Guid);
            });
            modelBuilder.Entity<Advertise>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.Advertise).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Advertise_Log>(o =>
            {
                o.HasOne(u => u.Advertise).WithMany(u => u.Advertise_Logs).HasForeignKey(f => f.FK_Adid);
                o.HasOne(u => u.User).WithMany(u => u.Advertise_Logs).HasForeignKey(f => f.FK_Uid);
                o.HasOne(u => u.Token).WithMany(u => u.Advertise_Logs).HasForeignKey(f => f.FK_Tid);
            });
            modelBuilder.Entity<Article>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.Articles).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Directory>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.Directory).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<StoreSetDetail>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.StoreSetDetails).HasForeignKey(f => f.FK_WebsiteId);
                o.HasOne(f => f.StoreSet).WithMany(u => u.storeSetDetails).HasForeignKey(f => f.FK_StoreSetId);
            });
            modelBuilder.Entity<StoreSetGroup>(o =>
            {
                o.HasMany(f => f.StoreSets).WithOne(u => u.storeSetGroup).HasForeignKey(f => f.FK_StoreSetGroupId);
            });
            modelBuilder.Entity<storeSetItem>(o =>
            {
                o.HasOne(f => f.storeSet).WithMany(u => u.storeSetItem).HasForeignKey(f => f.FK_StoreSetId);
            });
            modelBuilder.Entity<CustSearch>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.CustSearchs).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<AuditLog>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.AuditLogs).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<MappingCompanyAndWebsites>(o =>
            {
                o.HasOne(f => f.Website).WithMany(w => w.Company).HasForeignKey(e => e.FK_WebsiteId);
                o.HasOne(f => f.Company).WithMany(w => w.Websites).HasForeignKey(e => e.FK_CompanyId);
            });
            modelBuilder.Entity<Recipient>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.Recipients).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Permissions>(o =>
            {
                o.HasOne(f => f.User).WithMany(w => w.Permissions).HasForeignKey(e => e.FK_UserId);
                o.HasOne(f => f.Role).WithMany(w => w.Permissions).HasForeignKey(e => e.FK_RoleId);
                o.HasOne(f => f.Website).WithMany(w => w.Permissions).HasForeignKey(e => e.FK_WebsiteId);
            });
            modelBuilder.Entity<PermissionDetail>(o =>
            {
                o.HasOne(f => f.User).WithMany(w => w.PermissionDetails).HasForeignKey(e => e.FK_UserId);
                o.HasOne(f => f.Role).WithMany(w => w.PermissionDetails).HasForeignKey(e => e.FK_RoleId);
                o.HasOne(f => f.Website).WithMany(w => w.PermissionDetails).HasForeignKey(e => e.FK_WebsiteId);
            });
            modelBuilder.Entity<Remote>(o =>
            {
                o.HasOne(f => f.User).WithMany(w => w.Remotes).HasForeignKey(e => e.FK_UserId);
                o.HasOne(f => f.WebMenu).WithMany(w => w.Remotes).HasForeignKey(e => e.FK_WebmenuId);
                o.HasOne(f => f.Article).WithMany(w => w.Remotes).HasForeignKey(e => e.FK_ArticleId);
                o.HasOne(f => f.Prod).WithMany(w => w.Remotes).HasForeignKey(e => e.FK_ProdId);
                o.HasOne(f => f.TechnicalCertificate).WithMany(w => w.Remotes).HasForeignKey(e => e.FK_TechCertId);
            });
            modelBuilder.Entity<NotFoundImage>(o =>
            {
                o.Property(t => t.CreateDate).HasDefaultValueSql("getdate()");
                o.HasOne(f => f.Website).WithMany(w => w.NotFoundImages).HasForeignKey(e => e.FK_WebsiteId);
            });
            modelBuilder.Entity<Core.Models.JsonObject>(o =>
            {
                o.Property(t => t.CreationTime).HasDefaultValueSql("getdate()");
                o.HasOne(f => f.FK_Website).WithMany(w => w.jsonObjects).HasForeignKey(e => e.FK_WebsiteId);
            });
            modelBuilder.Entity<Contact>(o =>
            {
                o.HasOne(f => f.WebMenu).WithMany(w => w.Contacts).HasForeignKey(e => e.FK_WebMenuId);
            });
            base.OnModelCreating(modelBuilder);
            new SeedHelper(modelBuilder).SeedHost();
        }
    }
}
