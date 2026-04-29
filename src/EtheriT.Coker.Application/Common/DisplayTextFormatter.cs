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

            bool hasDiscountRule =
                source.DiscountFreightType != null &&
                source.Low_Con != null &&
                source.Low_Con > 0 &&
                source.Dis_Freight != null &&
                source.Dis_Freight >= 0;

            string discountText = "";

            if (hasDiscountRule)
            {
                discountText = source.DiscountFreightType switch
                {
                    DiscountFreightType.指定折抵後運費 =>
                        source.Dis_Freight == 0
                            ? $"滿{source.Low_Con}元免運"
                            : $"滿{source.Low_Con}元運費{source.Dis_Freight}元",

                    DiscountFreightType.折抵固定運費金額 =>
                        $"滿{source.Low_Con}元折抵運費{source.Dis_Freight}元",

                    _ => ""
                };
            }

            switch (source.FreightType)
            {
                case FreightTypeEnum.免運費:
                    freightText = "免運費";
                    break;

                case FreightTypeEnum.單筆計算:
                    freightText = $"單筆計算{source.Freight}元";

                    if (!string.IsNullOrWhiteSpace(discountText))
                    {
                        freightText += $"({discountText})";
                    }

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

                            var boxDetailText = string.IsNullOrWhiteSpace(discountText)
                                ? feeText
                                : $"{feeText}，{discountText}";

                            freightText = includePackingHint
                                ? $"依箱計費（依實際裝箱結果計算，{boxDetailText}）"
                                : $"依箱計費（{boxDetailText}）";
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
                ShippingTypeEnum.OK取貨 => "OKMARTC2C",
                ShippingTypeEnum.全家取貨 => "FAMIC2C",
                ShippingTypeEnum.Seven取貨 => "UNIMARTC2C",
                ShippingTypeEnum.萊爾富取貨 => "HILIFEC2C",
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
