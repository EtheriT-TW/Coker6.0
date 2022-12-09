using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Order;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

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
                var db_prod_stock = db.Prod_Stocks.Where(e => e.Id == dto.FK_PSid).FirstOrDefault();
                var db_shoppingcart = db.ShoppingCarts.Where(e => e.FK_Tid == dto.FK_Tid & e.FK_PSid == dto.FK_PSid & !e.IsDeleted).FirstOrDefault();
                var db_token = db.Tokens.Where(e => e.id == dto.FK_Tid).FirstOrDefault();
                var db_prod = db.Prods.Where(e => e.Id == db_prod_stock.FK_Pid).FirstOrDefault();
                if (db_prod_stock != null)
                {
                    if (db_token != null && db_prod != null)
                    {
                        var user_id = (db_token != null && db_token.UserID != null) ? db_token.UserID : null;
                        var price = (db_prod != null) ? (int)(db_prod.Price * dto.Quantity) : 0;
                        if (db_shoppingcart == null)
                        {
                            Core.Models.ShoppingCart sc = new Core.Models.ShoppingCart
                            {
                                FK_Tid = dto.FK_Tid,
                                FK_Uid = user_id,
                                FK_PSid = dto.FK_PSid,
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

                            Core.Models.Prod_Log pl = new Core.Models.Prod_Log
                            {

                                FK_Pid = db_prod.Id,
                                FK_Uid = user_id,
                                FK_Tid = dto.FK_Tid,
                                Action = 3,
                                Db_Name = "ShoppingCart"
                            };
                            db.Prod_Logs.Add(pl);

                            db.SaveChanges();
                            output.Message = "N" + sc.Id.ToString();
                        }
                        else
                        {
                            db_shoppingcart.Quantity += dto.Quantity;
                            db_shoppingcart.LastModificationTime = DateTime.Now;
                            db_shoppingcart.LastModifierUserId = user_id;
                            output.Message = "U" + db_shoppingcart.Id.ToString();
                            db.SaveChanges();
                        }
                        output.Success = true;
                    }
                }
                else
                {
                    output.Success = false;
                    output.Error = "資料有誤";
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> QuantityUpdate(ShoppingQuantityUpdateDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            output.Success = true;
            try
            {
                var db_shoppingcart = db.ShoppingCarts.Where(e => e.Id == dto.Id).FirstOrDefault();
                var db_token = db.Tokens.Where(e => e.id == dto.FK_Tid).FirstOrDefault();

                if (db_shoppingcart != null && db_token != null)
                {
                    var user_id = (db_token != null && db_token.UserID != null) ? db_token.UserID : null;
                    db_shoppingcart.Quantity = dto.Quantity;
                    db_shoppingcart.LastModificationTime = DateTime.Now;
                    db_shoppingcart.LastModifierUserId = user_id;
                    output.Message = db_shoppingcart.Id.ToString();
                    db.SaveChanges();
                    output.Success = true;
                }
                output.Success = false;
                output.Error = "資料有誤";
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<List<ShoppingCartGetAllDto>> GetAll(String Tid)
        {
            try
            {
                var db_shoppingcart = db.ShoppingCarts;
                var db_prod_stock = db.Prod_Stocks;
                var db_prod = db.Prods;

                if (db_shoppingcart != null)
                {
                    var output = await (from sc in db_shoppingcart
                                        where sc.FK_Tid == Guid.Parse(Tid) && !sc.IsDeleted
                                        orderby sc.Ser_No
                                        from ps in db_prod_stock
                                        where ps.Id == sc.FK_PSid
                                        from p in db_prod
                                        where p.Id == ps.FK_Pid
                                        select new ShoppingCartGetAllDto
                                        {
                                            SCId = sc.Id,
                                            PId = p.Id,
                                            Title = p.Title,
                                            Description = p.Description,
                                            Price = p.Price,
                                            Quantity = sc.Quantity,
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
        public async Task<ShoppingCartGetDrop> GetDropOne(long id)
        {
            try
            {
                var db_shoppingcart = db.ShoppingCarts.Where(e => e.Id == id).FirstOrDefault();
                var db_prod_stock = db.Prod_Stocks.Where(e => e.Id == db_shoppingcart.FK_PSid).FirstOrDefault();
                var db_prod = db.Prods.Where(e => e.Id == db_prod_stock.FK_Pid).FirstOrDefault();

                if (db_shoppingcart != null)
                {
                    ShoppingCartGetDrop output = new ShoppingCartGetDrop()
                    {
                        SCId = db_shoppingcart.Id,
                        PId = db_prod.Id,
                        Title = db_prod.Title,
                        Quantity = db_shoppingcart.Quantity,
                        Price = db_prod.Price
                    };

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
