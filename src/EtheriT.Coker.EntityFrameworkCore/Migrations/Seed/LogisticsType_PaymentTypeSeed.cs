using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.EntityFrameworkCore.Migrations.Seed
{
    public class LogisticsType_PaymentTypeSeed
    {
        public static void Seed(ModelBuilder modelBuilder) {
            modelBuilder.Entity<LogisticsPaymentRestriction>().HasData(
                new LogisticsPaymentRestriction {
                    Id = 1,
                    ShippingType = ShippingTypeEnum.郵寄掛號,
                    FK_Pid = 1
                }
            );
        }
    }
}
