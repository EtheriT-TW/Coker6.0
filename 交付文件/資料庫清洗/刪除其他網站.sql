use [Coker6]
go
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- 僅對隔離還原副本使用。0 只查結構，不寫入、不清洗；1 才執行清理。
DECLARE @Execute bit = 1;
DECLARE @ExpectedDatabase sysname = N'Coker6';
DECLARE @DeliveryAdminUsers TABLE (Id bigint PRIMARY KEY);
INSERT INTO @DeliveryAdminUsers VALUES (1), (2498), (12659);
IF DB_NAME() <> @ExpectedDatabase
    THROW 51000, N'目前資料庫不是指定的隔離副本。', 1;
IF @@TRANCOUNT <> 0
    THROW 51000, N'請先結束既有交易後再執行。', 1;
-- 結構基準：20261020025140_AddSiteRuntimeDiagnostics / CokerDbContextModelSnapshot。
-- 全部 dbo 業務表都必須已審核；新增／刪除／改名／型別、長度、NULL 變更一律停止。
IF ISNULL(HAS_PERMS_BY_NAME(DB_NAME(), N'DATABASE', N'VIEW DEFINITION'), 0) <> 1
    THROW 51000, N'需要資料庫 VIEW DEFINITION 權限才能完整核對結構。', 1;
IF EXISTS
(
    SELECT 1 FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE t.is_ms_shipped = 0 AND s.name <> N'dbo'
      AND (s.name <> N'HangFire' OR t.name NOT IN
          (N'Schema', N'Job', N'State', N'JobParameter', N'JobQueue', N'Server',
           N'List', N'Set', N'Counter', N'AggregatedCounter', N'Hash'))
)
    THROW 51000, N'發現未審核的非 dbo 資料表，請先補上清洗規則。', 1;
