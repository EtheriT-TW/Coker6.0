using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Order;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop.Infrastructure;
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
                var db_ps = db.Prod_Stocks.Where(e => e.FK_Pid == dto.FK_Pid && e.FK_S1id == dto.FK_S1id && e.FK_S2id == dto.FK_S2id).FirstOrDefault();
                var db_shoppingcart = db.ShoppingCarts.Where(e => e.FK_Tid == dto.FK_Tid && e.FK_PSid == db_ps.Id && !e.IsDeleted).FirstOrDefault();
                var db_token = db.Tokens.Where(e => e.id == dto.FK_Tid).FirstOrDefault();
                var db_prod = db.Prods.Where(e => e.Id == db_ps.FK_Pid).FirstOrDefault();
                if (db_ps != null)
                {
                    if (db_token != null && db_prod != null)
                    {
                        var price = (db_prod != null) ? (int)(db_ps.Price * dto.Quantity) : 0;
                        if (db_shoppingcart == null)
                        {
                            Core.Models.ShoppingCart sc = new Core.Models.ShoppingCart
                            {
                                FK_Tid = dto.FK_Tid,
                                FK_Uid = db_token.UserID,
                                FK_PSid = db_ps.Id,
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
                                FK_Uid = db_token.UserID,
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
                            db_shoppingcart.LastModifierUserId = db_token.UserID;
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
                                        from ps in db_prod_stock
                                        where ps.Id == sc.FK_PSid
                                        from p in db_prod
                                        where p.Id == ps.FK_Pid
                                        select new ShoppingCartGetAllDto
                                        {
                                            SCId = sc.Id,
                                            PId = p.Id,
                                            Title = p.Title,
                                            S1Title = ps.FK_S1id.ToString(),
                                            S2Title = ps.FK_S2id.ToString(),
                                            Description = p.Description,
                                            Price = ps.Price,
                                            Quantity = sc.Quantity,
                                        }).ToListAsync();


                    var db_sp = db.Prod_Specs.ToList();
                    foreach (var item in output)
                    {
                        item.S1Title = int.Parse(item.S1Title) == 0 ? "" : db_sp[int.Parse(item.S1Title) - 1].Title;
                        item.S2Title = int.Parse(item.S2Title) == 0 ? "" : db_sp[int.Parse(item.S2Title) - 1].Title;
                    }

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
                var db_sc = db.ShoppingCarts.Where(e => e.Id == id).FirstOrDefault();
                var db_ps = db.Prod_Stocks.Where(e => e.Id == db_sc.FK_PSid).FirstOrDefault();
                var db_prod = db.Prods.Where(e => e.Id == db_ps.FK_Pid).FirstOrDefault();

                if (db_sc != null)
                {
                    ShoppingCartGetDrop output = new ShoppingCartGetDrop()
                    {
                        SCId = db_sc.Id,
                        PId = db_prod.Id,
                        Title = db_prod.Title,
                        S1Title = db_ps.FK_S1id.ToString(),
                        S2Title = db_ps.FK_S2id.ToString(),
                        Quantity = db_sc.Quantity,
                        Price = db_ps.Price
                    };

                    var db_sp = db.Prod_Specs.ToList();

                    output.S1Title = int.Parse(output.S1Title) == 0 ? "" : db_sp[int.Parse(output.S1Title) - 1].Title;
                    output.S2Title = int.Parse(output.S2Title) == 0 ? "" : db_sp[int.Parse(output.S2Title) - 1].Title;

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
