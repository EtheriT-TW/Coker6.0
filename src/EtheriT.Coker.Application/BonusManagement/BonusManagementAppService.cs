using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        public async Task<ResponseMessageDto> Save(CreateOrUpdateSettingsDto input)
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

                //result.Success = true;
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
    }
}
