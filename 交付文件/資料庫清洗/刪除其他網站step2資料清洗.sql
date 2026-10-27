/*
    Coker 資料庫打包：第二階段匿名化

    執行前提：
    1. 僅能對「從正式環境還原出的本機／隔離副本」執行。
    2. 先執行既有的「只保留 WebsiteId 23、36」清理腳本。
    3. 請先停止 MVC、Public 與 HangFire Server。
    4. 必須設定 @ExpectedDatabase、@DeliveryAdminUsers 與 @AuditUserId。
    5. @RunCleaning=1、@DoCommit=1 時，提交後自動備份並驗證匿名化母版。
       第三步會自動讀取已驗證的母版路徑，不必另做手動備份。

    注意：
    - 交付管理者的 Users 帳號、密碼及基本資料不會修改。
    - 其他 Users 將移除，只留下交付管理者。
    - FrontUsers 無法再登入，但保留 UUID，以維持訂單、紅利等既有關聯。
    - 網站名稱、展示內容、站台功能設定、圖片與附件檔名均保留。
*/

USE [Coker6];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ExpectedDatabase sysname = N'Coker6';
DECLARE @DoCommit bit = 1;                  -- 第一次必須保持 0；確認結果後才改成 1

-- SQL Server 主機上的既有目錄；服務帳號必須具有寫入權限。
DECLARE @MasterBackupDirectory nvarchar(3000)
    = N'D:\易碩\專案\newCoker\EtheriT.Coker.WebApplication\db';
DECLARE @CleaningCommitted bit = 0;
DECLARE @MasterBackupFile nvarchar(4000);

DECLARE @DeliveryAdminUsers TABLE
(
    Id bigint NOT NULL PRIMARY KEY
);

-- 必填：列出所有可能交付給開發者的 dbo.Users.Id。
INSERT INTO @DeliveryAdminUsers (Id)
VALUES
    (1),(2498),(12659);

-- 必填：暫時承接 Audit UserId 的帳號，且必須包含在上方清單內。
DECLARE @AuditUserId bigint = 1;

IF DB_NAME() <> @ExpectedDatabase
BEGIN
    RAISERROR(N'目前資料庫不是指定的打包副本，停止執行。', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM @DeliveryAdminUsers)
BEGIN
    RAISERROR(N'尚未設定 @DeliveryAdminUsers，停止執行。', 16, 1);
    RETURN;
END;

IF @AuditUserId IS NULL
   OR NOT EXISTS
   (
       SELECT 1
       FROM @DeliveryAdminUsers
       WHERE Id = @AuditUserId
   )
BEGIN
    RAISERROR(N'@AuditUserId 必須包含在 @DeliveryAdminUsers 清單內。', 16, 1);
    RETURN;
END;

IF EXISTS
(
    SELECT 1
    FROM @DeliveryAdminUsers AS keepUser
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Users AS usersTable
        WHERE usersTable.Id = keepUser.Id
    )
)
BEGIN
    RAISERROR(N'找不到一或多個指定的交付管理者，停止執行。', 16, 1);
    RETURN;
END;

IF (SELECT COUNT(*) FROM dbo.Websites WHERE Id IN (23, 36)) <> 2
   OR EXISTS (SELECT 1 FROM dbo.Websites WHERE Id NOT IN (23, 36))
BEGIN
    SELECT
        Id,
        Title,
        OrgName
    FROM dbo.Websites
    ORDER BY Id;

    RAISERROR(N'Websites 尚未清理成只保留 23、36，停止執行。', 16, 1);
    RETURN;
END;

-- @DoCommit=0 會真的執行清洗後回滾，仍可能耗時、寫入大量交易記錄。
-- 僅檢查結構時保持 @RunCleaning=0；需要演練時改為 1。
DECLARE @RunCleaning bit = 1;
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

IF @RunCleaning = 0
BEGIN
    PRINT N'結構檢查完成，未清洗資料。演練請設 @RunCleaning=1、@DoCommit=0。';
    RETURN;
