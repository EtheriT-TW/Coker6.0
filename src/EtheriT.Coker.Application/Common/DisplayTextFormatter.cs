using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Common
{
    public static class DisplayTextFormatter
    {
        public static string Freight(
            LogisticsSetting source,
            bool includeStatus = false,
            bool detailedBoxText = true,
            bool includePackingHint = true)
        {
            if (source == null) return "";

            var logisticsText = source.LogisticsType.ToString()
                .Replace("_", "/")
                .Replace("Seven", "7-11");

            string freightText;

            switch (source.FreightType)
            {
                case FreightTypeEnum.免運費:
                    freightText = "免運費";
                    break;

                case FreightTypeEnum.單筆計算:
                    freightText =
                        source.Freight == source.Dis_Freight
                            ? $"單筆計算{source.Freight}元"
                            : $"單筆計算{source.Freight}元(滿{source.Low_Con}元{(source.Dis_Freight == 0 ? "免運" : $"運費{source.Dis_Freight}元")})";
                    break;

                case FreightTypeEnum.依箱計費:
                    var fees = source.logisticsBoxFees?
                        .Where(f => !f.IsDeleted && f.logisticsBox != null && !f.logisticsBox.IsDeleted)
                        .OrderBy(f => f.logisticsBox!.Sort)
                        .ThenBy(f => f.logisticsBox!.CapacityPoint)
                        .ToList();

                    if (fees != null && fees.Any())
                    {
                        if (detailedBoxText)
                        {
                            var feeText = string.Join(" / ",
                                fees.Select(f => $"{f.logisticsBox!.Name} {FormatAmount(f.Fee)}元"));

                            freightText = includePackingHint
                                ? $"依箱計費（依實際裝箱結果計算，{feeText}）"
                                : $"依箱計費（{feeText}）";
                        }
                        else
                        {
                            freightText = $"依箱計費(已設定{fees.Count}種箱型)";
                        }
                    }
                    else
                    {
                        freightText = "依箱計費（尚未設定箱型費用）";
                    }
                    break;

                default:
                    freightText = source.FreightType.ToString();
                    break;
            }

            var result = $"{logisticsText}，{freightText}";

            if (includeStatus)
            {
                result = $"{source.FreightStatusType} - {result}";
            }

            return result;
        }
        public static string LogisticsSubType(ShippingTypeEnum logisticsType)
        {
            return logisticsType switch
            {
                ShippingTypeEnum.綠界_大宗寄倉_全家 => "FAMI",
                ShippingTypeEnum.綠界_大宗寄倉_711超商 => "UNIMART",
                ShippingTypeEnum.綠界_大宗寄倉_711冷凍店取 => "UNIMARTFREEZE",
                ShippingTypeEnum.綠界_大宗寄倉_萊爾富 => "HILIFE",
                ShippingTypeEnum.綠界_門市寄取_全家 => "FAMIC2C",
                ShippingTypeEnum.綠界_門市寄取_711超商 => "UNIMARTC2C",
                ShippingTypeEnum.綠界_門市寄取_萊爾富 => "HILIFEC2C",
                ShippingTypeEnum.綠界_門市寄取_OK超商 => "OKMARTC2C",
                _ => ""
            };
        }

        private static string FormatAmount(decimal amount)
        {
            return amount % 1 == 0
                ? ((int)amount).ToString()
                : amount.ToString("0.##");
        }
    }
}
