using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Bonus;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.Configurations;
using EtheriT.Coker.EntityFrameworkCore.Migrations.Seed;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Directory = EtheriT.Coker.Core.Models.Directory;

namespace EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore
{
    public class CokerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<FrontUser> FrontUsers { get; set; }
        public DbSet<Account_Log> Account_Logs { get; set; }
        public DbSet<Website> Websites { get; set; }
        public DbSet<MappingUserAndWebsite> MappingUserAndWebsites { get; set; }
        public DbSet<MappingFrontUserAndWebsite> MappingFrontUserAndWebsite { get; set; }
        public DbSet<MappingOldNewUUID> MappingOldNewUUID { get; set; }
        public DbSet<MappingLogisticsSettingAndProd> MappingLogisticsSettingAndProd { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Prod> Prods { get; set; }
        public DbSet<Marketing> Marketing { get; set; }
        public DbSet<Marquee> Marquees { get; set; }
        public DbSet<WebMenu> WebMenus { get; set; }
        public DbSet<Order_Header> Order_Headers { get; set; }
        public DbSet<Order_Details> Order_Details { get; set; }
        public DbSet<Order_Logistics> Order_Logistics { get; set; }
        public DbSet<LogisticsSetting> LogisticsSettings { get; set; }
        public DbSet<LogisticsBox> LogisticsBoxs { get; set; }
        public DbSet<LogisticsBoxFee> LogisticsBoxFees { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<PaymentTypesValue> PaymentTypesValues { get; set; }
        public DbSet<LogisticsPaymentRestriction> LogisticsType_Payments { get; set; }
        public DbSet<ThirdParty> ThirdParties { get; set; }
        public DbSet<ThirdPartyKeypair> ThirdPartyKeypairs { get; set; }
        public DbSet<ThirdPartyKeypairValue> ThirdPartyKeypairValues { get; set; }
        public DbSet<Prod_Spec> Prod_Specs { get; set; }
        public DbSet<Prod_Spec_Type> Prod_Spec_Types { get; set; }
        public DbSet<Prod_Stock> Prod_Stocks { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<SearchLog> SearchLogs { get; set; }
        public DbSet<Prod_Log> Prod_Logs { get; set; }
        public DbSet<Favorites> Favorites { get; set; }
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
        public DbSet<DirectoryFacetRange> DirectoryFacetRanges { get; set; }
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
        public DbSet<Template> Templates { get; set; }
        public DbSet<TemplateSections> TemplateSections { get; set; }
        public DbSet<FooterTemplate> FooterTemplates { get; set; }
        public DbSet<UserGrouping> UserGroupings { get; set; }
        public DbSet<UserTagStatistic> UserTagStatistics { get; set; }
        public DbSet<UserActivityTags> UserActivityTags { get; set; }
        public DbSet<UserGroupingDetail> UserGroupingDetails { get; set; }
		public DbSet<FlowSize> FlowSizes { get; set; }
        public DbSet<Bonus> Bonus { get; set; }
        public DbSet<BonusLog> BonusLog { get; set; }
        public DbSet<BonusLiability> BonusLiabilities { get; set; }
        public DbSet<BonusLogDetail> bonusLogDetails { get; set; }
        public DbSet<WebsiteCacheState> WebsiteCacheStates { get; set; }

        public CokerDbContext(DbContextOptions<CokerDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 掃描所有繼承自 FullAuditedEntity 的類別
            var entityType = typeof(FullAuditedEntity); // 基類型
            var configurations = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && entityType.IsAssignableFrom(t)) // 篩選繼承類別
                .Select(entity => typeof(FullAuditedEntityConfiguration<>).MakeGenericType(entity)) // 動態構造類型
                .ToList();

            // 動態套用配置
            foreach (var configType in configurations)
            {
                var configurationInstance = Activator.CreateInstance(configType); // 建立配置類別的實例
                modelBuilder.ApplyConfiguration((dynamic)configurationInstance); // 使用 ApplyConfiguration
            }

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<FrontUser>(o =>
            {
                o.HasIndex(x => new { x.UUID, x.IsDeleted }).IsUnique();
                o.HasOne(f => f.User).WithMany(u => u.frontUsers).HasForeignKey(f => f.FK_User);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<UserActivityTags>(o =>
            {
                o.Property(e => e.CreateTime).HasDefaultValueSql("getdate()");
                o.HasOne(e => e.Remote).WithMany(e => e.UserActivityTags).HasForeignKey(f => f.FK_RemoteId);
            });
            modelBuilder.Entity<UserTagStatistic>(o =>
            {
                o.Property(e => e.LastModificationTime).HasDefaultValueSql("getdate()");
                o.Property(e => e.LastActivityTime).HasDefaultValueSql("getdate()");
                o.HasOne(e => e.Tag).WithMany(e => e.UserTagStatistics).HasForeignKey(f => f.FK_TagId);
            });
            modelBuilder.Entity<UserGrouping>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<UserGroupingDetail>(o =>
            {
                o.HasKey(ugd => new { ugd.UUID, ugd.FK_GropingId });
                o.HasOne(e => e.userGrouping).WithMany(e => e.UserGroupingDetails).HasForeignKey(f => f.FK_GropingId);
            });
            modelBuilder.Entity<Website>(o =>
            {
                o.Property(w => w.Level).HasDefaultValue(WebsiteLevelEnum.形象).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Template>(o =>
            {
                o.Property(w => w.Css).HasDefaultValue("").Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                o.HasOne(w => w.Website).WithMany(t => t.Templates).HasForeignKey(f => f.FK_WebsiteID);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<TemplateSections>(o =>
            {
                o.HasOne(w => w.template).WithMany(t => t.templateSections).HasForeignKey(f => f.FK_TemplateID);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<FooterTemplate>(o =>
            {
                o.HasOne(w => w.templateSections).WithOne(t => t.footerTemplates).HasForeignKey<FooterTemplate>(f => f.FK_TemplateSectionsId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<MappingUserAndWebsite>(o =>
            {
                o.HasOne(u => u.User).WithMany(u => u.Webs).HasForeignKey(f => f.UserId);
                o.HasOne(w => w.Website).WithMany(w => w.Users).HasForeignKey(f => f.WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<MappingFrontUserAndWebsite>(o =>
            {
                o.HasOne(u => u.User).WithMany(u => u.Websites).HasForeignKey(f => f.FK_UserId);
                o.HasOne(w => w.Website).WithMany(w => w.FrontUsers).HasForeignKey(f => f.FK_WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<MappingOldNewUUID>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<MappingLogisticsSettingAndProd>(o =>
            {
                o.HasKey(m => new { m.FK_LogisticsSettingId, m.FK_ProdId });
                o.HasOne(u => u.LogisticsSetting).WithMany(u => u.MappingLogisticsSettingAndProds).HasForeignKey(e => e.FK_LogisticsSettingId);
                o.HasOne(u => u.Prod).WithMany(u => u.MappingLogisticsSettingAndProds).HasForeignKey(f => f.FK_ProdId);
            });
            modelBuilder.Entity<Marketing>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.Marketing).HasForeignKey(f => f.FK_WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Marquee>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.Marquees).HasForeignKey(f => f.FK_WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Token>(o =>
            {
                o.Property(t => t.id).HasDefaultValueSql("newid()").Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                o.HasMany(t => t.ShoppingCarts).WithMany(l => l.Tokens).UsingEntity<Dictionary<string, object>>(
                   "TokenMapShoppingCarts", // 這是中間表的名稱
                   j => j
                       .HasOne<ShoppingCart>()
                       .WithMany()
                       .HasForeignKey("FK_Tid") // 使用 FK_Tid 作為關聯
                       .OnDelete(DeleteBehavior.Restrict), // 刪除紀錄不影響Token保留
                   j => j
                       .HasOne<Token>()
                       .WithMany()
                       .HasForeignKey("UUID") // 使用 UUID 作為關聯
                       .OnDelete(DeleteBehavior.Restrict), // 刪除Token不影響Log紀錄
                   j =>
                   {
                       j.HasKey("UUID", "FK_Tid"); // 設定主鍵
                   }
               );
            });
            modelBuilder.Entity<WebMenu>(o =>
            {
                o.HasIndex(m => m.Title);
                o.HasOne(u => u.Website).WithMany(u => u.WebMenus).HasForeignKey(f => f.FK_WebsiteId);
                o.HasOne(t => t.FK_TopNode).WithMany(u => u.FK_ChildNodes).HasForeignKey(f => f.FK_TopNodeId).IsRequired(false);
                o.Property(m => m.VisibleHeader).HasDefaultValue(true);
                o.Property(m => m.VisibleFooter).HasDefaultValue(true);
                o.Property(m => m.VisibleTitle).HasDefaultValue(true);
                o.Property(m => m.ShowToMenu).HasDefaultValue(true);
                o.Property(m => m.RemovedFromShelves).HasDefaultValue(false);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Order_Details>(o =>
            {
                o.HasOne(u => u.Order_Header).WithMany(u => u.Order_Details).HasForeignKey(f => f.FK_OId);
                o.HasOne(u => u.ShoppingCart).WithMany(u => u.Order_Details).HasForeignKey(f => f.FK_SCId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Order_Logistics>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<LogisticsSetting>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.LogisticsSettings).HasForeignKey(f => f.FK_WebsiteId);
                o.Property(l => l.FreightStatusType).HasDefaultValue(FreightStatusTypeEnum.一般);
                o.Property(l => l.DiscountFreightType).HasDefaultValue(DiscountFreightType.指定折抵後運費);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<LogisticsBox>(o => {
                o.HasOne(u => u.Website).WithMany(u => u.logisticsBoxes).HasForeignKey(f => f.FK_WebsiteId);
                o.Property(l => l.IsActive).HasDefaultValue(true);
                o.HasIndex(x => new { x.FK_WebsiteId, x.CapacityPoint })
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<LogisticsBoxFee>(o => {
                o.HasOne(u => u.LogisticsSetting).WithMany(u => u.logisticsBoxFees).HasForeignKey(f => f.FK_LogisticsSettingId).OnDelete(DeleteBehavior.NoAction);
                o.HasOne(u => u.logisticsBox).WithMany(u => u.logisticsBoxFees).HasForeignKey(f => f.FK_LogisticsBoxId);
                o.HasIndex(x => new { x.FK_LogisticsBoxId, x.FK_LogisticsSettingId })
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<PaymentType>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<LogisticsPaymentRestriction>(o =>
            {
                o.HasOne(u => u.PaymentType).WithMany(u => u.LogisticsType_Payments).HasForeignKey(f => f.FK_Pid);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<ThirdPartyKeypair>(o =>
            {
                o.HasOne(u => u.ThirdParty).WithMany(u => u.ThirdPartyKeypair).HasForeignKey(f => f.FK_TPid);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<ThirdPartyKeypairValue>(o =>
            {
                o.HasOne(u => u.ThirdPartyKeypair).WithMany(u => u.thirdPartyKeypairValues).HasForeignKey(f => f.FK_ThirdPartyKeypairId);
                o.HasOne(u => u.Website).WithMany(u => u.thirdPartyKeypairValues).HasForeignKey(f => f.FK_WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<PaymentTypesValue>(o =>
            {
                o.HasOne(u => u.paymentType).WithMany(u => u.paymentTypesValues).HasForeignKey(f => f.FK_PaymentTypesId);
                o.HasOne(u => u.website).WithMany(u => u.paymentTypesValues).HasForeignKey(f => f.FK_WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<ThirdParty>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<ObjectType>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<MappingWebsiteRelationship>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Prod>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.Prods).HasForeignKey(f => f.FK_WebsiteId);
                o.HasIndex(u => u.Title);
                o.Property(p => p.Visible).HasDefaultValue(true);
                o.Property(p => p.RemovedFromShelves).HasDefaultValue(false);
                o.Property(p => p.Status).HasDefaultValue(ProdStatusEnum.一般);
                o.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<Prod_TechCert>(o =>
            {
                o.HasOne(u => u.Prod).WithMany(u => u.TechnicalCertificates).HasForeignKey(f => f.FK_PId);
                o.HasOne(u => u.TechnicalCertificate).WithMany(u => u.prods).HasForeignKey(f => f.FK_TCId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Prod_Log>(o =>
            {
                o.HasOne(u => u.Prod).WithMany(u => u.Prod_Logs).HasForeignKey(f => f.FK_Pid);
                o.Property(e => e.CreationTime).HasDefaultValueSql("getdate()");
            });
            modelBuilder.Entity<Bonus>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
                o.Property(e => e.Status).HasDefaultValue(BonusStatusEnum.Active);
            });
            modelBuilder.Entity<BonusLog>(o =>
            {
                o.HasOne(x => x.User)
                 .WithMany(u => u.BonusLogs)
                 .HasForeignKey(x => x.UUID)
                 .HasPrincipalKey(u => u.UUID);

                o.Property(x => x.ExecutionTime)
                    .HasDefaultValueSql("GETDATE()")
                    .ValueGeneratedOnAdd();

                o.Property(x => x.Type).HasDefaultValue(BonusLogTypeEnum.Unknown);
            });
            modelBuilder.Entity<BonusLogDetail>(o =>
            {
                o.HasKey(b => new { b.FK_BonusId, b.FK_BonusLogsId });
                o.HasOne(b => b.Bonus).WithMany(b => b.BonusLogDetails).HasForeignKey(b => b.FK_BonusId);
                o.HasOne(b => b.BonusLog).WithMany(b => b.BonusLogDetails).HasForeignKey(b => b.FK_BonusLogsId);
            });
            modelBuilder.Entity<BonusLiability>(o =>
            {
                o.HasKey(b => new { b.UUID });
            });
            modelBuilder.Entity<Favorites>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Prod_Spec>(o =>
            {
                o.HasOne(u => u.Prod_Spec_Type).WithMany(u => u.Prod_Specs).HasForeignKey(f => f.FK_Tid);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Prod_Spec_Type>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.Prod_Spec_Types).HasForeignKey(f => f.FK_WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Prod_Stock>(o =>
            {
                o.HasOne(u => u.Prod).WithMany(u => u.Prod_Stocks).HasForeignKey(f => f.FK_Pid).OnDelete(DeleteBehavior.Cascade);
                o.Property(p => p.IsTimePrice).HasDefaultValue(false);
                o.Property(p => p.PackingPoint).HasDefaultValue(1);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<ShoppingCart>(o =>
            {
                o.HasOne(u => u.Prod_Stock).WithMany(u => u.ShoppingCarts).HasForeignKey(f => f.FK_PSid);
                o.HasOne(u => u.Prod_Price).WithMany(u => u.ShoppingCarts).HasForeignKey(f => f.FK_PriceId).OnDelete(DeleteBehavior.SetNull);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Order_Header>(o =>
            {
                o.HasOne(u => u.PaymentType).WithMany(u => u.Order_Headers).HasForeignKey(f => f.Payment);
                o.HasOne(u => u.LogisticsSetting).WithMany(u => u.Order_Headers).HasForeignKey(f => f.Shipping);
                o.Property(e => e.InvoiceType).HasDefaultValue(InvoiceTypeEnum.個人發票);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<SearchLog>(o =>
            {
                o.HasOne(s => s.Website).WithMany(w => w.SearchLogs).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Html_Content>(o =>
            {
                o.HasOne(c => c.Website).WithMany(u => u.Html_Contents).HasForeignKey(f => f.FK_WebsiteId);
                o.HasOne(c => c.ObjectClassify).WithMany(o => o.html_Contents).HasForeignKey(c => c.Type);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<TechnicalCertificate>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.TechnicalCertificates).HasForeignKey(f => f.FK_WebsiteId);
                o.Property(p => p.Css).HasDefaultValue(string.Empty);
                o.Property(p => p.Html).HasDefaultValue(string.Empty);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Prod_Price>(o =>
            {
                o.HasOne(u => u.Prod_Stock).WithMany(u => u.Prod_Prices).HasForeignKey(f => f.FK_PSId).OnDelete(DeleteBehavior.Cascade);
                o.HasOne(u => u.Role).WithMany(u => u.Prod_Prices).HasForeignKey(f => f.FK_RId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Role>(o =>
            {
                o.Property(w => w.Type).HasDefaultValue(RoleTypeEnum.前台).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<MappingUserAndRole>(o =>
            {
                o.HasOne(w => w.Role).WithMany(w => w.Users).HasForeignKey(f => f.RoleId);
                o.HasOne(u => u.User).WithMany(u => u.Roles).HasForeignKey(f => f.UserId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Tag>(o =>
            {
                o.HasIndex(t => new { t.Title, t.FK_WebsiteId }).HasFilter("[IsDeleted] = 0").IsUnique();
                o.HasOne(u => u.Website).WithMany(u => u.Tags).HasForeignKey(f => f.FK_WebsiteId);
                o.Property(t => t.IsTemporary).HasDefaultValue(false);
                o.HasQueryFilter(e => !e.IsDeleted && !e.IsTemporary);
            });
            modelBuilder.Entity<Tag_Associate>(o =>
            {
                o.HasOne(u => u.Tag).WithMany(u => u.Tag_Associates).HasForeignKey(f => f.FK_TId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Tag_Group>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Tag_TagGroup>(o =>
            {
                o.HasOne(u => u.Tag).WithMany(u => u.Tag_TagGroups).HasForeignKey(f => f.FK_TId);
                o.HasOne(u => u.Tag_Group).WithMany(u => u.Tag_TagGroups).HasForeignKey(f => f.FK_TGId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<FileUpload>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.Files).HasForeignKey(f => f.FK_WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<FlowSize>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.flowSizes).HasForeignKey(f => f.FK_WebsiteId);
                o.HasIndex(e => e.actionTime);
            });
            modelBuilder.Entity<FileBind>(o =>
            {
                o.HasOne(b => b.fileUpload).WithMany(f => f.fileBinds).HasForeignKey(f => f.FK_FileUploadId);
                o.HasKey(b => b.Guid);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<FileBindMore>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Advertise>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.Advertise).HasForeignKey(f => f.FK_WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Advertise_Log>(o =>
            {
                o.HasOne(u => u.Advertise).WithMany(u => u.Advertise_Logs).HasForeignKey(f => f.FK_Adid);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Article>(o =>
            {
                o.HasIndex(a => a.Title);
                o.HasIndex(a => a.RemovedFromShelves);
                o.HasIndex(a => a.permanent);
                o.HasIndex(a => a.Visible);
                o.HasIndex(a => a.SerNO);
                o.HasIndex(a => a.NodeDate);
                o.HasIndex(a => a.StartTime);
                o.HasIndex(a => a.EndTime);
                o.HasOne(f => f.Website).WithMany(u => u.Articles).HasForeignKey(f => f.FK_WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Directory>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.Directory).HasForeignKey(f => f.FK_WebsiteId);
                o.HasOne(f => f.html_Content).WithMany(u => u.Directories).HasForeignKey(f => f.FK_DefaultLayout);
                o.Property(e => e.FacetType).HasDefaultValue(DirectoryFacetTypeEnum.None);
                o.Property(e => e.CalendarType).HasDefaultValue(DirectoryCalendarTypeEnum.None);
                o.HasIndex(e => e.FacetType);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<DirectoryFacetRange>(o => {
                o.HasOne(f => f.Directory).WithMany(u => u.DirectoryFacetRanges).HasForeignKey(f => f.FK_DirectoryId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<StoreSetDetail>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.StoreSetDetails).HasForeignKey(f => f.FK_WebsiteId);
                o.HasOne(f => f.StoreSet).WithMany(u => u.storeSetDetails).HasForeignKey(f => f.FK_StoreSetId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<StoreSetGroup>(o =>
            {
                o.HasMany(f => f.StoreSets).WithOne(u => u.storeSetGroup).HasForeignKey(f => f.FK_StoreSetGroupId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<StoreSet>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<storeSetItem>(o =>
            {
                o.HasOne(f => f.storeSet).WithMany(u => u.storeSetItems).HasForeignKey(f => f.FK_StoreSetId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<CustSearch>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.CustSearchs).HasForeignKey(f => f.FK_WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<AuditLog>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.AuditLogs).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<MappingCompanyAndWebsites>(o =>
            {
                o.HasOne(f => f.Website).WithMany(w => w.Company).HasForeignKey(e => e.FK_WebsiteId);
                o.HasOne(f => f.Company).WithMany(w => w.Websites).HasForeignKey(e => e.FK_CompanyId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Recipient>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.Recipients).HasForeignKey(f => f.FK_WebsiteId);
                o.HasQueryFilter(e => !e.IsDeleted);
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
                o.HasIndex(f => f.FK_WebsiteId);
                o.HasIndex(f => f.FK_UserId);
                o.HasIndex(f => f.FK_WebmenuId);
                o.HasIndex(f => f.FK_ArticleId);
                o.HasIndex(f => f.FK_ProdId);
                o.HasIndex(f => f.FK_TechCertId);
                o.HasIndex(f => f.State);
                o.HasIndex(f => f.ExecutionTime);
                o.HasIndex(f => f.UUID);
                o.Property(f => f.State).HasDefaultValue(RemoteStateEnum.未處理);
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
                o.Property(x => x.CacheKey).HasDefaultValue(WebsiteCacheKeys.Menu);
                o.HasIndex(x => new { x.FK_WebsiteId, x.CacheKey }).IsUnique();
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<WebsiteCacheState>(o =>
            {
                o.HasOne(f => f.Website).WithMany(w => w.websiteCacheStates).HasForeignKey(e => e.FK_WebsiteId);
                o.Property(x => x.Version).HasDefaultValue(1);
                o.HasIndex(x => new { x.FK_WebsiteId, x.CacheKey }).IsUnique();
            });

            modelBuilder.Entity<Contact>(o =>
            {
                o.HasOne(f => f.WebMenu).WithMany(w => w.Contacts).HasForeignKey(e => e.FK_WebMenuId);
                o.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Company>(o =>
            {
                o.HasQueryFilter(e => !e.IsDeleted);
            });

			new SeedHelper(modelBuilder).SeedHost();
        }
    }
}
