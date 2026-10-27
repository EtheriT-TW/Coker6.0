/*
    從已匿名化的共用母版 .bak，一次產生多位開發者各自的交付備份。

    每位開發者的處理流程：
    1. 從 @SourceBackupFile 還原全新的 @WorkingDatabase。
    2. 將所有 User 關聯改指向該交付帳號。
    3. 刪除其他 Users、角色對應、網站對應及個人權限。
    4. 產生包含 UserId 與 FileLabel 的 .bak。
    5. 執行 RESTORE VERIFYONLY。
    6. 下一位開發者重新從相同母版還原，因此彼此完全獨立。

    安全設計：
    - 腳本固定從 master 執行。
    - 禁止將工作資料庫設定為 Coker6 或 Coker_DatabaseInit_Test。
    - 只會刪除／覆寫 @WorkingDatabase。
    - @Execute = 0 時只顯示執行計畫，不讀取整份備份，也不會建立或刪除資料庫。

    重要：
    - 預設從 @MasterDatabase 自動取得第二步已備份且驗證成功的母版路徑。
    - 也可手動指定 @SourceBackupFile，但必須是已完成新版清洗的匿名化母版。
    - 所有檔案路徑都是 SQL Server 主機看到的路徑。
    - SQL Server 服務帳號必須對來源檔有讀取權，並對輸出目錄有寫入權。
*/

USE [master];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- 允許在同一個 SSMS 查詢視窗／連線中重複執行。
DROP TABLE IF EXISTS #Results;
DROP TABLE IF EXISTS #BackupFiles;
DROP TABLE IF EXISTS #Deliveries;

--------------------------------------------------------------------
-- 1. 必填設定
--------------------------------------------------------------------

-- 預設 NULL：自動使用第二步登記的母版。只有另選母版時才填完整 .bak 路徑。
DECLARE @SourceBackupFile nvarchar(4000) = NULL;
DECLARE @MasterDatabase sysname = N'Coker6';

DECLARE @OutputDirectory nvarchar(4000)
    = N'D:\易碩\專案\newCoker\EtheriT.Coker.WebApplication\db';

DECLARE @WorkingDatabase sysname = N'Coker6_Delivery_Work';

-- 第一次必須保持 0；確認計畫後才改成 1。
DECLARE @Execute bit = 1;

-- 0：全部完成後刪除工作資料庫；1：保留最後一位的工作資料庫供人工檢查。
DECLARE @KeepLastWorkingDatabase bit = 0;

CREATE TABLE #Deliveries
(
    UserId bigint NOT NULL PRIMARY KEY,
    FileLabel nvarchar(100) NOT NULL
);

-- FileLabel 會直接出現在檔名中，請填可辨識的英文或中文名稱。
INSERT INTO #Deliveries (UserId, FileLabel)
VALUES
  (2498, N'Perry'),
  (12659, N'wain')
;

--------------------------------------------------------------------
-- 2. 設定與安全檢查
--------------------------------------------------------------------

