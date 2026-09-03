using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Advertise;
using EtheriT.Coker.Application.Shared.Dto.enumType.Bonus;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
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
        public DbSet<ComponentPurpose> ComponentPurposes { get; set; }
        public DbSet<HtmlContentPurpose> HtmlContentPurposes { get; set; }
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
        public DbSet<RemoteDailyStatistic> RemoteDailyStatistics { get; set; }
        public DbSet<RemoteHourlyStatistic> RemoteHourlyStatistics { get; set; }
        public DbSet<RemoteDailyAggregationRun> RemoteDailyAggregationRuns { get; set; }
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
        public DbSet<HtmlSanitizeState> HtmlSanitizeStates { get; set; }
        public DbSet<MarketingCampaign> MarketingCampaigns { get; set; }
        public DbSet<MarketingRule> MarketingRules { get; set; }
        public DbSet<MarketingCondition> MarketingConditions { get; set; }
        public DbSet<MarketingReward> MarketingRewards { get; set; }
        public DbSet<MarketingRewardItem> MarketingRewardItems { get; set; }
        public DbSet<MarketingScopeItem> MarketingScopeItems { get; set; }
        public DbSet<BackgroundTaskRecord> BackgroundTasks { get; set; }
        public DbSet<UserNotification> Notifications { get; set; }
        public DbSet<PageTextBackfillState> PageTextBackfillStates { get; set; }
        public DbSet<CdnProviderIpRange> CdnProviderIpRanges { get; set; }
        public DbSet<CdnProviderSyncState> CdnProviderSyncStates { get; set; }

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
            modelBuilder.Entity<FrontUser>(o =>
            {
                o.HasIndex(x => new { x.UUID, x.IsDeleted }).IsUnique();
                o.HasOne(f => f.User).WithMany(u => u.frontUsers).HasForeignKey(f => f.FK_User);
            });
            modelBuilder.Entity<MappingOldNewUUID>(o =>
            {
                o.HasIndex(x => new { x.TempUUID, x.UserUUID });
            });
            modelBuilder.Entity<UserActivityTags>(o =>
            {
                o.Property(e => e.CreateTime).HasDefaultValueSql("getdate()");
                o.HasOne(e => e.Remote).WithMany(e => e.UserActivityTags).HasForeignKey(f => f.FK_RemoteId);
            });
            modelBuilder.Entity<BackgroundTaskRecord>(o =>
            {
                o.ToTable("BackgroundTasks");
                o.HasIndex(x => x.ActiveKey)
                    .IsUnique()
                    .HasFilter("[ActiveKey] IS NOT NULL AND [IsDeleted] = 0");
                o.HasIndex(x => x.StorageKey).IsUnique();
                o.HasIndex(x => new { x.FK_WebsiteId, x.FK_UserId, x.Status });
                o.HasIndex(x => x.ExpireTime);
            });
            modelBuilder.Entity<UserNotification>(o =>
            {
                o.ToTable("Notifications");
                o.HasIndex(x => new { x.FK_WebsiteId, x.FK_UserId, x.IsRead });
                o.HasIndex(x => x.FK_BackgroundTaskId);
            });
            modelBuilder.Entity<PageTextBackfillState>(o =>
            {
                o.ToTable("PageTextBackfillStates");
                o.HasIndex(x => new { x.FK_WebsiteId, x.ContentType }).IsUnique();
                o.HasIndex(x => new { x.Status, x.LastModificationTime });
            });
            modelBuilder.Entity<CdnProviderIpRange>(o =>
            {
                o.ToTable("CdnProviderIpRanges");
                o.Property(x => x.Provider).IsRequired();
                o.Property(x => x.Cidr).IsRequired();
                o.Property(x => x.IpVersion).HasColumnType("tinyint");
                o.HasIndex(x => new { x.Provider, x.Cidr })
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");
                o.HasIndex(x => new { x.Provider, x.IpVersion, x.IsDeleted });
            });
            modelBuilder.Entity<CdnProviderSyncState>(o =>
            {
                o.ToTable("CdnProviderSyncStates");
                o.Property(x => x.Provider).IsRequired();
                o.Property(x => x.ConsecutiveFailureCount).HasDefaultValue(0);
                o.Property(x => x.AlertSent).HasDefaultValue(false);
                o.HasIndex(x => x.Provider)
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");
            });
            modelBuilder.Entity<UserTagStatistic>(o =>
            {
                o.Property(e => e.LastModificationTime).HasDefaultValueSql("getdate()");
                o.Property(e => e.LastActivityTime).HasDefaultValueSql("getdate()");
                o.HasOne(e => e.Tag).WithMany(e => e.UserTagStatistics).HasForeignKey(f => f.FK_TagId);
            });
            modelBuilder.Entity<UserGroupingDetail>(o =>
            {
                o.HasKey(ugd => new { ugd.UUID, ugd.FK_GropingId });
                o.HasOne(e => e.userGrouping).WithMany(e => e.UserGroupingDetails).HasForeignKey(f => f.FK_GropingId);
            });
            modelBuilder.Entity<Website>(o =>
            {
                o.Property(w => w.Level).HasDefaultValue(WebsiteLevelEnum.形象).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
            });
            modelBuilder.Entity<Template>(o =>
            {
                o.Property(w => w.Css).HasDefaultValue("").Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                o.HasOne(w => w.Website).WithMany(t => t.Templates).HasForeignKey(f => f.FK_WebsiteID);
            });
            modelBuilder.Entity<TemplateSections>(o =>
            {
                o.HasOne(w => w.template).WithMany(t => t.templateSections).HasForeignKey(f => f.FK_TemplateID);
            });
            modelBuilder.Entity<FooterTemplate>(o =>
            {
                o.HasOne(w => w.templateSections).WithOne(t => t.footerTemplates).HasForeignKey<FooterTemplate>(f => f.FK_TemplateSectionsId);
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
            modelBuilder.Entity<MappingLogisticsSettingAndProd>(o =>
            {
                o.HasKey(m => new { m.FK_LogisticsSettingId, m.FK_ProdId });
                o.HasOne(u => u.LogisticsSetting).WithMany(u => u.MappingLogisticsSettingAndProds).HasForeignKey(e => e.FK_LogisticsSettingId);
                o.HasOne(u => u.Prod).WithMany(u => u.MappingLogisticsSettingAndProds).HasForeignKey(f => f.FK_ProdId);
            });
            modelBuilder.Entity<Marquee>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.Marquees).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Token>(o =>
            {
                o.Property(t => t.id).HasDefaultValueSql("newid()").Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                o.HasIndex(t => new { t.EndTime, t.id })
                    .HasFilter("[EndTime] IS NOT NULL");
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
            });
            modelBuilder.Entity<Order_Details>(o =>
            {
                o.HasOne(u => u.Order_Header).WithMany(u => u.Order_Details).HasForeignKey(f => f.FK_OId);
                o.HasOne(u => u.ShoppingCart)
                    .WithMany(u => u.Order_Details)
                    .HasForeignKey(f => f.FK_SCId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<LogisticsSetting>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.LogisticsSettings).HasForeignKey(f => f.FK_WebsiteId);
                o.Property(l => l.FreightStatusType).HasDefaultValue(FreightStatusTypeEnum.一般);
                o.Property(l => l.DiscountFreightType).HasDefaultValue(DiscountFreightType.指定折抵後運費);
            });
            modelBuilder.Entity<LogisticsBox>(o => {
                o.HasOne(u => u.Website).WithMany(u => u.logisticsBoxes).HasForeignKey(f => f.FK_WebsiteId);
                o.Property(l => l.IsActive).HasDefaultValue(true);
                o.HasIndex(x => new { x.FK_WebsiteId, x.CapacityPoint })
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");
            });
            modelBuilder.Entity<LogisticsBoxFee>(o => {
                o.HasOne(u => u.LogisticsSetting).WithMany(u => u.logisticsBoxFees).HasForeignKey(f => f.FK_LogisticsSettingId).OnDelete(DeleteBehavior.NoAction);
                o.HasOne(u => u.logisticsBox).WithMany(u => u.logisticsBoxFees).HasForeignKey(f => f.FK_LogisticsBoxId);
                o.HasIndex(x => new { x.FK_LogisticsBoxId, x.FK_LogisticsSettingId })
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");
            });
            modelBuilder.Entity<PaymentType>(o =>
            {
                o.Property(x => x.MinAmount).HasPrecision(18, 2);
                o.Property(x => x.MaxAmount).HasPrecision(18, 2);
            });
            modelBuilder.Entity<LogisticsPaymentRestriction>(o =>
            {
                o.ToTable(t => t.HasCheckConstraint(
                    "CK_LogisticsType_Payments_RuleScope",
                    "([ShippingType] IS NOT NULL AND [FK_LogisticsSettingId] IS NULL) OR ([ShippingType] IS NULL AND [FK_LogisticsSettingId] IS NOT NULL)"));
                o.HasOne(u => u.PaymentType).WithMany(u => u.LogisticsType_Payments).HasForeignKey(f => f.FK_PaymentTypeId);
                o.HasOne(x => x.LogisticsSetting).WithMany(x => x.LogisticsPaymentRestrictions).HasForeignKey(x => x.FK_LogisticsSettingId).OnDelete(DeleteBehavior.Cascade);
                o.Property(x => x.OverrideMinAmount).HasPrecision(18, 2);
                o.Property(x => x.OverrideMaxAmount).HasPrecision(18, 2);
                o.HasIndex(x => new
                {
                    x.ShippingType,
                    x.FK_PaymentTypeId
                })
                    .HasFilter("[FK_LogisticsSettingId] IS NULL AND [IsDeleted] = 0")
                    .IsUnique();

                o.HasIndex(x => new
                {
                    x.FK_LogisticsSettingId,
                    x.FK_PaymentTypeId
                })
                    .HasFilter("[FK_LogisticsSettingId] IS NOT NULL AND [IsDeleted] = 0")
                    .IsUnique();

            });
            modelBuilder.Entity<ThirdPartyKeypair>(o =>
            {
                o.HasOne(u => u.ThirdParty).WithMany(u => u.ThirdPartyKeypair).HasForeignKey(f => f.FK_TPid);
            });
            modelBuilder.Entity<ThirdPartyKeypairValue>(o =>
            {
                o.HasOne(u => u.ThirdPartyKeypair).WithMany(u => u.thirdPartyKeypairValues).HasForeignKey(f => f.FK_ThirdPartyKeypairId);
                o.HasOne(u => u.Website).WithMany(u => u.thirdPartyKeypairValues).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<PaymentTypesValue>(o =>
            {
                o.HasOne(u => u.paymentType).WithMany(u => u.paymentTypesValues).HasForeignKey(f => f.FK_PaymentTypesId);
                o.HasOne(u => u.website).WithMany(u => u.paymentTypesValues).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<Prod>(o =>
            {
                // Keep this relationship aligned with the production constraint.
                // Cascade would create two delete paths from Websites through
                // Prods and LogisticsSettings to MappingLogisticsSettingAndProd.
                o.HasOne(u => u.Website)
                    .WithMany(u => u.Prods)
                    .HasForeignKey(f => f.FK_WebsiteId)
                    .OnDelete(DeleteBehavior.NoAction);
                o.HasIndex(u => u.Title);
                o.Property(p => p.Visible).HasDefaultValue(true);
                o.Property(p => p.RemovedFromShelves).HasDefaultValue(false);
                o.Property(p => p.Status).HasDefaultValue(ProdStatusEnum.一般);
            });

            modelBuilder.Entity<Prod_TechCert>(o =>
            {
                o.HasOne(u => u.Prod).WithMany(u => u.TechnicalCertificates).HasForeignKey(f => f.FK_PId);
                o.HasOne(u => u.TechnicalCertificate).WithMany(u => u.prods).HasForeignKey(f => f.FK_TCId);
            });
            modelBuilder.Entity<Prod_Log>(o =>
            {
                o.HasOne(u => u.Prod)
                    .WithMany(u => u.Prod_Logs)
                    .HasForeignKey(f => f.FK_Pid)
                    .OnDelete(DeleteBehavior.NoAction);
                o.Property(e => e.CreationTime).HasDefaultValueSql("getdate()");
            });
            modelBuilder.Entity<Bonus>(o =>
            {
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
            modelBuilder.Entity<Prod_Spec>(o =>
            {
                o.HasOne(u => u.Prod_Spec_Type)
                    .WithMany(u => u.Prod_Specs)
                    .HasForeignKey(f => f.FK_Tid)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<Prod_Spec_Type>(o =>
            {
                o.Property(p => p.SeoVariantProperty).HasDefaultValue(SeoVariantPropertyEnum.None);
                o.HasOne(u => u.Website)
                    .WithMany(u => u.Prod_Spec_Types)
                    .HasForeignKey(f => f.FK_WebsiteId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<Prod_Stock>(o =>
            {
                o.HasOne(u => u.Prod)
                    .WithMany(u => u.Prod_Stocks)
                    .HasForeignKey(f => f.FK_Pid)
                    .OnDelete(DeleteBehavior.NoAction);
                o.Property(p => p.IsTimePrice).HasDefaultValue(false);
                o.Property(p => p.PackingPoint).HasDefaultValue(1);
                o.Property(p => p.Visible).HasDefaultValue(true).ValueGeneratedNever();
            });
            modelBuilder.Entity<ShoppingCart>(o =>
            {
                o.HasOne(u => u.Prod_Stock)
                    .WithMany(u => u.ShoppingCarts)
                    .HasForeignKey(f => f.FK_PSid)
                    .OnDelete(DeleteBehavior.NoAction);

                o.HasOne(u => u.MarketingRewardItem)
                    .WithMany(u => u.ShoppingCarts)
                    .HasForeignKey(f => f.FK_MarketingRewardItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                o.HasOne(u => u.Prod_Price).WithMany(u => u.ShoppingCarts).HasForeignKey(f => f.FK_PriceId).OnDelete(DeleteBehavior.SetNull);
                o.HasIndex(f => new { f.FK_Tid, f.IsOrder });
            });
            modelBuilder.Entity<Order_Header>(o =>
            {
                o.HasOne(u => u.PaymentType).WithMany(u => u.Order_Headers).HasForeignKey(f => f.Payment);
                o.HasOne(u => u.LogisticsSetting).WithMany(u => u.Order_Headers).HasForeignKey(f => f.Shipping);
                o.Property(e => e.InvoiceType).HasDefaultValue(InvoiceTypeEnum.個人發票);
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
            modelBuilder.Entity<ComponentPurpose>(o =>
            {
                o.HasIndex(e => e.Code).IsUnique();
            });
            modelBuilder.Entity<HtmlContentPurpose>(o =>
            {
                o.HasOne(e => e.HtmlContent)
                    .WithMany(e => e.HtmlContentPurposes)
                    .HasForeignKey(e => e.FK_HtmlContentId)
                    .OnDelete(DeleteBehavior.Cascade);
                o.HasOne(e => e.ComponentPurpose)
                    .WithMany(e => e.HtmlContentPurposes)
                    .HasForeignKey(e => e.FK_ComponentPurposeId)
                    .OnDelete(DeleteBehavior.Cascade);
                o.HasIndex(e => new { e.FK_HtmlContentId, e.FK_ComponentPurposeId })
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");
            });
            modelBuilder.Entity<TechnicalCertificate>(o =>
            {
                o.HasOne(u => u.Website).WithMany(u => u.TechnicalCertificates).HasForeignKey(f => f.FK_WebsiteId);
                o.Property(p => p.Css).HasDefaultValue(string.Empty);
                o.Property(p => p.Html).HasDefaultValue(string.Empty);
            });
            modelBuilder.Entity<Prod_Price>(o =>
            {
                o.HasOne(u => u.Prod_Stock)
                    .WithMany(u => u.Prod_Prices)
                    .HasForeignKey(f => f.FK_PSId)
                    .OnDelete(DeleteBehavior.NoAction);
                o.HasOne(u => u.Role).WithMany(u => u.Prod_Prices).HasForeignKey(f => f.FK_RId);
            });
            modelBuilder.Entity<Role>(o =>
            {
                o.Property(w => w.Type).HasDefaultValue(RoleTypeEnum.前台).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
            });
            modelBuilder.Entity<MappingUserAndRole>(o =>
            {
                o.HasOne(w => w.Role).WithMany(w => w.Users).HasForeignKey(f => f.RoleId);
                o.HasOne(u => u.User).WithMany(u => u.Roles).HasForeignKey(f => f.UserId);
            });
            modelBuilder.Entity<Tag>(o =>
            {
                o.HasIndex(t => new { t.Title, t.FK_WebsiteId }).HasFilter("[IsDeleted] = 0").IsUnique();
                o.HasOne(u => u.Website).WithMany(u => u.Tags).HasForeignKey(f => f.FK_WebsiteId);
                o.Property(t => t.IsTemporary).HasDefaultValue(false);
                o.HasQueryFilter("TemporaryFilter", e => !e.IsTemporary);
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
            modelBuilder.Entity<FlowSize>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.flowSizes).HasForeignKey(f => f.FK_WebsiteId);
                o.HasIndex(e => e.actionTime);
            });
            modelBuilder.Entity<FileBind>(o =>
            {
                o.HasOne(b => b.fileUpload).WithMany(f => f.fileBinds).HasForeignKey(f => f.FK_FileUploadId);
                o.HasKey(b => b.Guid);
            });
            modelBuilder.Entity<Advertise>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.Advertise).HasForeignKey(f => f.FK_WebsiteId);
                o.Property(a => a.ActionType).HasDefaultValue(AdvertiseActionType.Link);
            });
            modelBuilder.Entity<Advertise_Log>(o =>
            {
                o.HasOne(u => u.Advertise).WithMany(u => u.Advertise_Logs).HasForeignKey(f => f.FK_Adid);
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
            });
            modelBuilder.Entity<Directory>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.Directory).HasForeignKey(f => f.FK_WebsiteId);
                o.HasOne(f => f.html_Content).WithMany(u => u.Directories).HasForeignKey(f => f.FK_DefaultLayout);
                o.Property(e => e.FacetType).HasDefaultValue(DirectoryFacetTypeEnum.None);
                o.Property(e => e.CalendarType).HasDefaultValue(DirectoryCalendarTypeEnum.None);
                o.HasIndex(e => e.FacetType);
            });
            modelBuilder.Entity<DirectoryFacetRange>(o => {
                o.HasOne(f => f.Directory).WithMany(u => u.DirectoryFacetRanges).HasForeignKey(f => f.FK_DirectoryId);
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
                o.HasOne(f => f.storeSet).WithMany(u => u.storeSetItems).HasForeignKey(f => f.FK_StoreSetId);
            });
            modelBuilder.Entity<CustSearch>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.CustSearchs).HasForeignKey(f => f.FK_WebsiteId);
            });
            modelBuilder.Entity<AuditLog>(o =>
            {
                o.HasOne(f => f.Website).WithMany(u => u.AuditLogs).HasForeignKey(f => f.FK_WebsiteId);
                o.HasIndex(f => f.ExecutionTime);
                o.HasIndex(f => new { f.FK_WebsiteId, f.ServiceName, f.MethodName, f.ExecutionTime })
                    .HasDatabaseName("IX_AuditLogs_CanvasHistory");
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
                o.HasIndex(f => f.FK_WebsiteId);
                o.HasIndex(f => f.FK_UserId);
                o.HasIndex(f => f.FK_WebmenuId);
                o.HasIndex(f => f.FK_ArticleId);
                o.HasIndex(f => f.FK_ProdId);
                o.HasIndex(f => f.FK_TechCertId);
                o.HasIndex(f => f.State);
                o.HasIndex(f => f.ExecutionTime);
                o.HasIndex(f => f.UUID);
                o.HasIndex(f => new { f.FK_WebsiteId, f.LastHeartbeatAt })
                    .HasDatabaseName("IX_Remotes_FK_WebsiteId_LastHeartbeatAt_Online")
                    .HasFilter("[LastHeartbeatAt] IS NOT NULL AND [TrackingEventId] IS NOT NULL")
                    .IncludeProperties(f => new
                    {
                        f.TrackingEventId,
                        f.FK_UserId,
                        f.UUID,
                        f.IsEngaged
                    });
                o.HasIndex(f => f.TrackingEventId)
                    .IsUnique()
                    .HasFilter("[TrackingEventId] IS NOT NULL");
                o.Property(f => f.State).HasDefaultValue(RemoteStateEnum.未處理);
            });
            modelBuilder.Entity<RemoteDailyStatistic>(o =>
            {
                o.Property(f => f.StatisticDate).HasColumnType("date");
                o.HasIndex(f => new
                {
                    f.StatisticDate,
                    f.FK_WebsiteId,
                    f.Scope,
                    f.FK_WebmenuId,
                    f.FK_ArticleId,
                    f.FK_ProdId,
                    f.FK_TechCertId
                }).IsUnique();
                o.HasIndex(f => new { f.FK_WebsiteId, f.StatisticDate, f.Scope });
                o.HasIndex(f => new { f.FK_WebmenuId, f.StatisticDate });
                o.HasIndex(f => new { f.FK_ArticleId, f.StatisticDate });
                o.HasIndex(f => new { f.FK_ProdId, f.StatisticDate });
            });
            modelBuilder.Entity<RemoteHourlyStatistic>(o =>
            {
                o.Property(f => f.StatisticHour).HasColumnType("datetime2(0)");
                o.HasIndex(f => new { f.StatisticHour, f.FK_WebsiteId }).IsUnique();
                o.HasIndex(f => new { f.FK_WebsiteId, f.StatisticHour });
            });
            modelBuilder.Entity<RemoteDailyAggregationRun>(o =>
            {
                o.Property(f => f.StatisticDate).HasColumnType("date");
                o.HasIndex(f => f.StatisticDate).IsUnique();
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
                o.HasIndex(x => new { x.FK_WebsiteId, x.CacheKey, x.FK_AId }).IsUnique();
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
                o.HasIndex(e => e.FK_WebMenuId)
                    .HasDatabaseName("IX_Contacts_FK_WebMenuId");
                o.HasIndex(e => new { e.FK_WebMenuId, e.Status, e.CreationTime })
                    .HasDatabaseName("IX_Contacts_FK_WebMenuId_Status_CreationTime_Active")
                    .HasFilter("[IsDeleted] = 0")
                    .IncludeProperties(e => new { e.Name, e.UserName });
            });

            modelBuilder.Entity<HtmlSanitizeState>(o =>
            {
                o.HasIndex(e => new
                {
                    e.FK_WebsiteId,
                    e.SourceType,
                    e.FK_Bid,
                    e.ContentKey,
                    e.SanitizePolicy
                }).IsUnique();

                o.HasOne(f => f.Website).WithMany(w => w.htmlSanitizeStates).HasForeignKey(e => e.FK_WebsiteId);

                o.Property(e => e.ContentKey).HasDefaultValue("Default");

                o.Property(e => e.SanitizePolicy).HasDefaultValue("PublicHtml");
            });

            modelBuilder.Entity<MarketingCampaign>(o =>
            {
                o.HasOne(x => x.Website)
                    .WithMany(w => w.MarketingCampaigns)
                    .HasForeignKey(x => x.FK_WebsiteId);

                o.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                o.Property(x => x.Description)
                    .HasMaxLength(500);

                o.Property(x => x.Status)
                    .HasDefaultValue(MarketingDisplayStatusEnum.草稿);

                o.Property(x => x.CampaignType)
                    .HasDefaultValue(MarketingCampaignTypeEnum.滿額優惠);

                o.Property(x => x.Priority)
                    .HasDefaultValue(0);

                o.Property(x => x.CanStack)
                    .HasDefaultValue(false);

                o.Property(x => x.Repeatable)
                    .HasDefaultValue(false);

                o.Property(x => x.NeverEnd)
                    .HasDefaultValue(false);

                o.HasIndex(x => new { x.FK_WebsiteId, x.Status });
                o.HasIndex(x => new { x.FK_WebsiteId, x.StartTime, x.EndTime });

            });

            modelBuilder.Entity<MarketingRule>(o =>
            {
                o.HasOne(x => x.MarketingCampaign)
                    .WithMany(x => x.Rules)
                    .HasForeignKey(x => x.FK_MarketingCampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                o.Property(x => x.ScopeType)
                    .HasDefaultValue(MarketingScopeTypeEnum.AllOrder);

                o.Property(x => x.Enabled)
                    .HasDefaultValue(true);

                o.Property(x => x.SortOrder)
                    .HasDefaultValue(0);

                o.HasIndex(x => new { x.FK_MarketingCampaignId, x.Enabled });

            });

            modelBuilder.Entity<MarketingCondition>(o =>
            {
                o.HasOne(x => x.MarketingRule)
                    .WithOne(x => x.Condition)
                    .HasForeignKey<MarketingCondition>(x => x.FK_MarketingRuleId)
                    .OnDelete(DeleteBehavior.Cascade);

                o.Property(x => x.MinAmount)
                    .HasColumnType("decimal(18,2)");

                o.Property(x => x.ConditionType)
                    .HasDefaultValue(MarketingConditionTypeEnum.OrderAmount);

                o.Property(x => x.OnlyScopeItems)
                    .HasDefaultValue(false);

                o.Property(x => x.ExcludeDiscountedItems)
                    .HasDefaultValue(false);

                o.HasIndex(x => x.FK_MarketingRuleId)
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");

            });

            modelBuilder.Entity<MarketingReward>(o =>
            {
                o.HasOne(x => x.MarketingRule)
                    .WithOne(x => x.Reward)
                    .HasForeignKey<MarketingReward>(x => x.FK_MarketingRuleId)
                    .OnDelete(DeleteBehavior.Cascade);

                o.Property(x => x.DeliveryType)
                    .HasDefaultValue(MarketingRewardDeliveryTypeEnum.ApplyImmediately);

                o.Property(x => x.DiscountAmount)
                    .HasColumnType("decimal(18,2)");

                o.Property(x => x.DiscountPercent)
                    .HasColumnType("decimal(5,2)");

                o.Property(x => x.MaxDiscountAmount)
                    .HasColumnType("decimal(18,2)");

                o.Property(x => x.SelectionQuantityPerQualification)
                    .HasDefaultValue(1);

                o.HasIndex(x => x.FK_MarketingRuleId)
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");

            });

            modelBuilder.Entity<MarketingRewardItem>(o =>
            {
                o.HasOne(x => x.MarketingReward)
                    .WithMany(x => x.Items)
                    .HasForeignKey(x => x.FK_MarketingRewardId)
                    .OnDelete(DeleteBehavior.Cascade);

                o.HasOne(x => x.ProdStock)
                    .WithMany()
                    .HasForeignKey(x => x.FK_ProdStockId)
                    .OnDelete(DeleteBehavior.NoAction);

                o.Property(x => x.OfferPrice)
                    .HasColumnType("decimal(18,2)");

                o.Property(x => x.MaxQuantityPerOrder)
                    .HasDefaultValue(1);

                o.Property(x => x.Enabled)
                    .HasDefaultValue(true);

                o.Property(x => x.SortOrder)
                    .HasDefaultValue(0);

                o.HasIndex(x => new
                {
                    x.FK_MarketingRewardId,
                    x.FK_ProdStockId
                })
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");

                o.HasIndex(x => new
                {
                    x.FK_MarketingRewardId,
                    x.Enabled,
                    x.SortOrder
                });
            });

            modelBuilder.Entity<MarketingScopeItem>(o =>
            {
                o.HasOne(x => x.MarketingRule)
                    .WithMany(x => x.ScopeItems)
                    .HasForeignKey(x => x.FK_MarketingRuleId)
                    .OnDelete(DeleteBehavior.Cascade);

                o.Property(x => x.RequiredQuantityPerQualification)
                    .HasDefaultValue(1);

                o.HasIndex(x => new
                {
                    x.FK_MarketingRuleId,
                    x.TargetType,
                    x.TargetId
                });

            });

            new SeedHelper(modelBuilder).SeedHost();
        }
    }
}
