using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Company;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.BonusManagement
{
    public interface IBonusManagementAppService
    {
        /// <summary>
        /// 取得紅利設定
        /// </summary>
        /// <returns></returns>
        Task<GetBonusSettingForEditOutput> GetBonusSettingForEdit();

        /// <summary>
        /// 取得紅利設定說明
        /// </summary>
        /// <returns></returns>
        Task<GetBonusSettingHelpTextForEditOutput> GetBonusSettingHelpTextForEdit();

        /// <summary>
        /// 儲存紅利設定
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<ResponseMessageDto> SaveSetting(CreateOrUpdateSettingsDto input);

        /// <summary>
        /// 儲存前端使用者紅利異動
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<ResponseMessageDto> SaveTransaction(CreateUserTransactionDto input);

        /// <summary>
        /// 取得前端使用者列表
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        Task<JsonResult> GetFrontUsers(DataSourceLoadOptions loadOptions, bool isShowCurrentMonthBirthdayOnly);

        /// <summary>
        /// 取得紅利異動紀錄列表
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        Task<JsonResult> GetBonusLogForDataGrid(DataSourceLoadOptions loadOptions);

        /// <summary>
        /// 取得前端使用者紅利總合資訊
        /// </summary>
        /// <param name="frontUsers"></param>
        /// <returns></returns>
        Task<List<GetQueryFrontUsersTotalAvaliableBonusOutput>> GetQueryFrontUsersTotalAvaliableBonus(List<Guid> frontUsersUUID);

        /// <summary>
        /// 取得前端使用者紅利異動紀錄
        /// </summary>
        /// <param name="frontUserUUID"></param>
        /// <param name="topRecordCount"></param>
        /// <returns></returns>
        Task<List<GetQueryFrontUsersBonusLogOutput>> GetQueryFrontUsersBonusLog(Guid frontUserUUID, int topRecordCount);
        /// <summary>
        /// 取消交易退還紅利
        /// </summary>
        /// <param name="memberUuid"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<ResponseMessageDto> RefundRedeemByOrderAsync(Guid memberUuid, long orderId, string? reason = null);
        /// <summary>
        /// 取消交易追回紅利
        /// </summary>
        /// <param name="memberUuid"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<ResponseMessageDto> RevokeEarnByOrderAsync(Guid memberUuid, long orderId, string? reason = null);
    }
}