IF @WorkingDatabase IN
(
    N'Coker6',
    N'Coker_DatabaseInit_Test',
    N'master',
    N'model',
    N'msdb',
    N'tempdb'
)
OR @WorkingDatabase = @MasterDatabase
BEGIN
    RAISERROR(N'@WorkingDatabase 是受保護的資料庫名稱，停止執行。', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM #Deliveries)
BEGIN
    RAISERROR(N'尚未設定任何交付帳號，停止執行。', 16, 1);
    RETURN;
END;

IF EXISTS
(
    SELECT 1
    FROM #Deliveries
    WHERE UserId <= 0
       OR LTRIM(RTRIM(FileLabel)) = N''
       OR FileLabel LIKE N'%[\/:*?"<>|]%'
       OR FileLabel LIKE N'%..%'
)
BEGIN
    RAISERROR(N'UserId 或 FileLabel 不合法；FileLabel 不可空白、包含路徑符號或 ..。', 16, 1);
    RETURN;
END;

SET @OutputDirectory = LTRIM(RTRIM(@OutputDirectory));

IF RIGHT(@OutputDirectory, 1) NOT IN (N'\', N'/')
    SET @OutputDirectory += N'\';

-- 來源備份的完整可讀性驗證移至 @Execute=1 之後；預設只顯示計畫。

DECLARE @RunStamp nvarchar(32)
    = CONVERT(nvarchar(8), GETDATE(), 112)
      + N'_'
      + REPLACE(CONVERT(nvarchar(8), GETDATE(), 108), N':', N'');

SELECT
    d.UserId,
    d.FileLabel,
    @OutputDirectory
        + N'Coker6_Delivery_User_'
        + CONVERT(nvarchar(20), d.UserId)
        + N'_'
        + d.FileLabel
        + N'_'
        + @RunStamp
        + N'.bak' AS PlannedBackupFile
FROM #Deliveries AS d
ORDER BY d.UserId;

IF @Execute = 0
BEGIN
    PRINT N'計畫檢查完成，未修改任何資料庫，也未產生交付備份。確認後將 @Execute 改成 1。';
    DROP TABLE IF EXISTS #Deliveries;
    RETURN;
END;

-- 只在實際執行時查詢來源資料庫；不修改 Coker6，也不建立母版。
IF NULLIF(LTRIM(RTRIM(@SourceBackupFile)), N'') IS NULL
BEGIN
    IF DB_ID(@MasterDatabase) IS NULL
        THROW 51000, N'找不到第二步使用的母版資料庫；請確認 @MasterDatabase，或手動指定母版路徑。', 1;
    DECLARE @ReadMasterPathSql nvarchar(max) = N'USE ' + QUOTENAME(@MasterDatabase) + N';
        IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class = 0
            AND name = N''CokerDeliverySanitizationVersion''
            AND CONVERT(nvarchar(100), value) = N''2026-10-24-v2'')
            THROW 51000, N''請先完成新版第二步清洗及自動備份。'', 1;
        SELECT @Path = CONVERT(nvarchar(4000), value)
        FROM sys.extended_properties WHERE class = 0 AND name = N''CokerDeliveryMasterBackupFile'';';
    EXEC sys.sp_executesql @ReadMasterPathSql, N'@Path nvarchar(4000) OUTPUT', @Path=@SourceBackupFile OUTPUT;
    IF NULLIF(LTRIM(RTRIM(@SourceBackupFile)), N'') IS NULL
        THROW 51000, N'尚無已驗證母版路徑，請先執行第二步（@RunCleaning=1、@DoCommit=1）完成自動備份。', 1;
END;
SELECT @SourceBackupFile AS SourceMasterBackupFile;

--------------------------------------------------------------------
-- 3. 讀取母版備份的邏輯檔案
-- 完整驗證可能耗時，僅實際執行才讀取來源備份。
RESTORE VERIFYONLY FROM DISK = @SourceBackupFile;
--------------------------------------------------------------------

CREATE TABLE #BackupFiles
(
    LogicalName nvarchar(128) NOT NULL,
    PhysicalName nvarchar(260) NOT NULL,
    [Type] char(1) NOT NULL,
    FileGroupName nvarchar(128) NULL,
    Size numeric(20, 0) NOT NULL,
    MaxSize numeric(20, 0) NOT NULL,
    FileID bigint NOT NULL,
    CreateLSN numeric(25, 0) NOT NULL,
    DropLSN numeric(25, 0) NULL,
    UniqueID uniqueidentifier NOT NULL,
    ReadOnlyLSN numeric(25, 0) NULL,
    ReadWriteLSN numeric(25, 0) NULL,
    BackupSizeInBytes bigint NOT NULL,
    SourceBlockSize int NOT NULL,
    FileGroupID int NOT NULL,
    LogGroupGUID uniqueidentifier NULL,
    DifferentialBaseLSN numeric(25, 0) NULL,
    DifferentialBaseGUID uniqueidentifier NULL,
    IsReadOnly bit NOT NULL,
    IsPresent bit NOT NULL,
    TDEThumbprint varbinary(32) NULL,
    SnapshotURL nvarchar(360) NULL
);

DECLARE @FileListSql nvarchar(max)
    = N'RESTORE FILELISTONLY FROM DISK = N'''
      + REPLACE(@SourceBackupFile, N'''', N'''''')
      + N''';';

INSERT INTO #BackupFiles
EXEC sys.sp_executesql @FileListSql;

IF NOT EXISTS (SELECT 1 FROM #BackupFiles WHERE [Type] = 'D')
   OR NOT EXISTS (SELECT 1 FROM #BackupFiles WHERE [Type] = 'L')
BEGIN
    RAISERROR(N'來源備份缺少資料檔或記錄檔，停止執行。', 16, 1);
    RETURN;
END;

DECLARE @DefaultDataPath nvarchar(4000)
    = CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultDataPath'));
DECLARE @DefaultLogPath nvarchar(4000)
    = CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultLogPath'));

IF @DefaultDataPath IS NULL OR LTRIM(RTRIM(@DefaultDataPath)) = N''
BEGIN
    SELECT TOP (1)
        @DefaultDataPath
            = LEFT(physical_name, LEN(physical_name) - CHARINDEX(N'\', REVERSE(physical_name)) + 1)
    FROM sys.master_files
    WHERE database_id = DB_ID(N'master')
      AND [type] = 0;
END;

IF @DefaultLogPath IS NULL OR LTRIM(RTRIM(@DefaultLogPath)) = N''
BEGIN
    SELECT TOP (1)
        @DefaultLogPath
            = LEFT(physical_name, LEN(physical_name) - CHARINDEX(N'\', REVERSE(physical_name)) + 1)
    FROM sys.master_files
    WHERE database_id = DB_ID(N'master')
      AND [type] = 1;
END;

IF RIGHT(@DefaultDataPath, 1) NOT IN (N'\', N'/')
    SET @DefaultDataPath += N'\';

IF RIGHT(@DefaultLogPath, 1) NOT IN (N'\', N'/')
    SET @DefaultLogPath += N'\';

DECLARE @MoveClauses nvarchar(max);

;WITH FileDestinations AS
(
    SELECT
        LogicalName,
        [Type],
        FileID,
        ROW_NUMBER() OVER
        (
            PARTITION BY [Type]
            ORDER BY FileID
        ) AS TypeSequence
    FROM #BackupFiles
)
SELECT
    @MoveClauses = STRING_AGG
    (
        CONVERT
        (
            nvarchar(max),
            N'MOVE N'''
            + REPLACE(LogicalName, N'''', N'''''')
            + N''' TO N'''
            + REPLACE
              (
                  CASE
                      WHEN [Type] = 'L'
                          THEN @DefaultLogPath
                               + @WorkingDatabase
                               + CASE
                                     WHEN TypeSequence = 1 THEN N'_log'
                                     ELSE N'_log_' + CONVERT(nvarchar(20), TypeSequence)
                                 END
                               + N'.ldf'
                      ELSE @DefaultDataPath
                           + @WorkingDatabase
                           + CASE
                                 WHEN TypeSequence = 1 THEN N''
                                 ELSE N'_' + CONVERT(nvarchar(20), TypeSequence)
                             END
                           + CASE WHEN TypeSequence = 1 THEN N'.mdf' ELSE N'.ndf' END
                  END,
                  N'''',
                  N''''''
              )
            + N''''
        ),
        N', '
    )
FROM FileDestinations;

--------------------------------------------------------------------
-- 4. 每位交付者：還原、只留單一 User、備份、驗證
--------------------------------------------------------------------

CREATE TABLE #Results
(
    UserId bigint NOT NULL,
    FileLabel nvarchar(100) NOT NULL,
    BackupFile nvarchar(4000) NOT NULL,
    BackupResult nvarchar(200) NOT NULL
);

DECLARE @CurrentUserId bigint;
DECLARE @CurrentFileLabel nvarchar(100);
DECLARE @CurrentBackupFile nvarchar(4000);
DECLARE @RestoreSql nvarchar(max);
DECLARE @FinalizeSql nvarchar(max);
DECLARE @DropSql nvarchar(max);

DECLARE DeliveryCursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT UserId, FileLabel
    FROM #Deliveries
    ORDER BY UserId;

BEGIN TRY
    OPEN DeliveryCursor;
    FETCH NEXT FROM DeliveryCursor INTO @CurrentUserId, @CurrentFileLabel;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT N'開始處理 UserId='
            + CONVERT(nvarchar(20), @CurrentUserId)
            + N'，交付名稱='
            + @CurrentFileLabel;

        IF DB_ID(@WorkingDatabase) IS NOT NULL
        BEGIN
            SET @DropSql
                = N'ALTER DATABASE ' + QUOTENAME(@WorkingDatabase)
                + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; '
                + N'DROP DATABASE ' + QUOTENAME(@WorkingDatabase) + N';';

            EXEC sys.sp_executesql @DropSql;
        END;

        SET @RestoreSql
            = N'RESTORE DATABASE ' + QUOTENAME(@WorkingDatabase)
            + N' FROM DISK = N'''
            + REPLACE(@SourceBackupFile, N'''', N'''''')
            + N''' WITH '
            + @MoveClauses
            + N', RECOVERY, REPLACE, STATS = 10;';

        EXEC sys.sp_executesql @RestoreSql;

        SET @FinalizeSql
            = N'USE ' + QUOTENAME(@WorkingDatabase) + N';
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Users
        WHERE Id = @FinalUserId
    )
    BEGIN
        RAISERROR(N''來源母版找不到指定的交付管理者 UserId。'', 16, 1);
    END;

-- 結構基準：20261020025140_AddSiteRuntimeDiagnostics / CokerDbContextModelSnapshot。
-- 全部 dbo 業務表都必須已審核；新增／刪除／改名／型別、長度、NULL 變更一律停止。
IF ISNULL(HAS_PERMS_BY_NAME(DB_NAME(), N''DATABASE'', N''VIEW DEFINITION''), 0) <> 1
    THROW 51000, N''需要資料庫 VIEW DEFINITION 權限才能完整核對結構。'', 1;
IF EXISTS
(
    SELECT 1 FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE t.is_ms_shipped = 0 AND s.name <> N''dbo''
      AND (s.name <> N''HangFire'' OR t.name NOT IN
          (N''Schema'', N''Job'', N''State'', N''JobParameter'', N''JobQueue'', N''Server'',
           N''List'', N''Set'', N''Counter'', N''AggregatedCounter'', N''Hash''))
)
    THROW 51000, N''發現未審核的非 dbo 資料表，請先補上清洗規則。'', 1;
DECLARE @ReviewedSchema TABLE (TableName sysname PRIMARY KEY, ColumnSpec nvarchar(max));
INSERT INTO @ReviewedSchema (TableName, ColumnSpec) VALUES
    (N''Account_Logs'', N''[BrowserInfo] nvarchar(512) NULL,[ClientIpAddress] nvarchar(64) NULL,[CorrelationId] nvarchar(128) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[CurrentStatus] int NULL,[DetailsJson] nvarchar(max) NULL,[ErrorTimes] int NOT NULL,[EventName] nvarchar(80) NULL,[FailureReason] nvarchar(100) NULL,[Id] bigint NOT NULL,[KeyFingerprint] nvarchar(64) NULL,[LastLoginTime] datetime2(7) NULL,[LockTime] datetime2(7) NULL,[NewEmail] nvarchar(150) NULL,[OldEmail] nvarchar(150) NULL,[PreviousStatus] int NULL,[RecipientEmail] nvarchar(150) NULL,[Status] int NOT NULL,[Success] bit NULL,[UUID] uniqueidentifier NOT NULL,[VerificationMethod] nvarchar(80) NULL,[WebsiteId] bigint NOT NULL''),
    (N''Advertise'', N''[ActionType] int NOT NULL,[Clicks] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Describe] nvarchar(300) NULL,[EndDate] datetime2(7) NULL,[Exposure] int NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Id] bigint NOT NULL,[Img] nvarchar(100) NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Link] nvarchar(255) NULL,[Permanent] bit NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[SerNO] int NOT NULL,[StartDate] datetime2(7) NULL,[Target] bit NOT NULL,[Title] nvarchar(300) NOT NULL,[Type] bigint NOT NULL,[Visible] bit NOT NULL''),
    (N''Advertise_Logs'', N''[Action] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_Adid] bigint NOT NULL,[FK_UserId] bigint NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[UUID] uniqueidentifier NOT NULL''),
    (N''Article'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DataJson] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[EndTime] datetime2(7) NULL,[EventEndDate] datetime2(7) NULL,[EventStartDate] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Latitude] float NULL,[Longitude] float NULL,[NewsletterCss] nvarchar(max) NULL,[NewsletterHtml] nvarchar(max) NULL,[NodeDate] datetime2(7) NULL,[PageText] nvarchar(max) NULL,[Popular] int NOT NULL,[PopularVisible] bit NOT NULL,[RemovedFromShelves] bit NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[SerNO] int NOT NULL,[StartTime] datetime2(7) NULL,[Subtitle] nvarchar(max) NULL,[Title] nvarchar(150) NULL,[Visible] bit NOT NULL,[permanent] bit NOT NULL''),
    (N''AuditLogs'', N''[BrowserInfo] nvarchar(512) NULL,[ClientIpAddress] nvarchar(64) NULL,[ClientName] nvarchar(128) NULL,[CustomData] nvarchar(2000) NULL,[Exception] nvarchar(2000) NULL,[ExecutionDuration] int NOT NULL,[ExecutionTime] datetime2(7) NOT NULL,[FK_WebsiteId] bigint NULL,[Id] bigint NOT NULL,[ImpersonatorTenantId] int NOT NULL,[ImpersonatorUserId] bigint NOT NULL,[MethodName] nvarchar(256) NULL,[Parameters] nvarchar(max) NULL,[ReturnValue] nvarchar(max) NULL,[ServiceName] nvarchar(256) NULL,[UserId] bigint NULL''),
    (N''BackgroundTasks'', N''[ActiveKey] nvarchar(250) NULL,[CompletionTime] datetime2(7) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Error] nvarchar(4000) NULL,[ExpireTime] datetime2(7) NULL,[FK_UserId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[HangfireJobId] nvarchar(100) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Message] nvarchar(500) NOT NULL,[Progress] int NOT NULL,[ResultFileName] nvarchar(255) NULL,[ResultFilePath] nvarchar(1000) NULL,[ResultJson] nvarchar(max) NULL,[SourceFilePath] nvarchar(1000) NULL,[StartTime] datetime2(7) NULL,[Status] int NOT NULL,[StorageKey] uniqueidentifier NOT NULL,[Type] int NOT NULL''),
    (N''Bonus'', N''[Amount] int NOT NULL,[Balance] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[EndDate] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Note] nvarchar(max) NOT NULL,[StartDate] datetime2(7) NOT NULL,[Status] int NOT NULL,[UUID] uniqueidentifier NOT NULL''),
    (N''BonusLiabilities'', N''[OutstandingPoints] int NOT NULL,[UUID] uniqueidentifier NOT NULL,[UpdatedAt] datetime2(7) NOT NULL''),
    (N''BonusLog'', N''[Amount] int NOT NULL,[ExecutionTime] datetime2(7) NOT NULL,[Id] bigint NOT NULL,[Note] nvarchar(max) NOT NULL,[RefKey] bigint NULL,[Type] int NOT NULL,[UUID] uniqueidentifier NOT NULL''),
    (N''bonusLogDetails'', N''[FK_BonusId] bigint NOT NULL,[FK_BonusLogsId] bigint NOT NULL,[UsedAmount] bigint NOT NULL''),
    (N''CdnProviderIpRanges'', N''[Cidr] nvarchar(64) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IpVersion] tinyint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LastVerifiedTime] datetime2(7) NOT NULL,[Provider] nvarchar(50) NOT NULL''),
    (N''CdnProviderSyncStates'', N''[AlertSent] bit NOT NULL,[ConsecutiveFailureCount] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastAlertTime] datetime2(7) NULL,[LastAttemptTime] datetime2(7) NULL,[LastError] nvarchar(4000) NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LastSuccessTime] datetime2(7) NULL,[Provider] nvarchar(50) NOT NULL''),
    (N''Companies'', N''[Address] nvarchar(150) NOT NULL,[Contact] nvarchar(100) NOT NULL,[ContactEmail] nvarchar(150) NULL,[ContactJobTitle] nvarchar(100) NULL,[ContactPhone] nvarchar(50) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[CustomerType] int NOT NULL,[CustomerTypeOther] nvarchar(50) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(150) NOT NULL,[Id] bigint NOT NULL,[InvoiceInfo] nvarchar(500) NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(200) NOT NULL,[Phone] nvarchar(50) NULL,[Remark] nvarchar(2000) NULL,[SalesOwner] nvarchar(50) NULL,[TaxID] nvarchar(10) NOT NULL''),
    (N''ComponentPurposes'', N''[Code] nvarchar(100) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(150) NOT NULL,[SerNo] int NOT NULL,[Visible] bit NOT NULL''),
    (N''Contacts'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(250) NOT NULL,[FK_SourceId] bigint NULL,[FK_WebMenuId] bigint NOT NULL,[FromDate] nvarchar(max) NULL,[Html] nvarchar(max) NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(200) NOT NULL,[Reply] nvarchar(max) NOT NULL,[ReplyTime] datetime2(7) NULL,[SourceType] int NULL,[Status] int NOT NULL,[TargetEmail] nvarchar(500) NOT NULL,[UserName] nvarchar(200) NOT NULL''),
    (N''CustSearch'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Placeholder] nvarchar(100) NOT NULL,[SearchAllArticle] bit NOT NULL,[SearchAllMenu] bit NOT NULL,[SearchAllProd] bit NOT NULL,[SerNo] int NOT NULL,[Title] nvarchar(100) NULL,[Visible] bit NOT NULL''),
    (N''Directory'', N''[CalendarType] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(150) NULL,[FK_DefaultLayout] bigint NULL,[FK_Mid] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[FacetType] int NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[SortBy] int NOT NULL,[Title] nvarchar(60) NULL,[Type] int NOT NULL,[Visible] bit NOT NULL''),
    (N''DirectoryFacetRanges'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Enabled] bit NOT NULL,[End] int NOT NULL,[FK_DirectoryId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Sort] int NOT NULL,[Start] int NOT NULL''),
    (N''Favorites'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_AssocId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Type] int NOT NULL,[UUID] uniqueidentifier NOT NULL''),
    (N''FileBindMores'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileBindGuid] uniqueidentifier NOT NULL,[FK_FileUploadId] bigint NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[type] int NOT NULL''),
    (N''FileBinds'', N''[AreaKey] nvarchar(max) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileUploadId] bigint NULL,[Guid] uniqueidentifier NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsVisible] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MediaLink] nvarchar(max) NOT NULL,[Name] nvarchar(max) NOT NULL,[SerNo] int NOT NULL,[Sid] bigint NOT NULL,[num] int NOT NULL,[type] int NOT NULL''),
    (N''FileCleanupCandidates'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileUploadId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[FirstDetectedTime] datetime2(7) NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastConfirmedTime] datetime2(7) NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Reason] nvarchar(500) NOT NULL''),
    (N''FileRecycleBinBindings'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileBindGuid] uniqueidentifier NOT NULL,[FK_FileUploadId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL''),
    (N''FileRecycleBinItems'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileUploadId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[OriginalPath] nvarchar(200) NOT NULL,[Reason] nvarchar(500) NOT NULL,[RecycledTime] datetime2(7) NOT NULL''),
    (N''FileReferences'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_FileUploadId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastConfirmedTime] datetime2(7) NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[NormalizedPath] nvarchar(1000) NOT NULL,[OccurrenceCount] int NOT NULL,[PhysicalFileExists] bit NOT NULL,[SourceField] nvarchar(80) NOT NULL,[SourceId] bigint NOT NULL,[SourceState] nvarchar(30) NOT NULL,[SourceType] nvarchar(80) NOT NULL''),
    (N''FileUploads'', N''[ContentType] nvarchar(200) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[DownloadFileName] nvarchar(200) NULL,[FK_WebsiteId] bigint NOT NULL,[FileGuid] uniqueidentifier NULL,[GuidKey] uniqueidentifier NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsEncryption] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[OriginalFileName] nvarchar(200) NOT NULL,[Size] bigint NOT NULL''),
    (N''FlowSizes'', N''[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[RequestSize] bigint NOT NULL,[ResponseSize] bigint NOT NULL,[Total] bigint NOT NULL,[actionTime] datetime2(7) NOT NULL''),
    (N''FooterTemplates'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_TemplateSectionsId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[css] nvarchar(max) NULL,[html] nvarchar(max) NULL,[saveCss] nvarchar(max) NULL,[saveHtml] nvarchar(max) NULL''),
    (N''FrontUsers'', N''[Account] nvarchar(100) NULL,[Address] nvarchar(250) NULL,[Birthday] datetime2(7) NULL,[CellPhone] nvarchar(50) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(150) NULL,[ErrorTimes] int NOT NULL,[FK_User] bigint NULL,[ForgeIDSendDate] datetime2(7) NULL,[ForgetID] uniqueidentifier NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Level] bigint NULL,[LockTime] datetime2(7) NULL,[Name] nvarchar(150) NOT NULL,[OpenDate] datetime2(7) NULL,[OpenID] uniqueidentifier NOT NULL,[OpenIDSendDate] datetime2(7) NOT NULL,[Password] nvarchar(max) NOT NULL,[PrivacyAgreeTime] datetime2(7) NULL,[Sex] int NULL,[Status] int NOT NULL,[TelPhone] nvarchar(50) NULL,[UUID] uniqueidentifier NOT NULL''),
    (N''Html_Contents'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Disp_opt] bit NOT NULL,[EndDate] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Icon] nvarchar(100) NULL,[Id] bigint NOT NULL,[Img] nvarchar(100) NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Link] nvarchar(255) NULL,[Menu_id] bigint NULL,[ObjectType] int NOT NULL,[Ser_no] int NOT NULL,[StartDate] datetime2(7) NULL,[Target] bit NOT NULL,[Title] nvarchar(300) NULL,[Type] bigint NOT NULL,[permanent] bit NOT NULL''),
    (N''HtmlContentPurposes'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_ComponentPurposeId] bigint NOT NULL,[FK_HtmlContentId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL''),
    (N''HtmlSanitizeStates'', N''[ContentHash] nvarchar(64) NOT NULL,[ContentKey] nvarchar(50) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[FK_Bid] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[SanitizePolicy] nvarchar(50) NOT NULL,[SanitizeVersion] nvarchar(30) NOT NULL,[SourceType] int NOT NULL''),
    (N''JsonObjects'', N''[CacheKey] nvarchar(450) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_AId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[Json] nvarchar(max) NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Version] bigint NOT NULL''),
    (N''LogisticsBoxFees'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_LogisticsBoxId] bigint NOT NULL,[FK_LogisticsSettingId] bigint NOT NULL,[Fee] decimal(18,2) NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL''),
    (N''LogisticsBoxs'', N''[CapacityPoint] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsActive] bit NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(max) NOT NULL,[Sort] int NOT NULL''),
    (N''LogisticsSettings'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Dis_Freight] int NULL,[DiscountFreightType] int NULL,[FK_WebsiteId] bigint NOT NULL,[Freight] int NULL,[FreightAmt2] int NULL,[FreightStatusType] int NOT NULL,[FreightType] int NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LogisticsType] int NOT NULL,[Low_Con] int NULL,[PreserveType] int NOT NULL,[Set_Default] bit NOT NULL,[SupportCashOnDelivery] bit NOT NULL,[Title] nvarchar(50) NOT NULL''),
    (N''LogisticsType_Payments'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_LogisticsSettingId] bigint NULL,[FK_PaymentTypeId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsEnabled] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[OverrideMaxAmount] decimal(18,2) NULL,[OverrideMinAmount] decimal(18,2) NULL,[ShippingType] int NULL''),
    (N''MappingCompanyAndWebsites'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_CompanyId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL''),
    (N''MappingFrontUserAndWebsite'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_UserId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL''),
    (N''MappingLogisticsSettingAndProd'', N''[FK_LogisticsSettingId] bigint NOT NULL,[FK_ProdId] bigint NOT NULL''),
    (N''MappingOldNewUUID'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[TempUUID] uniqueidentifier NOT NULL,[UserUUID] uniqueidentifier NOT NULL''),
    (N''MappingUserAndPlatformRoles'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[PlatformRoleId] bigint NOT NULL,[UserId] bigint NOT NULL''),
    (N''MappingUserAndRoles'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[RoleId] bigint NOT NULL,[UUID] uniqueidentifier NOT NULL,[UserId] bigint NOT NULL''),
    (N''MappingUserAndWebsites'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[UserId] bigint NOT NULL,[WebsiteId] bigint NOT NULL''),
    (N''MappingWebsiteRelationship'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FatherId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[WebsiteId] bigint NOT NULL''),
    (N''MarketingCampaigns'', N''[CampaignType] int NOT NULL,[CanStack] bit NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(500) NULL,[EndTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(100) NOT NULL,[NeverEnd] bit NOT NULL,[Priority] int NOT NULL,[Repeatable] bit NOT NULL,[StartTime] datetime2(7) NOT NULL,[Status] int NOT NULL''),
    (N''MarketingConditions'', N''[ConditionType] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[ExcludeDiscountedItems] bit NOT NULL,[FK_MarketingRuleId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MinAmount] decimal(18,2) NULL,[MinQuantity] int NULL,[OnlyScopeItems] bit NOT NULL''),
    (N''MarketingRewardItems'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Enabled] bit NOT NULL,[FK_MarketingRewardId] bigint NOT NULL,[FK_ProdStockId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MaxQuantityPerOrder] int NOT NULL,[OfferPrice] decimal(18,2) NOT NULL,[SortOrder] int NOT NULL''),
    (N''MarketingRewards'', N''[BonusAmount] int NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[DeliveryType] int NOT NULL,[DiscountAmount] decimal(18,2) NULL,[DiscountPercent] decimal(5,2) NULL,[FK_CouponTemplateId] bigint NULL,[FK_GiftProductId] bigint NULL,[FK_GiftProductStockId] bigint NULL,[FK_MarketingRuleId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MaxDiscountAmount] decimal(18,2) NULL,[MaxSelectionQuantityPerOrder] int NULL,[SelectionQuantityPerQualification] int NOT NULL''),
    (N''MarketingRules'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Enabled] bit NOT NULL,[FK_MarketingCampaignId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[RuleType] int NOT NULL,[ScopeType] int NOT NULL,[SortOrder] int NOT NULL''),
    (N''MarketingScopeItems'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_MarketingRuleId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[RequiredQuantityPerQualification] int NOT NULL,[TargetId] bigint NOT NULL,[TargetType] int NOT NULL''),
    (N''Marquees'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[EndTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[StartTime] datetime2(7) NULL,[disp_opt] bit NOT NULL,[link] nvarchar(255) NOT NULL,[permanent] bit NOT NULL,[placement] nvarchar(max) NOT NULL,[ser_no] int NOT NULL,[target] bit NOT NULL,[title] nvarchar(120) NOT NULL''),
    (N''NotFoundImage'', N''[CreateDate] datetime2(7) NOT NULL,[FK_WebsiteId] bigint NOT NULL,[From] nvarchar(255) NOT NULL,[Id] bigint NOT NULL,[Url] nvarchar(255) NOT NULL''),
    (N''Notifications'', N''[ActionUrl] nvarchar(1000) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_BackgroundTaskId] bigint NULL,[FK_UserId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsRead] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Message] nvarchar(1000) NOT NULL,[Title] nvarchar(250) NOT NULL,[Type] int NOT NULL''),
    (N''ObjectTypes'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[SerNo] int NOT NULL,[Title] nvarchar(150) NOT NULL''),
    (N''Order_Details'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_OId] bigint NOT NULL,[FK_SCId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL''),
    (N''Order_Headers'', N''[Bonus] int NULL,[Carrier] nvarchar(max) NULL,[CompletedDate] datetime2(7) NULL,[CouponId] bigint NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Discount] decimal(18,2) NULL,[DiscountBreakdownJson] nvarchar(max) NULL,[FK_UUID] uniqueidentifier NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Fk_Tid] uniqueidentifier NOT NULL,[Fk_UserId] bigint NULL,[Freight] decimal(18,2) NOT NULL,[GetBonus] int NULL,[Id] bigint NOT NULL,[InvoiceAddress] nvarchar(250) NULL,[InvoiceRecipient] int NULL,[InvoiceTitle] nvarchar(max) NULL,[InvoiceType] int NULL,[IsDeleted] bit NOT NULL,[IsTemp] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Memo] nvarchar(500) NULL,[Orderer] nvarchar(150) NOT NULL,[OrdererAddress] nvarchar(250) NOT NULL,[OrdererCellPhone] nvarchar(50) NOT NULL,[OrdererEmail] nvarchar(150) NOT NULL,[OrdererSex] int NULL,[OrdererTelePhone] nvarchar(50) NULL,[OrdererZipCode] nvarchar(max) NOT NULL,[Payment] bigint NOT NULL,[PersonalInvoiceType] int NULL,[Recipient] nvarchar(150) NOT NULL,[RecipientAddress] nvarchar(250) NOT NULL,[RecipientCellPhone] nvarchar(50) NOT NULL,[RecipientEmail] nvarchar(150) NOT NULL,[RecipientSex] int NULL,[RecipientTelePhone] nvarchar(50) NULL,[RecipientZipCode] nvarchar(max) NOT NULL,[Remark] nvarchar(500) NULL,[RepayDate] datetime2(7) NULL,[RepayTimes] int NULL,[Service_Charge] int NULL,[Shipping] bigint NOT NULL,[State] int NOT NULL,[Subtotal] decimal(18,2) NOT NULL,[SystemMemo] nvarchar(1000) NULL,[TrackingNumber] nvarchar(20) NULL,[TransactionId] nvarchar(max) NULL,[UniformId] nvarchar(max) NULL,[refundTransactionDate] datetime2(7) NULL,[refundTransactionId] nvarchar(max) NULL''),
    (N''Order_Logistics'', N''[AllPayLogisticsID] nvarchar(max) NULL,[BookingNote] nvarchar(max) NULL,[CVSAddress] nvarchar(max) NULL,[CVSOutSide] nvarchar(max) NULL,[CVSPaymentNo] nvarchar(max) NULL,[CVSStoreID] nvarchar(max) NULL,[CVSStoreName] nvarchar(max) NULL,[CVSTelephone] nvarchar(max) NULL,[CVSValidationNo] nvarchar(max) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_OhId] bigint NOT NULL,[GoodsWeight] decimal(18,2) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LogisticsStatusCode] nvarchar(max) NULL,[LogisticsSubType] nvarchar(max) NOT NULL,[LogisticsType] nvarchar(max) NOT NULL,[MerchantTradeDate] datetime2(7) NULL,[MerchantTradeNo] nvarchar(max) NULL,[ReceiverAddress] nvarchar(max) NULL,[ReceiverCellPhone] nvarchar(max) NULL,[ReceiverEmail] nvarchar(max) NULL,[ReceiverName] nvarchar(max) NULL,[ReceiverPhone] nvarchar(max) NULL,[ReceiverZipCode] nvarchar(max) NULL,[RtnMerchantTradeNo] nvarchar(max) NULL,[RtnOrderNo] nvarchar(max) NULL,[SenderAddress] nvarchar(max) NULL,[SenderCellPhone] nvarchar(max) NULL,[SenderName] nvarchar(max) NULL,[SenderPhone] nvarchar(max) NULL,[SenderZipCode] nvarchar(max) NULL,[Specification] nvarchar(max) NULL,[Temperature] nvarchar(max) NULL,[UpdateStatusDate] datetime2(7) NULL''),
    (N''PageTextBackfillStates'', N''[CompletionTime] datetime2(7) NULL,[ContentType] nvarchar(20) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[FailedCount] int NOT NULL,[FailedIdsJson] nvarchar(max) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastError] nvarchar(4000) NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LastProcessedId] bigint NOT NULL,[ProcessedCount] int NOT NULL,[RemainingNullCount] int NOT NULL,[StartTime] datetime2(7) NULL,[Status] nvarchar(30) NOT NULL,[TargetMaxId] bigint NOT NULL,[TotalCount] int NOT NULL''),
    (N''PaymentTypes'', N''[CanRefund] bit NOT NULL,[Code] nvarchar(50) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[CvsStoreSelectionMode] int NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_ThirdPartyId] bigint NOT NULL,[Icons] nvarchar(max) NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MaxAmount] decimal(18,2) NULL,[MinAmount] decimal(18,2) NOT NULL,[RefundWorkDay] int NOT NULL,[RepayAfterMinutes] int NULL,[SerNo] int NOT NULL,[ThirdPartyId] bigint NULL,[Title] nvarchar(50) NULL,[Used] bit NOT NULL''),
    (N''PaymentTypesValues'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_PaymentTypesId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Used] bit NOT NULL''),
    (N''PermissionDetail'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[FK_RoleId] bigint NULL,[FK_TargetId] bigint NULL,[FK_UserId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsGranted] bit NOT NULL,[Name] nvarchar(300) NOT NULL,[Type] int NOT NULL''),
    (N''Permissions'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[FK_RoleId] bigint NULL,[FK_UserId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsGranted] bit NOT NULL,[Name] nvarchar(300) NOT NULL''),
    (N''PlatformAdministratorInvitations'', N''[ApprovedAtUtc] datetime2(7) NULL,[ApprovedByUserId] bigint NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[EmailVerifiedAtUtc] datetime2(7) NULL,[ExpiresAtUtc] datetime2(7) NOT NULL,[Id] bigint NOT NULL,[InvitedEmail] nvarchar(150) NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MvcRoleId] bigint NULL,[PlatformRoleId] bigint NULL,[RevokedAtUtc] datetime2(7) NULL,[RevokedByUserId] bigint NULL,[UserId] bigint NOT NULL''),
    (N''PlatformDomains'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[DomainName] nvarchar(255) NOT NULL,[EndDate] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsProxyPurchase] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[PasswordCipher] nvarchar(1000) NULL,[Registrar] nvarchar(200) NULL,[Remark] nvarchar(2000) NULL,[StartDate] datetime2(7) NULL''),
    (N''PlatformRoles'', N''[Code] nvarchar(80) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(500) NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsEnabled] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(100) NOT NULL,[Sort] int NOT NULL''),
    (N''PlatformWebsites'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_CompanyId] bigint NOT NULL,[FK_PlatformDomainId] bigint NULL,[FK_WebsiteId] bigint NULL,[HostLocation] nvarchar(200) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsDomainPending] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Level] int NULL,[Locale] nvarchar(10) NULL,[Name] nvarchar(250) NOT NULL,[Remark] nvarchar(2000) NULL,[ServiceEndDate] datetime2(7) NULL,[ServiceStartDate] datetime2(7) NULL,[Status] int NOT NULL,[TerminatedDate] datetime2(7) NULL,[Url] nvarchar(500) NULL''),
    (N''Prod_Logs'', N''[Action] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[FK_Pid] bigint NOT NULL,[FK_UserId] bigint NULL,[Id] bigint NOT NULL,[Remark] nvarchar(300) NOT NULL,[UUID] uniqueidentifier NOT NULL''),
    (N''Prod_Prices'', N''[Bonus] int NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_PSId] bigint NOT NULL,[FK_RId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Price] decimal(18,2) NULL''),
    (N''Prod_Spec_Types'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[SeoVariantProperty] int NOT NULL,[Type] nvarchar(max) NOT NULL''),
    (N''Prod_Specs'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_Tid] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Title] nvarchar(max) NOT NULL''),
    (N''Prod_Stocks'', N''[Alert_Qty] int NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_Pid] bigint NOT NULL,[FK_S1id] bigint NULL,[FK_S2id] bigint NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsTimePrice] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Min_Qty] int NULL,[PackingPoint] int NOT NULL,[Price] decimal(18,2) NOT NULL,[Ser_No] int NOT NULL,[SpecDescription] nvarchar(3000) NULL,[Stock] int NULL,[SubItemNo] nvarchar(max) NULL,[Visible] bit NOT NULL''),
    (N''Prod_TechCerts'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_PId] bigint NOT NULL,[FK_TCId] bigint NOT NULL,[Id] bigint NOT NULL,[IsChecked] bit NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL''),
    (N''Prods'', N''[Clicks] int NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(3000) NOT NULL,[Discount] float NULL,[EndTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Id] bigint NOT NULL,[Introduction] nvarchar(3000) NOT NULL,[IsDeleted] bit NOT NULL,[ItemNo] nvarchar(100) NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[NoStockManagement] bit NOT NULL,[PageText] nvarchar(max) NULL,[Popular] int NOT NULL,[PopularVisible] bit NOT NULL,[RemovedFromShelves] bit NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[Ser_No] int NOT NULL,[StartTime] datetime2(7) NULL,[Status] int NOT NULL,[Title] nvarchar(150) NOT NULL,[Visible] bit NOT NULL,[oStatus] int NULL,[permanent] bit NOT NULL''),
    (N''ProvisioningAgentStatuses'', N''[AgentVersion] nvarchar(50) NOT NULL,[AppPoolMetricsJson] nvarchar(max) NOT NULL,[CpuUsagePercent] float NULL,[DiskMetricsJson] nvarchar(max) NOT NULL,[DryRun] bit NOT NULL,[LastSeenAtUtc] datetime2(7) NOT NULL,[MachineName] nvarchar(200) NOT NULL,[MemoryTotalBytes] bigint NOT NULL,[MemoryUsagePercent] float NULL,[MemoryUsedBytes] bigint NOT NULL,[ServerId] nvarchar(100) NOT NULL,[TlsSnapshotJson] nvarchar(max) NULL,[WorkerId] nvarchar(200) NOT NULL''),
    (N''ProvisioningMetricSamples'', N''[CpuUsagePercent] float NULL,[DiskMetricsJson] nvarchar(max) NOT NULL,[Id] bigint NOT NULL,[MemoryTotalBytes] bigint NOT NULL,[MemoryUsagePercent] float NULL,[MemoryUsedBytes] bigint NOT NULL,[SampledAtUtc] datetime2(7) NOT NULL,[ServerId] nvarchar(100) NOT NULL''),
    (N''ProvisioningSiteMetricSamples'', N''[ApplicationPoolName] nvarchar(200) NOT NULL,[CpuUsagePercent] float NULL,[Id] bigint NOT NULL,[OnlineVisitors] bigint NULL,[PrivateMemoryBytes] bigint NOT NULL,[ProcessIdsJson] nvarchar(max) NOT NULL,[RuntimeMetricsJson] nvarchar(max) NULL,[SampledAtUtc] datetime2(7) NOT NULL,[ServerId] nvarchar(100) NOT NULL,[WebsiteIdsJson] nvarchar(max) NOT NULL,[WebsiteNamesJson] nvarchar(max) NOT NULL,[WorkingSetBytes] bigint NOT NULL''),
    (N''ProvisioningTasks'', N''[AttemptCount] int NOT NULL,[ClaimedBy] nvarchar(200) NULL,[CompletedAtUtc] datetime2(7) NULL,[CreatedAtUtc] datetime2(7) NOT NULL,[Id] bigint NOT NULL,[LeaseExpiresAtUtc] datetime2(7) NULL,[PayloadJson] nvarchar(max) NOT NULL,[ResultMessage] nvarchar(4000) NULL,[StartedAtUtc] datetime2(7) NULL,[Status] int NOT NULL,[TargetServerId] nvarchar(100) NOT NULL,[Type] int NOT NULL''),
    (N''Recipients'', N''[Address] nvarchar(300) NOT NULL,[CVSAddress] nvarchar(300) NULL,[CVSOutSide] nvarchar(10) NULL,[CVSStoreID] nvarchar(20) NULL,[CVSStoreName] nvarchar(150) NULL,[CVSTelephone] nvarchar(30) NULL,[CellPhone] nvarchar(16) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(150) NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LogisticsType] int NULL,[Name] nvarchar(150) NOT NULL,[Sex] int NOT NULL,[TelePhone] nvarchar(30) NOT NULL,[UUID] uniqueidentifier NOT NULL,[ZipCode] nvarchar(10) NULL''),
    (N''RemoteDailyAggregationRuns'', N''[AggregationVersion] int NOT NULL,[CompletedAt] datetime2(7) NOT NULL,[Id] bigint NOT NULL,[SourceRows] bigint NOT NULL,[StatisticDate] date NOT NULL''),
    (N''RemoteDailyStatistics'', N''[AggregatedAt] datetime2(7) NOT NULL,[EffectiveUniqueVisitors] bigint NOT NULL,[EffectiveViews] bigint NOT NULL,[FK_ArticleId] bigint NOT NULL,[FK_ProdId] bigint NOT NULL,[FK_TechCertId] bigint NOT NULL,[FK_WebmenuId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[LegacyViews] bigint NOT NULL,[PageViews] bigint NOT NULL,[Scope] tinyint NOT NULL,[StatisticDate] date NOT NULL,[TotalVisibleSeconds] bigint NOT NULL,[UniqueVisitors] bigint NOT NULL''),
    (N''RemoteHourlyStatistics'', N''[AggregatedAt] datetime2(7) NOT NULL,[EffectiveUniqueVisitors] bigint NOT NULL,[EffectiveViews] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[LegacyViews] bigint NOT NULL,[PageViews] bigint NOT NULL,[StatisticHour] datetime2(0) NOT NULL,[TotalVisibleSeconds] bigint NOT NULL,[UniqueVisitors] bigint NOT NULL''),
    (N''Remotes'', N''[AnalyticsAggregatedAt] datetime2(7) NULL,[BrowserInfo] nvarchar(512) NULL,[ClientIpAddress] nvarchar(64) NULL,[EngagedAt] datetime2(7) NULL,[ExecutionTime] datetime2(7) NOT NULL,[FK_ArticleId] bigint NULL,[FK_ProdId] bigint NULL,[FK_TechCertId] bigint NULL,[FK_UserId] bigint NULL,[FK_WebmenuId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[HabitsProcessedAt] datetime2(7) NULL,[HasInteraction] bit NULL,[Id] bigint NOT NULL,[IsEngaged] bit NULL,[LastHeartbeatAt] datetime2(7) NULL,[LastStatComputedAt] datetime2(7) NOT NULL,[LeaveTime] datetime2(7) NOT NULL,[State] int NOT NULL,[TimeOnPage] int NOT NULL,[TrackingEventId] uniqueidentifier NULL,[TrafficQuality] int NULL,[UUID] uniqueidentifier NOT NULL''),
    (N''Roles'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsSuperUser] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(max) NOT NULL,[Ser_No] int NOT NULL,[Type] int NOT NULL''),
    (N''SearchLogs'', N''[ClientIpAddress] nvarchar(255) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[FK_CustSearchId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[Key] nvarchar(100) NOT NULL,[UUID] uniqueidentifier NOT NULL''),
    (N''SecondaryContacts'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(150) NULL,[FK_CompanyId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[JobTitle] nvarchar(100) NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Name] nvarchar(100) NOT NULL,[Phone] nvarchar(50) NULL,[Sort] int NOT NULL''),
    (N''ShoppingCarts'', N''[AdditionalParentLabel] nvarchar(max) NULL,[AdditionalParentShoppingCartId] bigint NULL,[Bonus] int NULL,[CVSAddress] nvarchar(max) NULL,[CVSOutSide] nvarchar(max) NULL,[CVSStoreID] nvarchar(max) NULL,[CVSStoreName] nvarchar(max) NULL,[CVSTelephone] nvarchar(max) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Discont] decimal(18,2) NULL,[FK_MarketingRewardItemId] bigint NULL,[FK_PSid] bigint NOT NULL,[FK_PriceId] bigint NULL,[FK_S1id] bigint NULL,[FK_S2id] bigint NULL,[FK_Tid] uniqueidentifier NOT NULL,[FK_Uid] bigint NULL,[Id] bigint NOT NULL,[IsAdditional] bit NOT NULL,[IsDeleted] bit NOT NULL,[IsOrder] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LogisticsSubType] nvarchar(max) NULL,[OldQuantity] int NOT NULL,[Price] decimal(18,2) NOT NULL,[PriceType] int NULL,[ProdName] nvarchar(max) NULL,[ProductId] bigint NULL,[Quantity] int NOT NULL,[S1Title] nvarchar(max) NULL,[S2Title] nvarchar(max) NULL,[Ser_No] int NOT NULL,[UUID] uniqueidentifier NOT NULL''),
    (N''StoreSet'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DefaultValue] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_StoreSetGroupId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Level] int NULL,[jobID] nvarchar(max) NOT NULL,[key] nvarchar(50) NOT NULL,[maxlength] int NULL,[memo] nvarchar(max) NULL,[name] nvarchar(150) NOT NULL,[pattern] nvarchar(max) NULL,[type] int NOT NULL''),
    (N''StoreSetDetail'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_StoreSetId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[enable] bit NOT NULL,[value] nvarchar(max) NULL''),
    (N''StoreSetGroup'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[Id] bigint NOT NULL,[Image] nvarchar(150) NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Title] nvarchar(100) NOT NULL''),
    (N''StoreSetItems'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_StoreSetId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDefault] bit NOT NULL,[IsDeleted] bit NOT NULL,[Key] nvarchar(50) NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Level] int NULL,[Value] nvarchar(150) NOT NULL''),
    (N''Tag_Associates'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_AId] bigint NOT NULL,[FK_TId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Type] int NOT NULL''),
    (N''Tag_Groups'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Disp_Opt] bit NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Title] nvarchar(100) NULL''),
    (N''Tag_TagGroups'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_TGId] bigint NOT NULL,[FK_TId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL''),
    (N''Tags'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsTemporary] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[ProductIdfUpdatedAtUtc] datetime2(7) NULL,[ProductIdfWeight] float NOT NULL,[Title] nvarchar(50) NOT NULL''),
    (N''TechnicalCertificates'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(250) NULL,[Disp_opt] bit NOT NULL,[EndDate] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NOT NULL,[Id] bigint NOT NULL,[Img] nvarchar(100) NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Permanent] bit NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[Ser_no] int NOT NULL,[StartDate] datetime2(7) NULL,[Title] nvarchar(300) NULL''),
    (N''Templates'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[Enable] bit NOT NULL,[FK_ThemeId] bigint NULL,[FK_WebsiteID] bigint NOT NULL,[HeadType] int NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LayoutConfig] nvarchar(max) NOT NULL,[LayoutType] int NOT NULL,[Name] nvarchar(150) NOT NULL,[templateTypeEnum] int NOT NULL''),
    (N''TemplateSections'', N''[ContentConfig] nvarchar(max) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_TemplateID] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[sectionType] int NOT NULL''),
    (N''ThirdParties'', N''[AuditUrl] nvarchar(255) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[MaxPay] int NULL,[Memo] nvarchar(100) NULL,[PaymentUrl] nvarchar(255) NULL,[RefundUrl] nvarchar(255) NULL,[ServiceType] int NOT NULL,[Title] nvarchar(50) NOT NULL,[TokenUrl] nvarchar(255) NULL,[ser_no] int NOT NULL''),
    (N''ThirdPartyKeypairs'', N''[Code] nvarchar(50) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_TPid] bigint NOT NULL,[Id] bigint NOT NULL,[InputType] int NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[PromptText] nvarchar(50) NULL,[Title] nvarchar(50) NULL''),
    (N''ThirdPartyKeypairValues'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_ThirdPartyKeypairId] bigint NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Value] nvarchar(300) NOT NULL''),
    (N''TokenMapShoppingCarts'', N''[FK_Tid] bigint NOT NULL,[UUID] uniqueidentifier NOT NULL''),
    (N''Tokens'', N''[EndTime] datetime2(7) NULL,[PrivacyAgreeTime] datetime2(7) NULL,[StartTime] datetime2(7) NOT NULL,[UUID] uniqueidentifier NOT NULL,[UserID] bigint NULL,[id] uniqueidentifier NOT NULL,[ip] nvarchar(max) NOT NULL,[websiteId] bigint NOT NULL''),
    (N''UserActivityTags'', N''[CreateTime] datetime2(7) NOT NULL,[FK_RemoteId] bigint NOT NULL,[FK_TId] bigint NOT NULL,[Id] uniqueidentifier NOT NULL,[Weight] real NOT NULL''),
    (N''UserGroupingDetails'', N''[FK_GropingId] bigint NOT NULL,[UUID] uniqueidentifier NOT NULL''),
    (N''UserGroupings'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NOT NULL,[Enable] bit NOT NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Title] nvarchar(max) NOT NULL''),
    (N''Users'', N''[Account] nvarchar(100) NULL,[Address] nvarchar(250) NULL,[CellPhone] nvarchar(50) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Email] nvarchar(150) NULL,[ErrorTimes] int NOT NULL,[ForgeIDSendDate] datetime2(7) NULL,[ForgetID] uniqueidentifier NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Level] int NULL,[LockTime] datetime2(7) NULL,[Name] nvarchar(150) NOT NULL,[Nickname] nvarchar(50) NULL,[Password] nvarchar(max) NOT NULL,[Sex] int NULL,[Status] int NULL,[TelPhone] nvarchar(50) NULL,[Total] int NULL,[UUID] uniqueidentifier NULL,[UniformId] nvarchar(max) NULL''),
    (N''UserTagStatistics'', N''[FK_TagId] bigint NOT NULL,[Id] bigint NOT NULL,[LastActivityTime] datetime2(7) NOT NULL,[LastModificationTime] datetime2(7) NOT NULL,[TotalTimes] int NOT NULL,[UUID] uniqueidentifier NOT NULL,[Weight] float NOT NULL''),
    (N''WebMenus'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[FK_RootNodeId] bigint NULL,[FK_TopNodeId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Id] bigint NOT NULL,[ImgId] bigint NULL,[IsDeleted] bit NOT NULL,[LanBar] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LinkUrl] nvarchar(255) NULL,[OverImgId] bigint NULL,[PageText] nvarchar(max) NULL,[PageType] int NOT NULL,[Popular] int NOT NULL,[PopularVisible] bit NOT NULL,[RemovedFromShelves] bit NOT NULL,[RouterName] nvarchar(50) NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[SerNO] int NOT NULL,[ShowToMenu] bit NOT NULL,[SubTitle] nvarchar(100) NULL,[Target] bit NULL,[Title] nvarchar(100) NULL,[Visible] bit NOT NULL,[VisibleFooter] bit NOT NULL,[VisibleHeader] bit NOT NULL,[VisibleTitle] bit NOT NULL,[icon] nvarchar(50) NULL''),
    (N''WebsiteAlternateUrls'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_PlatformDomainId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Sort] int NOT NULL,[Url] nvarchar(255) NOT NULL''),
    (N''WebsiteCacheStates'', N''[CacheKey] nvarchar(250) NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Version] bigint NOT NULL''),
    (N''WebsiteEnterpriseEmails'', N''[AccountType] int NOT NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[ExpirationDate] datetime2(7) NOT NULL,[FK_CompanyId] bigint NOT NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[OtherType] nvarchar(200) NULL,[Remark] nvarchar(2000) NULL,[TotalAccounts] int NOT NULL''),
    (N''Websites'', N''[Contact] nvarchar(max) NULL,[ContactMail] nvarchar(200) NULL,[Contract] nvarchar(200) NULL,[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DefaultUrl] nvarchar(255) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[EndDate] datetime2(7) NULL,[FK_PlatformDomainId] bigint NULL,[HostLocation] nvarchar(200) NULL,[Icon] nvarchar(max) NULL,[Id] bigint NOT NULL,[IsDeleted] bit NOT NULL,[IsDomainPending] bit NOT NULL,[Keywords] nvarchar(max) NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LayoutType] int NULL,[Level] int NOT NULL,[Locale] nvarchar(10) NOT NULL,[Logo] nvarchar(max) NULL,[OrgName] nvarchar(100) NOT NULL,[PlatformRemark] nvarchar(2000) NULL,[ServiceStatus] int NOT NULL,[StartDate] datetime2(7) NULL,[Statement] nvarchar(max) NULL,[SuspendedDate] datetime2(7) NULL,[TerminatedDate] datetime2(7) NULL,[Title] nvarchar(250) NOT NULL,[Type] nvarchar(10) NOT NULL'');
-- 2026-10-24：依實際副本回報加入四張舊表；可不存在，存在時仍嚴格核對欄位。
INSERT INTO @ReviewedSchema (TableName, ColumnSpec)
SELECT legacy.TableName, legacy.ColumnSpec
FROM (VALUES
    (N''__EFMigrationsHistoryOld_20260728'', N''[MigrationId] nvarchar(150) NOT NULL,[ProductVersion] nvarchar(32) NOT NULL''),
    (N''Html_ContentsTemp'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Disp_opt] bit NOT NULL,[EndDate] datetime2(7) NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Icon] nvarchar(100) NULL,[Id] bigint NOT NULL,[Img] nvarchar(100) NULL,[IsDeleted] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[Link] nvarchar(255) NULL,[Menu_id] bigint NULL,[ObjectType] int NOT NULL,[Ser_no] int NOT NULL,[StartDate] datetime2(7) NULL,[Target] bit NOT NULL,[Title] nvarchar(300) NULL,[Type] bigint NOT NULL,[permanent] bit NOT NULL''),
    (N''Prod_Prices_temp0719'', N''[FK_PSId] bigint NOT NULL,[FK_RId] bigint NOT NULL,[Price] float NULL,[RowNum] bigint NULL,[id] bigint NOT NULL''),
    (N''regWebMenus'', N''[CreationTime] datetime2(7) NOT NULL,[CreatorUserId] bigint NOT NULL,[Css] nvarchar(max) NULL,[DeleterUserId] bigint NULL,[DeletionTime] datetime2(7) NULL,[Description] nvarchar(max) NULL,[FK_RootNodeId] bigint NULL,[FK_TopNodeId] bigint NULL,[FK_WebsiteId] bigint NOT NULL,[Html] nvarchar(max) NULL,[Id] bigint NOT NULL,[ImgId] bigint NULL,[IsDeleted] bit NOT NULL,[LanBar] bit NOT NULL,[LastModificationTime] datetime2(7) NULL,[LastModifierUserId] bigint NULL,[LinkUrl] nvarchar(255) NULL,[OverImgId] bigint NULL,[PageType] int NOT NULL,[Popular] int NOT NULL,[PopularVisible] bit NOT NULL,[RemovedFromShelves] bit NOT NULL,[RouterName] nvarchar(50) NOT NULL,[SaveCss] nvarchar(max) NULL,[SaveHtml] nvarchar(max) NULL,[SerNO] int NOT NULL,[ShowToMenu] bit NOT NULL,[Target] bit NULL,[Title] nvarchar(100) NULL,[Visible] bit NOT NULL,[VisibleFooter] bit NOT NULL,[VisibleHeader] bit NOT NULL,[VisibleTitle] bit NOT NULL,[icon] nvarchar(50) NULL'')
) legacy(TableName, ColumnSpec)
WHERE OBJECT_ID(N''dbo.'' + QUOTENAME(legacy.TableName), N''U'') IS NOT NULL;
DECLARE @ActualSchema TABLE (TableName sysname PRIMARY KEY, ColumnSpec nvarchar(max));
INSERT INTO @ActualSchema
SELECT t.name,
    STRING_AGG(CONVERT(nvarchar(max), QUOTENAME(c.name) + N'' '' + TYPE_NAME(c.user_type_id)
        + CASE
            WHEN TYPE_NAME(c.user_type_id) IN (N''nvarchar'', N''nchar'')
                THEN N''('' + CASE WHEN c.max_length = -1 THEN N''max'' ELSE CONVERT(nvarchar(10), c.max_length / 2) END + N'')''
            WHEN TYPE_NAME(c.user_type_id) IN (N''varchar'', N''char'', N''varbinary'', N''binary'')
                THEN N''('' + CASE WHEN c.max_length = -1 THEN N''max'' ELSE CONVERT(nvarchar(10), c.max_length) END + N'')''
            WHEN TYPE_NAME(c.user_type_id) IN (N''decimal'', N''numeric'')
                THEN N''('' + CONVERT(nvarchar(10), c.precision) + N'','' + CONVERT(nvarchar(10), c.scale) + N'')''
            WHEN TYPE_NAME(c.user_type_id) IN (N''datetime2'', N''time'', N''datetimeoffset'')
                THEN N''('' + CONVERT(nvarchar(10), c.scale) + N'')''
            ELSE N'''' END
        + CASE WHEN c.is_nullable = 1 THEN N'' NULL'' ELSE N'' NOT NULL'' END), N'','')
        WITHIN GROUP (ORDER BY c.name COLLATE Latin1_General_100_BIN2)
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
WHERE s.name = N''dbo'' AND t.is_ms_shipped = 0 AND t.name <> N''__EFMigrationsHistory''
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
    THROW 51000, N''資料庫結構與已審核清洗基準不同，請先審核差異並更新腳本；禁止直接略過檢查。'', 1;
END;
    -- 阻擋任何工作、歷史個資或快取殘留；存在即失敗，避免只顯示報表仍繼續交付。
    DECLARE @ResidualSql nvarchar(max);
    SELECT @ResidualSql = STRING_AGG(CONVERT(nvarchar(max),
        N''IF EXISTS (SELECT 1 FROM '' + QUOTENAME(s.name) + N''.'' + QUOTENAME(t.name)
        + N'') THROW 51000, N''''交付禁止殘留：'' + REPLACE(s.name + N''.'' + t.name, N'''''''', N'''''''''''') + N'''''', 1;''), CHAR(10))
    FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE (s.name = N''dbo'' AND t.name IN (N''UserActivityTags'',N''Remotes'',N''bonusLogDetails'',N''BonusLog'',N''Account_Logs'',N''AuditLogs'',N''SearchLogs'',N''Advertise_Logs'',N''Prod_Logs'',N''FlowSizes'',N''RemoteDailyStatistics'',N''RemoteHourlyStatistics'',N''RemoteDailyAggregationRuns'',N''ProvisioningSiteMetricSamples'',N''ProvisioningMetricSamples'',N''ProvisioningAgentStatuses'',N''ProvisioningTasks'',N''FileRecycleBinBindings'',N''FileRecycleBinItems'',N''FileCleanupCandidates'',N''FileReferences'',N''JsonObjects'',N''PageTextBackfillStates'',N''WebsiteCacheStates'',N''HtmlSanitizeStates'',N''PlatformAdministratorInvitations'',N''Tokens'',N''TokenMapShoppingCarts'',N''ThirdPartyKeypairValues'',N''Notifications'',N''BackgroundTasks'',N''MappingOldNewUUID'', N''__EFMigrationsHistoryOld_20260728'', N''Html_ContentsTemp'', N''Prod_Prices_temp0719'', N''regWebMenus''))
       OR (s.name = N''HangFire'' AND t.name <> N''Schema'');
    EXEC sys.sp_executesql @ResidualSql;
    IF EXISTS (SELECT 1 FROM dbo.Recipients WHERE Email NOT LIKE N''%@example.invalid''
                  OR ZipCode <> N''000'' OR CVSStoreID IS NOT NULL OR CVSStoreName IS NOT NULL
                  OR CVSAddress IS NOT NULL OR CVSTelephone IS NOT NULL OR CVSOutSide IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Contacts WHERE Email NOT LIKE N''%@example.invalid''
                  OR TargetEmail <> N''contact@example.invalid'')
       OR EXISTS (SELECT 1 FROM dbo.SecondaryContacts WHERE Email IS NULL OR Email NOT LIKE N''%@example.invalid''
                  OR Phone IS NOT NULL OR JobTitle IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Companies WHERE Email NOT LIKE N''%@example.invalid''
                  OR Phone IS NOT NULL OR InvoiceInfo IS NOT NULL OR SalesOwner IS NOT NULL
                  OR ContactPhone IS NOT NULL OR ContactEmail IS NOT NULL OR Remark IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Websites WHERE HostLocation IS NOT NULL OR PlatformRemark IS NOT NULL
                  OR ContactMail NOT LIKE N''%@example.invalid'')
       OR EXISTS (SELECT 1 FROM dbo.PlatformWebsites WHERE HostLocation IS NOT NULL OR Remark IS NOT NULL
                  OR Url NOT LIKE N''https://website-%.example.invalid'')
       OR EXISTS (SELECT 1 FROM dbo.PlatformDomains WHERE Registrar IS NOT NULL OR PasswordCipher IS NOT NULL
                  OR Remark IS NOT NULL OR DomainName NOT LIKE N''domain-%.example.invalid'')
       OR EXISTS (SELECT 1 FROM dbo.WebsiteAlternateUrls WHERE Url NOT LIKE N''https://alternate-%.example.invalid'')
       OR EXISTS (SELECT 1 FROM dbo.WebsiteEnterpriseEmails WHERE OtherType IS NOT NULL OR Remark IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Order_Logistics WHERE RtnMerchantTradeNo IS NOT NULL OR RtnOrderNo IS NOT NULL
                  OR CVSOutSide IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.ShoppingCarts WHERE CVSOutSide IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Users WHERE ForgetID IS NOT NULL OR ForgeIDSendDate IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.CdnProviderSyncStates WHERE LastError IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Bonus WHERE Note <> N''[已匿名化]'')
       OR EXISTS (SELECT 1 FROM dbo.StoreSetDetail d JOIN dbo.StoreSet s ON s.Id = d.FK_StoreSetId
                  WHERE s.[key] IN (N''SMTPPath'', N''SMTPPort'', N''SMTPAccount'', N''SMTPPassword'', N''GA4'', N''GTM'', N''GoogleAds'')
                    AND (d.enable <> 0 OR d.value IS NOT NULL))
        THROW 51000, N''新增敏感欄位或對外設定仍有殘留，取消交付。'', 1;
    IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class = 0
                   AND name = N''CokerDeliverySanitizationVersion''
                   AND CONVERT(nvarchar(100), value) = N''2026-10-24-v2'')
        THROW 51000, N''來源母版缺少新版清洗提交標記，請重新完成 Step 2 並備份。'', 1;
    -- 母版必須已完成新版清洗，否則禁止產生交付檔。
    IF (SELECT COUNT(*) FROM dbo.Websites WHERE Id IN (23, 36)) <> 2
       OR EXISTS (SELECT 1 FROM dbo.Websites WHERE Id NOT IN (23, 36))
       OR EXISTS (SELECT 1 FROM dbo.ProvisioningTasks)
       OR EXISTS (SELECT 1 FROM dbo.ProvisioningAgentStatuses)
       OR EXISTS (SELECT 1 FROM dbo.ProvisioningMetricSamples)
       OR EXISTS (SELECT 1 FROM dbo.ProvisioningSiteMetricSamples)
       OR EXISTS (SELECT 1 FROM dbo.PlatformAdministratorInvitations)
       OR EXISTS (SELECT 1 FROM dbo.Tokens)
       OR EXISTS (SELECT 1 FROM dbo.ThirdPartyKeypairValues)
       OR EXISTS (SELECT 1 FROM dbo.JsonObjects)
       OR EXISTS (SELECT 1 FROM dbo.Account_Logs)
       OR EXISTS (SELECT 1 FROM dbo.AuditLogs)
       OR EXISTS (SELECT 1 FROM dbo.PlatformDomains WHERE PasswordCipher IS NOT NULL OR Remark IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.Companies WHERE ContactEmail IS NOT NULL OR Remark IS NOT NULL)
       OR EXISTS (SELECT 1 FROM dbo.FrontUsers WHERE Email IS NULL OR Email NOT LIKE N''%@example.invalid'')
       OR EXISTS (SELECT 1 FROM dbo.Order_Headers WHERE OrdererEmail NOT LIKE N''%@example.invalid''
                    OR RecipientEmail NOT LIKE N''%@example.invalid'' OR DiscountBreakdownJson IS NOT NULL)
        THROW 51000, N''來源母版未通過新版匿名化檢查。'', 1;

    BEGIN TRANSACTION;

    UPDATE dbo.ShoppingCarts SET FK_Uid = @FinalUserId WHERE FK_Uid IS NOT NULL;
    DELETE FROM dbo.MappingUserAndPlatformRoles WHERE UserId <> @FinalUserId;

    UPDATE dbo.FrontUsers
    SET FK_User = CASE
        WHEN FK_User = @FinalUserId THEN FK_User
        ELSE NULL
    END;

    UPDATE dbo.Order_Headers
    SET Fk_UserId = CASE
        WHEN Fk_UserId IS NULL THEN NULL
        ELSE @FinalUserId
    END;

    DECLARE @AuditObjectId int;
    DECLARE @AuditTable nvarchar(517);
    DECLARE @AuditSet nvarchar(max);
    DECLARE @AuditSql nvarchar(max);

    DECLARE FinalAuditTableCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT
            t.object_id,
            QUOTENAME(s.name) + N''.'' + QUOTENAME(t.name)
        FROM sys.tables AS t
        INNER JOIN sys.schemas AS s
            ON s.schema_id = t.schema_id
        WHERE t.is_ms_shipped = 0
          AND s.name = N''dbo''
          AND
          (
              EXISTS
              (
                  SELECT 1
                  FROM sys.columns AS c
                  WHERE c.object_id = t.object_id
                    AND c.name = N''CreatorUserId''
              )
              OR EXISTS
              (
                  SELECT 1
                  FROM sys.columns AS c
                  WHERE c.object_id = t.object_id
                    AND c.name = N''LastModifierUserId''
              )
              OR EXISTS
              (
                  SELECT 1
                  FROM sys.columns AS c
                  WHERE c.object_id = t.object_id
                    AND c.name = N''DeleterUserId''
              )
          );

    OPEN FinalAuditTableCursor;
    FETCH NEXT FROM FinalAuditTableCursor INTO @AuditObjectId, @AuditTable;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @AuditSet = N'''';

        IF EXISTS
        (
            SELECT 1
            FROM sys.columns
            WHERE object_id = @AuditObjectId
              AND name = N''CreatorUserId''
        )
            SET @AuditSet = @AuditSet
                + CASE WHEN LEN(@AuditSet) > 0 THEN N'', '' ELSE N'''' END
                + N''[CreatorUserId] = @AuditFinalUserId'';

        IF EXISTS
        (
            SELECT 1
            FROM sys.columns
            WHERE object_id = @AuditObjectId
              AND name = N''LastModifierUserId''
        )
            SET @AuditSet = @AuditSet
                + CASE WHEN LEN(@AuditSet) > 0 THEN N'', '' ELSE N'''' END
                + N''[LastModifierUserId] = CASE WHEN [LastModifierUserId] IS NULL ''
                + N''THEN NULL ELSE @AuditFinalUserId END'';

        IF EXISTS
        (
            SELECT 1
            FROM sys.columns
            WHERE object_id = @AuditObjectId
              AND name = N''DeleterUserId''
        )
            SET @AuditSet = @AuditSet
                + CASE WHEN LEN(@AuditSet) > 0 THEN N'', '' ELSE N'''' END
                + N''[DeleterUserId] = CASE WHEN [DeleterUserId] IS NULL ''
                + N''THEN NULL ELSE @AuditFinalUserId END'';

        SET @AuditSql = N''UPDATE '' + @AuditTable + N'' SET '' + @AuditSet + N'';'';

        EXEC sys.sp_executesql
            @AuditSql,
            N''@AuditFinalUserId bigint'',
            @AuditFinalUserId = @FinalUserId;

        FETCH NEXT FROM FinalAuditTableCursor INTO @AuditObjectId, @AuditTable;
    END;

    CLOSE FinalAuditTableCursor;
    DEALLOCATE FinalAuditTableCursor;

    DELETE FROM dbo.PermissionDetail
    WHERE FK_UserId IS NOT NULL
      AND FK_UserId <> @FinalUserId;

    DELETE FROM dbo.Permissions
    WHERE FK_UserId IS NOT NULL
      AND FK_UserId <> @FinalUserId;

    DELETE FROM dbo.MappingUserAndRoles
    WHERE UserId <> @FinalUserId;

    DELETE FROM dbo.MappingUserAndWebsites
    WHERE UserId <> @FinalUserId;

    DELETE FROM dbo.Users
    WHERE Id <> @FinalUserId;

    IF (SELECT COUNT(*) FROM dbo.Users) <> 1
       OR NOT EXISTS
       (
           SELECT 1
           FROM dbo.Users
           WHERE Id = @FinalUserId
       )
    BEGIN
        RAISERROR(N''Users 最終結果不是指定的唯一帳號。'', 16, 1);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF CURSOR_STATUS(''local'', ''FinalAuditTableCursor'') >= 0
        CLOSE FinalAuditTableCursor;

    IF CURSOR_STATUS(''local'', ''FinalAuditTableCursor'') > -3
        DEALLOCATE FinalAuditTableCursor;

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @FinalizeError nvarchar(4000) = ERROR_MESSAGE();
    THROW;
END CATCH;';

        EXEC sys.sp_executesql
            @FinalizeSql,
            N'@FinalUserId bigint',
            @FinalUserId = @CurrentUserId;

        SET @CurrentBackupFile
            = @OutputDirectory
            + N'Coker6_Delivery_User_'
            + CONVERT(nvarchar(20), @CurrentUserId)
            + N'_'
            + @CurrentFileLabel
            + N'_'
            + @RunStamp
            + N'.bak';

        BACKUP DATABASE @WorkingDatabase
        TO DISK = @CurrentBackupFile
        WITH
            COPY_ONLY,
            INIT,
            COMPRESSION,
            CHECKSUM,
            STATS = 10;

        RESTORE VERIFYONLY
        FROM DISK = @CurrentBackupFile
        WITH CHECKSUM;

        INSERT INTO #Results
        (
            UserId,
            FileLabel,
            BackupFile,
            BackupResult
        )
        VALUES
        (
            @CurrentUserId,
            @CurrentFileLabel,
            @CurrentBackupFile,
            N'完成，且 RESTORE VERIFYONLY 驗證成功'
        );

        FETCH NEXT FROM DeliveryCursor INTO @CurrentUserId, @CurrentFileLabel;
    END;

    CLOSE DeliveryCursor;
    DEALLOCATE DeliveryCursor;

    IF @KeepLastWorkingDatabase = 0
       AND DB_ID(@WorkingDatabase) IS NOT NULL
    BEGIN
        SET @DropSql
            = N'ALTER DATABASE ' + QUOTENAME(@WorkingDatabase)
            + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; '
            + N'DROP DATABASE ' + QUOTENAME(@WorkingDatabase) + N';';

        EXEC sys.sp_executesql @DropSql;
    END;

    SELECT
        UserId,
        FileLabel,
        BackupFile,
        BackupResult
    FROM #Results
    ORDER BY UserId;

    PRINT N'全部交付備份均已完成。原始資料庫未被修改。';

    DROP TABLE IF EXISTS #Results;
    DROP TABLE IF EXISTS #BackupFiles;
    DROP TABLE IF EXISTS #Deliveries;
END TRY
BEGIN CATCH
    IF CURSOR_STATUS('local', 'DeliveryCursor') >= 0
        CLOSE DeliveryCursor;

    IF CURSOR_STATUS('local', 'DeliveryCursor') > -3
        DEALLOCATE DeliveryCursor;

    DECLARE @ErrorMessage nvarchar(4000) = ERROR_MESSAGE();

    SELECT
        UserId,
        FileLabel,
        BackupFile,
        BackupResult
    FROM #Results
    ORDER BY UserId;

    DROP TABLE IF EXISTS #Results;
    DROP TABLE IF EXISTS #BackupFiles;
    DROP TABLE IF EXISTS #Deliveries;

    RAISERROR
    (
        N'批次交付備份失敗。已完成的備份不受影響；工作資料庫會保留供檢查。錯誤：%s',
        16,
        1,
        @ErrorMessage
    );
END CATCH;
GO
