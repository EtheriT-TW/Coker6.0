using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Order;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace EtheriT.Coker.Application.ShoppingCart
{
    public class ShoppingCartAppService : IShoppingCartAppService
    {
        private readonly CokerDbContext db;
        private readonly ITokenAppService tokenAppService;
        private readonly IConfiguration configuration;
        public ShoppingCartAppService(
            CokerDbContext db,
            ITokenAppService tokenAppService,
            IConfiguration configuration
        )
        {
            this.db = db;
            this.tokenAppService = tokenAppService;
            this.configuration = configuration;
        }
        public async Task<ResponseMessageDto> AddUp(ShoppingCartAddUpDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var Token = tokenAppService.CheckToken();

                var userid = new List<Guid>();
                if (Token.IsLogin)
                {
                    userid.Add(UUID);
                    var uuids = await db.MappingOldNewUUID.Where(e => e.UserUUID == UUID).ToListAsync();
                    if (uuids.Count > 0)
                    {
                        foreach (var item in uuids)
                        {
                            if (item.TempUUID != Guid.Empty) userid.Add(item.TempUUID);
                        }
                    }
                }

                var db_ps = db.Prod_Stocks.Where(e => e.FK_Pid == dto.FK_Pid && e.FK_S1id == dto.FK_S1id && e.FK_S2id == dto.FK_S2id).FirstOrDefault();
                var db_shoppingcart = db.ShoppingCarts.Where(e => (e.FK_Tid == Token.RefreshToken || userid.Contains(e.UUID)) && e.FK_PSid == db_ps.Id && !e.IsOrder).FirstOrDefault();
                var db_token = db.Tokens.Where(e => e.id == Token.RefreshToken).FirstOrDefault();
                var db_prod = db.Prods.Where(e => e.Id == db_ps.FK_Pid).FirstOrDefault();

                if (db_ps != null)
                {
                    if (db_token != null && db_prod != null && Token.RefreshToken != null)
                    {
                        var price = (db_prod != null) ? (int)(db_ps.Price * dto.Quantity) : 0;
                        if (db_shoppingcart == null)
                        {
                            Core.Models.ShoppingCart sc = new Core.Models.ShoppingCart
                            {
                                FK_Tid = (Guid)Token.RefreshToken,
                                UUID = UUID,
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
                                FK_UserId = db_token.UserID,
                                UUID = UUID,
                                Action = (int)LogActionEnum.加入購物車,
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
                            db_shoppingcart.LastModifierUserId = db_shoppingcart.CreatorUserId;
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
                var UUID = await tokenAppService.GetUUID();
                var token = tokenAppService.CheckToken();
                var db_shoppingcart = db.ShoppingCarts.Where(e => e.Id == dto.Id && !e.IsOrder).FirstOrDefault();
                var db_token = db.Tokens.Where(e => e.id == token.RefreshToken).FirstOrDefault();

                if (db_shoppingcart != null && db_token != null)
                {
                    db_shoppingcart.Quantity = dto.Quantity;
                    db_shoppingcart.LastModificationTime = DateTime.Now;
                    db_shoppingcart.LastModifierUserId = db_shoppingcart.CreatorUserId;
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
        public async Task<List<ShoppingCartGetAllDto>> GetAll()
        {
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var Token = tokenAppService.CheckToken();

                var userid = new List<Guid>();
                if (Token.IsLogin)
                {
                    userid.Add(UUID);
                    var uuids = await db.MappingOldNewUUID.Where(e => e.UserUUID == UUID).ToListAsync();
                    if (uuids.Count > 0)
                    {
                        foreach (var item in uuids)
                        {
                            if (item.TempUUID != Guid.Empty) userid.Add(item.TempUUID);
                        }
                    }
                }

                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

                var output = await (from sc in db.ShoppingCarts
                                    where userid.Count == 0 ? sc.FK_Tid == Token.RefreshToken : userid.Contains(sc.UUID) && !sc.IsOrder
                                    from ps in db.Prod_Stocks
                                    where ps.Id == sc.FK_PSid
                                    from pp in db.Prod_Prices
                                    where pp.FK_PSId == ps.Id
                                    from p in db.Prods
                                    where p.FK_WebsiteId == WebsiteId && p.Id == ps.FK_Pid
                                    select new ShoppingCartGetAllDto
                                    {
                                        SCId = sc.Id,
                                        PId = p.Id,
                                        Title = p.Title,
                                        S1Title = ps.FK_S1id.ToString(),
                                        S2Title = ps.FK_S2id.ToString(),
                                        Description = p.Description,
                                        Price = pp.Price ?? 0,
                                        OldPrice = sc.Price,
                                        Quantity = sc.Quantity,
                                        ImagePath = ((from f in db.FileBinds.Include(e => e.fileUpload)
                                              .Where(e => e.fileUpload != null && e.fileUpload.FK_WebsiteId == WebsiteId)
                                              .Where(e => e.fileUpload != null && !e.IsDeleted && !e.fileUpload.IsDeleted)
                                              .Where(e => e.Sid == p.Id && e.type == (int)FileBindTypeEnum.產品)
                                              .OrderBy(e => e.SerNo).ThenBy(e => e.CreationTime)
                                                      select new DirectoryReleInfoDto
                                                      {
                                                          Link = f.fileUpload != null ? f.fileUpload.DownloadFileName ?? "" : ""
                                                      }).FirstOrDefault() ?? new DirectoryReleInfoDto()).Link
                                    }).ToListAsync();

                if (output != null)
                {
                    var temp_output = output;
                    output = new List<ShoppingCartGetAllDto>();
                    for (var i = 0; i < temp_output.Count; i++)
                    {
                        if (output.Count == 0 || output.FindIndex(e => e.PId == temp_output[i].PId) < 0) output.Add(temp_output[i]);
                        else output[output.FindIndex(e => e.PId == temp_output[i].PId)].Quantity += temp_output[i].Quantity;
                    }

                    var db_sp = db.Prod_Specs.ToList();
                    foreach (var item in output)
                    {
                        item.S1Title = int.Parse(item.S1Title) == 0 ? "" : db_sp.Find(e => e.Id == int.Parse(item.S1Title))?.Title;
                        item.S2Title = int.Parse(item.S2Title) == 0 ? "" : db_sp.Find(e => e.Id == int.Parse(item.S2Title))?.Title;
                    }

                    return output;
                }
                else throw new Exception("查無購物車資料");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }
        public async Task<ShoppingCartGetDrop> GetDropOne(long id, bool isorder)
        {
            try
            {
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

                var db_sc = db.ShoppingCarts.Where(e => e.Id == id && e.IsOrder == isorder).FirstOrDefault();
                var db_ps = db.Prod_Stocks.Where(e => e.Id == db_sc.FK_PSid).FirstOrDefault();
                var db_prod = db.Prods.Where(e => e.FK_WebsiteId == WebsiteId && e.Id == db_ps.FK_Pid).FirstOrDefault();
                var db_price = db.Prod_Prices.Where(e => e.FK_PSId == db_ps.Id).FirstOrDefault();
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
                        Price = db_price == null ? 0 : db_price.Price ?? 0,
                        ImagePath = ((from f in db.FileBinds.Include(e => e.fileUpload)
                                                  .Where(e => e.fileUpload != null && e.fileUpload.FK_WebsiteId == WebsiteId)
                                                  .Where(e => e.fileUpload != null && !e.IsDeleted && !e.fileUpload.IsDeleted)
                                                  .Where(e => e.Sid == db_prod.Id && e.type == (int)FileBindTypeEnum.產品)
                                                  .OrderBy(e => e.SerNo).ThenBy(e => e.CreationTime)
                                      select new DirectoryReleInfoDto
                                      {
                                          Link = f.fileUpload != null ? f.fileUpload.DownloadFileName ?? "" : ""
                                      }).FirstOrDefault() ?? new DirectoryReleInfoDto()).Link

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
                    result.DeleterUserId = result.CreatorUserId;
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