END;
BEGIN TRY
    BEGIN TRANSACTION;

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


    ----------------------------------------------------------------
    -- 1. 停用並清除安全性憑證、Token、背景工作與 Cache
    ----------------------------------------------------------------

    IF OBJECT_ID(N'dbo.TokenMapShoppingCarts', N'U') IS NOT NULL
        DELETE FROM dbo.TokenMapShoppingCarts;

    -- 清除快取、掃描結果、歷史紀錄與基礎設施 JSON，避免保留個資副本或重播佈署工作。
    DELETE FROM dbo.PlatformAdministratorInvitations;
    DELETE FROM dbo.UserActivityTags;
    DELETE FROM dbo.Remotes;
    DELETE FROM dbo.bonusLogDetails;
    DELETE FROM dbo.BonusLog;
    DELETE FROM dbo.Account_Logs;
    DELETE FROM dbo.AuditLogs;
    DELETE FROM dbo.SearchLogs;
    DELETE FROM dbo.Advertise_Logs;
    DELETE FROM dbo.Prod_Logs;
    DELETE FROM dbo.FlowSizes;
    DELETE FROM dbo.RemoteDailyStatistics;
    DELETE FROM dbo.RemoteHourlyStatistics;
    DELETE FROM dbo.RemoteDailyAggregationRuns;
    DELETE FROM dbo.ProvisioningSiteMetricSamples;
    DELETE FROM dbo.ProvisioningMetricSamples;
    DELETE FROM dbo.ProvisioningAgentStatuses;
    DELETE FROM dbo.ProvisioningTasks;
    DELETE FROM dbo.FileRecycleBinBindings;
    DELETE FROM dbo.FileRecycleBinItems;
    DELETE FROM dbo.FileCleanupCandidates;
    DELETE FROM dbo.FileReferences;
    DELETE FROM dbo.JsonObjects;
    DELETE FROM dbo.PageTextBackfillStates;
    DELETE FROM dbo.WebsiteCacheStates;
    DELETE FROM dbo.HtmlSanitizeStates;
    UPDATE dbo.CdnProviderSyncStates SET LastError = NULL;
    UPDATE dbo.Bonus SET Note = N'[已匿名化]';

    -- 保留交付管理者登入資料，但撤銷密碼重設權杖。
    UPDATE dbo.Users SET ForgetID = NULL, ForgeIDSendDate = NULL;

    UPDATE dbo.Companies SET Phone = NULL, InvoiceInfo = NULL, SalesOwner = NULL,
        CustomerTypeOther = NULL, ContactJobTitle = NULL, ContactPhone = NULL,
        ContactEmail = NULL, Remark = NULL;
    UPDATE dbo.SecondaryContacts
    SET Name = CONCAT(N'測試聯絡人', Id), JobTitle = NULL, Phone = NULL,
        Email = CONCAT(N'contact_', Id, N'@example.invalid');
    UPDATE dbo.WebsiteEnterpriseEmails SET OtherType = NULL, Remark = NULL;
    UPDATE dbo.PlatformWebsites SET HostLocation = NULL, Remark = NULL,
        Url = CONCAT(N'https://website-', FK_WebsiteId, N'.example.invalid');
    UPDATE dbo.Websites SET HostLocation = NULL, PlatformRemark = NULL;
    UPDATE dbo.PlatformDomains SET PasswordCipher = NULL, Registrar = NULL, Remark = NULL,
        DomainName = CONCAT(N'domain-', Id, N'.example.invalid');
    UPDATE dbo.WebsiteAlternateUrls
    SET Url = CONCAT(N'https://alternate-', Id, N'.example.invalid');

    DELETE FROM dbo.Tokens;
    DELETE FROM dbo.ThirdPartyKeypairValues;

    -- 只停用可能對外連線、寄信或回傳正式站追蹤資料的設定；
    -- 購物、會員、版面與前台展示設定全部保留。
    UPDATE detail
    SET
        detail.enable = 0,
        detail.value = NULL
    FROM dbo.StoreSetDetail AS detail
    INNER JOIN dbo.StoreSet AS setting
        ON setting.Id = detail.FK_StoreSetId
    WHERE setting.[key] IN
    (
        N'SMTPPath',
        N'SMTPPort',
        N'SMTPAccount',
        N'SMTPPassword',
        N'GA4',
        N'GTM',
        N'GoogleAds'
    );

    IF OBJECT_ID(N'dbo.Notifications', N'U') IS NOT NULL
        DELETE FROM dbo.Notifications;

    IF OBJECT_ID(N'dbo.BackgroundTasks', N'U') IS NOT NULL
        DELETE FROM dbo.BackgroundTasks;

    IF OBJECT_ID(N'dbo.MappingOldNewUUID', N'U') IS NOT NULL
        DELETE FROM dbo.MappingOldNewUUID;

    ----------------------------------------------------------------
    -- 2. 清除 HangFire 工作資料，保留 Schema 版本表供程式重用
    ----------------------------------------------------------------

    IF OBJECT_ID(N'HangFire.State', N'U') IS NOT NULL
        DELETE FROM HangFire.State;

    IF OBJECT_ID(N'HangFire.JobParameter', N'U') IS NOT NULL
        DELETE FROM HangFire.JobParameter;

    IF OBJECT_ID(N'HangFire.JobQueue', N'U') IS NOT NULL
        DELETE FROM HangFire.JobQueue;

    IF OBJECT_ID(N'HangFire.Job', N'U') IS NOT NULL
        DELETE FROM HangFire.Job;

    IF OBJECT_ID(N'HangFire.Server', N'U') IS NOT NULL
        DELETE FROM HangFire.Server;

    IF OBJECT_ID(N'HangFire.[List]', N'U') IS NOT NULL
        DELETE FROM HangFire.[List];

    IF OBJECT_ID(N'HangFire.[Set]', N'U') IS NOT NULL
        DELETE FROM HangFire.[Set];

    IF OBJECT_ID(N'HangFire.Counter', N'U') IS NOT NULL
        DELETE FROM HangFire.Counter;

    IF OBJECT_ID(N'HangFire.AggregatedCounter', N'U') IS NOT NULL
        DELETE FROM HangFire.AggregatedCounter;

    IF OBJECT_ID(N'HangFire.Hash', N'U') IS NOT NULL
        DELETE FROM HangFire.Hash;

    ----------------------------------------------------------------
    -- 3. 購物車只保留被訂單明細引用的紀錄
    ----------------------------------------------------------------

    DELETE sc
    FROM dbo.ShoppingCarts AS sc
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Order_Details AS od
        WHERE od.FK_SCId = sc.Id
    );

    ----------------------------------------------------------------
    -- 4. FrontUsers 全部匿名化並使密碼失效
    ----------------------------------------------------------------

    UPDATE fu
    SET
        Account = CONCAT(N'frontuser_', fu.Id, N'@example.invalid'),
        Password = CONCAT(N'INVALIDATED-', fu.Id, N'-', CONVERT(nvarchar(36), NEWID())),
        Name = CONCAT(N'測試會員', fu.Id),
        Email = CONCAT(N'frontuser_', fu.Id, N'@example.invalid'),
        CellPhone = N'0900000000',
        TelPhone = N'0200000000',
        Address = CONCAT(N'測試地址 ', fu.Id, N' 號'),
        Birthday = CASE WHEN fu.Birthday IS NULL THEN NULL ELSE CONVERT(datetime2, '2000-01-01') END,
        OpenID = NEWID(),
        ForgetID = NULL,
        ForgeIDSendDate = NULL,
        LockTime = GETDATE(),
        FK_User = CASE
            WHEN EXISTS
            (
                SELECT 1
                FROM @DeliveryAdminUsers AS keepUser
                WHERE keepUser.Id = fu.FK_User
            )
            THEN fu.FK_User
            ELSE NULL
        END
    FROM dbo.FrontUsers AS fu;

    ----------------------------------------------------------------
    -- 5. 訂單、物流、購物車快照
    ----------------------------------------------------------------

    UPDATE oh
    SET
        Fk_UserId = CASE
            WHEN oh.Fk_UserId IS NULL THEN NULL
            WHEN EXISTS
            (
                SELECT 1
                FROM @DeliveryAdminUsers AS keepUser
                WHERE keepUser.Id = oh.Fk_UserId
            )
            THEN oh.Fk_UserId
            ELSE @AuditUserId
        END,
        Orderer = CONCAT(N'測試訂購人', oh.Id),
        OrdererEmail = CONCAT(N'order_', oh.Id, N'@example.invalid'),
        OrdererTelePhone = N'0200000000',
        OrdererCellPhone = N'0900000000',
        OrdererAddress = CONCAT(N'測試訂購地址 ', oh.Id, N' 號'),
        OrdererZipCode = N'000',
        Recipient = CONCAT(N'測試收件人', oh.Id),
        RecipientEmail = CONCAT(N'recipient_', oh.Id, N'@example.invalid'),
        RecipientTelePhone = N'0200000000',
        RecipientCellPhone = N'0900000000',
        RecipientAddress = CONCAT(N'測試收件地址 ', oh.Id, N' 號'),
        RecipientZipCode = N'000',
        Remark = CASE WHEN oh.Remark IS NULL THEN NULL ELSE N'[已匿名化]' END,
        Carrier = CASE WHEN oh.Carrier IS NULL THEN NULL ELSE CONCAT(N'TEST-CARRIER-', oh.Id) END,
        InvoiceTitle = CASE WHEN oh.InvoiceTitle IS NULL THEN NULL ELSE CONCAT(N'測試發票抬頭', oh.Id) END,
        UniformId = CASE WHEN oh.UniformId IS NULL THEN NULL ELSE N'00000000' END,
        InvoiceAddress = CASE WHEN oh.InvoiceAddress IS NULL THEN NULL ELSE N'測試發票地址' END,
        Memo = CASE WHEN oh.Memo IS NULL THEN NULL ELSE N'[已匿名化]' END,
        SystemMemo = CASE WHEN oh.SystemMemo IS NULL THEN NULL ELSE N'[已匿名化]' END,
        DiscountBreakdownJson = NULL,
        TrackingNumber = CASE
            WHEN oh.TrackingNumber IS NULL THEN NULL
            ELSE CONVERT(nvarchar(20), oh.Id)
        END,
        TransactionId = CASE
            WHEN oh.TransactionId IS NULL THEN NULL
            ELSE CONCAT(N'TEST-TXN-', oh.Id)
        END,
        refundTransactionId = CASE
            WHEN oh.refundTransactionId IS NULL THEN NULL
            ELSE CONCAT(N'TEST-REFUND-', oh.Id)
        END
    FROM dbo.Order_Headers AS oh;

    UPDATE ol
    SET
        RtnMerchantTradeNo = NULL,
        RtnOrderNo = NULL,
        CVSOutSide = NULL,
        MerchantTradeNo = CASE
            WHEN ol.MerchantTradeNo IS NULL THEN NULL
            ELSE CONCAT(N'TEST-TRADE-', ol.Id)
        END,
        AllPayLogisticsID = CASE
            WHEN ol.AllPayLogisticsID IS NULL THEN NULL
            ELSE CONCAT(N'TEST-LOGISTICS-', ol.Id)
        END,
        BookingNote = CASE WHEN ol.BookingNote IS NULL THEN NULL ELSE N'[已匿名化]' END,
        CVSStoreID = CASE WHEN ol.CVSStoreID IS NULL THEN NULL ELSE N'TEST-STORE' END,
        CVSStoreName = CASE WHEN ol.CVSStoreName IS NULL THEN NULL ELSE N'測試門市' END,
        CVSAddress = CASE WHEN ol.CVSAddress IS NULL THEN NULL ELSE N'測試門市地址' END,
        CVSTelephone = CASE WHEN ol.CVSTelephone IS NULL THEN NULL ELSE N'0200000000' END,
        CVSPaymentNo = CASE WHEN ol.CVSPaymentNo IS NULL THEN NULL ELSE CONCAT(N'TEST-PAY-', ol.Id) END,
        CVSValidationNo = CASE
            WHEN ol.CVSValidationNo IS NULL THEN NULL
            ELSE CONCAT(N'TEST-VALID-', ol.Id)
        END,
        SenderName = CASE WHEN ol.SenderName IS NULL THEN NULL ELSE N'測試寄件人' END,
        SenderPhone = CASE WHEN ol.SenderPhone IS NULL THEN NULL ELSE N'0200000000' END,
        SenderCellPhone = CASE WHEN ol.SenderCellPhone IS NULL THEN NULL ELSE N'0900000000' END,
        SenderZipCode = CASE WHEN ol.SenderZipCode IS NULL THEN NULL ELSE N'000' END,
        SenderAddress = CASE WHEN ol.SenderAddress IS NULL THEN NULL ELSE N'測試寄件地址' END,
        ReceiverName = CASE WHEN ol.ReceiverName IS NULL THEN NULL ELSE N'測試收件人' END,
        ReceiverPhone = CASE WHEN ol.ReceiverPhone IS NULL THEN NULL ELSE N'0200000000' END,
        ReceiverCellPhone = CASE WHEN ol.ReceiverCellPhone IS NULL THEN NULL ELSE N'0900000000' END,
        ReceiverZipCode = CASE WHEN ol.ReceiverZipCode IS NULL THEN NULL ELSE N'000' END,
        ReceiverAddress = CASE WHEN ol.ReceiverAddress IS NULL THEN NULL ELSE N'測試收件地址' END,
        ReceiverEmail = CASE
            WHEN ol.ReceiverEmail IS NULL THEN NULL
            ELSE CONCAT(N'logistics_', ol.Id, N'@example.invalid')
        END
    FROM dbo.Order_Logistics AS ol;

    UPDATE sc
    SET
        CVSOutSide = NULL,
        FK_Uid = CASE WHEN sc.FK_Uid IS NULL THEN NULL ELSE @AuditUserId END,
        CVSStoreID = CASE WHEN sc.CVSStoreID IS NULL THEN NULL ELSE N'TEST-STORE' END,
        CVSStoreName = CASE WHEN sc.CVSStoreName IS NULL THEN NULL ELSE N'測試門市' END,
        CVSAddress = CASE WHEN sc.CVSAddress IS NULL THEN NULL ELSE N'測試門市地址' END,
        CVSTelephone = CASE WHEN sc.CVSTelephone IS NULL THEN NULL ELSE N'0200000000' END
    FROM dbo.ShoppingCarts AS sc;

    ----------------------------------------------------------------
    -- 6. 收件人、聯絡表單、公司與網站聯絡資訊
    ----------------------------------------------------------------

    UPDATE r
    SET
        Name = CONCAT(N'測試收件人', r.Id),
        Email = CONCAT(N'recipient_', r.Id, N'@example.invalid'),
        Address = CONCAT(N'測試地址 ', r.Id, N' 號'),
        CellPhone = N'0900000000',
        TelePhone = N'0200000000',
        ZipCode = N'000',
        CVSStoreID = NULL,
        CVSStoreName = NULL,
        CVSAddress = NULL,
        CVSTelephone = NULL,
        CVSOutSide = NULL
    FROM dbo.Recipients AS r;

    UPDATE c
    SET
        Html = N'[聯絡內容已匿名化]',
        Name = CONCAT(N'測試聯絡人', c.Id),
        UserName = CONCAT(N'test_contact_', c.Id),
        Email = CONCAT(N'contact_', c.Id, N'@example.invalid'),
        TargetEmail = N'contact@example.invalid',
        Reply = CASE WHEN c.Reply = N'' THEN N'' ELSE N'[回覆內容已匿名化]' END,
        FromDate = NULL,
        SourceType = NULL,
        FK_SourceId = NULL
    FROM dbo.Contacts AS c;

    UPDATE company
    SET
        Name = CONCAT(N'測試公司', company.Id),
        TaxID = RIGHT(N'00000000' + CONVERT(nvarchar(20), company.Id), 8),
        Contact = CONCAT(N'測試聯絡人', company.Id),
        Email = CONCAT(N'company_', company.Id, N'@example.invalid'),
        Address = CONCAT(N'測試公司地址 ', company.Id, N' 號')
    FROM dbo.Companies AS company;

    UPDATE w
    SET
        Contact = N'測試聯絡人',
        ContactMail = CONCAT(N'website_', w.Id, N'@example.invalid'),
        Contract = CASE WHEN w.Contract IS NULL THEN NULL ELSE N'[已匿名化]' END
    FROM dbo.Websites AS w;

    ----------------------------------------------------------------
    -- 7. 保留上傳檔案中繼資料與檔名，確保圖片及附件仍可正常顯示
    --    實體檔案內容仍需在資料庫之外另行檢查是否含敏感資訊。
    ----------------------------------------------------------------

    ----------------------------------------------------------------
    -- 8. 將所有 Audit UserId 統一指向交付管理者
    ----------------------------------------------------------------

    DECLARE @AuditObjectId int;
    DECLARE @AuditTable nvarchar(517);
    DECLARE @AuditSet nvarchar(max);
    DECLARE @AuditSql nvarchar(max);

    DECLARE AuditTableCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT
            t.object_id,
            QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
        FROM sys.tables AS t
        JOIN sys.schemas AS s
            ON s.schema_id = t.schema_id
        WHERE t.is_ms_shipped = 0
          AND s.name = N'dbo'
          AND
          (
              EXISTS
              (
                  SELECT 1
                  FROM sys.columns AS c
                  WHERE c.object_id = t.object_id
                    AND c.name = N'CreatorUserId'
              )
              OR EXISTS
              (
                  SELECT 1
                  FROM sys.columns AS c
                  WHERE c.object_id = t.object_id
                    AND c.name = N'LastModifierUserId'
              )
              OR EXISTS
              (
                  SELECT 1
                  FROM sys.columns AS c
                  WHERE c.object_id = t.object_id
                    AND c.name = N'DeleterUserId'
              )
          );

    OPEN AuditTableCursor;
    FETCH NEXT FROM AuditTableCursor INTO @AuditObjectId, @AuditTable;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @AuditSet = N'';

        IF EXISTS
        (
            SELECT 1 FROM sys.columns
            WHERE object_id = @AuditObjectId AND name = N'CreatorUserId'
        )
            SET @AuditSet = @AuditSet
                + CASE WHEN LEN(@AuditSet) > 0 THEN N', ' ELSE N'' END
                + N'[CreatorUserId] = @AdminId';

        IF EXISTS
        (
            SELECT 1 FROM sys.columns
            WHERE object_id = @AuditObjectId AND name = N'LastModifierUserId'
        )
            SET @AuditSet = @AuditSet
                + CASE WHEN LEN(@AuditSet) > 0 THEN N', ' ELSE N'' END
                + N'[LastModifierUserId] = CASE WHEN [LastModifierUserId] IS NULL '
                + N'THEN NULL ELSE @AdminId END';

        IF EXISTS
        (
            SELECT 1 FROM sys.columns
            WHERE object_id = @AuditObjectId AND name = N'DeleterUserId'
        )
            SET @AuditSet = @AuditSet
                + CASE WHEN LEN(@AuditSet) > 0 THEN N', ' ELSE N'' END
                + N'[DeleterUserId] = CASE WHEN [DeleterUserId] IS NULL '
                + N'THEN NULL ELSE @AdminId END';

        SET @AuditSql = N'UPDATE ' + @AuditTable + N' SET ' + @AuditSet + N';';

        EXEC sp_executesql
            @AuditSql,
            N'@AdminId bigint',
            @AdminId = @AuditUserId;

        FETCH NEXT FROM AuditTableCursor INTO @AuditObjectId, @AuditTable;
    END;

    CLOSE AuditTableCursor;
    DEALLOCATE AuditTableCursor;

    ----------------------------------------------------------------
    -- 9. 只保留交付管理者 Users 帳號
    ----------------------------------------------------------------

    DELETE FROM dbo.PermissionDetail
    WHERE FK_UserId IS NOT NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM @DeliveryAdminUsers AS keepUser
          WHERE keepUser.Id = dbo.PermissionDetail.FK_UserId
      );

    DELETE FROM dbo.Permissions
    WHERE FK_UserId IS NOT NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM @DeliveryAdminUsers AS keepUser
          WHERE keepUser.Id = dbo.Permissions.FK_UserId
      );

    DELETE FROM dbo.MappingUserAndRoles
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM @DeliveryAdminUsers AS keepUser
        WHERE keepUser.Id = dbo.MappingUserAndRoles.UserId
    );

    DELETE FROM dbo.MappingUserAndWebsites
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM @DeliveryAdminUsers AS keepUser
        WHERE keepUser.Id = dbo.MappingUserAndWebsites.UserId
    );

    DELETE FROM dbo.MappingUserAndPlatformRoles
    WHERE UserId NOT IN (SELECT Id FROM @DeliveryAdminUsers);

    DELETE FROM dbo.Users
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM @DeliveryAdminUsers AS keepUser
        WHERE keepUser.Id = dbo.Users.Id
    );

    IF (SELECT COUNT(*) FROM dbo.Users) <> (SELECT COUNT(*) FROM @DeliveryAdminUsers)
    BEGIN
        RAISERROR(N'Users 最終筆數與交付管理者清單不一致，停止執行。', 16, 1);
    END;

    ----------------------------------------------------------------
    -- 10. 演練／提交前檢查
    ----------------------------------------------------------------

    SELECT N'Websites' AS TableName, COUNT(*) AS [RowCount] FROM dbo.Websites
    UNION ALL SELECT N'Users', COUNT(*) FROM dbo.Users
    UNION ALL SELECT N'FrontUsers', COUNT(*) FROM dbo.FrontUsers
    UNION ALL SELECT N'Tokens', COUNT(*) FROM dbo.Tokens
    UNION ALL SELECT N'ShoppingCarts', COUNT(*) FROM dbo.ShoppingCarts
    UNION ALL SELECT N'ThirdPartyKeypairValues', COUNT(*) FROM dbo.ThirdPartyKeypairValues
    UNION ALL SELECT N'BackgroundTasks', COUNT(*) FROM dbo.BackgroundTasks
    UNION ALL SELECT N'Notifications', COUNT(*) FROM dbo.Notifications
    UNION ALL SELECT N'PageTextBackfillStates', COUNT(*) FROM dbo.PageTextBackfillStates
    UNION ALL SELECT N'JsonObjects', COUNT(*) FROM dbo.JsonObjects;

    SELECT
        N'FrontUsers 非測試 Email' AS CheckName,
        COUNT(*) AS LeakCount
    FROM dbo.FrontUsers
    WHERE Email IS NOT NULL
      AND Email NOT LIKE N'%@example.invalid'
    UNION ALL
    SELECT
        N'Order_Headers 非測試訂購人 Email',
        COUNT(*)
    FROM dbo.Order_Headers
    WHERE OrdererEmail NOT LIKE N'%@example.invalid'
    UNION ALL
    SELECT
        N'Order_Headers 非測試收件人 Email',
        COUNT(*)
    FROM dbo.Order_Headers
    WHERE RecipientEmail NOT LIKE N'%@example.invalid'
    UNION ALL
    SELECT
        N'Recipients 非測試 Email',
        COUNT(*)
    FROM dbo.Recipients
    WHERE Email NOT LIKE N'%@example.invalid'
    UNION ALL
    SELECT
        N'Contacts 非測試 Email',
        COUNT(*)
    FROM dbo.Contacts
    WHERE Email NOT LIKE N'%@example.invalid'
    UNION ALL
    SELECT
        N'ThirdPartyKeypairValues 殘留',
        COUNT(*)
    FROM dbo.ThirdPartyKeypairValues
    UNION ALL
    SELECT
        N'Tokens 殘留',
        COUNT(*)
    FROM dbo.Tokens
    UNION ALL
    SELECT
        N'StoreSetDetail 對外連線設定殘留',
        COUNT(*)
    FROM dbo.StoreSetDetail AS detail
    INNER JOIN dbo.StoreSet AS setting
        ON setting.Id = detail.FK_StoreSetId
    WHERE setting.[key] IN
    (
        N'SMTPPath',
        N'SMTPPort',
        N'SMTPAccount',
        N'SMTPPassword',
        N'GA4',
        N'GTM',
        N'GoogleAds'
    )
      AND
      (
          detail.enable <> 0
          OR detail.value IS NOT NULL
      );

    SELECT
        u.Id,
        u.Account,
        u.Name,
        u.Email
    FROM dbo.Users AS u;

    SELECT
        m.UserId,
        m.WebsiteId
    FROM dbo.MappingUserAndWebsites AS m
    WHERE EXISTS
    (
        SELECT 1
        FROM @DeliveryAdminUsers AS keepUser
        WHERE keepUser.Id = m.UserId
    )
    ORDER BY m.UserId, m.WebsiteId;

    IF EXISTS (SELECT 1 FROM dbo.PlatformDomains WHERE PasswordCipher IS NOT NULL OR Remark IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.ProvisioningTasks)
       OR EXISTS (SELECT 1 FROM dbo.ProvisioningAgentStatuses)
       OR EXISTS (SELECT 1 FROM dbo.ProvisioningMetricSamples)
       OR EXISTS (SELECT 1 FROM dbo.ProvisioningSiteMetricSamples)
       OR EXISTS (SELECT 1 FROM dbo.PlatformAdministratorInvitations)
       OR EXISTS (SELECT 1 FROM dbo.JsonObjects)
       OR EXISTS (SELECT 1 FROM dbo.AuditLogs)
       OR EXISTS (SELECT 1 FROM dbo.Account_Logs)
       OR EXISTS (SELECT 1 FROM dbo.Tokens)
       OR EXISTS (SELECT 1 FROM dbo.ThirdPartyKeypairValues)
       OR EXISTS (SELECT 1 FROM dbo.FrontUsers WHERE Email IS NULL OR Email NOT LIKE N'%@example.invalid')
       OR EXISTS (SELECT 1 FROM dbo.Order_Headers WHERE OrdererEmail NOT LIKE N'%@example.invalid'
                   OR RecipientEmail NOT LIKE N'%@example.invalid' OR DiscountBreakdownJson IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.SecondaryContacts WHERE Email NOT LIKE N'%@example.invalid')
       OR EXISTS (SELECT 1 FROM dbo.Companies WHERE ContactEmail IS NOT NULL OR Remark IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Users WHERE ForgetID IS NOT NULL OR ForgeIDSendDate IS NOT NULL)
        THROW 51000, N'敏感資料殘留檢查失敗，取消匿名化。', 1;
    -- 阻擋任何工作、歷史個資或快取殘留；存在即失敗，避免只顯示報表仍繼續交付。
    DECLARE @ResidualSql nvarchar(max);
    SELECT @ResidualSql = STRING_AGG(CONVERT(nvarchar(max),
        N'IF EXISTS (SELECT 1 FROM ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
        + N') THROW 51000, N''交付禁止殘留：' + REPLACE(s.name + N'.' + t.name, N'''', N'''''') + N''', 1;'), CHAR(10))
    FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE (s.name = N'dbo' AND t.name IN (N'UserActivityTags',N'Remotes',N'bonusLogDetails',N'BonusLog',N'Account_Logs',N'AuditLogs',N'SearchLogs',N'Advertise_Logs',N'Prod_Logs',N'FlowSizes',N'RemoteDailyStatistics',N'RemoteHourlyStatistics',N'RemoteDailyAggregationRuns',N'ProvisioningSiteMetricSamples',N'ProvisioningMetricSamples',N'ProvisioningAgentStatuses',N'ProvisioningTasks',N'FileRecycleBinBindings',N'FileRecycleBinItems',N'FileCleanupCandidates',N'FileReferences',N'JsonObjects',N'PageTextBackfillStates',N'WebsiteCacheStates',N'HtmlSanitizeStates',N'PlatformAdministratorInvitations',N'Tokens',N'TokenMapShoppingCarts',N'ThirdPartyKeypairValues',N'Notifications',N'BackgroundTasks',N'MappingOldNewUUID', N'__EFMigrationsHistoryOld_20260728', N'Html_ContentsTemp', N'Prod_Prices_temp0719', N'regWebMenus'))
       OR (s.name = N'HangFire' AND t.name <> N'Schema');
    EXEC sys.sp_executesql @ResidualSql;
    IF EXISTS (SELECT 1 FROM dbo.Recipients WHERE Email NOT LIKE N'%@example.invalid'
                  OR ZipCode <> N'000' OR CVSStoreID IS NOT NULL OR CVSStoreName IS NOT NULL
                  OR CVSAddress IS NOT NULL OR CVSTelephone IS NOT NULL OR CVSOutSide IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Contacts WHERE Email NOT LIKE N'%@example.invalid'
                  OR TargetEmail <> N'contact@example.invalid')
       OR EXISTS (SELECT 1 FROM dbo.SecondaryContacts WHERE Email IS NULL OR Email NOT LIKE N'%@example.invalid'
                  OR Phone IS NOT NULL OR JobTitle IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Companies WHERE Email NOT LIKE N'%@example.invalid'
                  OR Phone IS NOT NULL OR InvoiceInfo IS NOT NULL OR SalesOwner IS NOT NULL
                  OR ContactPhone IS NOT NULL OR ContactEmail IS NOT NULL OR Remark IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Websites WHERE HostLocation IS NOT NULL OR PlatformRemark IS NOT NULL
                  OR ContactMail NOT LIKE N'%@example.invalid')
       OR EXISTS (SELECT 1 FROM dbo.PlatformWebsites WHERE HostLocation IS NOT NULL OR Remark IS NOT NULL
                  OR Url NOT LIKE N'https://website-%.example.invalid')
       OR EXISTS (SELECT 1 FROM dbo.PlatformDomains WHERE Registrar IS NOT NULL OR PasswordCipher IS NOT NULL
                  OR Remark IS NOT NULL OR DomainName NOT LIKE N'domain-%.example.invalid')
       OR EXISTS (SELECT 1 FROM dbo.WebsiteAlternateUrls WHERE Url NOT LIKE N'https://alternate-%.example.invalid')
       OR EXISTS (SELECT 1 FROM dbo.WebsiteEnterpriseEmails WHERE OtherType IS NOT NULL OR Remark IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Order_Logistics WHERE RtnMerchantTradeNo IS NOT NULL OR RtnOrderNo IS NOT NULL
                  OR CVSOutSide IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.ShoppingCarts WHERE CVSOutSide IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Users WHERE ForgetID IS NOT NULL OR ForgeIDSendDate IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.CdnProviderSyncStates WHERE LastError IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Bonus WHERE Note <> N'[已匿名化]')
       OR EXISTS (SELECT 1 FROM dbo.StoreSetDetail d JOIN dbo.StoreSet s ON s.Id = d.FK_StoreSetId
                  WHERE s.[key] IN (N'SMTPPath', N'SMTPPort', N'SMTPAccount', N'SMTPPassword', N'GA4', N'GTM', N'GoogleAds')
                    AND (d.enable <> 0 OR d.value IS NOT NULL))
        THROW 51000, N'新增敏感欄位或對外設定仍有殘留，取消交付。', 1;
    IF @DoCommit = 1
    BEGIN
        -- 只有實際通過檢查並提交的母版才帶有此標記；Step 3 必須驗證。
        IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE class = 0 AND name = N'CokerDeliverySanitizationVersion')
            EXEC sys.sp_updateextendedproperty @name=N'CokerDeliverySanitizationVersion', @value=N'2026-10-24-v2';
        ELSE
            EXEC sys.sp_addextendedproperty @name=N'CokerDeliverySanitizationVersion', @value=N'2026-10-24-v2';
        -- 新一輪清洗開始提交時撤銷舊路徑；若新備份失敗，第三步不可誤用舊母版。
        IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE class = 0 AND name = N'CokerDeliveryMasterBackupFile')
            EXEC sys.sp_dropextendedproperty @name=N'CokerDeliveryMasterBackupFile';
        COMMIT TRANSACTION;
        SET @CleaningCommitted = 1;
        PRINT N'匿名化已提交，開始產生母版備份。';

        SET @MasterBackupDirectory = LTRIM(RTRIM(@MasterBackupDirectory));
        IF NULLIF(@MasterBackupDirectory, N'') IS NULL
            THROW 51000, N'母版備份目錄不可空白。', 1;
        IF RIGHT(@MasterBackupDirectory, 1) NOT IN (N'\', N'/')
            SET @MasterBackupDirectory += N'\';
        -- 每次使用唯一檔名，避免覆寫上一輪成功母版。
        SET @MasterBackupFile = @MasterBackupDirectory + @ExpectedDatabase
            + N'_SanitizedMaster_' + CONVERT(nvarchar(8), GETDATE(), 112) + N'_'
            + REPLACE(CONVERT(nvarchar(8), GETDATE(), 108), N':', N'') + N'_'
            + CONVERT(nvarchar(36), NEWID()) + N'.bak';
        IF LEN(@MasterBackupFile) > 3750
            THROW 51000, N'母版路徑過長，無法登記備份位置。', 1;

        BACKUP DATABASE @ExpectedDatabase TO DISK = @MasterBackupFile
        WITH COPY_ONLY, COMPRESSION, CHECKSUM, STATS = 10;
        RESTORE VERIFYONLY FROM DISK = @MasterBackupFile WITH CHECKSUM;

        -- 驗證成功才登記；第三步在同一 SQL Server 上自動讀取此路徑。
        EXEC sys.sp_addextendedproperty @name=N'CokerDeliveryMasterBackupFile', @value=@MasterBackupFile;
        SELECT @MasterBackupFile AS VerifiedMasterBackupFile,
            N'母版備份及驗證完成，可直接執行第三步。' AS Result;
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT N'演練完成，所有變更已回滾。請檢查輸出後，將 @DoCommit 改成 1 再執行。';
    END;
END TRY
BEGIN CATCH
    IF CURSOR_STATUS('local', 'AuditTableCursor') >= 0
        CLOSE AuditTableCursor;

    IF CURSOR_STATUS('local', 'AuditTableCursor') > -3
        DEALLOCATE AuditTableCursor;

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    IF @CleaningCommitted = 1
    BEGIN
        DECLARE @BackupError nvarchar(2048) = LEFT(
            N'匿名化已提交，但母版備份、驗證或路徑登記失敗；沒有可自動交付的新母版。請排除備份目錄／權限／空間問題後重新執行第二步。原因：'
            + ERROR_MESSAGE(), 2048);
        THROW 51000, @BackupError, 1;
    END;
    THROW;
END CATCH;
GO
