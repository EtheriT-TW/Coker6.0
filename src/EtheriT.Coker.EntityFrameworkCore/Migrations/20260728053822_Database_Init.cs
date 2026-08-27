using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Database_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackgroundTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_UserId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: false),
                    StorageKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActiveKey = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    HangfireJobId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SourceFilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResultFilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResultFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ResultJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpireTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bonus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Balance = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bonus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BonusLiabilities",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutstandingPoints = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusLiabilities", x => x.UUID);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComponentPurposes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SerNo = table.Column<int>(type: "int", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentPurposes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_AssocId = table.Column<long>(type: "bigint", nullable: false),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileBindMores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type = table.Column<int>(type: "int", nullable: false),
                    FK_FileBindGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_FileUploadId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileBindMores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MappingOldNewUUID",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserUUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TempUUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingOldNewUUID", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MappingWebsiteRelationship",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherId = table.Column<long>(type: "bigint", nullable: false),
                    WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingWebsiteRelationship", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_UserId = table.Column<long>(type: "bigint", nullable: false),
                    FK_BackgroundTaskId = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjectTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SerNo = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order_Logistics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_OhId = table.Column<long>(type: "bigint", nullable: false),
                    LogisticsType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogisticsSubType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantTradeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MerchantTradeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AllPayLogisticsID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookingNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSStoreID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSStoreName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSTelephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSOutSide = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSPaymentNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSValidationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoodsWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderCellPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverCellPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Temperature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogisticsStatusCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateStatusDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_Logistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageTextBackfillStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TargetMaxId = table.Column<long>(type: "bigint", nullable: false),
                    LastProcessedId = table.Column<long>(type: "bigint", nullable: false),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    ProcessedCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    RemainingNullCount = table.Column<int>(type: "int", nullable: false),
                    FailedIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageTextBackfillStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: true),
                    IsSuperUser = table.Column<bool>(type: "bit", nullable: false),
                    Ser_No = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreSetGroup",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreSetGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tag_Groups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Disp_Opt = table.Column<bool>(type: "bit", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThirdParties",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PaymentUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TokenUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RefundUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MaxPay = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ser_no = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<int>(type: "int", nullable: false),
                    Memo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdParties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    websiteId = table.Column<long>(type: "bigint", nullable: false),
                    PrivacyAgreeTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Enable = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sex = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: true),
                    Account = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CellPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TelPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Total = table.Column<int>(type: "int", nullable: true),
                    UniformId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorTimes = table.Column<int>(type: "int", nullable: false),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LockTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Websites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefaultUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    OrgName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Locale = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LayoutType = table.Column<int>(type: "int", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Statement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contract = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Css = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactMail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Websites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreSet",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    memo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: true),
                    FK_StoreSetGroupId = table.Column<long>(type: "bigint", nullable: false),
                    maxlength = table.Column<int>(type: "int", nullable: true),
                    pattern = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    jobID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreSet_StoreSetGroup_FK_StoreSetGroupId",
                        column: x => x.FK_StoreSetGroupId,
                        principalTable: "StoreSetGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    SerNo = table.Column<int>(type: "int", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Icons = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CanRefund = table.Column<bool>(type: "bit", nullable: false),
                    RefundWorkDay = table.Column<int>(type: "int", nullable: false),
                    FK_ThirdPartyId = table.Column<long>(type: "bigint", nullable: false),
                    RepayAfterMinutes = table.Column<int>(type: "int", nullable: true),
                    ThirdPartyId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTypes_ThirdParties_ThirdPartyId",
                        column: x => x.ThirdPartyId,
                        principalTable: "ThirdParties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThirdPartyKeypairs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_TPid = table.Column<long>(type: "bigint", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PromptText = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InputType = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdPartyKeypairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThirdPartyKeypairs_ThirdParties_FK_TPid",
                        column: x => x.FK_TPid,
                        principalTable: "ThirdParties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupingDetails",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_GropingId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupingDetails", x => new { x.UUID, x.FK_GropingId });
                    table.ForeignKey(
                        name: "FK_UserGroupingDetails_UserGroupings_FK_GropingId",
                        column: x => x.FK_GropingId,
                        principalTable: "UserGroupings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FrontUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Account = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CellPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TelPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OpenDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Level = table.Column<long>(type: "bigint", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Sex = table.Column<int>(type: "int", nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorTimes = table.Column<int>(type: "int", nullable: false),
                    LockTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpenID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpenIDSendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ForgetID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ForgeIDSendDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrivacyAgreeTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FK_User = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrontUsers", x => x.Id);
                    table.UniqueConstraint("AK_FrontUsers_UUID", x => x.UUID);
                    table.ForeignKey(
                        name: "FK_FrontUsers_Users_FK_User",
                        column: x => x.FK_User,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MappingUserAndRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingUserAndRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MappingUserAndRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MappingUserAndRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Account_Logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorTimes = table.Column<int>(type: "int", nullable: false),
                    LockTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Account_Logs_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Advertise",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<long>(type: "bigint", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerNO = table.Column<int>(type: "int", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    Exposure = table.Column<int>(type: "int", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Describe = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Html = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Css = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaveHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaveCss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target = table.Column<bool>(type: "bit", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Permanent = table.Column<bool>(type: "bit", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advertise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Advertise_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Article",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Subtitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Longitude = table.Column<double>(type: "float", maxLength: 50, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    SerNO = table.Column<int>(type: "int", nullable: false),
                    Popular = table.Column<int>(type: "int", nullable: false),
                    PopularVisible = table.Column<bool>(type: "bit", nullable: false),
                    RemovedFromShelves = table.Column<bool>(type: "bit", nullable: false),
                    SaveHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaveCss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Html = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Css = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewsletterHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewsletterCss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PageText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NodeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    permanent = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Article", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Article_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrowserInfo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ClientIpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CustomData = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExecutionDuration = table.Column<int>(type: "int", nullable: false),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImpersonatorTenantId = table.Column<int>(type: "int", nullable: false),
                    ImpersonatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    MethodName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    ReturnValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustSearch",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerNo = table.Column<int>(type: "int", nullable: false),
                    SearchAllProd = table.Column<bool>(type: "bit", nullable: false),
                    SearchAllArticle = table.Column<bool>(type: "bit", nullable: false),
                    SearchAllMenu = table.Column<bool>(type: "bit", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    Placeholder = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustSearch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustSearch_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileUploads",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    GuidKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DownloadFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    FileGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsEncryption = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileUploads_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlowSizes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    RequestSize = table.Column<long>(type: "bigint", nullable: false),
                    ResponseSize = table.Column<long>(type: "bigint", nullable: false),
                    Total = table.Column<long>(type: "bigint", nullable: false),
                    actionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowSizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowSizes_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Html_Contents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Menu_id = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<long>(type: "bigint", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Html = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Css = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ser_no = table.Column<int>(type: "int", nullable: false),
                    Disp_opt = table.Column<bool>(type: "bit", nullable: false),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Target = table.Column<bool>(type: "bit", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    permanent = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Html_Contents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Html_Contents_ObjectTypes_Type",
                        column: x => x.Type,
                        principalTable: "ObjectTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Html_Contents_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HtmlSanitizeStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    FK_Bid = table.Column<long>(type: "bigint", nullable: false),
                    ContentKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Default"),
                    SanitizePolicy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "PublicHtml"),
                    SanitizeVersion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HtmlSanitizeStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HtmlSanitizeStates_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JsonObjects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CacheKey = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "menu"),
                    Json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FK_AId = table.Column<long>(type: "bigint", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JsonObjects_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsBoxs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapacityPoint = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogisticsBoxs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogisticsBoxs_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PreserveType = table.Column<int>(type: "int", nullable: false),
                    LogisticsType = table.Column<int>(type: "int", nullable: false),
                    FreightType = table.Column<int>(type: "int", nullable: false),
                    FreightStatusType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DiscountFreightType = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    Freight = table.Column<int>(type: "int", nullable: true),
                    Low_Con = table.Column<int>(type: "int", nullable: true),
                    Dis_Freight = table.Column<int>(type: "int", nullable: true),
                    Set_Default = table.Column<bool>(type: "bit", nullable: false),
                    FreightAmt2 = table.Column<int>(type: "int", nullable: true),
                    SupportCashOnDelivery = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogisticsSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogisticsSettings_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MappingCompanyAndWebsites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingCompanyAndWebsites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MappingCompanyAndWebsites_Companies_FK_CompanyId",
                        column: x => x.FK_CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MappingCompanyAndWebsites_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MappingUserAndWebsites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingUserAndWebsites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MappingUserAndWebsites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MappingUserAndWebsites_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketingCampaigns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CampaignType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NeverEnd = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CanStack = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Repeatable = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingCampaigns_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Marquees",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    placement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    disp_opt = table.Column<bool>(type: "bit", nullable: false),
                    ser_no = table.Column<int>(type: "int", nullable: false),
                    link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    target = table.Column<bool>(type: "bit", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    permanent = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marquees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Marquees_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotFoundImage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    From = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotFoundImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotFoundImage_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_UserId = table.Column<long>(type: "bigint", nullable: true),
                    FK_RoleId = table.Column<long>(type: "bigint", nullable: true),
                    FK_TargetId = table.Column<long>(type: "bigint", nullable: true),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionDetail_Roles_FK_RoleId",
                        column: x => x.FK_RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PermissionDetail_Users_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PermissionDetail_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_UserId = table.Column<long>(type: "bigint", nullable: true),
                    FK_RoleId = table.Column<long>(type: "bigint", nullable: true),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Roles_FK_RoleId",
                        column: x => x.FK_RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Permissions_Users_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Permissions_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prod_Spec_Types",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prod_Spec_Types", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prod_Spec_Types_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Prods",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ser_No = table.Column<int>(type: "int", nullable: false),
                    ItemNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Introduction = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    permanent = table.Column<bool>(type: "bit", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RemovedFromShelves = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NoStockManagement = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    oStatus = table.Column<int>(type: "int", nullable: true),
                    Clicks = table.Column<int>(type: "int", nullable: true),
                    SaveHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaveCss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Html = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PageText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Css = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prods_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Recipients",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CellPhone = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TelePhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Sex = table.Column<int>(type: "int", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipients_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SearchLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FK_CustSearchId = table.Column<long>(type: "bigint", nullable: false),
                    ClientIpAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchLogs_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsTemporary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnicalCertificates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Disp_opt = table.Column<bool>(type: "bit", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Ser_no = table.Column<int>(type: "int", nullable: false),
                    Html = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    Css = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Permanent = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicalCertificates_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FK_WebsiteID = table.Column<long>(type: "bigint", nullable: false),
                    LayoutType = table.Column<int>(type: "int", nullable: false),
                    HeadType = table.Column<int>(type: "int", nullable: false),
                    templateTypeEnum = table.Column<int>(type: "int", nullable: false),
                    FK_ThemeId = table.Column<long>(type: "bigint", nullable: true),
                    LayoutConfig = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Css = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    Enable = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Templates_Websites_FK_WebsiteID",
                        column: x => x.FK_WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebMenus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    SerNO = table.Column<int>(type: "int", nullable: false),
                    Popular = table.Column<int>(type: "int", nullable: false),
                    PopularVisible = table.Column<bool>(type: "bit", nullable: false),
                    ImgId = table.Column<long>(type: "bigint", nullable: true),
                    OverImgId = table.Column<long>(type: "bigint", nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Target = table.Column<bool>(type: "bit", nullable: true),
                    LanBar = table.Column<bool>(type: "bit", nullable: false),
                    SaveHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaveCss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Html = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Css = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PageText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PageType = table.Column<int>(type: "int", nullable: false),
                    RouterName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FK_TopNodeId = table.Column<long>(type: "bigint", nullable: true),
                    FK_RootNodeId = table.Column<long>(type: "bigint", nullable: true),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    VisibleHeader = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    VisibleFooter = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    VisibleTitle = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RemovedFromShelves = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ShowToMenu = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebMenus_WebMenus_FK_RootNodeId",
                        column: x => x.FK_RootNodeId,
                        principalTable: "WebMenus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebMenus_WebMenus_FK_TopNodeId",
                        column: x => x.FK_TopNodeId,
                        principalTable: "WebMenus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WebMenus_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebsiteCacheStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    CacheKey = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteCacheStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebsiteCacheStates_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreSetDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_StoreSetId = table.Column<long>(type: "bigint", nullable: false),
                    enable = table.Column<bool>(type: "bit", nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreSetDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreSetDetail_StoreSet_FK_StoreSetId",
                        column: x => x.FK_StoreSetId,
                        principalTable: "StoreSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreSetDetail_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreSetItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FK_StoreSetId = table.Column<long>(type: "bigint", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreSetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreSetItems_StoreSet_FK_StoreSetId",
                        column: x => x.FK_StoreSetId,
                        principalTable: "StoreSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTypesValues",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_PaymentTypesId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypesValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTypesValues_PaymentTypes_FK_PaymentTypesId",
                        column: x => x.FK_PaymentTypesId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTypesValues_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThirdPartyKeypairValues",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ThirdPartyKeypairId = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdPartyKeypairValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThirdPartyKeypairValues_ThirdPartyKeypairs_FK_ThirdPartyKeypairId",
                        column: x => x.FK_ThirdPartyKeypairId,
                        principalTable: "ThirdPartyKeypairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThirdPartyKeypairValues_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BonusLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefKey = table.Column<long>(type: "bigint", nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusLog_FrontUsers_UUID",
                        column: x => x.UUID,
                        principalTable: "FrontUsers",
                        principalColumn: "UUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MappingFrontUserAndWebsite",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_UserId = table.Column<long>(type: "bigint", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingFrontUserAndWebsite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MappingFrontUserAndWebsite_FrontUsers_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "FrontUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MappingFrontUserAndWebsite_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Advertise_Logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_Adid = table.Column<long>(type: "bigint", nullable: false),
                    FK_UserId = table.Column<long>(type: "bigint", nullable: true),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advertise_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Advertise_Logs_Advertise_FK_Adid",
                        column: x => x.FK_Adid,
                        principalTable: "Advertise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileBinds",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sid = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    num = table.Column<int>(type: "int", nullable: false),
                    SerNo = table.Column<int>(type: "int", nullable: false),
                    MediaLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AreaKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    FK_FileUploadId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileBinds", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_FileBinds_FileUploads_FK_FileUploadId",
                        column: x => x.FK_FileUploadId,
                        principalTable: "FileUploads",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Directory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    FK_Mid = table.Column<long>(type: "bigint", nullable: true),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    SortBy = table.Column<int>(type: "int", nullable: false),
                    FK_DefaultLayout = table.Column<long>(type: "bigint", nullable: true),
                    FacetType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CalendarType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Directory_Html_Contents_FK_DefaultLayout",
                        column: x => x.FK_DefaultLayout,
                        principalTable: "Html_Contents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Directory_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HtmlContentPurposes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_HtmlContentId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ComponentPurposeId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HtmlContentPurposes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HtmlContentPurposes_ComponentPurposes_FK_ComponentPurposeId",
                        column: x => x.FK_ComponentPurposeId,
                        principalTable: "ComponentPurposes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HtmlContentPurposes_Html_Contents_FK_HtmlContentId",
                        column: x => x.FK_HtmlContentId,
                        principalTable: "Html_Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsBoxFees",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_LogisticsSettingId = table.Column<long>(type: "bigint", nullable: false),
                    FK_LogisticsBoxId = table.Column<long>(type: "bigint", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogisticsBoxFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogisticsBoxFees_LogisticsBoxs_FK_LogisticsBoxId",
                        column: x => x.FK_LogisticsBoxId,
                        principalTable: "LogisticsBoxs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LogisticsBoxFees_LogisticsSettings_FK_LogisticsSettingId",
                        column: x => x.FK_LogisticsSettingId,
                        principalTable: "LogisticsSettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LogisticsType_Payments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShippingType = table.Column<int>(type: "int", nullable: true),
                    FK_LogisticsSettingId = table.Column<long>(type: "bigint", nullable: true),
                    FK_PaymentTypeId = table.Column<long>(type: "bigint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    OverrideMinAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OverrideMaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogisticsType_Payments", x => x.Id);
                    table.CheckConstraint("CK_LogisticsType_Payments_RuleScope", "([ShippingType] IS NOT NULL AND [FK_LogisticsSettingId] IS NULL) OR ([ShippingType] IS NULL AND [FK_LogisticsSettingId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_LogisticsType_Payments_LogisticsSettings_FK_LogisticsSettingId",
                        column: x => x.FK_LogisticsSettingId,
                        principalTable: "LogisticsSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LogisticsType_Payments_PaymentTypes_FK_PaymentTypeId",
                        column: x => x.FK_PaymentTypeId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Order_Headers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fk_UserId = table.Column<long>(type: "bigint", nullable: true),
                    Fk_Tid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Orderer = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OrdererSex = table.Column<int>(type: "int", nullable: true),
                    OrdererEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OrdererTelePhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrdererCellPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrdererAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    OrdererZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RecipientSex = table.Column<int>(type: "int", nullable: true),
                    RecipientEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RecipientTelePhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecipientCellPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecipientAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    RecipientZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InvoiceRecipient = table.Column<int>(type: "int", nullable: true),
                    InvoiceType = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    PersonalInvoiceType = table.Column<int>(type: "int", nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UniformId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Shipping = table.Column<long>(type: "bigint", nullable: false),
                    Payment = table.Column<long>(type: "bigint", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Bonus = table.Column<int>(type: "int", nullable: true),
                    GetBonus = table.Column<int>(type: "int", nullable: true),
                    CouponId = table.Column<long>(type: "bigint", nullable: true),
                    Freight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Service_Charge = table.Column<int>(type: "int", nullable: true),
                    Memo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SystemMemo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    refundTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    refundTransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RepayTimes = table.Column<int>(type: "int", nullable: true),
                    RepayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsTemp = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_Headers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_Headers_LogisticsSettings_Shipping",
                        column: x => x.Shipping,
                        principalTable: "LogisticsSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Headers_PaymentTypes_Payment",
                        column: x => x.Payment,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketingRules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_MarketingCampaignId = table.Column<long>(type: "bigint", nullable: false),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    ScopeType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingRules_MarketingCampaigns_FK_MarketingCampaignId",
                        column: x => x.FK_MarketingCampaignId,
                        principalTable: "MarketingCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prod_Specs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_Tid = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prod_Specs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prod_Specs_Prod_Spec_Types_FK_Tid",
                        column: x => x.FK_Tid,
                        principalTable: "Prod_Spec_Types",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MappingLogisticsSettingAndProd",
                columns: table => new
                {
                    FK_LogisticsSettingId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ProdId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingLogisticsSettingAndProd", x => new { x.FK_LogisticsSettingId, x.FK_ProdId });
                    table.ForeignKey(
                        name: "FK_MappingLogisticsSettingAndProd_LogisticsSettings_FK_LogisticsSettingId",
                        column: x => x.FK_LogisticsSettingId,
                        principalTable: "LogisticsSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MappingLogisticsSettingAndProd_Prods_FK_ProdId",
                        column: x => x.FK_ProdId,
                        principalTable: "Prods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prod_Logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_Pid = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_UserId = table.Column<long>(type: "bigint", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prod_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prod_Logs_Prods_FK_Pid",
                        column: x => x.FK_Pid,
                        principalTable: "Prods",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Prod_Stocks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_Pid = table.Column<long>(type: "bigint", nullable: false),
                    FK_S1id = table.Column<long>(type: "bigint", nullable: true),
                    FK_S2id = table.Column<long>(type: "bigint", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: true),
                    Alert_Qty = table.Column<int>(type: "int", nullable: true),
                    Min_Qty = table.Column<int>(type: "int", nullable: true),
                    Ser_No = table.Column<int>(type: "int", nullable: false),
                    PackingPoint = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsTimePrice = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SubItemNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecDescription = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prod_Stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prod_Stocks_Prods_FK_Pid",
                        column: x => x.FK_Pid,
                        principalTable: "Prods",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tag_Associates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_TId = table.Column<long>(type: "bigint", nullable: false),
                    FK_AId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag_Associates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tag_Associates_Tags_FK_TId",
                        column: x => x.FK_TId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tag_TagGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_TId = table.Column<long>(type: "bigint", nullable: false),
                    FK_TGId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag_TagGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tag_TagGroups_Tag_Groups_FK_TGId",
                        column: x => x.FK_TGId,
                        principalTable: "Tag_Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tag_TagGroups_Tags_FK_TId",
                        column: x => x.FK_TId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTagStatistics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_TagId = table.Column<long>(type: "bigint", nullable: false),
                    TotalTimes = table.Column<int>(type: "int", nullable: false),
                    LastActivityTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTagStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTagStatistics_Tags_FK_TagId",
                        column: x => x.FK_TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prod_TechCerts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_PId = table.Column<long>(type: "bigint", nullable: false),
                    FK_TCId = table.Column<long>(type: "bigint", nullable: false),
                    IsChecked = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prod_TechCerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prod_TechCerts_Prods_FK_PId",
                        column: x => x.FK_PId,
                        principalTable: "Prods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prod_TechCerts_TechnicalCertificates_FK_TCId",
                        column: x => x.FK_TCId,
                        principalTable: "TechnicalCertificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateSections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_TemplateID = table.Column<long>(type: "bigint", nullable: false),
                    sectionType = table.Column<int>(type: "int", nullable: false),
                    ContentConfig = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateSections_Templates_FK_TemplateID",
                        column: x => x.FK_TemplateID,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebMenuId = table.Column<long>(type: "bigint", nullable: false),
                    Html = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TargetEmail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Reply = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReplyTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FromDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceType = table.Column<int>(type: "int", nullable: true),
                    FK_SourceId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_WebMenus_FK_WebMenuId",
                        column: x => x.FK_WebMenuId,
                        principalTable: "WebMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Remotes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_UserId = table.Column<long>(type: "bigint", nullable: true),
                    FK_WebmenuId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    FK_ProdId = table.Column<long>(type: "bigint", nullable: true),
                    FK_TechCertId = table.Column<long>(type: "bigint", nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastStatComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TimeOnPage = table.Column<int>(type: "int", nullable: false),
                    ClientIpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BrowserInfo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Remotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Remotes_Article_FK_ArticleId",
                        column: x => x.FK_ArticleId,
                        principalTable: "Article",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Remotes_Prods_FK_ProdId",
                        column: x => x.FK_ProdId,
                        principalTable: "Prods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Remotes_TechnicalCertificates_FK_TechCertId",
                        column: x => x.FK_TechCertId,
                        principalTable: "TechnicalCertificates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Remotes_Users_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Remotes_WebMenus_FK_WebmenuId",
                        column: x => x.FK_WebmenuId,
                        principalTable: "WebMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bonusLogDetails",
                columns: table => new
                {
                    FK_BonusId = table.Column<long>(type: "bigint", nullable: false),
                    FK_BonusLogsId = table.Column<long>(type: "bigint", nullable: false),
                    UsedAmount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bonusLogDetails", x => new { x.FK_BonusId, x.FK_BonusLogsId });
                    table.ForeignKey(
                        name: "FK_bonusLogDetails_BonusLog_FK_BonusLogsId",
                        column: x => x.FK_BonusLogsId,
                        principalTable: "BonusLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bonusLogDetails_Bonus_FK_BonusId",
                        column: x => x.FK_BonusId,
                        principalTable: "Bonus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DirectoryFacetRanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_DirectoryId = table.Column<long>(type: "bigint", nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    Start = table.Column<int>(type: "int", nullable: false),
                    End = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryFacetRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectoryFacetRanges_Directory_FK_DirectoryId",
                        column: x => x.FK_DirectoryId,
                        principalTable: "Directory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketingConditions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_MarketingRuleId = table.Column<long>(type: "bigint", nullable: false),
                    ConditionType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MinQuantity = table.Column<int>(type: "int", nullable: true),
                    OnlyScopeItems = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExcludeDiscountedItems = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingConditions_MarketingRules_FK_MarketingRuleId",
                        column: x => x.FK_MarketingRuleId,
                        principalTable: "MarketingRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketingRewards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_MarketingRuleId = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    MaxDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FK_CouponTemplateId = table.Column<long>(type: "bigint", nullable: true),
                    BonusAmount = table.Column<int>(type: "int", nullable: true),
                    FK_GiftProductId = table.Column<long>(type: "bigint", nullable: true),
                    FK_GiftProductStockId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingRewards_MarketingRules_FK_MarketingRuleId",
                        column: x => x.FK_MarketingRuleId,
                        principalTable: "MarketingRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketingScopeItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_MarketingRuleId = table.Column<long>(type: "bigint", nullable: false),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingScopeItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingScopeItems_MarketingRules_FK_MarketingRuleId",
                        column: x => x.FK_MarketingRuleId,
                        principalTable: "MarketingRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prod_Prices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_PSId = table.Column<long>(type: "bigint", nullable: false),
                    FK_RId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Bonus = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prod_Prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prod_Prices_Prod_Stocks_FK_PSId",
                        column: x => x.FK_PSId,
                        principalTable: "Prod_Stocks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prod_Prices_Roles_FK_RId",
                        column: x => x.FK_RId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FooterTemplates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_TemplateSectionsId = table.Column<long>(type: "bigint", nullable: false),
                    html = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    css = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    saveHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    saveCss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FooterTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FooterTemplates_TemplateSections_FK_TemplateSectionsId",
                        column: x => x.FK_TemplateSectionsId,
                        principalTable: "TemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserActivityTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_RemoteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_TId = table.Column<long>(type: "bigint", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivityTags_Remotes_FK_RemoteId",
                        column: x => x.FK_RemoteId,
                        principalTable: "Remotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCarts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_Tid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_Uid = table.Column<long>(type: "bigint", nullable: true),
                    FK_PSid = table.Column<long>(type: "bigint", nullable: false),
                    FK_PriceId = table.Column<long>(type: "bigint", nullable: true),
                    FK_S1id = table.Column<long>(type: "bigint", nullable: true),
                    FK_S2id = table.Column<long>(type: "bigint", nullable: true),
                    OldQuantity = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discont = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Bonus = table.Column<int>(type: "int", nullable: true),
                    PriceType = table.Column<int>(type: "int", nullable: true),
                    IsAdditional = table.Column<bool>(type: "bit", nullable: false),
                    Ser_No = table.Column<int>(type: "int", nullable: false),
                    IsOrder = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: true),
                    ProdName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    S1Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    S2Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogisticsSubType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSStoreID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSStoreName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSTelephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSOutSide = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_Prod_Prices_FK_PriceId",
                        column: x => x.FK_PriceId,
                        principalTable: "Prod_Prices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_Prod_Stocks_FK_PSid",
                        column: x => x.FK_PSid,
                        principalTable: "Prod_Stocks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Order_Details",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_OId = table.Column<long>(type: "bigint", nullable: false),
                    FK_SCId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_Details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_Details_Order_Headers_FK_OId",
                        column: x => x.FK_OId,
                        principalTable: "Order_Headers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Details_ShoppingCarts_FK_SCId",
                        column: x => x.FK_SCId,
                        principalTable: "ShoppingCarts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TokenMapShoppingCarts",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_Tid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenMapShoppingCarts", x => new { x.UUID, x.FK_Tid });
                    table.ForeignKey(
                        name: "FK_TokenMapShoppingCarts_ShoppingCarts_FK_Tid",
                        column: x => x.FK_Tid,
                        principalTable: "ShoppingCarts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TokenMapShoppingCarts_Tokens_UUID",
                        column: x => x.UUID,
                        principalTable: "Tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ComponentPurposes",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "Name", "SerNo", "Visible" },
                values: new object[] { 1L, "product-import-directory", new DateTime(2026, 7, 17, 0, 0, 0, 0, DateTimeKind.Local), 2L, null, null, null, null, "商品匯入目錄", 10, true });

            migrationBuilder.InsertData(
                table: "ObjectTypes",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "SerNo", "Title" },
                values: new object[,]
                {
                    { 1L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "目錄" },
                    { 2L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "廣告" },
                    { 3L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "編排樣式" },
                    { 4L, new DateTime(2023, 10, 27, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "電子報樣版" },
                    { 5L, new DateTime(2024, 7, 12, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "樣版" },
                    { 6L, new DateTime(2024, 7, 12, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "框架" },
                    { 7L, new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "廣告Banner" },
                    { 8L, new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "標題設計" },
                    { 9L, new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "按鈕設計" },
                    { 10L, new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "多欄位編排" },
                    { 11L, new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "進階" },
                    { 12L, new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "廣告(加購項目)" },
                    { 13L, new DateTime(2026, 3, 4, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "文章樣板" },
                    { 99L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "小工具" },
                    { 999L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "自訂" }
                });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "CanRefund", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "Icons", "LastModificationTime", "LastModifierUserId", "MaxAmount", "MinAmount", "RefundWorkDay", "RepayAfterMinutes", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[,]
                {
                    { 1L, false, "atm", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 1L, "pay05.jpg", null, null, null, 1m, -1, null, 1, null, "ATM", false },
                    { 2L, true, "PchomePayCARD", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "pay_08_信用卡.jpg", null, null, 199999m, 30m, 0, 10, 3, null, "信用卡付款", false },
                    { 3L, true, "PchomePayATM", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "pay05.jpg", null, null, 49999m, 1m, 3, null, 8, null, "ATM(虛擬帳戶)", false },
                    { 4L, true, "PchomePayPI", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "pay02.jpg", null, null, 199999m, 1m, 0, 10, 7, null, "PI錢包付款", false }
                });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "CanRefund", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "Icons", "IsDeleted", "LastModificationTime", "LastModifierUserId", "MaxAmount", "MinAmount", "RefundWorkDay", "RepayAfterMinutes", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[] { 5L, true, "PchomePayACCT", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "", true, null, null, null, 1m, 0, 10, 500, null, "支付連餘額付款", false });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "CanRefund", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "Icons", "LastModificationTime", "LastModifierUserId", "MaxAmount", "MinAmount", "RefundWorkDay", "RepayAfterMinutes", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[,]
                {
                    { 6L, true, "PchomePayEACH", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "pay06_支付連銀行.jpg", null, null, 49999m, 1m, 3, null, 9, null, "支付連銀行支付付款", false },
                    { 7L, false, "PCHomeIPL7", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "pay03.jpg", null, null, 20000m, 65m, 3, null, 10, null, "7-11貨到付款", false },
                    { 8L, false, "PCHomeIPLFM", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "pay04.jpg", null, null, 20000m, 65m, 3, null, 11, null, "全家貨到付款", false }
                });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "CanRefund", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "Icons", "IsDeleted", "LastModificationTime", "LastModifierUserId", "MaxAmount", "MinAmount", "RefundWorkDay", "RepayAfterMinutes", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[] { 9L, false, "PCHomeIPLOK", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "ok1_0.jpg", true, null, null, 20000m, 65m, 3, null, 500, null, "OK貨到付款", false });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "CanRefund", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "Icons", "LastModificationTime", "LastModifierUserId", "MaxAmount", "MinAmount", "RefundWorkDay", "RepayAfterMinutes", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[,]
                {
                    { 10L, false, "PCHomeIPLHL", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "life_0.jpg", null, null, 20000m, 65m, 3, null, 12, null, "萊爾富貨到付款", false },
                    { 11L, true, "PchomePayInstallment3", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "pay_08_信用卡.jpg", null, null, 199999m, 30m, 0, 10, 4, null, "線上刷卡3期分期付款", false },
                    { 12L, true, "PchomePayInstallment6", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "pay_08_信用卡.jpg", null, null, 199999m, 30m, 0, 10, 5, null, "線上刷卡6期分期付款", false },
                    { 13L, true, "PchomePayInstallment12", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "pay_08_信用卡.jpg", null, null, 199999m, 30m, 0, 10, 6, null, "線上刷卡12期分期付款", false },
                    { 14L, true, "LinePay", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, "pay01.jpg", null, null, 50000m, 1m, 0, 10, 2, null, "LINEPay", false },
                    { 15L, false, "PCHomeIBRCD", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, "pay_07.jpg", null, null, 20000m, 25m, 3, null, 13, null, "超商條碼付款", false },
                    { 16L, true, "ECPayCreditCard", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "pay_08_信用卡.jpg", null, null, 199999m, 6m, 21, 10, 14, null, "信用卡付款", false },
                    { 17L, true, "ECPayUnionPay", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "pay_08_信用卡.jpg", null, null, 199999m, 6m, 21, 10, 15, null, "信用卡付款(銀聯卡)", false },
                    { 18L, true, "ECPayCreditInstallment_3", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "pay_08_信用卡.jpg", null, null, 199999m, 6m, 21, 10, 16, null, "信用卡分期付款3期", false },
                    { 19L, true, "ECPayCreditInstallment_6", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "pay_08_信用卡.jpg", null, null, 199999m, 6m, 21, 10, 17, null, "信用卡分期付款6期", false },
                    { 20L, true, "ECPayCreditInstallment_12", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "pay_08_信用卡.jpg", null, null, 199999m, 6m, 21, 10, 18, null, "信用卡分期付款12期", false },
                    { 21L, false, "ECPayATM", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "pay05.jpg", null, null, 49999m, 17m, -1, null, 19, null, "ATM(虛擬帳戶)", false },
                    { 22L, false, "ECPayBarcode", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "pay_07.jpg", null, null, 20000m, 16m, -1, null, 20, null, "超商條碼付款", false },
                    { 23L, false, "ECPayCVS", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "", null, null, 20000m, 31m, -1, null, 21, null, "超商代碼付款", false }
                });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "CanRefund", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "Icons", "IsDeleted", "LastModificationTime", "LastModifierUserId", "MaxAmount", "MinAmount", "RefundWorkDay", "RepayAfterMinutes", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[,]
                {
                    { 24L, false, "ECPayCVS_FAMILY", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "pay04.jpg", true, null, null, 20000m, 31m, -1, null, 22, null, "超商代碼付款(全家)", false },
                    { 25L, false, "ECPayCVS_HILIFE", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "life_0.jpg", true, null, null, 20000m, 31m, -1, null, 23, null, "超商代碼付款(萊爾富)", false },
                    { 26L, false, "ECPayCVS_IBON", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "pay03.jpg", true, null, null, 20000m, 31m, -1, null, 24, null, "超商代碼付款(7-11)", false }
                });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "CanRefund", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "Icons", "LastModificationTime", "LastModifierUserId", "MaxAmount", "MinAmount", "RefundWorkDay", "RepayAfterMinutes", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[,]
                {
                    { 27L, false, "ECPayApplePay", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, "", null, null, 199999m, 6m, -1, 10, 25, null, "ApplePay", false },
                    { 28L, false, "COD", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 5L, "trans_icon.jpg", null, null, null, 1m, -1, null, 1, null, "貨到付款", false },
                    { 29L, false, "Post", new DateTime(2025, 12, 26, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, "", null, null, null, 1m, -1, null, 1, null, "郵政劃撥", false }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_WebsiteId", "IsSuperUser", "LastModificationTime", "LastModifierUserId", "Name", "Ser_No" },
                values: new object[] { 1L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, false, null, null, "系統總管理者", 0 });

            migrationBuilder.InsertData(
                table: "StoreSetGroup",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Image", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[,]
                {
                    { 1L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "/images/icon_google.png", null, null, "Google設定" },
                    { 2L, new DateTime(2024, 7, 23, 14, 26, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", null, null, "商店設定" },
                    { 3L, new DateTime(2024, 12, 5, 18, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", null, null, "信件伺服器設定" },
                    { 4L, new DateTime(2024, 12, 5, 18, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", null, null, "版型設定" },
                    { 5L, new DateTime(2025, 3, 28, 18, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", null, null, "其他設定" },
                    { 6L, new DateTime(2025, 5, 7, 17, 7, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", null, null, "紅利設定" },
                    { 7L, new DateTime(2025, 12, 16, 17, 7, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", null, null, "會員設定" }
                });

            migrationBuilder.InsertData(
                table: "ThirdParties",
                columns: new[] { "Id", "AuditUrl", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "MaxPay", "Memo", "PaymentUrl", "RefundUrl", "ServiceType", "Title", "TokenUrl", "ser_no" },
                values: new object[,]
                {
                    { 1L, null, new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, null, null, null, null, 1, "轉帳", null, 1 },
                    { 2L, null, new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, null, null, null, null, 1, "支付連", null, 500 },
                    { 3L, null, new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, null, null, null, null, 1, "LINE Pay", null, 500 },
                    { 4L, null, new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, null, "Apple pay 須再跟綠界開通服務，並請洽詢網站平台業務單位加購服務設定。", null, null, 1, "綠界支付", null, 500 },
                    { 5L, null, new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, null, null, null, null, 1, "貨到付款", null, 2 },
                    { 6L, null, new DateTime(2025, 12, 26, 15, 9, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, null, null, null, null, 1, "郵政劃撥", null, 1 },
                    { 7L, null, new DateTime(2025, 12, 26, 15, 9, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, null, null, null, null, 2, "綠界物流", null, 1 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Account", "Address", "CellPhone", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Email", "ErrorTimes", "LastModificationTime", "LastModifierUserId", "Level", "LockTime", "Name", "Nickname", "Password", "Sex", "Status", "TelPhone", "Total", "UUID", "UniformId" },
                values: new object[,]
                {
                    { 1L, "EtheriT", null, "0906801568", new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1328), 0L, null, null, "service@ether.com.tw", 0, null, null, null, null, "易碩網際科技科技股份有限公司", null, "AQAAAAEAACcQAAAAEE3X/SrNcUs6zaH9K+51XEMp8G2z3r9d/5SYuLJpKy3TlYNX7DdHF6PDW8NxWk7CWg==", null, null, null, null, null, null },
                    { 2L, "lcb", null, "0920497649", new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1338), 0L, null, null, "lienmienchou@evergreen.com.tw", 0, null, null, null, null, "隆昌窯業", null, "AQAAAAEAACcQAAAAEE3X/SrNcUs6zaH9K+51XEMp8G2z3r9d/5SYuLJpKy3TlYNX7DdHF6PDW8NxWk7CWg==", null, null, null, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Websites",
                columns: new[] { "Id", "Contact", "ContactMail", "Contract", "CreationTime", "CreatorUserId", "Css", "DefaultUrl", "DeleterUserId", "DeletionTime", "Description", "EndDate", "Icon", "Keywords", "LastModificationTime", "LastModifierUserId", "LayoutType", "Locale", "Logo", "OrgName", "StartDate", "Statement", "Title", "Type" },
                values: new object[,]
                {
                    { 1L, null, null, null, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1441), 0L, null, null, null, null, null, null, null, null, null, null, null, "zh-tw", null, "coker6", null, null, "Coker雲端開店大師", "website" },
                    { 2L, null, null, null, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1443), 0L, null, null, null, null, null, null, null, null, null, null, null, "zh-tw", null, "lcb", null, null, "｜Derek｜德瑞克．隆昌窯業", "website" }
                });

            migrationBuilder.InsertData(
                table: "LogisticsType_Payments",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_LogisticsSettingId", "FK_PaymentTypeId", "IsEnabled", "LastModificationTime", "LastModifierUserId", "OverrideMaxAmount", "OverrideMinAmount", "ShippingType" },
                values: new object[,]
                {
                    { -1017L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 17 },
                    { -1016L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 16 },
                    { -1015L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 15 },
                    { -1014L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 14 },
                    { -1013L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 13 },
                    { -1012L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 12 },
                    { -1011L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 11 },
                    { -1010L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 10 },
                    { -1009L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 9 },
                    { -1008L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 8 },
                    { -1006L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 6 },
                    { -1005L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 5 },
                    { -1004L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 4 },
                    { -1003L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 3 },
                    { -1002L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 2 },
                    { -1001L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 1 },
                    { -817L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 17 },
                    { -816L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 16 },
                    { -815L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 15 },
                    { -814L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 14 },
                    { -813L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 13 },
                    { -812L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 12 },
                    { -811L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 11 },
                    { -810L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 10 },
                    { -809L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 9 },
                    { -808L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 8 },
                    { -807L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 7 },
                    { -806L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 6 },
                    { -805L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 5 },
                    { -803L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 3 },
                    { -802L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 2 },
                    { -801L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 1 },
                    { -717L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 17 },
                    { -716L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 16 },
                    { -715L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 15 },
                    { -714L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 14 },
                    { -713L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 13 },
                    { -712L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 12 },
                    { -711L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 11 },
                    { -710L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 10 },
                    { -709L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 9 },
                    { -708L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 8 },
                    { -707L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 7 },
                    { -706L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 6 },
                    { -704L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 4 },
                    { -703L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 3 },
                    { -702L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 2 },
                    { -701L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 1 }
                });

            migrationBuilder.InsertData(
                table: "MappingUserAndRoles",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "RoleId", "UUID", "UserId" },
                values: new object[] { 1L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, 1L, new Guid("00000000-0000-0000-0000-000000000000"), 1L });

            migrationBuilder.InsertData(
                table: "MappingUserAndWebsites",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "UserId", "WebsiteId" },
                values: new object[,]
                {
                    { 1L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1457), 0L, null, null, null, null, 1L, 1L },
                    { 2L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1458), 0L, null, null, null, null, 2L, 2L }
                });

            migrationBuilder.InsertData(
                table: "Prod_Spec_Types",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_WebsiteId", "LastModificationTime", "LastModifierUserId", "Type" },
                values: new object[,]
                {
                    { 1L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 2L, null, null, "顏色" },
                    { 2L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 2L, null, null, "尺寸" },
                    { 3L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 2L, null, null, "其他" }
                });

            migrationBuilder.InsertData(
                table: "Prods",
                columns: new[] { "Id", "Clicks", "CreationTime", "CreatorUserId", "Css", "DeleterUserId", "DeletionTime", "Description", "Discount", "EndTime", "FK_WebsiteId", "Html", "Introduction", "ItemNo", "LastModificationTime", "LastModifierUserId", "NoStockManagement", "PageText", "SaveCss", "SaveHtml", "Ser_No", "StartTime", "Title", "Visible", "oStatus", "permanent" },
                values: new object[,]
                {
                    { 1L, null, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, "奈米單體馬桶 W384 x D685 x H470mm\n直熱式微電腦馬桶座\n噴嘴紫外線殺菌\n獨立水壓系統\n腳觸設計\nEasy Touch開閉蓋技術\n第二代微波感應技術", null, null, 2L, null, "從座圈到噴嘴給您雙重防護\n不用動手全自動科技最體貼\n雙漩洗技術為您實現真乾淨", null, null, null, false, null, null, null, 500, null, "DE-R1073 德瑞克直熱式微電腦馬桶座／遙控型", true, null, true },
                    { 2L, null, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, "商品二的第一行說明\n商品二的第二行說明", null, null, 2L, null, "商品二的第一行介紹\n商品二的第二行介紹", null, null, null, false, null, null, null, 500, null, "C659NA 德瑞克Smart III淨未來智慧馬桶", true, null, true },
                    { 3L, null, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, "商品三的第一行說明\n商品二的第二行說明", null, null, 2L, null, "商品三的第一行介紹\n商品二的第二行介紹", null, null, null, false, null, null, null, 500, null, "L602 檯上三角盆", true, null, true },
                    { 4L, null, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, "L183NA檯上奈米方型盆W560 x D380 x H120mm\n1033PH四角型單孔單槍加高面盆龍頭歐洲省水二段Ø35短腳陶瓷心軸(附歐規按押無溢水排桿)", null, null, 2L, null, "最大容水量：11公升\n適用水壓：1~5kgf/㎝²", null, null, null, false, null, null, null, 500, null, "L183NA 檯上奈米方型盆", true, null, true }
                });

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DefaultValue", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[,]
                {
                    { 1L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 1L, null, null, null, "S001", "GA4", 12, "請輸入GOOGLE提供之驗證碼：G-xxxxxxxxxx", "Google Analytics(4)", "^G-\\w+", 1 },
                    { 2L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 1L, null, null, null, "S002", "google.translate", 50, "請選擇需要翻譯的語系（請洽詢客服加購功能）", "Google自動翻譯", "(?=[a-z]{2}-?[A-Z]{0,2},?)+", 4 },
                    { 3L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 2L, null, null, null, "E001", "storeBuyState", 50, "請選擇購物形式", "商品販售設定", "", 5 },
                    { 4L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 2L, null, null, 3, "E001", "storeMemo", 300, "可以輸入一段話，在結帳的時候對客戶做一些小提醒。", "結帳備註", "", 2 },
                    { 5L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 1L, null, null, null, "S001", "GTM", 12, "請輸入GOOGLE提供之驗證碼：GTM-xxxxxxx", "Google Tag Manager", "^GTM-\\w+", 1 },
                    { 6L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 2L, null, null, null, "E001", "linkMore", 255, "輸入一段連結，在商品頁中可以顯示了解更多按鈕。", "了解更多", "", 1 },
                    { 7L, new DateTime(2024, 11, 12, 11, 59, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 2L, null, null, 3, "E001", "prodCatalog", 255, "輸入商品目錄連結，可設定前台購物車(我要再選購)之按鈕。", "商品目錄", "", 1 },
                    { 8L, new DateTime(2024, 11, 12, 11, 59, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 7L, null, null, 2, "E001", "membershipTerms", 5000, "請輸入會員條款內文", "會員條款", "", 2 },
                    { 9L, new DateTime(2024, 12, 5, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 3L, null, null, null, "S001", "SMTPPath", 255, "請輸入SMTP Server", "SMTP Server", "", 1 },
                    { 10L, new DateTime(2024, 12, 5, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 3L, null, null, null, "S001", "SMTPPort", 5, "請輸入Port", "Port", "", 8 },
                    { 11L, new DateTime(2024, 12, 5, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 3L, null, null, null, "S001", "SMTPAccount", 100, "請輸入 SMTP 帳號；若不是 Email，系統將使用客服信箱作為寄件人", "帳號", "", 1 },
                    { 12L, new DateTime(2024, 12, 5, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 3L, null, null, null, "S001", "SMTPPassword", 50, "請輸入 密碼", "密碼", "", 10 },
                    { 13L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 1L, null, null, null, "S001", "GoogleAds", 14, "需先埋入GA，再輸入GOOGLE提供之轉換 ID：AW-xxxxxxxxxxx", "Google Ads", "^AW-\\w+", 1 },
                    { 14L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 5L, null, null, null, "Y001", "NoCopy", 14, "右鍵鎖定，文字圖片禁止圈選", "鎖右鍵", "", 4 },
                    { 15L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 6L, null, null, null, "B001", "SignupBonusPoints", 8, "加入會員贈送紅利點數", "迎新禮", "", 8 },
                    { 16L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 6L, null, null, null, "B001", "MinOrderForRedemption", 6, "單筆訂單消費滿足多少可使用紅利扣抵金額", "紅利扣抵條件", "", 8 },
                    { 17L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 6L, null, null, null, "B001", "MaxRedemptionPercent", 2, "單筆訂單抵扣%數上限", "最高抵扣%", "", 8 },
                    { 18L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 6L, null, null, null, "B001", "MinOrderForEarnPoints", 8, "消費滿額多少金額贈送紅利回饋金", "消費條件", "", 8 },
                    { 19L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 6L, null, null, null, "B001", "RewardRatePercent", 2, "消費滿足條件贈送幾%紅利回饋金", "獲得%數紅利", "", 8 },
                    { 20L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 6L, null, null, null, "B001", "RewardPointsExpireDays", 3, "每一筆紅利的有效天數，如無須限制可不輸入", "有效天數", "", 8 },
                    { 21L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 2L, null, null, null, "B001", "priceOrder", 3, "商品預設顯示的金額", "價格顯示順序", "", 5 },
                    { 22L, new DateTime(2024, 12, 5, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 3L, null, null, null, "S001", "EmailNotificationType", 50, "請選擇信件寄送方式", "信件寄送方式", "", 3 },
                    { 23L, new DateTime(2025, 12, 16, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, null, null, null, "M001", "MemberRegister", null, "是否開放註冊，若關閉註冊僅可在會員清單新增。", "開放註冊", "", 5 },
                    { 24L, new DateTime(2025, 12, 16, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 2L, null, null, null, "M001", "HasInvoice", null, "網站是否需要開立發票", "開立發票", "", 5 },
                    { 25L, new DateTime(2025, 12, 16, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 2L, null, null, null, "M001", "ExtraInviiceCarrier", null, "允許用戶使用發票載具類型。", "發票載具", "", 4 },
                    { 26L, new DateTime(2025, 12, 22, 14, 9, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, "### 關於電子郵件資料的使用說明\r\n\r\n尊敬的用戶，感謝您使用我們的服務。在使用第三方登入（如 Line 登入）時，我們會從您的帳號中取得您所提供的電子郵件地址。以下為我們使用電子郵件資料的說明：\r\n\r\n**1. 身分驗證**\r\n我們會使用您的電子郵件來確認您的身分，確保您在本平台上的登入狀態及安全性。當您使用電子郵件進行登入或註冊時，這些資料將會用於身分確認。\r\n\r\n**2. 忘記密碼通知信**\r\n當您忘記密碼並請求重設時，我們會將重設密碼的通知與相關說明寄送至您註冊時所提供的電子郵件地址，以協助您找回帳號的使用權限。\r\n\r\n**3. 購物通知信**\r\n在您進行購物時，若有訂單處理進度、商品出貨等相關狀況，我們會使用您的電子郵件地址向您發送通知，以便您隨時掌握購物狀態。\r\n\r\n**4. 付款成功通知信**\r\n當您完成付款後，系統將寄送付款成功通知信至您的電子郵件，以利您確認交易是否成功並保存交易紀錄。\r\n\r\n**5. 客服聯繫**\r\n當您與客服團隊聯繫時，我們會透過電子郵件回覆您的問題、提供協助，並處理相關客戶服務事宜。\r\n\r\n**隱私與資料保護說明**\r\n我們將妥善保護您的電子郵件資料，不會將其提供給第三方，除非基於法律要求或經您同意。您的電子郵件資料僅會用於上述用途，並依據隱私政策進行保護。\r\n\r\n如您對上述內容有任何疑問，歡迎隨時與我們聯繫。", null, null, 7L, null, null, null, "E001", "PrivacyPolicy", 5000, "請輸入隱私聲明內文(本區塊支援 Markdown 標記語法，### 表示標題，**文字** 表示字粗體)", "隱私聲明", "", 2 },
                    { 27L, new DateTime(2026, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 6L, null, null, null, "B001", "BonusEnabled", 0, "是否啟用紅利功能", "紅利功能啟用", "", 4 },
                    { 28L, new DateTime(2026, 7, 8, 10, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 6L, null, null, null, "B001", "MaximumDiscount", 8, "單筆訂單紅利抵扣上限", "最高折抵上限", "", 8 },
                    { 29L, new DateTime(2026, 7, 17, 11, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 2L, null, null, null, "E001", "ProductPageLayout", null, "設定商品頁面的顯示版型", "商品頁版型", "", 3 }
                });

            migrationBuilder.InsertData(
                table: "ThirdPartyKeypairs",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_TPid", "InputType", "LastModificationTime", "LastModifierUserId", "PromptText", "Title" },
                values: new object[,]
                {
                    { 1L, "bankNo", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 1L, 1, null, null, null, "匯款銀行代號" },
                    { 2L, "account", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 1L, 1, null, null, null, "匯款帳號" },
                    { 3L, "shopID", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 1L, 1, null, null, null, "戶名" },
                    { 4L, "account", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, 1, null, null, null, "PchomePayAppId" },
                    { 5L, "code1", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, 1, null, null, null, "PchomePaySecre" },
                    { 6L, "expire_days", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, 1, null, null, "※預設為5天，最短1天，最長可設定為5天，超過一律以5天計算", "允許繳費有效天數" },
                    { 7L, "account", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, 1, null, null, null, "Channel ID" },
                    { 8L, "code1", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, 1, null, null, null, "Channel Secret Key" },
                    { 9L, "MerchantID", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, 1, null, null, null, "商店代號" },
                    { 10L, "PlatformID", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, 1, null, null, "※非專案合作請留空", "平台代號" },
                    { 11L, "HashKey", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, 1, null, null, null, "HashKey" },
                    { 12L, "HashIV", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, 1, null, null, null, "HashIV" },
                    { 13L, "ExpireDate", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, 1, null, null, "※預設為3天，最短1天，最長可設定為60天，超過一律以60天計算", "ATM允許繳費有效天數" },
                    { 14L, "StoreExpireDate_Barcode", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, 1, null, null, "※預設為7天，最短1天，最長可設定為30天，超過一律以30天計算", "超商條碼繳費截止時間" },
                    { 15L, "StoreExpireDate_CVS", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, 1, null, null, "※預設為7天，最短1天，最長可設定為30天，超過一律以30天計算", "超商代碼繳費截止時間" },
                    { 16L, "PostAccount", new DateTime(2025, 12, 26, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, 1, null, null, null, "郵局帳號" },
                    { 17L, "PostName", new DateTime(2025, 12, 26, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, 1, null, null, null, "郵局戶名" },
                    { 18L, "MerchantID", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 1, null, null, null, "商店代號" },
                    { 19L, "PlatformID", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 1, null, null, "※非專案合作請留空", "平台代號" },
                    { 20L, "HashKey", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 1, null, null, null, "HashKey" },
                    { 21L, "HashIV", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 1, null, null, null, "HashIV" },
                    { 22L, "EnableB2C", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 2, null, null, "如需啟用請記得至綠界後台測標", "是否啟用大宗寄倉" },
                    { 23L, "EnableC2C", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 2, null, null, null, "是否啟用超商門市寄/取件" }
                });

            migrationBuilder.InsertData(
                table: "ThirdPartyKeypairs",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_TPid", "InputType", "IsDeleted", "LastModificationTime", "LastModifierUserId", "PromptText", "Title" },
                values: new object[] { 24L, "IsCollection", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 2, true, null, null, null, "是否代收貨款" });

            migrationBuilder.InsertData(
                table: "ThirdPartyKeypairs",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_TPid", "InputType", "LastModificationTime", "LastModifierUserId", "PromptText", "Title" },
                values: new object[] { 25L, "EnableHomeDelivery", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 7L, 2, null, null, null, "是否啟用宅配" });

            migrationBuilder.InsertData(
                table: "Prod_Specs",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_Tid", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[,]
                {
                    { 1L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1461), 2L, null, null, 1L, null, null, "白色" },
                    { 2L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1462), 2L, null, null, 1L, null, null, "灰色" },
                    { 3L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1463), 2L, null, null, 1L, null, null, "黑色" },
                    { 4L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1464), 2L, null, null, 2L, null, null, "小" },
                    { 5L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1465), 2L, null, null, 2L, null, null, "中" },
                    { 6L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466), 2L, null, null, 2L, null, null, "大" },
                    { 7L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466), 2L, null, null, 3L, null, null, "整組" },
                    { 8L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466), 2L, null, null, 3L, null, null, "L183NA 檯上奈米方型盆" },
                    { 9L, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466), 2L, null, null, 3L, null, null, "1033PH 四角型單孔單槍加高面盆龍頭" }
                });

            migrationBuilder.InsertData(
                table: "Prod_Stocks",
                columns: new[] { "Id", "Alert_Qty", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_Pid", "FK_S1id", "FK_S2id", "LastModificationTime", "LastModifierUserId", "Min_Qty", "Price", "Ser_No", "SpecDescription", "Stock", "SubItemNo" },
                values: new object[,]
                {
                    { 1L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 1L, 1L, 4L, null, null, 1, 30000m, 500, null, 100, null },
                    { 2L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 1L, 2L, 4L, null, null, 1, 28000m, 500, null, 100, null },
                    { 3L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 1L, 2L, 5L, null, null, 1, 28500m, 500, null, 100, null },
                    { 4L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 2L, 1L, 4L, null, null, 1, 9500m, 500, null, 100, null },
                    { 5L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 3L, 1L, 4L, null, null, 1, 13000m, 500, null, 100, null },
                    { 6L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 4L, 7L, 0L, null, null, 1, 24300m, 500, null, 100, null },
                    { 7L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 4L, 8L, 0L, null, null, 1, 9500m, 500, null, 100, null },
                    { 8L, 5, new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459), 2L, null, null, 4L, 9L, 0L, null, null, 1, 14800m, 500, null, 100, null }
                });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "IsDefault", "Key", "LastModificationTime", "LastModifierUserId", "Level", "Value" },
                values: new object[,]
                {
                    { 1L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, "zh-TW", null, null, null, "中文(繁體)" },
                    { 2L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, "zh-CN", null, null, null, "中文(簡體)" },
                    { 3L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, "en", null, null, null, "英文" },
                    { 4L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, "ja", null, null, null, "日文" },
                    { 5L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, false, "noPayNoShow", null, null, null, "不開放購物且不顯示商品售價" },
                    { 6L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, false, "noPay", null, null, null, "不開放購物但顯示商品售價" },
                    { 7L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, false, "menberPay", null, null, 3, "限制僅會員購物" },
                    { 8L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, false, "Pay", null, null, 3, "開放購物" },
                    { 9L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 21L, false, "HtoL", null, null, 1, "由高至低" },
                    { 10L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 21L, false, "LtoH", null, null, 1, "由低至高" },
                    { 11L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 22L, false, "0", null, null, null, "寄送完整表單" },
                    { 12L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 22L, false, "1", null, null, null, "簡易通知" },
                    { 13L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 23L, true, "1", null, null, null, "開放註冊" },
                    { 14L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 23L, false, "3", null, null, null, "關閉註冊" },
                    { 15L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 24L, false, "EnabledInvoice", null, null, null, "是" },
                    { 16L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 24L, false, "DisabledInvoice", null, null, null, "否" },
                    { 17L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 25L, false, "MobileCarrier", null, null, null, "手機載具" },
                    { 18L, new DateTime(2026, 7, 17, 11, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 29L, true, "Layout_1", null, null, null, "版型一" },
                    { 19L, new DateTime(2026, 7, 17, 11, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 29L, false, "Layout_2", null, null, null, "版型二" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_Logs_WebsiteId",
                table: "Account_Logs",
                column: "WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertise_FK_WebsiteId",
                table: "Advertise",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertise_Logs_FK_Adid",
                table: "Advertise_Logs",
                column: "FK_Adid");

            migrationBuilder.CreateIndex(
                name: "IX_Article_EndTime",
                table: "Article",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_Article_FK_WebsiteId",
                table: "Article",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Article_NodeDate",
                table: "Article",
                column: "NodeDate");

            migrationBuilder.CreateIndex(
                name: "IX_Article_permanent",
                table: "Article",
                column: "permanent");

            migrationBuilder.CreateIndex(
                name: "IX_Article_RemovedFromShelves",
                table: "Article",
                column: "RemovedFromShelves");

            migrationBuilder.CreateIndex(
                name: "IX_Article_SerNO",
                table: "Article",
                column: "SerNO");

            migrationBuilder.CreateIndex(
                name: "IX_Article_StartTime",
                table: "Article",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Article_Title",
                table: "Article",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Article_Visible",
                table: "Article",
                column: "Visible");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_FK_WebsiteId",
                table: "AuditLogs",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundTasks_ActiveKey",
                table: "BackgroundTasks",
                column: "ActiveKey",
                unique: true,
                filter: "[ActiveKey] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundTasks_ExpireTime",
                table: "BackgroundTasks",
                column: "ExpireTime");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundTasks_FK_WebsiteId_FK_UserId_Status",
                table: "BackgroundTasks",
                columns: new[] { "FK_WebsiteId", "FK_UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundTasks_StorageKey",
                table: "BackgroundTasks",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BonusLog_UUID",
                table: "BonusLog",
                column: "UUID");

            migrationBuilder.CreateIndex(
                name: "IX_bonusLogDetails_FK_BonusLogsId",
                table: "bonusLogDetails",
                column: "FK_BonusLogsId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentPurposes_Code",
                table: "ComponentPurposes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_FK_WebMenuId",
                table: "Contacts",
                column: "FK_WebMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_CustSearch_FK_WebsiteId",
                table: "CustSearch",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Directory_FacetType",
                table: "Directory",
                column: "FacetType");

            migrationBuilder.CreateIndex(
                name: "IX_Directory_FK_DefaultLayout",
                table: "Directory",
                column: "FK_DefaultLayout");

            migrationBuilder.CreateIndex(
                name: "IX_Directory_FK_WebsiteId",
                table: "Directory",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryFacetRanges_FK_DirectoryId",
                table: "DirectoryFacetRanges",
                column: "FK_DirectoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FileBinds_FK_FileUploadId",
                table: "FileBinds",
                column: "FK_FileUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_FK_WebsiteId",
                table: "FileUploads",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowSizes_actionTime",
                table: "FlowSizes",
                column: "actionTime");

            migrationBuilder.CreateIndex(
                name: "IX_FlowSizes_FK_WebsiteId",
                table: "FlowSizes",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_FooterTemplates_FK_TemplateSectionsId",
                table: "FooterTemplates",
                column: "FK_TemplateSectionsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FrontUsers_FK_User",
                table: "FrontUsers",
                column: "FK_User");

            migrationBuilder.CreateIndex(
                name: "IX_FrontUsers_UUID_IsDeleted",
                table: "FrontUsers",
                columns: new[] { "UUID", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Html_Contents_FK_WebsiteId",
                table: "Html_Contents",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Html_Contents_Type",
                table: "Html_Contents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_HtmlContentPurposes_FK_ComponentPurposeId",
                table: "HtmlContentPurposes",
                column: "FK_ComponentPurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_HtmlContentPurposes_FK_HtmlContentId_FK_ComponentPurposeId",
                table: "HtmlContentPurposes",
                columns: new[] { "FK_HtmlContentId", "FK_ComponentPurposeId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HtmlSanitizeStates_FK_WebsiteId_SourceType_FK_Bid_ContentKey_SanitizePolicy",
                table: "HtmlSanitizeStates",
                columns: new[] { "FK_WebsiteId", "SourceType", "FK_Bid", "ContentKey", "SanitizePolicy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JsonObjects_FK_WebsiteId_CacheKey_FK_AId",
                table: "JsonObjects",
                columns: new[] { "FK_WebsiteId", "CacheKey", "FK_AId" },
                unique: true,
                filter: "[FK_AId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsBoxFees_FK_LogisticsBoxId_FK_LogisticsSettingId",
                table: "LogisticsBoxFees",
                columns: new[] { "FK_LogisticsBoxId", "FK_LogisticsSettingId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsBoxFees_FK_LogisticsSettingId",
                table: "LogisticsBoxFees",
                column: "FK_LogisticsSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsBoxs_FK_WebsiteId_CapacityPoint",
                table: "LogisticsBoxs",
                columns: new[] { "FK_WebsiteId", "CapacityPoint" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsSettings_FK_WebsiteId",
                table: "LogisticsSettings",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsType_Payments_FK_LogisticsSettingId_FK_PaymentTypeId",
                table: "LogisticsType_Payments",
                columns: new[] { "FK_LogisticsSettingId", "FK_PaymentTypeId" },
                unique: true,
                filter: "[FK_LogisticsSettingId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsType_Payments_FK_PaymentTypeId",
                table: "LogisticsType_Payments",
                column: "FK_PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsType_Payments_ShippingType_FK_PaymentTypeId",
                table: "LogisticsType_Payments",
                columns: new[] { "ShippingType", "FK_PaymentTypeId" },
                unique: true,
                filter: "[FK_LogisticsSettingId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MappingCompanyAndWebsites_FK_CompanyId",
                table: "MappingCompanyAndWebsites",
                column: "FK_CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingCompanyAndWebsites_FK_WebsiteId",
                table: "MappingCompanyAndWebsites",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingFrontUserAndWebsite_FK_UserId",
                table: "MappingFrontUserAndWebsite",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingFrontUserAndWebsite_FK_WebsiteId",
                table: "MappingFrontUserAndWebsite",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingLogisticsSettingAndProd_FK_ProdId",
                table: "MappingLogisticsSettingAndProd",
                column: "FK_ProdId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingUserAndRoles_RoleId",
                table: "MappingUserAndRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingUserAndRoles_UserId",
                table: "MappingUserAndRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingUserAndWebsites_UserId",
                table: "MappingUserAndWebsites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingUserAndWebsites_WebsiteId",
                table: "MappingUserAndWebsites",
                column: "WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_FK_WebsiteId_StartTime_EndTime",
                table: "MarketingCampaigns",
                columns: new[] { "FK_WebsiteId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_FK_WebsiteId_Status",
                table: "MarketingCampaigns",
                columns: new[] { "FK_WebsiteId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingConditions_FK_MarketingRuleId",
                table: "MarketingConditions",
                column: "FK_MarketingRuleId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingRewards_FK_MarketingRuleId",
                table: "MarketingRewards",
                column: "FK_MarketingRuleId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingRules_FK_MarketingCampaignId_Enabled",
                table: "MarketingRules",
                columns: new[] { "FK_MarketingCampaignId", "Enabled" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingScopeItems_FK_MarketingRuleId_TargetType_TargetId",
                table: "MarketingScopeItems",
                columns: new[] { "FK_MarketingRuleId", "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_Marquees_FK_WebsiteId",
                table: "Marquees",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_NotFoundImage_FK_WebsiteId",
                table: "NotFoundImage",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_FK_BackgroundTaskId",
                table: "Notifications",
                column: "FK_BackgroundTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_FK_WebsiteId_FK_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "FK_WebsiteId", "FK_UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Order_Details_FK_OId",
                table: "Order_Details",
                column: "FK_OId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Details_FK_SCId",
                table: "Order_Details",
                column: "FK_SCId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Headers_Payment",
                table: "Order_Headers",
                column: "Payment");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Headers_Shipping",
                table: "Order_Headers",
                column: "Shipping");

            migrationBuilder.CreateIndex(
                name: "IX_PageTextBackfillStates_FK_WebsiteId_ContentType",
                table: "PageTextBackfillStates",
                columns: new[] { "FK_WebsiteId", "ContentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageTextBackfillStates_Status_LastModificationTime",
                table: "PageTextBackfillStates",
                columns: new[] { "Status", "LastModificationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypes_ThirdPartyId",
                table: "PaymentTypes",
                column: "ThirdPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypesValues_FK_PaymentTypesId",
                table: "PaymentTypesValues",
                column: "FK_PaymentTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypesValues_FK_WebsiteId",
                table: "PaymentTypesValues",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDetail_FK_RoleId",
                table: "PermissionDetail",
                column: "FK_RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDetail_FK_UserId",
                table: "PermissionDetail",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDetail_FK_WebsiteId",
                table: "PermissionDetail",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_FK_RoleId",
                table: "Permissions",
                column: "FK_RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_FK_UserId",
                table: "Permissions",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_FK_WebsiteId",
                table: "Permissions",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Logs_FK_Pid",
                table: "Prod_Logs",
                column: "FK_Pid");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Prices_FK_PSId",
                table: "Prod_Prices",
                column: "FK_PSId");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Prices_FK_RId",
                table: "Prod_Prices",
                column: "FK_RId");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Spec_Types_FK_WebsiteId",
                table: "Prod_Spec_Types",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Specs_FK_Tid",
                table: "Prod_Specs",
                column: "FK_Tid");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Stocks_FK_Pid",
                table: "Prod_Stocks",
                column: "FK_Pid");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_TechCerts_FK_PId",
                table: "Prod_TechCerts",
                column: "FK_PId");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_TechCerts_FK_TCId",
                table: "Prod_TechCerts",
                column: "FK_TCId");

            migrationBuilder.CreateIndex(
                name: "IX_Prods_FK_WebsiteId",
                table: "Prods",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Prods_Title",
                table: "Prods",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Recipients_FK_WebsiteId",
                table: "Recipients",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_ExecutionTime",
                table: "Remotes",
                column: "ExecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_ArticleId",
                table: "Remotes",
                column: "FK_ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_ProdId",
                table: "Remotes",
                column: "FK_ProdId");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_TechCertId",
                table: "Remotes",
                column: "FK_TechCertId");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_UserId",
                table: "Remotes",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_WebmenuId",
                table: "Remotes",
                column: "FK_WebmenuId");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_WebsiteId",
                table: "Remotes",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_State",
                table: "Remotes",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_UUID",
                table: "Remotes",
                column: "UUID");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_FK_WebsiteId",
                table: "SearchLogs",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_FK_PriceId",
                table: "ShoppingCarts",
                column: "FK_PriceId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_FK_PSid",
                table: "ShoppingCarts",
                column: "FK_PSid");

            migrationBuilder.CreateIndex(
                name: "IX_StoreSet_FK_StoreSetGroupId",
                table: "StoreSet",
                column: "FK_StoreSetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreSetDetail_FK_StoreSetId",
                table: "StoreSetDetail",
                column: "FK_StoreSetId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreSetDetail_FK_WebsiteId",
                table: "StoreSetDetail",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreSetItems_FK_StoreSetId",
                table: "StoreSetItems",
                column: "FK_StoreSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_Associates_FK_TId",
                table: "Tag_Associates",
                column: "FK_TId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_TagGroups_FK_TGId",
                table: "Tag_TagGroups",
                column: "FK_TGId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_TagGroups_FK_TId",
                table: "Tag_TagGroups",
                column: "FK_TId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_FK_WebsiteId",
                table: "Tags",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Title_FK_WebsiteId",
                table: "Tags",
                columns: new[] { "Title", "FK_WebsiteId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalCertificates_FK_WebsiteId",
                table: "TechnicalCertificates",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_FK_WebsiteID",
                table: "Templates",
                column: "FK_WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSections_FK_TemplateID",
                table: "TemplateSections",
                column: "FK_TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartyKeypairs_FK_TPid",
                table: "ThirdPartyKeypairs",
                column: "FK_TPid");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartyKeypairValues_FK_ThirdPartyKeypairId",
                table: "ThirdPartyKeypairValues",
                column: "FK_ThirdPartyKeypairId");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartyKeypairValues_FK_WebsiteId",
                table: "ThirdPartyKeypairValues",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenMapShoppingCarts_FK_Tid",
                table: "TokenMapShoppingCarts",
                column: "FK_Tid");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityTags_FK_RemoteId",
                table: "UserActivityTags",
                column: "FK_RemoteId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupingDetails_FK_GropingId",
                table: "UserGroupingDetails",
                column: "FK_GropingId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTagStatistics_FK_TagId",
                table: "UserTagStatistics",
                column: "FK_TagId");

            migrationBuilder.CreateIndex(
                name: "IX_WebMenus_FK_RootNodeId",
                table: "WebMenus",
                column: "FK_RootNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WebMenus_FK_TopNodeId",
                table: "WebMenus",
                column: "FK_TopNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WebMenus_FK_WebsiteId",
                table: "WebMenus",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_WebMenus_Title",
                table: "WebMenus",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteCacheStates_FK_WebsiteId_CacheKey",
                table: "WebsiteCacheStates",
                columns: new[] { "FK_WebsiteId", "CacheKey" },
                unique: true);

            migrationBuilder.Sql(
                """
                IF FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = N'CokerSearchCatalog')
                        CREATE FULLTEXT CATALOG [CokerSearchCatalog] AS DEFAULT;

                    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[WebMenus]'))
                        CREATE FULLTEXT INDEX ON [dbo].[WebMenus]
                        (
                            [Title] LANGUAGE 1028,
                            [Description] LANGUAGE 1028,
                            [PageText] LANGUAGE 1028
                        ) KEY INDEX [PK_WebMenus] ON [CokerSearchCatalog] WITH CHANGE_TRACKING AUTO;

                    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[Article]'))
                        CREATE FULLTEXT INDEX ON [dbo].[Article]
                        (
                            [Title] LANGUAGE 1028,
                            [Description] LANGUAGE 1028,
                            [PageText] LANGUAGE 1028
                        ) KEY INDEX [PK_Article] ON [CokerSearchCatalog] WITH CHANGE_TRACKING AUTO;

                    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[Prods]'))
                        CREATE FULLTEXT INDEX ON [dbo].[Prods]
                        (
                            [Title] LANGUAGE 1028,
                            [ItemNo] LANGUAGE 0,
                            [Introduction] LANGUAGE 1028,
                            [Description] LANGUAGE 1028,
                            [PageText] LANGUAGE 1028
                        ) KEY INDEX [PK_Prods] ON [CokerSearchCatalog] WITH CHANGE_TRACKING AUTO;
                END
                """, suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[Prods]'))
                    DROP FULLTEXT INDEX ON [dbo].[Prods];
                IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[Article]'))
                    DROP FULLTEXT INDEX ON [dbo].[Article];
                IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'[dbo].[WebMenus]'))
                    DROP FULLTEXT INDEX ON [dbo].[WebMenus];
                """, suppressTransaction: true);

            migrationBuilder.DropTable(
                name: "Account_Logs");

            migrationBuilder.DropTable(
                name: "Advertise_Logs");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BackgroundTasks");

            migrationBuilder.DropTable(
                name: "BonusLiabilities");

            migrationBuilder.DropTable(
                name: "bonusLogDetails");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "CustSearch");

            migrationBuilder.DropTable(
                name: "DirectoryFacetRanges");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "FileBindMores");

            migrationBuilder.DropTable(
                name: "FileBinds");

            migrationBuilder.DropTable(
                name: "FlowSizes");

            migrationBuilder.DropTable(
                name: "FooterTemplates");

            migrationBuilder.DropTable(
                name: "HtmlContentPurposes");

            migrationBuilder.DropTable(
                name: "HtmlSanitizeStates");

            migrationBuilder.DropTable(
                name: "JsonObjects");

            migrationBuilder.DropTable(
                name: "LogisticsBoxFees");

            migrationBuilder.DropTable(
                name: "LogisticsType_Payments");

            migrationBuilder.DropTable(
                name: "MappingCompanyAndWebsites");

            migrationBuilder.DropTable(
                name: "MappingFrontUserAndWebsite");

            migrationBuilder.DropTable(
                name: "MappingLogisticsSettingAndProd");

            migrationBuilder.DropTable(
                name: "MappingOldNewUUID");

            migrationBuilder.DropTable(
                name: "MappingUserAndRoles");

            migrationBuilder.DropTable(
                name: "MappingUserAndWebsites");

            migrationBuilder.DropTable(
                name: "MappingWebsiteRelationship");

            migrationBuilder.DropTable(
                name: "MarketingConditions");

            migrationBuilder.DropTable(
                name: "MarketingRewards");

            migrationBuilder.DropTable(
                name: "MarketingScopeItems");

            migrationBuilder.DropTable(
                name: "Marquees");

            migrationBuilder.DropTable(
                name: "NotFoundImage");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Order_Details");

            migrationBuilder.DropTable(
                name: "Order_Logistics");

            migrationBuilder.DropTable(
                name: "PageTextBackfillStates");

            migrationBuilder.DropTable(
                name: "PaymentTypesValues");

            migrationBuilder.DropTable(
                name: "PermissionDetail");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Prod_Logs");

            migrationBuilder.DropTable(
                name: "Prod_Specs");

            migrationBuilder.DropTable(
                name: "Prod_TechCerts");

            migrationBuilder.DropTable(
                name: "Recipients");

            migrationBuilder.DropTable(
                name: "SearchLogs");

            migrationBuilder.DropTable(
                name: "StoreSetDetail");

            migrationBuilder.DropTable(
                name: "StoreSetItems");

            migrationBuilder.DropTable(
                name: "Tag_Associates");

            migrationBuilder.DropTable(
                name: "Tag_TagGroups");

            migrationBuilder.DropTable(
                name: "ThirdPartyKeypairValues");

            migrationBuilder.DropTable(
                name: "TokenMapShoppingCarts");

            migrationBuilder.DropTable(
                name: "UserActivityTags");

            migrationBuilder.DropTable(
                name: "UserGroupingDetails");

            migrationBuilder.DropTable(
                name: "UserTagStatistics");

            migrationBuilder.DropTable(
                name: "WebsiteCacheStates");

            migrationBuilder.DropTable(
                name: "Advertise");

            migrationBuilder.DropTable(
                name: "BonusLog");

            migrationBuilder.DropTable(
                name: "Bonus");

            migrationBuilder.DropTable(
                name: "Directory");

            migrationBuilder.DropTable(
                name: "FileUploads");

            migrationBuilder.DropTable(
                name: "TemplateSections");

            migrationBuilder.DropTable(
                name: "ComponentPurposes");

            migrationBuilder.DropTable(
                name: "LogisticsBoxs");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "MarketingRules");

            migrationBuilder.DropTable(
                name: "Order_Headers");

            migrationBuilder.DropTable(
                name: "Prod_Spec_Types");

            migrationBuilder.DropTable(
                name: "StoreSet");

            migrationBuilder.DropTable(
                name: "Tag_Groups");

            migrationBuilder.DropTable(
                name: "ThirdPartyKeypairs");

            migrationBuilder.DropTable(
                name: "ShoppingCarts");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "Remotes");

            migrationBuilder.DropTable(
                name: "UserGroupings");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "FrontUsers");

            migrationBuilder.DropTable(
                name: "Html_Contents");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "MarketingCampaigns");

            migrationBuilder.DropTable(
                name: "LogisticsSettings");

            migrationBuilder.DropTable(
                name: "PaymentTypes");

            migrationBuilder.DropTable(
                name: "StoreSetGroup");

            migrationBuilder.DropTable(
                name: "Prod_Prices");

            migrationBuilder.DropTable(
                name: "Article");

            migrationBuilder.DropTable(
                name: "TechnicalCertificates");

            migrationBuilder.DropTable(
                name: "WebMenus");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ObjectTypes");

            migrationBuilder.DropTable(
                name: "ThirdParties");

            migrationBuilder.DropTable(
                name: "Prod_Stocks");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Prods");

            migrationBuilder.DropTable(
                name: "Websites");
        }
    }
}