DECLARE @ReviewedSchema TABLE (TableName sysname PRIMARY KEY, ColumnSpec nvarchar(max));
INSERT INTO @ReviewedSchema (TableName, ColumnSpec) VALUES
    (N'Account_Logs', N'[BrowserInfo] nvarchar(512) NULL,[ClientIpAddress] nvarchar(64) NULL,[CorrelationId] nvarchar(128) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[CurrentStatus] int NULL,[DetailsJson] nvarchar(max) NULL,[ErrorTimes] int NOT NULL,[EventName] nvarchar(80) NULL,[FailureReason] nvarchar(100) NULL,[Id] bigint NOT NULL,[KeyFingerprint] nvarchar(64) NULL,[LastLoginTime] datetime2(7) NULL,[LockTime] datetime2(7) NULL,[NewEmail] nvarchar(150) NULL,[OldEmail] nvarchar(150) NULL,[PreviousStatus] int NULL,[RecipientEmail] nvarchar(150) NULL,[Status] int NOT NULL,[Success] bit NULL,[UUID] uniqueidentifier NOT NULL,[VerificationMethod] nvarchar(80) NULL,[WebsiteId] bigint NOT NULL'),
    (N'Advertise', N'[ActionType] int NOT NULL,[Clicks] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Describe] nvarchar(300) NULL,[EndDate] datetime2(7) NULL,[Exposure] int NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Id] bigint NOT NULL,[Img] nvarchar(100) NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Link] nvarchar(255) NULL,[Permanent] bit NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[SerNO] int NOT NULL,[StartDate] datetime2(7) NULL,[Target] bit NOT NULL,[Title] nvarchar(300) NOT NULL,[Type] bigint NOT NULL,[Visible] bit NOT NULL'),
    (N'Advertise_Logs', N'[Action] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_Adid] bigint NOT NULL,[FK_UserId] bigint NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[UUID] uniqueidentifier NOT NULL'),
    (N'Article', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DataJson] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[EndTime] datetime2(7) NULL,[EventEndDate] datetime2(7) NULL,[EventStartDate] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Latitude] float NULL,[Longitude] float NULL,[NewsletterCss] nvarchar(max) NULL,[NewsletterHtml] nvarchar(max) NULL,[NodeDate] datetime2(7) NULL,[PageText] nvarchar(max) NULL,[Popular] int NOT NULL,[PopularVisible] bit NOT NULL,[RemovedFromShelves] bit NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[SerNO] int NOT NULL,[StartTime] datetime2(7) NULL,[Subtitle] nvarchar(max) NULL,[Title] nvarchar(150) NULL,[Visible] bit NOT NULL,[permanent] bit NOT NULL'),
    (N'AuditLogs', N'[BrowserInfo] nvarchar(512) NULL,[ClientIpAddress] nvarchar(64) NULL,[ClientName] nvarchar(128) NULL,[CustomData] nvarchar(2000) NULL,[Exception] nvarchar(2000) NULL,[ExecutionDuration] int NOT NULL,[ExecutionTime] datetime2(7) NOT NULL,[FK_WebsiteId] bigint NULL,[Id] bigint NOT NULL,[ImpersonatorTenantId] int NOT NULL,[ImpersonatorUserId] bigint NOT NULL,[MethodName] nvarchar(256) NULL,[Parameters] nvarchar(max) NULL,[ReturnValue] nvarchar(max) NULL,[ServiceName] nvarchar(256) NULL,[UserId] bigint NULL'),
    (N'BackgroundTasks', N'[ActiveKey] nvarchar(250) NULL,[CompletionTime] datetime2(7) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Error] nvarchar(4000) NULL,[ExpireTime] datetime2(7) NULL,[FK_UserId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[HangfireJobId] nvarchar(100) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Message] nvarchar(500) NOT NULL,[Progress] int NOT NULL,[ResultFileName] nvarchar(255) NULL,[ResultFilePath] nvarchar(1000) NULL,[ResultJson] nvarchar(max) NULL,[SourceFilePath] nvarchar(1000) NULL,[StartTime] datetime2(7) NULL,[Status] int NOT NULL,[StorageKey] uniqueidentifier NOT NULL,[Type] int NOT NULL'),
    (N'Bonus', N'[Amount] int NOT NULL,[Balance] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[EndDate] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Note] nvarchar(max) NOT NULL,[StartDate] datetime2(7) NOT NULL,[Status] int NOT NULL,[UUID] uniqueidentifier NOT NULL'),
    (N'BonusLiabilities', N'[OutstandingPoints] int NOT NULL,[UUID] uniqueidentifier NOT NULL,[UpdatedAt] datetime2(7) NOT NULL'),
    (N'BonusLog', N'[Amount] int NOT NULL,[ExecutionTime] datetime2(7) NOT NULL,[Id] bigint NOT NULL,[Note] nvarchar(max) NOT NULL,[RefKey] bigint NULL,[Type] int NOT NULL,[UUID] uniqueidentifier NOT NULL'),
    (N'bonusLogDetails', N'[FK_BonusId] bigint NOT NULL,[FK_BonusLogsId] bigint NOT NULL,[UsedAmount] bigint NOT NULL'),
    (N'CdnProviderIpRanges', N'[Cidr] nvarchar(64) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IpVersion] tinyint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LastVerifiedTime] datetime2(7) NOT NULL,[Provider] nvarchar(50) NOT NULL'),
    (N'CdnProviderSyncStates', N'[AlertSent] bit NOT NULL,[ConsecutiveFailureCount] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastAlertTime] datetime2(7) NULL,[LastAttemptTime] datetime2(7) NULL,[LastError] nvarchar(4000) NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LastSuccessTime] datetime2(7) NULL,[Provider] nvarchar(50) NOT NULL'),
    (N'Companies', N'[Address] nvarchar(150) NOT NULL,[Contact] nvarchar(100) NOT NULL,[ContactEmail] nvarchar(150) NULL,[ContactJobTitle] nvarchar(100) NULL,[ContactPhone] nvarchar(50) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[CustomerType] int NOT NULL,[CustomerTypeOther] nvarchar(50) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(150) NOT NULL,[Id] bigint NOT NULL,[InvoiceInfo] nvarchar(500) NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(200) NOT NULL,[Phone] nvarchar(50) NULL,[Remark] nvarchar(2000) NULL,[SalesOwner] nvarchar(50) NULL,[TaxID] nvarchar(10) NOT NULL'),
    (N'ComponentPurposes', N'[Code] nvarchar(100) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(150) NOT NULL,[SerNo] int NOT NULL,[Visible] bit NOT NULL'),
    (N'Contacts', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(250) NOT NULL,[FK_SourceId] bigint NULL,[FK_WebMenuId] bigint NOT NULL,[FromDate] nvarchar(max) NULL,[Html] nvarchar(max) NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(200) NOT NULL,[Reply] nvarchar(max) NOT NULL,[ReplyTime] datetime2(7) NULL,[SourceType] int NULL,[Status] int NOT NULL,[TargetEmail] nvarchar(500) NOT NULL,[UserName] nvarchar(200) NOT NULL'),
    (N'CustSearch', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Placeholder] nvarchar(100) NOT NULL,[SearchAllArticle] bit NOT NULL,[SearchAllMenu] bit NOT NULL,[SearchAllProd] bit NOT NULL,[SerNo] int NOT NULL,[Title] nvarchar(100) NULL,[Visible] bit NOT NULL'),
    (N'Directory', N'[CalendarType] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(150) NULL,[FK_DefaultLayout] bigint NULL,[FK_Mid] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[FacetType] int NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[SortBy] int NOT NULL,[Title] nvarchar(60) NULL,[Type] int NOT NULL,[Visible] bit NOT NULL'),
    (N'DirectoryFacetRanges', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Enabled] bit NOT NULL,[End] int NOT NULL,[FK_DirectoryId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Sort] int NOT NULL,[Start] int NOT NULL'),
    (N'Favorites', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_AssocId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Type] int NOT NULL,[UUID] uniqueidentifier NOT NULL'),
    (N'FileBindMores', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileBindGuid] uniqueidentifier NOT NULL,[FK_FileUploadId] bigint NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[type] int NOT NULL'),
    (N'FileBinds', N'[AreaKey] nvarchar(max) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileUploadId] bigint NULL,[Guid] uniqueidentifier NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsVisible] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MediaLink] nvarchar(max) NOT NULL,[Name] nvarchar(max) NOT NULL,[SerNo] int NOT NULL,[Sid] bigint NOT NULL,[num] int NOT NULL,[type] int NOT NULL'),
    (N'FileCleanupCandidates', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileUploadId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[FirstDetectedTime] datetime2(7) NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastConfirmedTime] datetime2(7) NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Reason] nvarchar(500) NOT NULL'),
    (N'FileRecycleBinBindings', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileBindGuid] uniqueidentifier NOT NULL,[FK_FileUploadId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL'),
    (N'FileRecycleBinItems', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileUploadId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[OriginalPath] nvarchar(200) NOT NULL,[Reason] nvarchar(500) NOT NULL,[RecycledTime] datetime2(7) NOT NULL'),
    (N'FileReferences', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileUploadId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastConfirmedTime] datetime2(7) NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[NormalizedPath] nvarchar(1000) NOT NULL,[OccurrenceCount] int NOT NULL,[PhysicalFileExists] bit NOT NULL,[SourceField] nvarchar(80) NOT NULL,[SourceId] bigint NOT NULL,[SourceState] nvarchar(30) NOT NULL,[SourceType] nvarchar(80) NOT NULL'),
    (N'FileUploads', N'[ContentType] nvarchar(200) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[DownloadFileName] nvarchar(200) NULL,[FK_WebsiteId] bigint NOT NULL,[FileGuid] uniqueidentifier NULL,[GuidKey] uniqueidentifier NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsEncryption] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[OriginalFileName] nvarchar(200) NOT NULL,[Size] bigint NOT NULL'),
    (N'FlowSizes', N'[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[RequestSize] bigint NOT NULL,[ResponseSize] bigint NOT NULL,[Total] bigint NOT NULL,[actionTime] datetime2(7) NOT NULL'),
    (N'FooterTemplates', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_TemplateSectionsId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[css] nvarchar(max) NULL,[html] nvarchar(max) NULL,[saveCss] nvarchar(max) NULL,[saveHtml] nvarchar(max) NULL'),
    (N'FrontUsers', N'[Account] nvarchar(100) NULL,[Address] nvarchar(250) NULL,[Birthday] datetime2(7) NULL,[CellPhone] nvarchar(50) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(150) NULL,[ErrorTimes] int NOT NULL,[FK_User] bigint NULL,[ForgeIDSendDate] datetime2(7) NULL,[ForgetID] uniqueidentifier NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Level] bigint NULL,[LockTime] datetime2(7) NULL,[Name] nvarchar(150) NOT NULL,[OpenDate] datetime2(7) NULL,[OpenID] uniqueidentifier NOT NULL,[OpenIDSendDate] datetime2(7) NOT NULL,[Password] nvarchar(max) NOT NULL,[PrivacyAgreeTime] datetime2(7) NULL,[Sex] int NULL,[Status] int NOT NULL,[TelPhone] nvarchar(50) NULL,[UUID] uniqueidentifier NOT NULL'),
    (N'Html_Contents', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Disp_opt] bit NOT NULL,[EndDate] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Icon] nvarchar(100) NULL,[Id] bigint NOT NULL,[Img] nvarchar(100) NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Link] nvarchar(255) NULL,[Menu_id] bigint NULL,[ObjectType] int NOT NULL,[Ser_no] int NOT NULL,[StartDate] datetime2(7) NULL,[Target] bit NOT NULL,[Title] nvarchar(300) NULL,[Type] bigint NOT NULL,[permanent] bit NOT NULL'),
    (N'HtmlContentPurposes', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_ComponentPurposeId] bigint NOT NULL,[FK_HtmlContentId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL'),
    (N'HtmlSanitizeStates', N'[ContentHash] nvarchar(64) NOT NULL,[ContentKey] nvarchar(50) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[FK_Bid] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[SanitizePolicy] nvarchar(50) NOT NULL,[SanitizeVersion] nvarchar(30) NOT NULL,[SourceType] int NOT NULL'),
    (N'JsonObjects', N'[CacheKey] nvarchar(450) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_AId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[Json] nvarchar(max) NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Version] bigint NOT NULL'),
    (N'LogisticsBoxFees', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_LogisticsBoxId] bigint NOT NULL,[FK_LogisticsSettingId] bigint NOT NULL,[Fee] decimal(18,2) NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL'),
    (N'LogisticsBoxs', N'[CapacityPoint] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsActive] bit NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(max) NOT NULL,[Sort] int NOT NULL'),
    (N'LogisticsSettings', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Dis_Freight] int NULL,[DiscountFreightType] int NULL,[FK_WebsiteId] bigint NOT NULL,[Freight] int NULL,[FreightAmt2] int NULL,[FreightStatusType] int NOT NULL,[FreightType] int NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LogisticsType] int NOT NULL,[Low_Con] int NULL,[PreserveType] int NOT NULL,[Set_Default] bit NOT NULL,[SupportCashOnDelivery] bit NOT NULL,[Title] nvarchar(50) NOT NULL'),
    (N'LogisticsType_Payments', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_LogisticsSettingId] bigint NULL,[FK_PaymentTypeId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsEnabled] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[OverrideMaxAmount] decimal(18,2) NULL,[OverrideMinAmount] decimal(18,2) NULL,[ShippingType] int NULL'),
    (N'MappingCompanyAndWebsites', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_CompanyId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL'),
    (N'MappingFrontUserAndWebsite', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_UserId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL'),
    (N'MappingLogisticsSettingAndProd', N'[FK_LogisticsSettingId] bigint NOT NULL,[FK_ProdId] bigint NOT NULL'),
    (N'MappingOldNewUUID', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[TempUUID] uniqueidentifier NOT NULL,[UserUUID] uniqueidentifier NOT NULL'),
    (N'MappingUserAndPlatformRoles', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[PlatformRoleId] bigint NOT NULL,[UserId] bigint NOT NULL'),
    (N'MappingUserAndRoles', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[RoleId] bigint NOT NULL,[UUID] uniqueidentifier NOT NULL,[UserId] bigint NOT NULL'),
    (N'MappingUserAndWebsites', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[UserId] bigint NOT NULL,[WebsiteId] bigint NOT NULL'),
    (N'MappingWebsiteRelationship', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FatherId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[WebsiteId] bigint NOT NULL'),
    (N'MarketingCampaigns', N'[CampaignType] int NOT NULL,[CanStack] bit NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(500) NULL,[EndTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(100) NOT NULL,[NeverEnd] bit NOT NULL,[Priority] int NOT NULL,[Repeatable] bit NOT NULL,[StartTime] datetime2(7) NOT NULL,[Status] int NOT NULL'),
    (N'MarketingConditions', N'[ConditionType] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[ExcludeDiscountedItems] bit NOT NULL,[FK_MarketingRuleId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MinAmount] decimal(18,2) NULL,[MinQuantity] int NULL,[OnlyScopeItems] bit NOT NULL'),
    (N'MarketingRewardItems', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Enabled] bit NOT NULL,[FK_MarketingRewardId] bigint NOT NULL,[FK_ProdStockId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MaxQuantityPerOrder] int NOT NULL,[OfferPrice] decimal(18,2) NOT NULL,[SortOrder] int NOT NULL'),
    (N'MarketingRewards', N'[BonusAmount] int NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[DeliveryType] int NOT NULL,[DiscountAmount] decimal(18,2) NULL,[DiscountPercent] decimal(5,2) NULL,[FK_CouponTemplateId] bigint NULL,[FK_GiftProductId] bigint NULL,[FK_GiftProductStockId] bigint NULL,[FK_MarketingRuleId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MaxDiscountAmount] decimal(18,2) NULL,[MaxSelectionQuantityPerOrder] int NULL,[SelectionQuantityPerQualification] int NOT NULL'),
    (N'MarketingRules', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Enabled] bit NOT NULL,[FK_MarketingCampaignId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[RuleType] int NOT NULL,[ScopeType] int NOT NULL,[SortOrder] int NOT NULL'),
    (N'MarketingScopeItems', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_MarketingRuleId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[RequiredQuantityPerQualification] int NOT NULL,[TargetId] bigint NOT NULL,[TargetType] int NOT NULL'),
    (N'Marquees', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[EndTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[StartTime] datetime2(7) NULL,[disp_opt] bit NOT NULL,[link] nvarchar(255) NOT NULL,[permanent] bit NOT NULL,[placement] nvarchar(max) NOT NULL,[ser_no] int NOT NULL,[target] bit NOT NULL,[title] nvarchar(120) NOT NULL'),
    (N'NotFoundImage', N'[CreateDate] datetime2(7) NOT NULL,[FK_WebsiteId] bigint NOT NULL,[From] nvarchar(255) NOT NULL,[Id] bigint NOT NULL,[Url] nvarchar(255) NOT NULL'),
    (N'Notifications', N'[ActionUrl] nvarchar(1000) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_BackgroundTaskId] bigint NULL,[FK_UserId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsRead] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Message] nvarchar(1000) NOT NULL,[Title] nvarchar(250) NOT NULL,[Type] int NOT NULL'),
    (N'ObjectTypes', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[SerNo] int NOT NULL,[Title] nvarchar(150) NOT NULL'),
    (N'Order_Details', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_OId] bigint NOT NULL,[FK_SCId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL'),
    (N'Order_Headers', N'[Bonus] int NULL,[Carrier] nvarchar(max) NULL,[CompletedDate] datetime2(7) NULL,[CouponId] bigint NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Discount] decimal(18,2) NULL,[DiscountBreakdownJson] nvarchar(max) NULL,[FK_UUID] uniqueidentifier NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Fk_Tid] uniqueidentifier NOT NULL,[Fk_UserId] bigint NULL,[Freight] decimal(18,2) NOT NULL,[GetBonus] int NULL,[Id] bigint NOT NULL,[InvoiceAddress] nvarchar(250) NULL,[InvoiceRecipient] int NULL,[InvoiceTitle] nvarchar(max) NULL,[InvoiceType] int NULL,[IsDeleted] bit NOT NULL,[IsTemp] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Memo] nvarchar(500) NULL,[Orderer] nvarchar(150) NOT NULL,[OrdererAddress] nvarchar(250) NOT NULL,[OrdererCellPhone] nvarchar(50) NOT NULL,[OrdererEmail] nvarchar(150) NOT NULL,[OrdererSex] int NULL,[OrdererTelePhone] nvarchar(50) NULL,[OrdererZipCode] nvarchar(max) NOT NULL,[Payment] bigint NOT NULL,[PersonalInvoiceType] int NULL,[Recipient] nvarchar(150) NOT NULL,[RecipientAddress] nvarchar(250) NOT NULL,[RecipientCellPhone] nvarchar(50) NOT NULL,[RecipientEmail] nvarchar(150) NOT NULL,[RecipientSex] int NULL,[RecipientTelePhone] nvarchar(50) NULL,[RecipientZipCode] nvarchar(max) NOT NULL,[Remark] nvarchar(500) NULL,[RepayDate] datetime2(7) NULL,[RepayTimes] int NULL,[Service_Charge] int NULL,[Shipping] bigint NOT NULL,[State] int NOT NULL,[Subtotal] decimal(18,2) NOT NULL,[SystemMemo] nvarchar(1000) NULL,[TrackingNumber] nvarchar(20) NULL,[TransactionId] nvarchar(max) NULL,[UniformId] nvarchar(max) NULL,[refundTransactionDate] datetime2(7) NULL,[refundTransactionId] nvarchar(max) NULL'),
    (N'Order_Logistics', N'[AllPayLogisticsID] nvarchar(max) NULL,[BookingNote] nvarchar(max) NULL,[CVSAddress] nvarchar(max) NULL,[CVSOutSide] nvarchar(max) NULL,[CVSPaymentNo] nvarchar(max) NULL,[CVSStoreID] nvarchar(max) NULL,[CVSStoreName] nvarchar(max) NULL,[CVSTelephone] nvarchar(max) NULL,[CVSValidationNo] nvarchar(max) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_OhId] bigint NOT NULL,[GoodsWeight] decimal(18,2) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LogisticsStatusCode] nvarchar(max) NULL,[LogisticsSubType] nvarchar(max) NOT NULL,[LogisticsType] nvarchar(max) NOT NULL,[MerchantTradeDate] datetime2(7) NULL,[MerchantTradeNo] nvarchar(max) NULL,[ReceiverAddress] nvarchar(max) NULL,[ReceiverCellPhone] nvarchar(max) NULL,[ReceiverEmail] nvarchar(max) NULL,[ReceiverName] nvarchar(max) NULL,[ReceiverPhone] nvarchar(max) NULL,[ReceiverZipCode] nvarchar(max) NULL,[RtnMerchantTradeNo] nvarchar(max) NULL,[RtnOrderNo] nvarchar(max) NULL,[SenderAddress] nvarchar(max) NULL,[SenderCellPhone] nvarchar(max) NULL,[SenderName] nvarchar(max) NULL,[SenderPhone] nvarchar(max) NULL,[SenderZipCode] nvarchar(max) NULL,[Specification] nvarchar(max) NULL,[Temperature] nvarchar(max) NULL,[UpdateStatusDate] datetime2(7) NULL'),
    (N'PageTextBackfillStates', N'[CompletionTime] datetime2(7) NULL,[ContentType] nvarchar(20) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[FailedCount] int NOT NULL,[FailedIdsJson] nvarchar(max) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastError] nvarchar(4000) NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LastProcessedId] bigint NOT NULL,[ProcessedCount] int NOT NULL,[RemainingNullCount] int NOT NULL,[StartTime] datetime2(7) NULL,[Status] nvarchar(30) NOT NULL,[TargetMaxId] bigint NOT NULL,[TotalCount] int NOT NULL'),
    (N'PaymentTypes', N'[CanRefund] bit NOT NULL,[Code] nvarchar(50) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[CvsStoreSelectionMode] int NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_ThirdPartyId] bigint NOT NULL,[Icons] nvarchar(max) NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MaxAmount] decimal(18,2) NULL,[MinAmount] decimal(18,2) NOT NULL,[RefundWorkDay] int NOT NULL,[RepayAfterMinutes] int NULL,[SerNo] int NOT NULL,[ThirdPartyId] bigint NULL,[Title] nvarchar(50) NULL,[Used] bit NOT NULL'),
    (N'PaymentTypesValues', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_PaymentTypesId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Used] bit NOT NULL'),
    (N'PermissionDetail', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[FK_RoleId] bigint NULL,[FK_TargetId] bigint NULL,[FK_UserId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsGranted] bit NOT NULL,[Name] nvarchar(300) NOT NULL,[Type] int NOT NULL'),
    (N'Permissions', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[FK_RoleId] bigint NULL,[FK_UserId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsGranted] bit NOT NULL,[Name] nvarchar(300) NOT NULL'),
    (N'PlatformAdministratorInvitations', N'[ApprovedAtUtc] datetime2(7) NULL,[ApprovedByUserId] bigint NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[EmailVerifiedAtUtc] datetime2(7) NULL,[ExpiresAtUtc] datetime2(7) NOT NULL,[Id] bigint NOT NULL,[InvitedEmail] nvarchar(150) NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MvcRoleId] bigint NULL,[PlatformRoleId] bigint NULL,[RevokedAtUtc] datetime2(7) NULL,[RevokedByUserId] bigint NULL,[UserId] bigint NOT NULL'),
    (N'PlatformDomains', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[DomainName] nvarchar(255) NOT NULL,[EndDate] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsProxyPurchase] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[PasswordCipher] nvarchar(1000) NULL,[Registrar] nvarchar(200) NULL,[Remark] nvarchar(2000) NULL,[StartDate] datetime2(7) NULL'),
    (N'PlatformRoles', N'[Code] nvarchar(80) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(500) NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsEnabled] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(100) NOT NULL,[Sort] int NOT NULL'),
    (N'PlatformWebsites', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_CompanyId] bigint NOT NULL,[FK_PlatformDomainId] bigint NULL,[FK_WebsiteId] bigint NULL,[HostLocation] nvarchar(200) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsDomainPending] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Level] int NULL,[Locale] nvarchar(10) NULL,[Name] nvarchar(250) NOT NULL,[Remark] nvarchar(2000) NULL,[ServiceEndDate] datetime2(7) NULL,[ServiceStartDate] datetime2(7) NULL,[Status] int NOT NULL,[TerminatedDate] datetime2(7) NULL,[Url] nvarchar(500) NULL'),
    (N'Prod_Logs', N'[Action] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[FK_Pid] bigint NOT NULL,[FK_UserId] bigint NULL,[Id] bigint NOT NULL,[Remark] nvarchar(300) NOT NULL,[UUID] uniqueidentifier NOT NULL'),
    (N'Prod_Prices', N'[Bonus] int NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_PSId] bigint NOT NULL,[FK_RId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Price] decimal(18,2) NULL'),
    (N'Prod_Spec_Types', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[SeoVariantProperty] int NOT NULL,[Type] nvarchar(max) NOT NULL'),
    (N'Prod_Specs', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_Tid] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Title] nvarchar(max) NOT NULL'),
    (N'Prod_Stocks', N'[Alert_Qty] int NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_Pid] bigint NOT NULL,[FK_S1id] bigint NULL,[FK_S2id] bigint NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsTimePrice] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Min_Qty] int NULL,[PackingPoint] int NOT NULL,[Price] decimal(18,2) NOT NULL,[Ser_No] int NOT NULL,[SpecDescription] nvarchar(3000) NULL,[Stock] int NULL,[SubItemNo] nvarchar(max) NULL,[Visible] bit NOT NULL'),
    (N'Prod_TechCerts', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_PId] bigint NOT NULL,[FK_TCId] bigint NOT NULL,[Id] bigint NOT NULL,[IsChecked] bit NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL'),
    (N'Prods', N'[Clicks] int NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(3000) NOT NULL,[Discount] float NULL,[EndTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Id] bigint NOT NULL,[Introduction] nvarchar(3000) NOT NULL,[IsDeleted] bit NOT NULL,[ItemNo] nvarchar(100) NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[NoStockManagement] bit NOT NULL,[PageText] nvarchar(max) NULL,[Popular] int NOT NULL,[PopularVisible] bit NOT NULL,[RemovedFromShelves] bit NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[Ser_No] int NOT NULL,[StartTime] datetime2(7) NULL,[Status] int NOT NULL,[Title] nvarchar(150) NOT NULL,[Visible] bit NOT NULL,[oStatus] int NULL,[permanent] bit NOT NULL'),
    (N'ProvisioningAgentStatuses', N'[AgentVersion] nvarchar(50) NOT NULL,[AppPoolMetricsJson] nvarchar(max) NOT NULL,[CpuUsagePercent] float NULL,[DiskMetricsJson] nvarchar(max) NOT NULL,[DryRun] bit NOT NULL,[LastSeenAtUtc] datetime2(7) NOT NULL,[MachineName] nvarchar(200) NOT NULL,[MemoryTotalBytes] bigint NOT NULL,[MemoryUsagePercent] float NULL,[MemoryUsedBytes] bigint NOT NULL,[ServerId] nvarchar(100) NOT NULL,[TlsSnapshotJson] nvarchar(max) NULL,[WorkerId] nvarchar(200) NOT NULL'),
    (N'ProvisioningMetricSamples', N'[CpuUsagePercent] float NULL,[DiskMetricsJson] nvarchar(max) NOT NULL,[Id] bigint NOT NULL,[MemoryTotalBytes] bigint NOT NULL,[MemoryUsagePercent] float NULL,[MemoryUsedBytes] bigint NOT NULL,[SampledAtUtc] datetime2(7) NOT NULL,[ServerId] nvarchar(100) NOT NULL'),
    (N'ProvisioningSiteMetricSamples', N'[ApplicationPoolName] nvarchar(200) NOT NULL,[CpuUsagePercent] float NULL,[Id] bigint NOT NULL,[OnlineVisitors] bigint NULL,[PrivateMemoryBytes] bigint NOT NULL,[ProcessIdsJson] nvarchar(max) NOT NULL,[RuntimeMetricsJson] nvarchar(max) NULL,[SampledAtUtc] datetime2(7) NOT NULL,[ServerId] nvarchar(100) NOT NULL,[WebsiteIdsJson] nvarchar(max) NOT NULL,[WebsiteNamesJson] nvarchar(max) NOT NULL,[WorkingSetBytes] bigint NOT NULL'),
    (N'ProvisioningTasks', N'[AttemptCount] int NOT NULL,[ClaimedBy] nvarchar(200) NULL,[CompletedAtUtc] datetime2(7) NULL,[CreatedAtUtc] datetime2(7) NOT NULL,[Id] bigint NOT NULL,[LeaseExpiresAtUtc] datetime2(7) NULL,[PayloadJson] nvarchar(max) NOT NULL,[ResultMessage] nvarchar(4000) NULL,[StartedAtUtc] datetime2(7) NULL,[Status] int NOT NULL,[TargetServerId] nvarchar(100) NOT NULL,[Type] int NOT NULL'),
    (N'Recipients', N'[Address] nvarchar(300) NOT NULL,[CVSAddress] nvarchar(300) NULL,[CVSOutSide] nvarchar(10) NULL,[CVSStoreID] nvarchar(20) NULL,[CVSStoreName] nvarchar(150) NULL,[CVSTelephone] nvarchar(30) NULL,[CellPhone] nvarchar(16) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(150) NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LogisticsType] int NULL,[Name] nvarchar(150) NOT NULL,[Sex] int NOT NULL,[TelePhone] nvarchar(30) NOT NULL,[UUID] uniqueidentifier NOT NULL,[ZipCode] nvarchar(10) NULL'),
    (N'RemoteDailyAggregationRuns', N'[AggregationVersion] int NOT NULL,[CompletedAt] datetime2(7) NOT NULL,[Id] bigint NOT NULL,[SourceRows] bigint NOT NULL,[StatisticDate] date NOT NULL'),
    (N'RemoteDailyStatistics', N'[AggregatedAt] datetime2(7) NOT NULL,[EffectiveUniqueVisitors] bigint NOT NULL,[EffectiveViews] bigint NOT NULL,[FK_ArticleId] bigint NOT NULL,[FK_ProdId] bigint NOT NULL,[FK_TechCertId] bigint NOT NULL,[FK_WebmenuId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[LegacyViews] bigint NOT NULL,[PageViews] bigint NOT NULL,[Scope] tinyint NOT NULL,[StatisticDate] date NOT NULL,[TotalVisibleSeconds] bigint NOT NULL,[UniqueVisitors] bigint NOT NULL'),
    (N'RemoteHourlyStatistics', N'[AggregatedAt] datetime2(7) NOT NULL,[EffectiveUniqueVisitors] bigint NOT NULL,[EffectiveViews] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[LegacyViews] bigint NOT NULL,[PageViews] bigint NOT NULL,[StatisticHour] datetime2(0) NOT NULL,[TotalVisibleSeconds] bigint NOT NULL,[UniqueVisitors] bigint NOT NULL'),
    (N'Remotes', N'[AnalyticsAggregatedAt] datetime2(7) NULL,[BrowserInfo] nvarchar(512) NULL,[ClientIpAddress] nvarchar(64) NULL,[EngagedAt] datetime2(7) NULL,[ExecutionTime] datetime2(7) NOT NULL,[FK_ArticleId] bigint NULL,[FK_ProdId] bigint NULL,[FK_TechCertId] bigint NULL,[FK_UserId] bigint NULL,[FK_WebmenuId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[HabitsProcessedAt] datetime2(7) NULL,[HasInteraction] bit NULL,[Id] bigint NOT NULL,[IsEngaged] bit NULL,[LastHeartbeatAt] datetime2(7) NULL,[LastStatComputedAt] datetime2(7) NOT NULL,[LeaveTime] datetime2(7) NOT NULL,[State] int NOT NULL,[TimeOnPage] int NOT NULL,[TrackingEventId] uniqueidentifier NULL,[TrafficQuality] int NULL,[UUID] uniqueidentifier NOT NULL'),
    (N'Roles', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsSuperUser] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(max) NOT NULL,[Ser_No] int NOT NULL,[Type] int NOT NULL'),
    (N'SearchLogs', N'[ClientIpAddress] nvarchar(255) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[FK_CustSearchId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[Key] nvarchar(100) NOT NULL,[UUID] uniqueidentifier NOT NULL'),
    (N'SecondaryContacts', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(150) NULL,[FK_CompanyId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[JobTitle] nvarchar(100) NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(100) NOT NULL,[Phone] nvarchar(50) NULL,[Sort] int NOT NULL'),
    (N'ShoppingCarts', N'[AdditionalParentLabel] nvarchar(max) NULL,[AdditionalParentShoppingCartId] bigint NULL,[Bonus] int NULL,[CVSAddress] nvarchar(max) NULL,[CVSOutSide] nvarchar(max) NULL,[CVSStoreID] nvarchar(max) NULL,[CVSStoreName] nvarchar(max) NULL,[CVSTelephone] nvarchar(max) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Discont] decimal(18,2) NULL,[FK_MarketingRewardItemId] bigint NULL,[FK_PSid] bigint NOT NULL,[FK_PriceId] bigint NULL,[FK_S1id] bigint NULL,[FK_S2id] bigint NULL,[FK_Tid] uniqueidentifier NOT NULL,[FK_Uid] bigint NULL,[Id] bigint NOT NULL,[IsAdditional] bit NOT NULL,[IsDeleted] bit NOT NULL,[IsOrder] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LogisticsSubType] nvarchar(max) NULL,[OldQuantity] int NOT NULL,[Price] decimal(18,2) NOT NULL,[PriceType] int NULL,[ProdName] nvarchar(max) NULL,[ProductId] bigint NULL,[Quantity] int NOT NULL,[S1Title] nvarchar(max) NULL,[S2Title] nvarchar(max) NULL,[Ser_No] int NOT NULL,[UUID] uniqueidentifier NOT NULL'),
    (N'StoreSet', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DefaultValue] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_StoreSetGroupId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Level] int NULL,[jobID] nvarchar(max) NOT NULL,[key] nvarchar(50) NOT NULL,[maxlength] int NULL,[memo] nvarchar(max) NULL,[name] nvarchar(150) NOT NULL,[pattern] nvarchar(max) NULL,[type] int NOT NULL'),
    (N'StoreSetDetail', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_StoreSetId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[enable] bit NOT NULL,[value] nvarchar(max) NULL'),
    (N'StoreSetGroup', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[Id] bigint NOT NULL,[Image] nvarchar(150) NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Title] nvarchar(100) NOT NULL'),
    (N'StoreSetItems', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_StoreSetId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDefault] bit NOT NULL,[IsDeleted] bit NOT NULL,[Key] nvarchar(50) NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Level] int NULL,[Value] nvarchar(150) NOT NULL'),
    (N'Tag_Associates', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_AId] bigint NOT NULL,[FK_TId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Type] int NOT NULL'),
    (N'Tag_Groups', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Disp_Opt] bit NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Title] nvarchar(100) NULL'),
    (N'Tag_TagGroups', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_TGId] bigint NOT NULL,[FK_TId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL'),
    (N'Tags', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsTemporary] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[ProductIdfUpdatedAtUtc] datetime2(7) NULL,[ProductIdfWeight] float NOT NULL,[Title] nvarchar(50) NOT NULL'),
    (N'TechnicalCertificates', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(250) NULL,[Disp_opt] bit NOT NULL,[EndDate] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NOT NULL,[Id] bigint NOT NULL,[Img] nvarchar(100) NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Permanent] bit NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[Ser_no] int NOT NULL,[StartDate] datetime2(7) NULL,[Title] nvarchar(300) NULL'),
    (N'Templates', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[Enable] bit NOT NULL,[FK_ThemeId] bigint NULL,[FK_WebsiteID] bigint NOT NULL,[HeadType] int NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LayoutConfig] nvarchar(max) NOT NULL,[LayoutType] int NOT NULL,[Name] nvarchar(150) NOT NULL,[templateTypeEnum] int NOT NULL'),
    (N'TemplateSections', N'[ContentConfig] nvarchar(max) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_TemplateID] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[sectionType] int NOT NULL'),
    (N'ThirdParties', N'[AuditUrl] nvarchar(255) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MaxPay] int NULL,[Memo] nvarchar(100) NULL,[PaymentUrl] nvarchar(255) NULL,[RefundUrl] nvarchar(255) NULL,[ServiceType] int NOT NULL,[Title] nvarchar(50) NOT NULL,[TokenUrl] nvarchar(255) NULL,[ser_no] int NOT NULL'),
    (N'ThirdPartyKeypairs', N'[Code] nvarchar(50) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_TPid] bigint NOT NULL,[Id] bigint NOT NULL,[InputType] int NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[PromptText] nvarchar(50) NULL,[Title] nvarchar(50) NULL'),
    (N'ThirdPartyKeypairValues', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_ThirdPartyKeypairId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Value] nvarchar(300) NOT NULL'),
    (N'TokenMapShoppingCarts', N'[FK_Tid] bigint NOT NULL,[UUID] uniqueidentifier NOT NULL'),
    (N'Tokens', N'[EndTime] datetime2(7) NULL,[PrivacyAgreeTime] datetime2(7) NULL,[StartTime] datetime2(7) NOT NULL,[UUID] uniqueidentifier NOT NULL,[UserID] bigint NULL,[id] uniqueidentifier NOT NULL,[ip] nvarchar(max) NOT NULL,[websiteId] bigint NOT NULL'),
    (N'UserActivityTags', N'[CreateTime] datetime2(7) NOT NULL,[FK_RemoteId] bigint NOT NULL,[FK_TId] bigint NOT NULL,[Id] uniqueidentifier NOT NULL,[Weight] real NOT NULL'),
    (N'UserGroupingDetails', N'[FK_GropingId] bigint NOT NULL,[UUID] uniqueidentifier NOT NULL'),
    (N'UserGroupings', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NOT NULL,[Enable] bit NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Title] nvarchar(max) NOT NULL'),
    (N'Users', N'[Account] nvarchar(100) NULL,[Address] nvarchar(250) NULL,[CellPhone] nvarchar(50) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(150) NULL,[ErrorTimes] int NOT NULL,[ForgeIDSendDate] datetime2(7) NULL,[ForgetID] uniqueidentifier NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Level] int NULL,[LockTime] datetime2(7) NULL,[Name] nvarchar(150) NOT NULL,[Nickname] nvarchar(50) NULL,[Password] nvarchar(max) NOT NULL,[Sex] int NULL,[Status] int NULL,[TelPhone] nvarchar(50) NULL,[Total] int NULL,[UUID] uniqueidentifier NULL,[UniformId] nvarchar(max) NULL'),
    (N'UserTagStatistics', N'[FK_TagId] bigint NOT NULL,[Id] bigint NOT NULL,[LastActivityTime] datetime2(7) NOT NULL,[LastModificationTime] datetime2(7) NOT NULL,[TotalTimes] int NOT NULL,[UUID] uniqueidentifier NOT NULL,[Weight] float NOT NULL'),
    (N'WebMenus', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[FK_RootNodeId] bigint NULL,[FK_TopNodeId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Id] bigint NOT NULL,[ImgId] bigint NULL,[IsDeleted] bit NOT NULL,[LanBar] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LinkUrl] nvarchar(255) NULL,[OverImgId] bigint NULL,[PageText] nvarchar(max) NULL,[PageType] int NOT NULL,[Popular] int NOT NULL,[PopularVisible] bit NOT NULL,[RemovedFromShelves] bit NOT NULL,[RouterName] nvarchar(50) NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[SerNO] int NOT NULL,[ShowToMenu] bit NOT NULL,[SubTitle] nvarchar(100) NULL,[Target] bit NULL,[Title] nvarchar(100) NULL,[Visible] bit NOT NULL,[VisibleFooter] bit NOT NULL,[VisibleHeader] bit NOT NULL,[VisibleTitle] bit NOT NULL,[icon] nvarchar(50) NULL'),
    (N'WebsiteAlternateUrls', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_PlatformDomainId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Sort] int NOT NULL,[Url] nvarchar(255) NOT NULL'),
    (N'WebsiteCacheStates', N'[CacheKey] nvarchar(250) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Version] bigint NOT NULL'),
    (N'WebsiteEnterpriseEmails', N'[AccountType] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[ExpirationDate] datetime2(7) NOT NULL,[FK_CompanyId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[OtherType] nvarchar(200) NULL,[Remark] nvarchar(2000) NULL,[TotalAccounts] int NOT NULL'),
    (N'Websites', N'[Contact] nvarchar(max) NULL,[ContactMail] nvarchar(200) NULL,[Contract] nvarchar(200) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DefaultUrl] nvarchar(255) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[EndDate] datetime2(7) NULL,[FK_PlatformDomainId] bigint NULL,[HostLocation] nvarchar(200) NULL,[Icon] nvarchar(max) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsDomainPending] bit NOT NULL,[Keywords] nvarchar(max) NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LayoutType] int NULL,[Level] int NOT NULL,[Locale] nvarchar(10) NOT NULL,[Logo] nvarchar(max) NULL,[OrgName] nvarchar(100) NOT NULL,[PlatformRemark] nvarchar(2000) NULL,[ServiceStatus] int NOT NULL,[StartDate] datetime2(7) NULL,[Statement] nvarchar(max) NULL,[SuspendedDate] datetime2(7) NULL,[TerminatedDate] datetime2(7) NULL,[Title] nvarchar(250) NOT NULL,[Type] nvarchar(10) NOT NULL');
-- 2026-10-24：依實際副本回報加入四張舊表；可不存在，存在時仍嚴格核對欄位。
INSERT INTO @ReviewedSchema (TableName, ColumnSpec)
SELECT legacy.TableName, legacy.ColumnSpec
FROM (VALUES
    (N'__EFMigrationsHistoryOld_20260728', N'[MigrationId] nvarchar(150) NOT NULL,[ProductVersion] nvarchar(32) NOT NULL'),
    (N'Html_ContentsTemp', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Disp_opt] bit NOT NULL,[EndDate] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Icon] nvarchar(100) NULL,[Id] bigint NOT NULL,[Img] nvarchar(100) NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Link] nvarchar(255) NULL,[Menu_id] bigint NULL,[ObjectType] int NOT NULL,[Ser_no] int NOT NULL,[StartDate] datetime2(7) NULL,[Target] bit NOT NULL,[Title] nvarchar(300) NULL,[Type] bigint NOT NULL,[permanent] bit NOT NULL'),
    (N'Prod_Prices_temp0719', N'[FK_PSId] bigint NOT NULL,[FK_RId] bigint NOT NULL,[Price] float NULL,[RowNum] bigint NULL,[id] bigint NOT NULL'),
    (N'regWebMenus', N'[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[FK_RootNodeId] bigint NULL,[FK_TopNodeId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Id] bigint NOT NULL,[ImgId] bigint NULL,[IsDeleted] bit NOT NULL,[LanBar] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LinkUrl] nvarchar(255) NULL,[OverImgId] bigint NULL,[PageType] int NOT NULL,[Popular] int NOT NULL,[PopularVisible] bit NOT NULL,[RemovedFromShelves] bit NOT NULL,[RouterName] nvarchar(50) NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[SerNO] int NOT NULL,[ShowToMenu] bit NOT NULL,[Target] bit NULL,[Title] nvarchar(100) NULL,[Visible] bit NOT NULL,[VisibleFooter] bit NOT NULL,[VisibleHeader] bit NOT NULL,[VisibleTitle] bit NOT NULL,[icon] nvarchar(50) NULL')
) legacy(TableName, ColumnSpec)
WHERE OBJECT_ID(N'dbo.' + QUOTENAME(legacy.TableName), N'U') IS NOT NULL;
DECLARE @ActualSchema TABLE (TableName sysname PRIMARY KEY, ColumnSpec nvarchar(max));
INSERT INTO @ActualSchema
SELECT t.name,
    STRING_AGG(CONVERT(nvarchar(max), QUOTENAME(c.name) + N' ' + TYPE_NAME(c.user_type_id)
        + CASE
            WHEN TYPE_NAME(c.user_type_id) IN (N'nvarchar', N'nchar')
                THEN N'(' + CASE WHEN c.max_length = -1 THEN N'max' ELSE CONVERT(nvarchar(10), c.max_length / 2) END + N')'
            WHEN TYPE_NAME(c.user_type_id) IN (N'varchar', N'char', N'varbinary', N'binary')
                THEN N'(' + CASE WHEN c.max_length = -1 THEN N'max' ELSE CONVERT(nvarchar(10), c.max_length) END + N')'
            WHEN TYPE_NAME(c.user_type_id) IN (N'decimal', N'numeric')
                THEN N'(' + CONVERT(nvarchar(10), c.precision) + N',' + CONVERT(nvarchar(10), c.scale) + N')'
            WHEN TYPE_NAME(c.user_type_id) IN (N'datetime2', N'time', N'datetimeoffset')
                THEN N'(' + CONVERT(nvarchar(10), c.scale) + N')'
            ELSE N'' END
        + CASE WHEN c.is_nullable = 1 THEN N' NULL' ELSE N' NOT NULL' END), N',')
        WITHIN GROUP (ORDER BY c.name COLLATE Latin1_General_100_BIN2)
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
WHERE s.name = N'dbo' AND t.is_ms_shipped = 0 AND t.name <> N'__EFMigrationsHistory'
GROUP BY t.name;
IF EXISTS
(
    SELECT 1 FROM @ReviewedSchema e FULL JOIN @ActualSchema a ON a.TableName = e.TableName
    WHERE e.TableName IS NULL OR a.TableName IS NULL
       OR e.ColumnSpec COLLATE Latin1_General_100_BIN2 <> a.ColumnSpec COLLATE Latin1_General_100_BIN2
)
BEGIN
    SELECT COALESCE(e.TableName, a.TableName) AS TableName,
        e.ColumnSpec AS ExpectedColumns, a.ColumnSpec AS ActualColumns
    FROM @ReviewedSchema e FULL JOIN @ActualSchema a ON a.TableName = e.TableName
    WHERE e.TableName IS NULL OR a.TableName IS NULL
       OR e.ColumnSpec COLLATE Latin1_General_100_BIN2 <> a.ColumnSpec COLLATE Latin1_General_100_BIN2;
    THROW 51000, N'資料庫結構與已審核清洗基準不同，請先審核差異並更新腳本；禁止直接略過檢查。', 1;
END;

IF EXISTS (SELECT 1 FROM @DeliveryAdminUsers k WHERE NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.Id = k.Id))
    THROW 51000, N'找不到指定交付帳號，請先確認 Step 1、2、3 的帳號清單。', 1;
IF @Execute = 0
BEGIN
    PRINT N'結構檢查完成，未清理資料。確認使用隔離副本後將 @Execute 改為 1。';
    RETURN;
END;
BEGIN TRY
BEGIN TRAN;

-- 清空舊版複製表，避免歷史 HTML、選單與價格隨交付備份外流；保留空表結構。
-- 只處理明確列名且已通過結構核對的四表；若受外鍵或 Trigger 阻擋則由外層回滾。
DECLARE @LegacyCleanupSql nvarchar(max);
SELECT @LegacyCleanupSql = STRING_AGG(CONVERT(nvarchar(max),
    N'DELETE FROM ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N';'
    + N' IF EXISTS (SELECT 1 FROM ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
    + N') THROW 51000, N''舊表仍有資料，取消清洗。'', 1;'), CHAR(10))
FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = N'dbo' AND t.name IN (N'__EFMigrationsHistoryOld_20260728', N'Html_ContentsTemp', N'Prod_Prices_temp0719', N'regWebMenus');
IF @LegacyCleanupSql IS NOT NULL EXEC sys.sp_executesql @LegacyCleanupSql;


------------------------------------------------------------
-- 0. 保留網站清單
------------------------------------------------------------
IF OBJECT_ID('tempdb..#KeepWebsites') IS NOT NULL DROP TABLE #KeepWebsites;

CREATE TABLE #KeepWebsites
(
    Id BIGINT NOT NULL PRIMARY KEY
);

INSERT INTO #KeepWebsites (Id)
VALUES (23), (36);

IF NOT EXISTS (SELECT 1 FROM dbo.Websites WHERE Id = 23)
BEGIN
    RAISERROR(N'WebsiteId 23 不存在，停止清洗。', 16, 1);
    ROLLBACK TRAN;
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Websites WHERE Id = 36)
BEGIN
    RAISERROR(N'WebsiteId 36 不存在，停止清洗。', 16, 1);
    ROLLBACK TRAN;
    RETURN;
END;


------------------------------------------------------------
-- 1. 建立保留資料暫存表
------------------------------------------------------------

IF OBJECT_ID('tempdb..#KeepFrontUsers') IS NOT NULL DROP TABLE #KeepFrontUsers;

SELECT DISTINCT m.FK_UserId AS Id
INTO #KeepFrontUsers
FROM dbo.MappingFrontUserAndWebsite m
JOIN #KeepWebsites w ON w.Id = m.FK_WebsiteId;

INSERT INTO #KeepFrontUsers (Id)
SELECT fu.Id FROM dbo.FrontUsers fu
WHERE NOT EXISTS (SELECT 1 FROM #KeepFrontUsers k WHERE k.Id = fu.Id)
  AND (EXISTS (SELECT 1 FROM dbo.Order_Headers oh JOIN #KeepWebsites w ON w.Id = oh.FK_WebsiteId WHERE oh.FK_UUID = fu.UUID)
       OR EXISTS (SELECT 1 FROM dbo.Recipients r JOIN #KeepWebsites w ON w.Id = r.FK_WebsiteId WHERE r.UUID = fu.UUID));

ALTER TABLE #KeepFrontUsers
ADD CONSTRAINT PK_KeepFrontUsers PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepUsers') IS NOT NULL DROP TABLE #KeepUsers;

SELECT DISTINCT m.UserId AS Id
INTO #KeepUsers
FROM dbo.MappingUserAndWebsites m
JOIN #KeepWebsites w ON w.Id = m.WebsiteId;

INSERT INTO #KeepUsers (Id)
SELECT DISTINCT fu.FK_User
FROM dbo.FrontUsers fu
JOIN #KeepFrontUsers kfu ON kfu.Id = fu.Id
WHERE fu.FK_User IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM #KeepUsers ku
      WHERE ku.Id = fu.FK_User
  );

INSERT INTO #KeepUsers (Id)
SELECT u.Id FROM dbo.Users u
WHERE u.Id IN (SELECT Id FROM @DeliveryAdminUsers)
  AND NOT EXISTS (SELECT 1 FROM #KeepUsers k WHERE k.Id = u.Id);

ALTER TABLE #KeepUsers
ADD CONSTRAINT PK_KeepUsers PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepRoles') IS NOT NULL DROP TABLE #KeepRoles;

SELECT DISTINCT r.Id
INTO #KeepRoles
FROM dbo.Roles r
WHERE r.FK_WebsiteId IS NULL
   OR r.FK_WebsiteId IN (SELECT Id FROM #KeepWebsites);

ALTER TABLE #KeepRoles
ADD CONSTRAINT PK_KeepRoles PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepProds') IS NOT NULL DROP TABLE #KeepProds;

SELECT p.Id
INTO #KeepProds
FROM dbo.Prods p
JOIN #KeepWebsites w ON w.Id = p.FK_WebsiteId;

ALTER TABLE #KeepProds
ADD CONSTRAINT PK_KeepProds PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepProdStocks') IS NOT NULL DROP TABLE #KeepProdStocks;

SELECT ps.Id
INTO #KeepProdStocks
FROM dbo.Prod_Stocks ps
JOIN #KeepProds p ON p.Id = ps.FK_Pid;

ALTER TABLE #KeepProdStocks
ADD CONSTRAINT PK_KeepProdStocks PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepOrders') IS NOT NULL DROP TABLE #KeepOrders;

SELECT oh.Id
INTO #KeepOrders
FROM dbo.Order_Headers oh
JOIN #KeepWebsites w ON w.Id = oh.FK_WebsiteId;

ALTER TABLE #KeepOrders
ADD CONSTRAINT PK_KeepOrders PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepShoppingCarts') IS NOT NULL DROP TABLE #KeepShoppingCarts;

SELECT DISTINCT sc.Id
INTO #KeepShoppingCarts
FROM dbo.ShoppingCarts sc
WHERE EXISTS (
        SELECT 1
        FROM #KeepProdStocks kps
        WHERE kps.Id = sc.FK_PSid
    )
   OR EXISTS (
        SELECT 1
        FROM dbo.Order_Details od
        JOIN #KeepOrders ko ON ko.Id = od.FK_OId
        WHERE od.FK_SCId = sc.Id
    );

ALTER TABLE #KeepShoppingCarts
ADD CONSTRAINT PK_KeepShoppingCarts PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepTokens') IS NOT NULL DROP TABLE #KeepTokens;

SELECT t.id
INTO #KeepTokens
FROM dbo.Tokens t
JOIN #KeepWebsites w ON w.Id = t.websiteId;

ALTER TABLE #KeepTokens
ADD CONSTRAINT PK_KeepTokens PRIMARY KEY (id);


IF OBJECT_ID('tempdb..#KeepLogisticsSettings') IS NOT NULL DROP TABLE #KeepLogisticsSettings;

SELECT ls.Id
INTO #KeepLogisticsSettings
FROM dbo.LogisticsSettings ls
JOIN #KeepWebsites w ON w.Id = ls.FK_WebsiteId;

ALTER TABLE #KeepLogisticsSettings
ADD CONSTRAINT PK_KeepLogisticsSettings PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepLogisticsBoxes') IS NOT NULL DROP TABLE #KeepLogisticsBoxes;

SELECT lb.Id
INTO #KeepLogisticsBoxes
FROM dbo.LogisticsBoxs lb
JOIN #KeepWebsites w ON w.Id = lb.FK_WebsiteId;

ALTER TABLE #KeepLogisticsBoxes
ADD CONSTRAINT PK_KeepLogisticsBoxes PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepTechnicalCertificates') IS NOT NULL DROP TABLE #KeepTechnicalCertificates;

SELECT tc.Id
INTO #KeepTechnicalCertificates
FROM dbo.TechnicalCertificates tc
JOIN #KeepWebsites w ON w.Id = tc.FK_WebsiteId;

ALTER TABLE #KeepTechnicalCertificates
ADD CONSTRAINT PK_KeepTechnicalCertificates PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepTags') IS NOT NULL DROP TABLE #KeepTags;

SELECT t.Id
INTO #KeepTags
FROM dbo.Tags t
JOIN #KeepWebsites w ON w.Id = t.FK_WebsiteId;

ALTER TABLE #KeepTags
ADD CONSTRAINT PK_KeepTags PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepTagGroups') IS NOT NULL DROP TABLE #KeepTagGroups;

SELECT tg.Id
INTO #KeepTagGroups
FROM dbo.Tag_Groups tg
JOIN #KeepWebsites w ON w.Id = tg.FK_WebsiteId;

ALTER TABLE #KeepTagGroups
ADD CONSTRAINT PK_KeepTagGroups PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepFileUploads') IS NOT NULL DROP TABLE #KeepFileUploads;

SELECT f.Id
INTO #KeepFileUploads
FROM dbo.FileUploads f
JOIN #KeepWebsites w ON w.Id = f.FK_WebsiteId;

ALTER TABLE #KeepFileUploads
ADD CONSTRAINT PK_KeepFileUploads PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepFileBinds') IS NOT NULL DROP TABLE #KeepFileBinds;

SELECT fb.Guid
INTO #KeepFileBinds
FROM dbo.FileBinds fb
WHERE fb.FK_FileUploadId IN (SELECT Id FROM #KeepFileUploads);

ALTER TABLE #KeepFileBinds
ADD CONSTRAINT PK_KeepFileBinds PRIMARY KEY (Guid);


IF OBJECT_ID('tempdb..#KeepWebMenus') IS NOT NULL DROP TABLE #KeepWebMenus;

SELECT wm.Id
INTO #KeepWebMenus
FROM dbo.WebMenus wm
JOIN #KeepWebsites w ON w.Id = wm.FK_WebsiteId;

ALTER TABLE #KeepWebMenus
ADD CONSTRAINT PK_KeepWebMenus PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepArticles') IS NOT NULL DROP TABLE #KeepArticles;

SELECT a.Id
INTO #KeepArticles
FROM dbo.Article a
JOIN #KeepWebsites w ON w.Id = a.FK_WebsiteId;

ALTER TABLE #KeepArticles
ADD CONSTRAINT PK_KeepArticles PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepAdvertise') IS NOT NULL DROP TABLE #KeepAdvertise;

SELECT a.Id
INTO #KeepAdvertise
FROM dbo.Advertise a
JOIN #KeepWebsites w ON w.Id = a.FK_WebsiteId;

ALTER TABLE #KeepAdvertise
ADD CONSTRAINT PK_KeepAdvertise PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepHtmlContents') IS NOT NULL DROP TABLE #KeepHtmlContents;

SELECT hc.Id
INTO #KeepHtmlContents
FROM dbo.Html_Contents hc
JOIN #KeepWebsites w ON w.Id = hc.FK_WebsiteId;

ALTER TABLE #KeepHtmlContents
ADD CONSTRAINT PK_KeepHtmlContents PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepDirectories') IS NOT NULL DROP TABLE #KeepDirectories;

SELECT d.Id
INTO #KeepDirectories
FROM dbo.Directory d
JOIN #KeepWebsites w ON w.Id = d.FK_WebsiteId;

ALTER TABLE #KeepDirectories
ADD CONSTRAINT PK_KeepDirectories PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepTemplates') IS NOT NULL DROP TABLE #KeepTemplates;

SELECT t.Id
INTO #KeepTemplates
FROM dbo.Templates t
JOIN #KeepWebsites w ON w.Id = t.FK_WebsiteID;

ALTER TABLE #KeepTemplates
ADD CONSTRAINT PK_KeepTemplates PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepTemplateSections') IS NOT NULL DROP TABLE #KeepTemplateSections;

SELECT ts.Id
INTO #KeepTemplateSections
FROM dbo.TemplateSections ts
JOIN #KeepTemplates kt ON kt.Id = ts.FK_TemplateID;

ALTER TABLE #KeepTemplateSections
ADD CONSTRAINT PK_KeepTemplateSections PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepMarketingCampaigns') IS NOT NULL DROP TABLE #KeepMarketingCampaigns;

SELECT mc.Id
INTO #KeepMarketingCampaigns
FROM dbo.MarketingCampaigns mc
JOIN #KeepWebsites w ON w.Id = mc.FK_WebsiteId;

ALTER TABLE #KeepMarketingCampaigns
ADD CONSTRAINT PK_KeepMarketingCampaigns PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepMarketingRules') IS NOT NULL DROP TABLE #KeepMarketingRules;

SELECT mr.Id
INTO #KeepMarketingRules
FROM dbo.MarketingRules mr
JOIN #KeepMarketingCampaigns kmc ON kmc.Id = mr.FK_MarketingCampaignId;

ALTER TABLE #KeepMarketingRules
ADD CONSTRAINT PK_KeepMarketingRules PRIMARY KEY (Id);


IF OBJECT_ID('tempdb..#KeepUserGroupings') IS NOT NULL DROP TABLE #KeepUserGroupings;

SELECT ug.Id
INTO #KeepUserGroupings
FROM dbo.UserGroupings ug
JOIN #KeepWebsites w ON w.Id = ug.FK_WebsiteId;

ALTER TABLE #KeepUserGroupings
ADD CONSTRAINT PK_KeepUserGroupings PRIMARY KEY (Id);


------------------------------------------------------------
-- 2. Log / 行為紀錄清空
-- 暫時移除指向 Log table 的 FK -> TRUNCATE -> 重建 FK
------------------------------------------------------------

IF OBJECT_ID('tempdb..#TruncateTables') IS NOT NULL DROP TABLE #TruncateTables;

CREATE TABLE #TruncateTables
(
    SchemaName SYSNAME NOT NULL,
    TableName SYSNAME NOT NULL
);

INSERT INTO #TruncateTables (SchemaName, TableName)
VALUES
    ('dbo', 'UserActivityTags'),
    ('dbo', 'Remotes'),
    ('dbo', 'Advertise_Logs'),
    ('dbo', 'Prod_Logs'),
    ('dbo', 'AuditLogs'),
    ('dbo', 'Account_Logs'),
    ('dbo', 'SearchLogs'),
    ('dbo', 'FlowSizes'),
    ('dbo', 'bonusLogDetails'),
    ('dbo', 'BonusLog'),
    ('dbo', 'RemoteDailyStatistics'),
    ('dbo', 'RemoteHourlyStatistics'),
    ('dbo', 'RemoteDailyAggregationRuns'),
    ('dbo', 'ProvisioningSiteMetricSamples'),
    ('dbo', 'ProvisioningMetricSamples'),
    ('dbo', 'ProvisioningAgentStatuses'),
    ('dbo', 'ProvisioningTasks');


IF OBJECT_ID('tempdb..#FkScripts') IS NOT NULL DROP TABLE #FkScripts;

CREATE TABLE #FkScripts
(
    RowNo INT IDENTITY(1,1) PRIMARY KEY,
    DropSql NVARCHAR(MAX) NOT NULL,
    CreateSql NVARCHAR(MAX) NOT NULL
);

INSERT INTO #FkScripts (DropSql, CreateSql)
SELECT
    DropSql =
        N'ALTER TABLE '
        + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
        + N'.'
        + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
        + N' DROP CONSTRAINT '
        + QUOTENAME(fk.name)
        + N';',

    CreateSql =
        N'ALTER TABLE '
        + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
        + N'.'
        + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
        + CASE WHEN fk.is_not_trusted = 1 THEN N' WITH NOCHECK ' ELSE N' WITH CHECK ' END
        + N' ADD CONSTRAINT '
        + QUOTENAME(fk.name)
        + N' FOREIGN KEY ('
        + ParentCols.ParentColumns
        + N') REFERENCES '
        + QUOTENAME(OBJECT_SCHEMA_NAME(fk.referenced_object_id))
        + N'.'
        + QUOTENAME(OBJECT_NAME(fk.referenced_object_id))
        + N' ('
        + RefCols.RefColumns
        + N')'
        + CASE fk.delete_referential_action
            WHEN 1 THEN N' ON DELETE CASCADE'
            WHEN 2 THEN N' ON DELETE SET NULL'
            WHEN 3 THEN N' ON DELETE SET DEFAULT'
            ELSE N''
          END
        + CASE fk.update_referential_action
            WHEN 1 THEN N' ON UPDATE CASCADE'
            WHEN 2 THEN N' ON UPDATE SET NULL'
            WHEN 3 THEN N' ON UPDATE SET DEFAULT'
            ELSE N''
          END
        + N';'
        + CASE WHEN fk.is_disabled = 1
            THEN N' ALTER TABLE '
                + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
                + N'.'
                + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
                + N' NOCHECK CONSTRAINT '
                + QUOTENAME(fk.name)
                + N';'
            ELSE N''
          END
FROM sys.foreign_keys fk
JOIN sys.tables rt
    ON rt.object_id = fk.referenced_object_id
JOIN sys.schemas rs
    ON rs.schema_id = rt.schema_id
JOIN #TruncateTables tt
    ON tt.SchemaName = rs.name
   AND tt.TableName = rt.name
CROSS APPLY
(
    SELECT STRING_AGG(QUOTENAME(pc.name), N', ')
           WITHIN GROUP (ORDER BY fkc.constraint_column_id) AS ParentColumns
    FROM sys.foreign_key_columns fkc
    JOIN sys.columns pc
        ON pc.object_id = fkc.parent_object_id
       AND pc.column_id = fkc.parent_column_id
    WHERE fkc.constraint_object_id = fk.object_id
) ParentCols
CROSS APPLY
(
    SELECT STRING_AGG(QUOTENAME(rc.name), N', ')
           WITHIN GROUP (ORDER BY fkc.constraint_column_id) AS RefColumns
    FROM sys.foreign_key_columns fkc
    JOIN sys.columns rc
        ON rc.object_id = fkc.referenced_object_id
       AND rc.column_id = fkc.referenced_column_id
    WHERE fkc.constraint_object_id = fk.object_id
) RefCols;


DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql = STRING_AGG(DropSql, CHAR(13) + CHAR(10))
FROM #FkScripts;

IF @sql IS NOT NULL AND LEN(@sql) > 0
BEGIN
    EXEC sp_executesql @sql;
END;


TRUNCATE TABLE dbo.RemoteDailyStatistics;
TRUNCATE TABLE dbo.RemoteHourlyStatistics;
TRUNCATE TABLE dbo.RemoteDailyAggregationRuns;
TRUNCATE TABLE dbo.ProvisioningSiteMetricSamples;
TRUNCATE TABLE dbo.ProvisioningMetricSamples;
TRUNCATE TABLE dbo.ProvisioningAgentStatuses;
TRUNCATE TABLE dbo.ProvisioningTasks;
TRUNCATE TABLE dbo.UserActivityTags;
TRUNCATE TABLE dbo.bonusLogDetails;

TRUNCATE TABLE dbo.Remotes;
TRUNCATE TABLE dbo.BonusLog;

TRUNCATE TABLE dbo.Advertise_Logs;
TRUNCATE TABLE dbo.Prod_Logs;
TRUNCATE TABLE dbo.AuditLogs;
TRUNCATE TABLE dbo.Account_Logs;
TRUNCATE TABLE dbo.SearchLogs;
TRUNCATE TABLE dbo.FlowSizes;


SET @sql = N'';

SELECT @sql = STRING_AGG(CreateSql, CHAR(13) + CHAR(10))
FROM #FkScripts;

IF @sql IS NOT NULL AND LEN(@sql) > 0
BEGIN
    EXEC sp_executesql @sql;
END;


------------------------------------------------------------
-- 3. 行銷活動資料
------------------------------------------------------------

UPDATE sc SET FK_MarketingRewardItemId = NULL
FROM dbo.ShoppingCarts sc
JOIN dbo.MarketingRewardItems item ON item.Id = sc.FK_MarketingRewardItemId
JOIN dbo.MarketingRewards reward ON reward.Id = item.FK_MarketingRewardId
WHERE reward.FK_MarketingRuleId NOT IN (SELECT Id FROM #KeepMarketingRules)
   OR item.FK_ProdStockId NOT IN (SELECT Id FROM #KeepProdStocks);

DELETE item FROM dbo.MarketingRewardItems item
JOIN dbo.MarketingRewards reward ON reward.Id = item.FK_MarketingRewardId
WHERE reward.FK_MarketingRuleId NOT IN (SELECT Id FROM #KeepMarketingRules)
   OR item.FK_ProdStockId NOT IN (SELECT Id FROM #KeepProdStocks);

DELETE FROM dbo.MarketingScopeItems
WHERE FK_MarketingRuleId NOT IN (SELECT Id FROM #KeepMarketingRules);

DELETE FROM dbo.MarketingRewards
WHERE FK_MarketingRuleId NOT IN (SELECT Id FROM #KeepMarketingRules);

DELETE FROM dbo.MarketingConditions
WHERE FK_MarketingRuleId NOT IN (SELECT Id FROM #KeepMarketingRules);

DELETE FROM dbo.MarketingRules
WHERE Id NOT IN (SELECT Id FROM #KeepMarketingRules);

DELETE FROM dbo.MarketingCampaigns
WHERE Id NOT IN (SELECT Id FROM #KeepMarketingCampaigns);


------------------------------------------------------------
-- 4. 訂單 / 購物車 / Token
------------------------------------------------------------

IF OBJECT_ID('dbo.TokenMapShoppingCarts') IS NOT NULL
BEGIN
    DELETE m
    FROM dbo.TokenMapShoppingCarts m
    WHERE NOT EXISTS (
            SELECT 1
            FROM #KeepTokens kt
            WHERE kt.id = m.UUID
        )
       OR NOT EXISTS (
            SELECT 1
            FROM #KeepShoppingCarts ksc
            WHERE ksc.Id = m.FK_Tid
        );
END;

DELETE FROM dbo.Order_Details
WHERE FK_OId NOT IN (SELECT Id FROM #KeepOrders)
   OR FK_SCId NOT IN (SELECT Id FROM #KeepShoppingCarts);

DELETE FROM dbo.Order_Logistics
WHERE FK_OhId NOT IN (SELECT Id FROM #KeepOrders);

DELETE FROM dbo.Order_Headers
WHERE Id NOT IN (SELECT Id FROM #KeepOrders);

DELETE FROM dbo.ShoppingCarts
WHERE Id NOT IN (SELECT Id FROM #KeepShoppingCarts);

DELETE FROM dbo.Tokens
WHERE id NOT IN (SELECT id FROM #KeepTokens);


------------------------------------------------------------
-- 5. 商品 / 規格 / 價格 / 商品物流關聯
------------------------------------------------------------

DELETE FROM dbo.MappingLogisticsSettingAndProd
WHERE FK_LogisticsSettingId NOT IN (SELECT Id FROM #KeepLogisticsSettings)
   OR FK_ProdId NOT IN (SELECT Id FROM #KeepProds);

DELETE FROM dbo.Prod_TechCerts
WHERE FK_PId NOT IN (SELECT Id FROM #KeepProds)
   OR FK_TCId NOT IN (SELECT Id FROM #KeepTechnicalCertificates);

DELETE FROM dbo.Prod_Prices
WHERE FK_PSId NOT IN (SELECT Id FROM #KeepProdStocks)
   OR FK_RId NOT IN (SELECT Id FROM #KeepRoles);

DELETE FROM dbo.Prod_Stocks
WHERE Id NOT IN (SELECT Id FROM #KeepProdStocks);

DELETE FROM dbo.Prods
WHERE Id NOT IN (SELECT Id FROM #KeepProds);

DELETE FROM dbo.Prod_Specs
WHERE FK_Tid NOT IN (
    SELECT Id
    FROM dbo.Prod_Spec_Types
    WHERE FK_WebsiteId IN (SELECT Id FROM #KeepWebsites)
);

DELETE FROM dbo.Prod_Spec_Types
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);


------------------------------------------------------------
-- 6. 物流 / 付款網站設定
------------------------------------------------------------

DELETE FROM dbo.LogisticsType_Payments
WHERE FK_LogisticsSettingId IS NOT NULL
  AND FK_LogisticsSettingId NOT IN (SELECT Id FROM #KeepLogisticsSettings);

DELETE FROM dbo.LogisticsBoxFees
WHERE FK_LogisticsSettingId NOT IN (SELECT Id FROM #KeepLogisticsSettings)
   OR FK_LogisticsBoxId NOT IN (SELECT Id FROM #KeepLogisticsBoxes);

DELETE FROM dbo.LogisticsBoxs
WHERE Id NOT IN (SELECT Id FROM #KeepLogisticsBoxes);

DELETE FROM dbo.LogisticsSettings
WHERE Id NOT IN (SELECT Id FROM #KeepLogisticsSettings);

DELETE FROM dbo.PaymentTypesValues
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);

DELETE FROM dbo.ThirdPartyKeypairValues
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);


------------------------------------------------------------
-- 7. 標籤 / 分群 / 使用者標籤統計
------------------------------------------------------------

DELETE FROM dbo.UserTagStatistics
WHERE FK_TagId NOT IN (SELECT Id FROM #KeepTags);

DELETE FROM dbo.Tag_TagGroups
WHERE FK_TId NOT IN (SELECT Id FROM #KeepTags)
   OR FK_TGId NOT IN (SELECT Id FROM #KeepTagGroups);

DELETE FROM dbo.Tag_Associates
WHERE FK_TId NOT IN (SELECT Id FROM #KeepTags);

DELETE FROM dbo.Tag_Groups
WHERE Id NOT IN (SELECT Id FROM #KeepTagGroups);

DELETE FROM dbo.Tags
WHERE Id NOT IN (SELECT Id FROM #KeepTags);

DELETE FROM dbo.UserGroupingDetails
WHERE FK_GropingId NOT IN (SELECT Id FROM #KeepUserGroupings);

DELETE FROM dbo.UserGroupings
WHERE Id NOT IN (SELECT Id FROM #KeepUserGroupings);


------------------------------------------------------------
-- 8. 檔案資料
------------------------------------------------------------

-- 檔案回收／掃描紀錄包含歷史路徑與操作原因，不交付，也避免邏輯關聯殘留。
TRUNCATE TABLE dbo.FileRecycleBinBindings;
TRUNCATE TABLE dbo.FileRecycleBinItems;
TRUNCATE TABLE dbo.FileCleanupCandidates;
TRUNCATE TABLE dbo.FileReferences;

DELETE FROM dbo.FileBindMores
WHERE FK_FileBindGuid NOT IN (SELECT Guid FROM #KeepFileBinds)
   OR (
        FK_FileUploadId IS NOT NULL
        AND FK_FileUploadId NOT IN (SELECT Id FROM #KeepFileUploads)
   );

DELETE FROM dbo.FileBinds
WHERE Guid NOT IN (SELECT Guid FROM #KeepFileBinds);

DELETE FROM dbo.FileUploads
WHERE Id NOT IN (SELECT Id FROM #KeepFileUploads);


------------------------------------------------------------
-- 9. 版型 / 頁尾
------------------------------------------------------------

DELETE FROM dbo.FooterTemplates
WHERE FK_TemplateSectionsId NOT IN (SELECT Id FROM #KeepTemplateSections);

DELETE FROM dbo.TemplateSections
WHERE Id NOT IN (SELECT Id FROM #KeepTemplateSections);

DELETE FROM dbo.Templates
WHERE Id NOT IN (SELECT Id FROM #KeepTemplates);


------------------------------------------------------------
-- 10. 目錄 / HTML / 選單 / 表單
------------------------------------------------------------

DELETE FROM dbo.DirectoryFacetRanges
WHERE FK_DirectoryId NOT IN (SELECT Id FROM #KeepDirectories);

DELETE FROM dbo.Directory
WHERE Id NOT IN (SELECT Id FROM #KeepDirectories);

DELETE FROM dbo.Contacts
WHERE FK_WebMenuId NOT IN (SELECT Id FROM #KeepWebMenus);

DELETE FROM dbo.WebMenus
WHERE Id NOT IN (SELECT Id FROM #KeepWebMenus);

DELETE FROM dbo.HtmlContentPurposes
WHERE FK_HtmlContentId NOT IN (SELECT Id FROM #KeepHtmlContents);

DELETE FROM dbo.Html_Contents
WHERE Id NOT IN (SELECT Id FROM #KeepHtmlContents);


------------------------------------------------------------
-- 11. 文章 / 廣告 / 技術認證 / 跑馬燈 / 搜尋設定
------------------------------------------------------------

DELETE FROM dbo.Article
WHERE Id NOT IN (SELECT Id FROM #KeepArticles);

DELETE FROM dbo.Advertise
WHERE Id NOT IN (SELECT Id FROM #KeepAdvertise);

DELETE FROM dbo.TechnicalCertificates
WHERE Id NOT IN (SELECT Id FROM #KeepTechnicalCertificates);

DELETE FROM dbo.Marquees
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);

DELETE FROM dbo.CustSearch
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);


------------------------------------------------------------
-- 12. 網站設定 / 快取 / 清洗狀態 / JsonObject
------------------------------------------------------------

DELETE FROM dbo.StoreSetDetail
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);

DELETE FROM dbo.WebsiteCacheStates
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);

DELETE FROM dbo.HtmlSanitizeStates
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);

DELETE FROM dbo.JsonObjects
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);

DELETE FROM dbo.NotFoundImage
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);


------------------------------------------------------------
-- 13. 權限 / 角色 / 使用者 / 會員
------------------------------------------------------------

-- 邀請含 Email 及尚未完成的啟用流程；平台角色對應對 Users 為 Restrict。
DELETE FROM dbo.PlatformAdministratorInvitations;
DELETE FROM dbo.MappingUserAndPlatformRoles
WHERE UserId NOT IN (SELECT Id FROM #KeepUsers);
DELETE FROM dbo.Notifications;
DELETE FROM dbo.BackgroundTasks;

DELETE FROM dbo.PermissionDetail
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites)
   OR (
        FK_UserId IS NOT NULL
        AND FK_UserId NOT IN (SELECT Id FROM #KeepUsers)
   )
   OR (
        FK_RoleId IS NOT NULL
        AND FK_RoleId NOT IN (SELECT Id FROM #KeepRoles)
   );

DELETE FROM dbo.Permissions
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites)
   OR (
        FK_UserId IS NOT NULL
        AND FK_UserId NOT IN (SELECT Id FROM #KeepUsers)
   )
   OR (
        FK_RoleId IS NOT NULL
        AND FK_RoleId NOT IN (SELECT Id FROM #KeepRoles)
   );

DELETE FROM dbo.MappingUserAndRoles
WHERE UserId NOT IN (SELECT Id FROM #KeepUsers)
   OR RoleId NOT IN (SELECT Id FROM #KeepRoles);

DELETE FROM dbo.MappingUserAndWebsites
WHERE WebsiteId NOT IN (SELECT Id FROM #KeepWebsites)
   OR UserId NOT IN (SELECT Id FROM #KeepUsers);

DELETE FROM dbo.MappingFrontUserAndWebsite
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites)
   OR FK_UserId NOT IN (SELECT Id FROM #KeepFrontUsers);

DELETE FROM dbo.Roles
WHERE Id NOT IN (SELECT Id FROM #KeepRoles);

DELETE FROM dbo.FrontUsers
WHERE Id NOT IN (SELECT Id FROM #KeepFrontUsers);

DELETE FROM dbo.Users
WHERE Id NOT IN (SELECT Id FROM #KeepUsers);


------------------------------------------------------------
-- 14. 紅利主檔 / 紅利負債
-- BonusLog / bonusLogDetails 已在 Log 區段清空
------------------------------------------------------------

DELETE FROM dbo.Bonus
WHERE UUID NOT IN (
    SELECT UUID
    FROM dbo.FrontUsers
    WHERE Id IN (SELECT Id FROM #KeepFrontUsers)
);

DELETE FROM dbo.BonusLiabilities
WHERE UUID NOT IN (
    SELECT UUID
    FROM dbo.FrontUsers
    WHERE Id IN (SELECT Id FROM #KeepFrontUsers)
);


------------------------------------------------------------
-- 15. 收藏 / 收件人 / 公司關聯
------------------------------------------------------------

DELETE FROM dbo.Favorites
WHERE UUID NOT IN (
    SELECT UUID
    FROM dbo.FrontUsers
    WHERE Id IN (SELECT Id FROM #KeepFrontUsers)
);

DELETE FROM dbo.Recipients
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);

DELETE FROM dbo.MappingCompanyAndWebsites
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);

DELETE FROM dbo.PlatformWebsites
WHERE FK_WebsiteId IS NULL OR FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);

IF OBJECT_ID('tempdb..#KeepCompanies') IS NOT NULL DROP TABLE #KeepCompanies;
SELECT FK_CompanyId AS Id INTO #KeepCompanies
FROM dbo.MappingCompanyAndWebsites
UNION SELECT FK_CompanyId FROM dbo.PlatformWebsites;

DELETE FROM dbo.SecondaryContacts
WHERE FK_CompanyId NOT IN (SELECT Id FROM #KeepCompanies);
-- 企業信箱在 20261019094906 已改掛 Companies，不再使用 FK_WebsiteId。
DELETE FROM dbo.WebsiteEnterpriseEmails
WHERE FK_CompanyId NOT IN (SELECT Id FROM #KeepCompanies);

DELETE FROM dbo.Companies
WHERE Id NOT IN (SELECT Id FROM #KeepCompanies);


------------------------------------------------------------
-- 16. 其他 mapping / 非網站直接關聯但不應外流的資料
------------------------------------------------------------

DELETE FROM dbo.MappingWebsiteRelationship
WHERE FatherId NOT IN (SELECT Id FROM #KeepWebsites)
   OR WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);

DELETE FROM dbo.MappingOldNewUUID
WHERE UserUUID NOT IN (
        SELECT UUID
        FROM dbo.FrontUsers
        WHERE Id IN (SELECT Id FROM #KeepFrontUsers)
    )
  AND TempUUID NOT IN (
        SELECT UUID
        FROM dbo.FrontUsers
        WHERE Id IN (SELECT Id FROM #KeepFrontUsers)
    );


------------------------------------------------------------
-- 17. 最後刪除非保留網站
------------------------------------------------------------

DELETE FROM dbo.WebsiteAlternateUrls
WHERE FK_WebsiteId NOT IN (SELECT Id FROM #KeepWebsites);

DELETE FROM dbo.Websites
WHERE Id NOT IN (SELECT Id FROM #KeepWebsites);


------------------------------------------------------------
-- 18. 清洗後檢查
------------------------------------------------------------

DELETE d FROM dbo.PlatformDomains d
WHERE NOT EXISTS (SELECT 1 FROM dbo.Websites w WHERE w.FK_PlatformDomainId = d.Id)
  AND NOT EXISTS (SELECT 1 FROM dbo.PlatformWebsites w WHERE w.FK_PlatformDomainId = d.Id)
  AND NOT EXISTS (SELECT 1 FROM dbo.WebsiteAlternateUrls w WHERE w.FK_PlatformDomainId = d.Id);

SELECT 'Websites' AS TableName, COUNT(*) AS Cnt FROM dbo.Websites
UNION ALL SELECT 'Users', COUNT(*) FROM dbo.Users
UNION ALL SELECT 'FrontUsers', COUNT(*) FROM dbo.FrontUsers
UNION ALL SELECT 'Prods', COUNT(*) FROM dbo.Prods
UNION ALL SELECT 'Prod_Stocks', COUNT(*) FROM dbo.Prod_Stocks
UNION ALL SELECT 'Order_Headers', COUNT(*) FROM dbo.Order_Headers
UNION ALL SELECT 'ShoppingCarts', COUNT(*) FROM dbo.ShoppingCarts
UNION ALL SELECT 'Remotes', COUNT(*) FROM dbo.Remotes
UNION ALL SELECT 'AuditLogs', COUNT(*) FROM dbo.AuditLogs
UNION ALL SELECT 'SearchLogs', COUNT(*) FROM dbo.SearchLogs
UNION ALL SELECT 'FlowSizes', COUNT(*) FROM dbo.FlowSizes
UNION ALL SELECT 'BonusLog', COUNT(*) FROM dbo.BonusLog
UNION ALL SELECT 'bonusLogDetails', COUNT(*) FROM dbo.bonusLogDetails;


IF EXISTS (SELECT 1 FROM dbo.Websites WHERE Id NOT IN (23, 36))
    THROW 51000, N'仍有非交付網站。', 1;
COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH;