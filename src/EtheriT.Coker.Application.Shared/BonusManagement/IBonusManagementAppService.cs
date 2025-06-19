using EtheriT.Coker.Application.Company;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
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
    }
}
