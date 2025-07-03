using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
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

        public BonusManagementAppService(IStoreSetAppService storeSetAppService,
                                         LoginUserData loginUserData,
                                         CokerDbContext db)
        {
            _storeSetAppService = storeSetAppService;
            this.loginUserData = loginUserData;
            this.db = db;
        }

        public async Task<GetBonusSettingForEditOutput> GetBonusSettingForEdit()
        {
            long websiteID = await loginUserData.GetWebsiteId();
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
                    SignupBonusPointsHelpText = bonusSettingItem.storeSets?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.SignupBonusPoints))?.memo,
                    MinOrderForRedemptionHelpText = bonusSettingItem.storeSets?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.MinOrderForRedemption))?.memo,
                    MaxRedemptionPercentHelpText = bonusSettingItem.storeSets?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.MaxRedemptionPercent))?.memo,
                    MinOrderForEarnPointsHelpText = bonusSettingItem.storeSets?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.MinOrderForEarnPoints))?.memo,
                    RewardRatePercentHelpText = bonusSettingItem.storeSets?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.RewardRatePercent))?.memo,
                    RewardPointsExpireDaysHelpText = bonusSettingItem.storeSets?.FirstOrDefault()?.storeSets?.FirstOrDefault(x => x.key == nameof(GetBonusSettingForEditOutput.RewardPointsExpireDays))?.memo
                };
            }

            return new GetBonusSettingHelpTextForEditOutput();
        }

        public async Task<JsonResult> GetFrontUsers(DataSourceLoadOptions loadOptions)
        {
            long websideId = await loginUserData.GetWebsiteId();
            var dataQuery = from user in db.FrontUsers
                            join site in db.MappingFrontUserAndWebsite on user.Id equals site.FK_UserId
                            where site.FK_WebsiteId == websideId &&
                                  user.IsDeleted == false
                            select user;

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
            long websiteID = await loginUserData.GetWebsiteId();

            if (!input.MemberUUID.Any())
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
                    .Where(x => input.MemberUUID.Contains(x.UUID) && !x.IsDeleted)
                    .ToListAsync();

                // 如果是扣除紅利，先檢核所有人的紅利是否足夠
                if (input.TransactionOperation == "-")
                {
                    var deductAmount = Math.Abs(input.TransactionPoint);
                    var insufficientUsers = new List<string>();

                    // 一次查詢所有相關用戶的可用紅利
                    var availableBonusData = await db.Bonus
                        .Where(x => input.MemberUUID.Contains(x.UUID) &&
                                   x.Balance > 0 &&
                                   x.StartDate <= currentDate &&
                                   (x.EndDate == null || x.EndDate >= currentDate) &&
                                   !x.IsDeleted)
                        .GroupBy(x => x.UUID)
                        .Select(g => new { UUID = g.Key, TotalBalance = g.Sum(b => b.Balance) })
                        .ToListAsync();

                    foreach (var frontUser in allFrontUsers)
                    {
                        var userBonus = availableBonusData.FirstOrDefault(x => x.UUID == frontUser.UUID);
                        var availableBalance = userBonus?.TotalBalance ?? 0;

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
                foreach (var memberUuid in input.MemberUUID)
                {
                    var frontUser = allFrontUsers.FirstOrDefault(x => x.UUID == memberUuid);

                    if (frontUser == null)
                        continue;

                    // 根據 TransactionOperation 判斷操作類型
                    if (input.TransactionOperation == "+")
                    {
                        // 取得紅利有效天數設定
                        var bonusSettings = await GetBonusSettingForEdit();
                        var expireDays = bonusSettings.RewardPointsExpireDays ?? 0; // 預設0天

                        // 新增紅利
                        var addAmount = Math.Abs(input.TransactionPoint);
                        AddBonusPoints(addAmount, expireDays, input.TransactionReason, frontUser, createUserId);
                    }
                    else if (input.TransactionOperation == "-")
                    {
                        // 扣除紅利
                        var deductAmount = Math.Abs(input.TransactionPoint);
                        DeductBonusPoints(deductAmount, input.TransactionReason, frontUser, createUserId);
                    }
                }

                await db.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
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
        private void AddBonusPoints(int points, int? expireDays, string? reason, FrontUser frontUser, long createUserId)
        {
            var currentDate = DateTime.Now;

            // 建立新的紅利記錄
            var bonus = new Bonus
            {
                UUID =  frontUser.UUID,
                Amount = points,
                Balance = points,
                StartDate = currentDate,
                EndDate = currentDate.AddDays(expireDays ?? 0),
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
                ExecutionTime = currentDate
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
        private void DeductBonusPoints(int deductAmount, string? reason, FrontUser frontUser, long createUserId)
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
                User = frontUser,
                ExecutionTime = currentDate
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

                db.BonusLog.Add(bonusLog);

                remainingDeduct -= deductFromThis;
            }
        }
    }
}
