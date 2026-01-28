using DevExpress.CodeParser;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Common;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Bonus;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.Dto.MailTemplate;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.BonusManagement
{
    public class BonusManagementAppService : IBonusManagementAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IStoreSetAppService _storeSetAppService;
        private readonly IMailTemplateAppService _mailTemplateAppService;
        private readonly MailAppService _mailAppService;
        private readonly IConfiguration _configuration;

        public BonusManagementAppService(IStoreSetAppService storeSetAppService,
                                         IMailTemplateAppService mailTemplateAppService,
                                         MailAppService mailAppService,
                                         IConfiguration configuration,
                                         LoginUserData loginUserData,
                                         CokerDbContext db)
        {
            _storeSetAppService = storeSetAppService;
            _mailTemplateAppService = mailTemplateAppService;
            _mailAppService = mailAppService;
            _configuration = configuration;
            this.loginUserData = loginUserData;
            this.db = db;
        }

        public async Task<GetBonusSettingForEditOutput> GetBonusSettingForEdit()
        {
            long websiteID = await loginUserData.GetWebsiteId();
            if (websiteID == 0) websiteID = loginUserData.GetFrontWebsiteId();
            var bonusSettingData = await _storeSetAppService.getValues(new Shared.Dto.StoreSet.StoreSetGetValueInput
            {
                SiteId = websiteID,
                StoreSetGroupId = 6
            });

            if (bonusSettingData?.Success == true)
            {
                return new GetBonusSettingForEditOutput
                {
                    SiteId = websiteID,
                    SignupBonusPoints = this.GetBonusSettingValue<int>(bonusSettingData, nameof(GetBonusSettingForEditOutput.SignupBonusPoints)),
                    MinOrderForRedemption = this.GetBonusSettingValue<decimal>(bonusSettingData, nameof(GetBonusSettingForEditOutput.MinOrderForRedemption)),
                    MaxRedemptionPercent = this.GetBonusSettingValue<decimal>(bonusSettingData, nameof(GetBonusSettingForEditOutput.MaxRedemptionPercent)),
                    MinOrderForEarnPoints = this.GetBonusSettingValue<decimal>(bonusSettingData, nameof(GetBonusSettingForEditOutput.MinOrderForEarnPoints)),
                    RewardRatePercent = this.GetBonusSettingValue<decimal>(bonusSettingData, nameof(GetBonusSettingForEditOutput.RewardRatePercent)),
                    RewardPointsExpireDays = this.GetBonusSettingValue<int>(bonusSettingData, nameof(GetBonusSettingForEditOutput.RewardPointsExpireDays)),
                };
            }

            return new GetBonusSettingForEditOutput();
        }

        public async Task<GetBonusSettingHelpTextForEditOutput> GetBonusSettingHelpTextForEdit()
        {
            long websiteID = await loginUserData.GetWebsiteId();
            var bonusSettingItem = await _storeSetAppService.getAll(new List<long> { 6 });
            if (bonusSettingItem?.Success == true)
            {
                return new GetBonusSettingHelpTextForEditOutput
                {
                    SignupBonusPointsHelpText = bonusSettingItem.storeGroups?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.SignupBonusPoints))?.memo,
                    MinOrderForRedemptionHelpText = bonusSettingItem.storeGroups?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.MinOrderForRedemption))?.memo,
                    MaxRedemptionPercentHelpText = bonusSettingItem.storeGroups?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.MaxRedemptionPercent))?.memo,
                    MinOrderForEarnPointsHelpText = bonusSettingItem.storeGroups?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.MinOrderForEarnPoints))?.memo,
                    RewardRatePercentHelpText = bonusSettingItem.storeGroups?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.RewardRatePercent))?.memo,
                    RewardPointsExpireDaysHelpText = bonusSettingItem.storeGroups?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.RewardPointsExpireDays))?.memo
                };
            }

            return new GetBonusSettingHelpTextForEditOutput();
        }

        public async Task<JsonResult> GetFrontUsers(DataSourceLoadOptions loadOptions, bool isShowCurrentMonthBirthdayOnly)
        {
            long websideId = await loginUserData.GetWebsiteId();
            var dataQuery = from user in db.FrontUsers
                            join site in db.MappingFrontUserAndWebsite on user.Id equals site.FK_UserId
                            where site.FK_WebsiteId == websideId &&
                                  user.IsDeleted == false
                            select user;

            if (isShowCurrentMonthBirthdayOnly)
            {
                DateTime currentDate = DateTime.Now;
                dataQuery = dataQuery.Where(x => x.Birthday.HasValue &&
                                                 x.Birthday.Value.Month == currentDate.Month);
            }

            var result = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);

            if (result != null && result.data != null)
            {
                result.data = (result.data as IEnumerable<FrontUser>)?.Select(x => new GetFrontUserDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    UUID = x.UUID,
                    Birthday = x.Birthday.HasValue ? x.Birthday.Value.ToString("yyyy/MM/dd") : null
                })?.ToList() ?? new List<GetFrontUserDto>();
            }

            return new JsonResult(result, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }

        public async Task<ResponseMessageDto> SaveSetting(CreateOrUpdateSettingsDto input)
        {
            ResponseMessageDto result = new ResponseMessageDto { Success = false };
            long websiteID = await loginUserData.GetWebsiteId();
            try
            {
                var createOrUpdateDto = new List<StoreSetDetailOutputDto>();
                var properties = typeof(CreateOrUpdateSettingsDto).GetProperties();
                foreach (var property in properties)
                {
                    //使用自己的Service來更新或插入資料
                    //var value = property.GetValue(input)?.ToString() ?? string.Empty;
                    //UpdateOrInsertStoreSetDetail(websiteID, property.Name, value);

                    //使用 StoreSetAppService 來更新或插入資料
                    createOrUpdateDto.Add(new StoreSetDetailOutputDto
                    {
                        key = property.Name,
                        value = new List<string> { property.GetValue(input)?.ToString() ?? string.Empty }
                    });

                }
                result = await _storeSetAppService.CreateOrUpdate(createOrUpdateDto);
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 儲存前端使用者紅利異動
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ResponseMessageDto> SaveTransaction(CreateUserTransactionDto input)
        {
            ResponseMessageDto result = new ResponseMessageDto { Success = false };
            var targetUuids = input.MemberUUID.ToList();
            if (input.EnableIdempotencyByRefKey && input.RefKey.HasValue && targetUuids.Any())
            {
                var exists = await db.BonusLog.Where(x =>
                    targetUuids.Contains(x.UUID) &&
                    x.RefKey == input.RefKey.Value &&
                    x.Type == input.Type).Select(x => x.UUID).ToListAsync();

                // 冪等：已做過就不重做
                if (exists.Any())
                {
                    var existedSet = exists.ToHashSet();
                    targetUuids = targetUuids.Where(u => !existedSet.Contains(u)).ToList();
                }

                if (!targetUuids.Any()){
                    result.Success = true;
                    return result; 
                }
            }
            long websiteID = await loginUserData.GetWebsiteId();

            if (!targetUuids.Any())
            {
                result.Error = "請至少選擇一位使用者進行紅利異動。";
                return result;
            }

            if (string.IsNullOrEmpty(input.TransactionOperation))
            {
                result.Error = "請選擇紅利異動操作類型。";
                return result;
            }

            if (input.TransactionPoint == 0)
            {
                result.Error = "紅利異動點數不能為零。";
                return result;
            }

            if (string.IsNullOrEmpty(input.TransactionReason))
            {
                result.Error = "請輸入紅利異動原因。";
                return result;
            }

            try
            {
                var currentDate = DateTime.Now;

                // 整批查詢所有相關用戶，避免重複查詢
                var allFrontUsers = await db.FrontUsers
                    .Where(x => targetUuids.Contains(x.UUID) && !x.IsDeleted)
                    .ToListAsync();

                // 如果是扣除紅利，先檢核所有人的紅利是否足夠
                if (input.TransactionOperation == "-")
                {
                    var deductAmount = Math.Abs(input.TransactionPoint);
                    var insufficientUsers = new List<string>();

                    // 一次查詢所有相關用戶的可用紅利
                    var availableBonusData = await GetQueryFrontUsersTotalAvaliableBonus(targetUuids);
                    
                    foreach (var frontUser in allFrontUsers)
                    {
                        var userBonus = availableBonusData.FirstOrDefault(x => x.UserUUID == frontUser.UUID);
                        var availableBalance = userBonus?.TotalAvaliableBonus ?? 0;

                        if (availableBalance < deductAmount)
                        {
                            insufficientUsers.Add($"{frontUser.Name}(可用:{availableBalance},需要:{deductAmount})");
                        }
                    }

                    // 如果有任何用戶紅利不足，直接返回錯誤
                    if (insufficientUsers.Any())
                    {
                        result.Error = $"以下用戶紅利不足，無法執行扣除操作：{string.Join(", ", insufficientUsers)}";
                        return result;
                    }
                }

                var createUserId = await loginUserData.GetUserId();
                foreach (var memberUuid in targetUuids)
                {
                    var frontUser = allFrontUsers.FirstOrDefault(x => x.UUID == memberUuid);

                    if (frontUser == null)
                        continue;

                    // 根據 TransactionOperation 判斷操作類型
                    if (input.TransactionOperation == "+")
                    {
                        // 取得紅利有效天數設定
                        var bonusSettings = await GetBonusSettingForEdit();
                        var expireDays = bonusSettings.RewardPointsExpireDays ?? null; // 預設null

                        // 新增紅利
                        var addAmount = Math.Abs(input.TransactionPoint);
                        AddBonusPoints(addAmount, expireDays, input.TransactionReason, frontUser, createUserId,input.RefKey,input.Type);
                    }
                    else if (input.TransactionOperation == "-")
                    {
                        // 扣除紅利
                        var deductAmount = Math.Abs(input.TransactionPoint);
                        DeductBonusPoints(deductAmount, input.TransactionReason, frontUser, createUserId,input.RefKey, input.Type);
                    }
                }

                await db.SaveChangesAsync();
                result.Success = true;

                if (input.IsSendMail)
                {
                    // 發送紅利異動通知郵件給所有前端使用者
                    this.SendTransactionMailToFrontUser(allFrontUsers, input);
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }


        private async void SendTransactionMailToFrontUser(List<FrontUser> frontUsers, CreateUserTransactionDto transactionInput)
        {
            try
            {
                var bonusSettings = GetBonusSettingForEdit().Result;
                var expireDays = bonusSettings.RewardPointsExpireDays;
                var rewardPointsExpireDateTime = expireDays.HasValue ? DateTime.Now.AddDays(expireDays.Value) : (DateTime?)null;

                var currentDate = DateTime.Now;
                // 一次查詢所有相關用戶的可用紅利
                var availableBonusData = db.Bonus
                    .Where(x => transactionInput.MemberUUID.Contains(x.UUID) &&
                               x.Balance > 0 &&
                               x.StartDate <= currentDate &&
                               (x.EndDate == null || x.EndDate >= currentDate) &&
                               !x.IsDeleted)
                    .GroupBy(x => x.UUID)
                    .Select(g => new { UUID = g.Key, TotalBalance = g.Sum(b => b.Balance) })
                    .ToList();

                long websiteID = _configuration.GetValue<long>("WebConfig:SiteId") == 0 ? loginUserData.GetWebsiteId().Result : _configuration.GetValue<long>("WebConfig:SiteId");
                var website = db.Websites.Where(e => e.Id == websiteID).FirstOrDefault();

                List<MailTemplateInputDto> mailTemplatesInput = new List<MailTemplateInputDto>();
                foreach (var item in frontUsers)
                {
                    mailTemplatesInput.Add(new MailTemplateInputDto
                    {
                        Key = item.UUID.ToString(),
                        Model = new TransactionMailTemplateModelDto
                        {
                            MemberName = item.Name,
                            MemberId = item.Id,
                            TransactionOperation = transactionInput.TransactionOperation,
                            TransactionOperationName = transactionInput.TransactionOperation == "+" ? "新增" : "扣除",
                            TransactionPoint = transactionInput.TransactionPoint,
                            TransactionDescription = transactionInput.TransactionReason,
                            RewardPointsExpireDateTime = rewardPointsExpireDateTime ?? DateTime.Now.AddDays(0),
                            TransactionDateTime = DateTime.Now,
                            TransactionBalance = availableBonusData.FirstOrDefault(x => x.UUID == item.UUID)?.TotalBalance ?? 0,
                            WebSiteName = website?.OrgName ?? string.Empty,
                            WebSiteUrl = website?.DefaultUrl ?? string.Empty
                        }
                    });
                }

                var mailContent = await _mailTemplateAppService.GetTemplateRenderAsync(MailTemplateTypeEnum.紅利異動, mailTemplatesInput);
                List<ResponseMessageDto> sendMailResponse = new List<ResponseMessageDto>();

                //一封一封寄送
                //List<SenderDto> senderDtos = new List<SenderDto>();
                //foreach (var item in frontUsers.Where(x => !string.IsNullOrEmpty(x.Email?.Trim())))
                //{
                //    senderDtos.Add(new SenderDto
                //    {
                //        Recipients = new List<MailUserDataDto>()
                //                    {
                //                        new MailUserDataDto()
                //                        {
                //                            Name = item.Name,
                //                            Email = item.Email ?? string.Empty,
                //                        }
                //                    },
                //        Subject = $"【紅利通知】您的紅利點數已異動（{transactionInput.TransactionOperation}{transactionInput.TransactionPoint} 點）",
                //        Body = mailContent.FirstOrDefault(x => x.Key == item.UUID.ToString())?.Body ?? string.Empty,
                //        Css = string.Empty,
                //    });
                //}

                //foreach (var item in senderDtos)
                //{
                //    sendMailResponse.Add(_mailAppService.sendMail(item, website?.Contact).Result);
                //}

                // BCC整批寄送
                SenderDto senderDto = new SenderDto
                {
                    Bcc = frontUsers.Where(x => !string.IsNullOrEmpty(x.Email?.Trim()))
                                    .Select(x => new MailUserDataDto { Email = x.Email ?? "" })
                                    .ToList(),
                    Subject = $"【紅利通知】您的紅利點數已異動（{transactionInput.TransactionOperation}{transactionInput.TransactionPoint} 點）",
                    Body = mailContent.FirstOrDefault()?.Body ?? string.Empty,
                };
                sendMailResponse.Add(_mailAppService.sendMail(senderDto, website?.Contact).Result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 取得紅利異動紀錄列表
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetBonusLogForDataGrid(DataSourceLoadOptions loadOptions)
        {
            long websideId = await loginUserData.GetWebsiteId();
            var dataQuery = from user in db.FrontUsers
                            join site in db.MappingFrontUserAndWebsite on user.Id equals site.FK_UserId
                            join bonusLog in db.BonusLog on user.UUID equals bonusLog.User.UUID
                            where site.FK_WebsiteId == websideId &&
                                  user.IsDeleted == false
                            select new GetBonusLogForDataGridDto
                            {
                                Id = user.Id,
                                Name = user.Name,
                                UUID = user.UUID,
                                Amount = bonusLog.Amount,
                                ExecutionTime = bonusLog.ExecutionTime,
                                Note = bonusLog.Note
                            };

            var result = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);

            if (result != null && result.data != null)
            {
                result.data = (result.data as IEnumerable<GetBonusLogForDataGridDto>)?.ToList() ?? new List<GetBonusLogForDataGridDto>();
            }

            return new JsonResult(result, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }

        /// <summary>
        /// 取得前端使用者紅利資訊
        /// </summary>
        /// <param name="frontUsers"></param>
        /// <returns></returns>
        public async Task<List<GetQueryFrontUsersTotalAvaliableBonusOutput>> GetQueryFrontUsersTotalAvaliableBonus(List<Guid> frontUsersUUID)
        {
            var currentDate = DateTime.Now;

            if (!frontUsersUUID.Any())
            {
                return new List<GetQueryFrontUsersTotalAvaliableBonusOutput>();
            }

            // 查詢所有使用者的有效紅利
            var availableBonusData = await db.Bonus
                .Where(b =>
                    frontUsersUUID.Contains(b.UUID)
                    && b.Balance > 0
                    && b.StartDate <= currentDate
                    && (b.EndDate == null || b.EndDate >= currentDate)
                    && !b.IsDeleted
                    && b.Status == BonusStatusEnum.Active)
                .GroupBy(b => b.UUID)
                .Select(g => new
                {
                    UUID = g.Key,
                    SumBalance = g.Sum(x => (int?)x.Balance) ?? 0,
                    Outstanding = db.BonusLiabilities
                        .Where(bl => bl.UUID == g.Key)
                        .Select(bl => (int?)bl.OutstandingPoints)
                        .Sum() ?? 0
                })
                .Select(x => new GetQueryFrontUsersTotalAvaliableBonusOutput
                {
                    UserUUID = x.UUID,
                    TotalAvaliableBonus = x.SumBalance - x.Outstanding
                })
                .ToListAsync();


            // 補齊沒有紅利的使用者（紅利為0或沒有紅利異動過的）
            var existed = availableBonusData.Select(x => x.UserUUID).ToHashSet();
            var missingUsers = frontUsersUUID
                .Where(uuid => !existed.Contains(uuid))
                .Select(uuid => new GetQueryFrontUsersTotalAvaliableBonusOutput
                {
                    UserUUID = uuid,
                    TotalAvaliableBonus = 0
                });

            return availableBonusData.Concat(missingUsers).ToList();
        }

        /// <summary>
        /// 取得前端使用者紅利異動紀錄
        /// </summary>
        /// <param name="frontUserUUID"></param>
        /// <param name="topRecordCount"></param>
        /// <returns></returns>
        public async Task<List<GetQueryFrontUsersBonusLogOutput>> GetQueryFrontUsersBonusLog(Guid frontUserUUID, int topRecordCount)
        {
            List<GetQueryFrontUsersBonusLogOutput> result = new List<GetQueryFrontUsersBonusLogOutput>();
            if (frontUserUUID != Guid.Empty)
            {
                var bonusQuery = from bonus in db.Bonus
                                 join user in db.FrontUsers on bonus.UUID equals user.UUID
                                 where user.UUID == frontUserUUID && !user.IsDeleted
                                 select new GetQueryFrontUsersBonusLogOutput
                                 {
                                     Amount = bonus.Amount,
                                     Note = bonus.Note,
                                     ExecuteTime = bonus.CreationTime,
                                     ExpireTime = bonus.EndDate
                                 };

                var bonusLogQuery = from bonusLog in db.BonusLog
                                    join user in db.FrontUsers on bonusLog.UUID equals user.UUID
                                    where user.UUID == frontUserUUID && !user.IsDeleted
                                    select new GetQueryFrontUsersBonusLogOutput
                                    {
                                        Amount = bonusLog.Amount,
                                        Note = bonusLog.Note,
                                        ExecuteTime = bonusLog.ExecutionTime,
                                        ExpireTime = null
                                    };

                // 將兩個查詢結果合併，並按執行時間降序排序後取前10筆
                var bonusResults = bonusQuery.OrderByDescending(x => x.ExecuteTime)
                                                   .Take(topRecordCount)
                                                   .ToList();
                var bonusLogResults = bonusLogQuery.OrderByDescending(x => x.ExecuteTime)
                                                         .Take(topRecordCount)
                                                         .ToList();

                result = bonusResults.Concat(bonusLogResults)
                                     .OrderByDescending(x => x.ExecuteTime)
                                     .Take(topRecordCount)
                                     .ToList();
            }

            return result;
        }

        /// <summary>
        /// 取得紅利設定的值，並嘗試將其轉換為指定的數值型別。
        /// </summary>
        /// <typeparam name="T">目標數值型別，必須為值型別 (struct)。</typeparam>
        /// <param name="bonusSettingData">包含紅利設定的資料物件。</param>
        /// <param name="settingKey">紅利設定的鍵值，用於查找對應的設定值。</param>
        /// <returns>轉換後的數值型別值，若轉換失敗或找不到對應的設定值，則回傳 null。</returns>
        /// <remarks>
        /// 此方法會根據指定的型別 (T) 嘗試進行型別轉換，目前支援 int、float、bool、DateTime、enum。
        /// 若轉換過程中發生例外，會捕捉並忽略該例外，回傳 null。
        /// ★★★string 型別請使用 GetBonusSettingValueAsString 方法。
        /// </remarks>
        private T? GetBonusSettingValue<T>(StoreSetResponseMessageDto bonusSettingData, string settingKey) where T : struct
        {
            var value = bonusSettingData.storeSetDetails?
                .FirstOrDefault(x => x.key == settingKey)?
                .value?.FirstOrDefault();

            if (value == null)
            {
                return null;
            }

            try
            {
                if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(value, out int intValue))
                    {
                        return (T?)(object)intValue;
                    }
                }
                else if (typeof(T) == typeof(decimal))
                {
                    if (decimal.TryParse(value, out decimal decimalValue))
                    {
                        return (T?)(object)decimalValue;
                    }
                }
            }
            catch
            {
                // Log or handle conversion error if necessary
            }

            return null;
        }

        /// <summary>
        /// 取得紅利設定的值，並以字串型別回傳。
        /// </summary>
        /// <param name="bonusSettingData">包含紅利設定的資料物件。</param>
        /// <param name="settingKey">紅利設定的鍵值，用於查找對應的設定值。</param>
        /// <returns>對應的設定值，若找不到設定值則回傳 null。</returns>
        /// <remarks>
        /// 此方法專門用於取得字串型別的紅利設定值，若設定值不存在或為 null，則回傳 null。
        /// </remarks>
        private string? GetBonusSettingValueAsString(StoreSetResponseMessageDto bonusSettingData, string settingKey)
        {
            var value = bonusSettingData.storeSetDetails?
                .FirstOrDefault(x => x.key == settingKey)?
                .value?.FirstOrDefault();

            return value;
        }


        private bool UpdateOrInsertStoreSetDetail(long websiteID, string key, string value)
        {
            try
            {
                long userId = loginUserData.GetUserId().Result;
                var updateRow = db.StoreSetDetail.FirstOrDefault(x => x.FK_WebsiteId == websiteID && x.StoreSet.key == key);
                if (updateRow != null)
                {
                    updateRow.value = value;
                    updateRow.LastModifierUserId = userId;
                    updateRow.LastModificationTime = DateTime.Now;
                    db.StoreSetDetail.Update(updateRow);
                }
                else
                {
                    if (userId > 0)
                    {
                        var newRow = new StoreSetDetail
                        {
                            CreatorUserId = userId,
                            FK_WebsiteId = websiteID,
                            StoreSet = db.StoreSet.First(x => x.key == key),
                            value = value
                        };
                        db.StoreSetDetail.Add(newRow);
                    }
                    else
                    {
                        throw new Exception("使用者未登入");
                    }
                }
                return db.SaveChanges() > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 新增紅利點數
        /// </summary>
        /// <param name="points">紅利點數</param>
        /// <param name="reason">異動原因</param>
        /// <param name="frontUser">前台使用者</param>
        /// <param name="RefKey">對應的資料Id，如訂單id</param>
        private void AddBonusPoints(int points, int? expireDays, string? reason, FrontUser frontUser, long createUserId, long? RefKey, BonusLogTypeEnum Type)
        {
            var currentDate = DateTime.Now;

            var liability = db.BonusLiabilities.FirstOrDefault(x => x.UUID == frontUser.UUID);
            if (liability != null && liability.OutstandingPoints > 0)
            {
                var payback = Math.Min(points, liability.OutstandingPoints);
                liability.OutstandingPoints -= payback;
                liability.UpdatedAt = currentDate;

                points -= payback; // 剩下的點數才進 Bonus
                db.BonusLiabilities.Update(liability);

                if (points <= 0)
                {
                    // 這次新增點數全部用來還欠點，不需要建立 Bonus，只記一筆 log（或視你要不要記）
                    db.BonusLog.Add(new BonusLog
                    {
                        Amount = 0, // 或記原始點數但加註已沖欠點
                        Note = $"{reason ?? "紅利入帳"}（先沖欠點 {payback} 點）",
                        User = frontUser,
                        ExecutionTime = currentDate,
                        RefKey = RefKey,
                        Type = Type
                    });
                    return;
                }
            }


            // 建立新的紅利記錄
            var bonus = new Bonus
            {
                UUID =  frontUser.UUID,
                Amount = points,
                Balance = points,
                StartDate = currentDate,
                EndDate = expireDays != null ? currentDate.AddDays(expireDays.Value) : DateTime.Parse("2099/12/31 23:59:59"),
                Note = reason ?? "系統新增紅利",
                CreatorUserId = createUserId,
                CreationTime = currentDate,
            };

            // 建立紅利異動記錄
            var bonusLog = new BonusLog
            {
                Amount = points,
                Note = reason ?? "系統新增紅利",
                User = frontUser,
                ExecutionTime = currentDate,
                RefKey = RefKey,
                Type = Type
            };

            db.Bonus.Add(bonus);
            db.BonusLog.Add(bonusLog);
        }

        /// <summary>
        /// 扣除紅利點數
        /// </summary>
        /// <param name="deductAmount">扣除點數</param>
        /// <param name="reason">異動原因</param>
        /// <param name="frontUser">前台使用者</param>
        private void DeductBonusPoints(int deductAmount, string? reason, FrontUser frontUser, long createUserId,long? RefKey, BonusLogTypeEnum Type)
        {
            var currentDate = DateTime.Now;

            // 1. 取得可用的紅利記錄(在有效期內且餘額大於0)
            var availableBonus = db.Bonus
                .Where(x => x.UUID == frontUser.UUID &&
                           x.Balance > 0 &&
                           x.StartDate <= currentDate &&
                           (x.EndDate == null || x.EndDate >= currentDate) &&
                           !x.IsDeleted)
                .OrderBy(x => x.EndDate) // 依到期日排序，快到期的先扣
                .ThenBy(x => x.StartDate);

            // 2. 建立紅利異動記錄
            var bonusLog = new BonusLog
            {
                Amount = -deductAmount, // 負值表示扣除
                Note = reason ?? "系統扣除紅利",
                RefKey = RefKey,
                User = frontUser,
                ExecutionTime = currentDate,
                Type = Type
            };

            // 3. 按順序扣除紅利並記錄明細
            var remainingDeduct = deductAmount;

            foreach (var bonus in availableBonus)
            {
                if (remainingDeduct <= 0)
                {
                    break;
                }

                var deductFromThis = Math.Min(remainingDeduct, bonus.Balance);

                // 更新紅利餘額
                bonus.Balance -= deductFromThis;
                bonus.LastModifierUserId = createUserId;
                bonus.LastModificationTime = currentDate;

                // 建立扣除明細記錄
                bonusLog.BonusLogDetails.Add(new BonusLogDetail
                {
                    FK_BonusId = bonus.Id,
                    UsedAmount = deductFromThis
                });

                remainingDeduct -= deductFromThis;
            }

            db.BonusLog.Add(bonusLog);
        }
        public async Task<ResponseMessageDto> RefundRedeemByOrderAsync(Guid memberUuid, long orderId, string? reason = null)
        {
            var result = new ResponseMessageDto { Success = false };
            var now = DateTime.Now;
            var createUserId = await loginUserData.GetUserId();

            try
            {
                // 1) 冪等：同訂單不可重複退回
                var alreadyRefund = await db.BonusLog
                    .AnyAsync(x => x.UUID == memberUuid &&
                                   x.RefKey == orderId &&
                                   x.Type == BonusLogTypeEnum.Refund);
                if (alreadyRefund)
                {
                    result.Success = true;
                    return result;
                }

                // 2) 找到原本折抵的 Redeem log（這裡以「最新一筆」為準，避免同訂單多次折抵的複雜度）
                var redeemLog = await db.BonusLog
                    .Include(x => x.BonusLogDetails)
                    .Where(x =>
                        x.UUID == memberUuid &&
                        x.RefKey == orderId &&
                        x.Type == BonusLogTypeEnum.Redeem)
                    .OrderByDescending(x => x.ExecutionTime) // 或 OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (redeemLog == null)
                {
                    result.Success = true;
                    return result;
                }

                if (redeemLog.Amount >= 0)
                {
                    result.Error = "該筆紀錄不是折抵（Amount 非負），無法退回。";
                    return result;
                }

                // 3) 逐筆回補到原本扣的 Bonus
                //    建議：一次抓出所有 Bonus，避免逐筆查詢
                var bonusIds = redeemLog.BonusLogDetails.Select(d => d.FK_BonusId).Distinct().ToList();
                var bonuses = await db.Bonus
                    .Where(b => bonusIds.Contains(b.Id) && b.UUID == memberUuid && !b.IsDeleted)
                    .ToDictionaryAsync(b => b.Id);

                int totalRefund = 0;

                foreach (var d in redeemLog.BonusLogDetails)
                {
                    if (!bonuses.TryGetValue(d.FK_BonusId, out var bonus))
                    {
                        // 找不到原 Bonus：通常代表資料被刪/不一致，這種建議直接中止，避免錯帳
                        result.Error = $"紅利券不存在或不一致（BonusId={d.FK_BonusId}），無法退回。";
                        return result;
                    }

                    // 回補 balance（不改 EndDate，不延壽）
                    bonus.Balance += (int)d.UsedAmount;
                    bonus.LastModifierUserId = createUserId;
                    bonus.LastModificationTime = now;

                    totalRefund += (int)d.UsedAmount;
                }

                // 4) 寫入 Refund log（Amount 正值）
                var refundLog = new BonusLog
                {
                    UUID = memberUuid,
                    Amount = totalRefund,
                    Note = reason ?? $"訂單{orderId} 取消折抵退回",
                    RefKey = orderId,
                    ExecutionTime = now,
                    Type = BonusLogTypeEnum.Refund,
                    BonusLogDetails = redeemLog.BonusLogDetails.Select(d => new BonusLogDetail
                    {
                        FK_BonusId = d.FK_BonusId,
                        UsedAmount = d.UsedAmount
                    }).ToList()
                };

                db.BonusLog.Add(refundLog);

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                return result;
            }
        }

        public async Task<ResponseMessageDto> RevokeEarnByOrderAsync(Guid memberUuid, long orderId, string? reason = null)
        {
            var result = new ResponseMessageDto { Success = false };
            var now = DateTime.Now;
            var createUserId = await loginUserData.GetUserId();

            try
            {
                // 1) 冪等：同訂單不可重複追回
                var alreadyRevoke = await db.BonusLog.AnyAsync(x =>
                    x.UUID == memberUuid &&
                    x.RefKey == orderId &&
                    x.Type == BonusLogTypeEnum.EarnRevoke);

                if (alreadyRevoke)
                {
                    result.Success = true;
                    return result;
                }

                // 2) 找原本 Earn log（代表「確實發過回饋」）
                var earnLog = await db.BonusLog
                    .FirstOrDefaultAsync(x =>
                        x.UUID == memberUuid &&
                        x.RefKey == orderId &&
                        x.Type == BonusLogTypeEnum.Earn);

                if (earnLog == null)
                {
                    // 沒發過就不用追回：視為成功（避免取消流程被卡死）
                    result.Success = true;
                    return result;
                }

                if (earnLog.Amount <= 0)
                {
                    result.Error = "Earn 紀錄 Amount 非正值，資料異常，無法追回。";
                    return result;
                }

                var needRevoke = earnLog.Amount;

                // 3) 算目前「可用」點數（已扣除 OutstandingPoints）
                var avail = (await GetQueryFrontUsersTotalAvaliableBonus(new List<Guid> { memberUuid }))
                    .FirstOrDefault()?.TotalAvaliableBonus ?? 0;

                var canDeduct = Math.Min(avail, needRevoke);
                var deficit = needRevoke - canDeduct;

                // 4) 取得使用者 entity（DeductBonusPoints 需要 FrontUser）
                var frontUser = await db.FrontUsers.FirstOrDefaultAsync(x => x.UUID == memberUuid && !x.IsDeleted);
                if (frontUser == null)
                {
                    result.Error = "查無會員資料，無法追回回饋紅利。";
                    return result;
                }

                // 5) 先扣「可扣的部分」
                if (canDeduct > 0)
                {
                    DeductBonusPoints(
                        canDeduct,
                        reason ?? $"訂單{orderId} 取消追回回饋紅利",
                        frontUser,
                        createUserId,
                        RefKey: orderId,
                        Type: BonusLogTypeEnum.EarnRevoke
                    );
                }

                // 6) 不足部分 -> 記欠點
                if (deficit > 0)
                {
                    var liability = await db.BonusLiabilities.FirstOrDefaultAsync(x => x.UUID == memberUuid);
                    if (liability == null)
                    {
                        liability = new BonusLiability
                        {
                            UUID = memberUuid,
                            OutstandingPoints = deficit,
                            UpdatedAt = now
                        };
                        db.BonusLiabilities.Add(liability);
                    }
                    else
                    {
                        liability.OutstandingPoints += deficit;
                        liability.UpdatedAt = now;
                        db.BonusLiabilities.Update(liability);
                    }

                    // 仍要寫一筆「完整追回」的 log（Amount = -needRevoke），避免之後冪等缺口
                    db.BonusLog.Add(new BonusLog
                    {
                        UUID = memberUuid,
                        Amount = -needRevoke,
                        Note = (reason ?? $"訂單{orderId} 取消追回回饋紅利") + $"（不足{deficit}點，已記入欠點）",
                        RefKey = orderId,
                        ExecutionTime = now,
                        Type = BonusLogTypeEnum.EarnRevoke
                    });
                }
                else
                {
                    // canDeduct == needRevoke：DeductBonusPoints 已寫入 BonusLog（Amount=-canDeduct）
                    // 這裡不必再補一筆額外 log
                }

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                return result;
            }
        }
    }
}
