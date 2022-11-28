using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Order;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Order
{
    public class OrderAppService : IOrderAppService
    {
        private readonly CokerDbContext db;
        public OrderAppService(
            CokerDbContext db
        )
        {
            this.db = db;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var result = db.Order_Headers;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted
                                    select new OrderHeaderGetAllListDto
                                    {
                                        Id = e.Id,
                                        Orderer = e.Orderer,
                                        RecipientAddress = e.RecipientAddress,
                                        Shipping = e.Shipping,
                                        Payment = e.Payment,
                                        State = e.State,
                                        Total = e.Total,
                                        CreationTime = e.CreationTime,
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無跑馬燈資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<OrderHeaderGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> Delete(int id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var result = db.Order_Headers.Where(e => e.Id == id).FirstOrDefault();

                if (result != null)
                {
                    result.IsDeleted = true;
                    result.DeletionTime = DateTime.Now;
                    db.SaveChanges();
                    output.Success = true;
                }
                else throw new Exception("查無訂單資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
    }
}
