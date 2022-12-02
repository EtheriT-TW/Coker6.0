using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.ShoppingCart
{
    public class ShoppingCartAppService : IShoppingCartAppService
    {
        private readonly CokerDbContext db;
        public ShoppingCartAppService(
            CokerDbContext db
        )
        {
            this.db = db;
        }
        public async Task<ResponseMessageDto> AddUp(ShoppingCartAddUpDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            output.Success = true;
            try
            {
                var db_token = db.Tokens.Where(e => e.id == dto.FK_Tid).FirstOrDefault();
                var db_prod = db.Prods.Where(e => e.Id == dto.FK_Pid).FirstOrDefault();

                var user_id = (db_token != null && db_token.UserID != null) ? db_token.UserID : null;
                var price = (db_prod != null) ? (int)(db_prod.Price * dto.Quantity) : 0;

                Core.Models.ShoppingCart sc = new Core.Models.ShoppingCart
                {
                    FK_Tid = dto.FK_Tid,
                    FK_Uid = user_id,
                    FK_Pid = dto.FK_Pid,
                    FK_S1id = dto.FK_S1id,
                    FK_S2id = dto.FK_S2id,
                    Quantity = dto.Quantity,
                    Price = price,
                    Discont = dto.Discont,
                    Bonus = dto.Bonus,
                    PriceType = dto.PriceType,
                    IsAdditional = dto.IsAdditional,
                    Ser_No = dto.Ser_No == null ? 500 : (int)dto.Ser_No
                };
                db.ShoppingCarts.Add(sc);
                db.SaveChanges();
                output.Success = true;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }

        public async Task<List<ShoppingCartGetDrop>> GetDrop(String id)
        {
            try
            {
                var db_shoppingcart = db.ShoppingCarts;
                var db_prod = db.Prods;

                if (db_shoppingcart != null)
                {
                    var output = await (from sc in db_shoppingcart
                                        where sc.FK_Tid == Guid.Parse(id) && !sc.IsDeleted
                                        orderby sc.Ser_No
                                        from p in db_prod
                                        where p.Id == sc.FK_Pid
                                        select new ShoppingCartGetDrop
                                        {
                                            Id = sc.Id,
                                            Title = p.Title,
                                            Quantity = sc.Quantity,
                                            Price = sc.Price
                                        }).ToListAsync();
                    return output;
                }
                else throw new Exception("查無購物車資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }

        public async Task<ResponseMessageDto> DeleteDrop(long id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var result = db.ShoppingCarts.Where(e => e.Id == id).FirstOrDefault();

                if (result != null)
                {
                    result.IsDeleted = true;
                    result.DeletionTime = DateTime.Now;
                    db.SaveChanges();
                    output.Success = true;
                }
                else throw new Exception("查無購物車資料");
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
