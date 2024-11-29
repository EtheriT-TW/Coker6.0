using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Asn1.Ess;

namespace EtheriT.Coker.Application.ShoppingCart
{
    public class ShoppingCartAppService : IShoppingCartAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITokenAppService tokenAppService;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        public ShoppingCartAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService,
            IConfiguration configuration,
            IMapper mapper
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tokenAppService = tokenAppService;
            this.configuration = configuration;
            this.mapper = mapper;
        }
        public async Task<ResponseMessageDto> UpdateUUID(Guid UserUUID, Guid TempUUID)
        {
            var response = new ResponseMessageDto();
            try
            {
                var userid = await db.FrontUsers.Where(e => e.UUID == UserUUID).Select(e => e.FK_User).FirstOrDefaultAsync();
                var tempsc = await db.ShoppingCarts.Where(e => e.UUID == TempUUID && !e.IsOrder).ToListAsync();
                var usersc = await db.ShoppingCarts.Where(e => e.UUID == UserUUID && !e.IsOrder).ToListAsync();
                if (tempsc.Any())
                {
                    for (var i = 0; i < tempsc.Count; i++)
                    {
                        tempsc[i].UUID = UserUUID;
                        tempsc[i].FK_Uid = userid ?? 0;
                    }
                    if (usersc.Any())
                    {
                        for (var i = 0; i < tempsc.Count; i++)
                        {
                            if (usersc.Find(e => e.FK_PSid == tempsc[i].FK_PSid) != null)
                            {
                                usersc[usersc.FindIndex(e => e.FK_PSid == tempsc[i].FK_PSid)].Quantity += tempsc[i].Quantity;
                                tempsc[i].IsDeleted = true;
                                tempsc[i].DeletionTime = DateTime.Now;
                            }
                        }
                    }
                    db.SaveChanges();
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> AddUp(ShoppingCartAddUpDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto() { Success = false };
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var token = (await tokenAppService.CheckToken(null)).RefreshToken;
                var userid = await db.FrontUsers.Where(e => e.UUID == UUID).Select(e => e.FK_User).FirstOrDefaultAsync();
                if (userid == null) userid = 0;

                if (token != null && UUID != Guid.Empty)
                {
                    var pro_stock = await db.Prod_Stocks.Where(e => dto.FK_PSid != null ? e.Id == dto.FK_PSid : e.FK_Pid == dto.FK_Pid && e.FK_S1id == dto.FK_S1id && e.FK_S2id == dto.FK_S2id).FirstOrDefaultAsync();
                    if (pro_stock != null)
                    {

                        if (pro_stock.Stock >= dto.Quantity)
                        {
                            Core.Models.ShoppingCart? sc = new Core.Models.ShoppingCart();

                            if (dto.Id != null) sc = await db.ShoppingCarts.Where(e => e.Id == dto.Id).FirstOrDefaultAsync();
                            else sc = await db.ShoppingCarts.Where(e => e.UUID == UUID && e.FK_PSid == pro_stock.Id).FirstOrDefaultAsync();

                            if (sc == null)
                            {
                                sc = mapper.Map<Core.Models.ShoppingCart>(pro_stock);
                                sc.Id = 0;
                                sc.Price = 0;
                                sc.Quantity = dto.Quantity;
                                sc.OldQuantity = dto.Quantity;
                                sc.FK_Tid = (Guid)token;
                                sc.FK_Uid = userid;
                                sc.UUID = UUID;
                                sc.Ser_No = 500;

                                Prod_Log pl = new Prod_Log
                                {
                                    FK_Pid = pro_stock.FK_Pid,
                                    FK_UserId = userid,
                                    UUID = UUID,
                                    Action = (int)LogActionEnum.加入購物車,
                                    Db_Name = "ShoppingCart"
                                };

                                pro_stock.Stock -= dto.Quantity;

                                db.ShoppingCarts.Add(sc);
                                db.SaveChanges();
                                response.Message = "N" + sc.Id.ToString();
                            }
                            else
                            {
                                if (sc != null)
                                {
                                    sc.Quantity += dto.Quantity;
                                    sc.OldQuantity += sc.Quantity;
                                    sc.LastModificationTime = DateTime.Now;
                                    sc.LastModifierUserId = userid;
                                    pro_stock.Stock -= dto.Quantity;

                                    db.SaveChanges();
                                    response.Message = "U" + sc.Id.ToString();
                                }
                                else throw new Exception("查無購物車資料");
                            }
                            response.Success = true;
                        }
                        else throw new Exception("庫存不足");
                    }
                    else throw new Exception("查無商品規格");
                }
                else throw new Exception("取得Token或UUID發生錯誤");
            }
            catch (Exception ex)
            {
                if (ex.Message == "庫存不足")
                {
                    response.Error = "商品庫存不足";
                    response.Message = "該商品規格庫存量已在瀏覽期間被更動，按下確定後將重整頁面。";
                }
                else
                {
                    response.Error = "Erro";
                    response.Message = ex.Message;
                }
            }
            return response;
        }
        public async Task<ResponseMessageDto> QuantityUpdate(ShoppingQuantityUpdateDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var sc = db.ShoppingCarts.Where(e => e.Id == dto.Id && !e.IsOrder).FirstOrDefault();

                if (sc != null)
                {
                    var pro_stock = await db.Prod_Stocks.Where(e => e.Id == sc.FK_PSid).FirstOrDefaultAsync();
                    if (pro_stock != null)
                    {
                        var quantity = dto.Quantity - sc.Quantity;
                        if (quantity > 0 && quantity > pro_stock.Stock) throw new Exception("庫存不足");
                        pro_stock.Stock -= quantity;
                        sc.Quantity = dto.Quantity;
                        sc.OldQuantity = dto.Quantity;
                        sc.LastModificationTime = DateTime.Now;
                        sc.LastModifierUserId = sc.CreatorUserId;
                        response.Message = sc.Id.ToString();
                        db.SaveChanges();
                        response.Success = true;
                    }
                    else throw new Exception("查無商品庫存");
                }
                else throw new Exception("查無購物車資料");
            }
            catch (Exception ex)
            {
                if (ex.Message == "庫存不足")
                {
                    response.Error = "商品庫存不足";
                    response.Message = "該商品規格庫存量已在瀏覽期間被更動，按下確定後將重整頁面。";
                }
                else
                {
                    response.Error = "Error";
                    response.Message = ex.Message;
                }
            }
            return response;
        }
        public async Task<List<ShoppingCartGetAllDto>> GetAll()
        {
            List<ShoppingCartGetAllDto> output = new List<ShoppingCartGetAllDto>();
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var Token = await tokenAppService.CheckToken(null);

                var userid = new List<Guid>();
                if (Token.IsLogin)
                {
                    var uuids = await db.MappingOldNewUUID.Where(e => e.UserUUID == UUID).ToListAsync();
                    userid.Add(UUID);
                    if (uuids.Count > 0)
                    {
                        foreach (var item in uuids)
                        {
                            if (item.TempUUID != Guid.Empty) userid.Add(item.TempUUID);
                        }
                    }
                }

                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

                output = await (from sc in db.ShoppingCarts
                                where userid.Count == 0 ? sc.FK_Tid == Token.RefreshToken : userid.Contains(sc.UUID)
                                where !sc.IsOrder
                                from ps in db.Prod_Stocks
                                where ps.Id == sc.FK_PSid
                                from pp in db.Prod_Prices
                                where pp.FK_PSId == ps.Id && pp.FK_RId == 1
                                from p in db.Prods
                                where p.FK_WebsiteId == WebsiteId && p.Id == ps.FK_Pid
                                select new ShoppingCartGetAllDto
                                {
                                    SCId = sc.Id,
                                    PSId = ps.Id,
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

                var token = await tokenAppService.CheckToken(null);
                long role = 0;
                if (token != null && token.IsLogin) role = await db.MappingUserAndRoles.Where(e => e.UUID == UUID).Select(e => e.RoleId).FirstOrDefaultAsync();

                if (output != null)
                {
                    var db_sp = db.Prod_Specs.ToList();
                    foreach (var item in output)
                    {
                        if (role > 1)
                        {
                            var price = await db.Prod_Prices.Where(e => e.FK_RId == role && e.FK_PSId == item.PSId).Select(e => e.Price).FirstOrDefaultAsync();
                            if (price != null && price != 0) item.Price = (double)price;
                        }
                        item.S1Title = int.Parse(item.S1Title) == 0 ? "" : db_sp.Find(e => e.Id == int.Parse(item.S1Title))?.Title;
                        item.S2Title = int.Parse(item.S2Title) == 0 ? "" : db_sp.Find(e => e.Id == int.Parse(item.S2Title))?.Title;
                    }
                }
                else throw new Exception("查無購物車資料");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return output;
        }
        public async Task<ShoppingCartGetDrop> GetDropOne(long id, bool isorder)
        {
            try
            {
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

                var db_sc = db.ShoppingCarts.Where(e => e.Id == id && e.IsOrder == isorder).FirstOrDefault();
                var db_ps = db.Prod_Stocks.Where(e => e.Id == db_sc.FK_PSid).FirstOrDefault();
                var db_prod = db.Prods.Where(e => e.FK_WebsiteId == WebsiteId && e.Id == db_ps.FK_Pid).FirstOrDefault();
                var db_price = db.Prod_Prices.Where(e => e.FK_PSId == db_ps.Id).ToList();
                if (db_sc != null)
                {
                    var checkToken = await tokenAppService.CheckToken(null);
                    if (checkToken.IsLogin)
                    {
                        Guid UUID = await tokenAppService.GetUUID();
                        var role = await db.MappingUserAndRoles.Where(e => e.UUID == UUID).Select(e => e.RoleId).FirstOrDefaultAsync();
                        role = role == 0 ? 1 : role;
                        var temp_price = new List<Prod_Price>();
                        foreach (var price in db_price)
                        {
                            if (price.FK_RId == role) temp_price.Add(price);
                        }
                        if (temp_price.Count() > 0) db_price = temp_price;
                    }

                    ShoppingCartGetDrop output = new ShoppingCartGetDrop()
                    {
                        SCId = db_sc.Id,
                        PId = db_prod.Id,
                        Title = db_prod.Title,
                        S1Title = db_ps.FK_S1id.ToString(),
                        S2Title = db_ps.FK_S2id.ToString(),
                        Quantity = db_sc.Quantity,
                        Price = db_sc.Price > 0 ? db_sc.Price : db_price == null ? 0 : db_price[0].Price ?? 0,
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

                    var db_sp = await db.Prod_Specs.ToListAsync();

                    output.S1Title = int.Parse(output.S1Title) == 0 ? "" : db_sp.Find(e => e.Id == int.Parse(output.S1Title))?.Title;
                    output.S2Title = int.Parse(output.S2Title) == 0 ? "" : db_sp.Find(e => e.Id == int.Parse(output.S2Title))?.Title;

                    return output;
                }
                else throw new Exception("查無購物車資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        // 改寫部分 後續會將舊程式碼移除
        public async Task<List<ShoppingCartDisplayDto>> GetDisplay(List<long> scids)
        {
            List<ShoppingCartDisplayDto> output = new List<ShoppingCartDisplayDto>();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();
            var token = await tokenAppService.CheckToken(null);
            Guid UUID = await tokenAppService.GetUUID();
            long roleid = 1;
            if (token != null && token.IsLogin)
            {
                var temp_roleid = await db.MappingUserAndRoles.Where(e => e.UUID == UUID).Select(e => e.Id).FirstOrDefaultAsync();
                if (temp_roleid != 0) roleid = temp_roleid;
            }

            try
            {
                var shoppingCarts = await db.ShoppingCarts.Include(e => e.Prod_Stock).ThenInclude(e => e.Prod).Where(e => scids.Contains(e.Id)).ToListAsync();

                foreach (var shoppingCart in shoppingCarts)
                {
                    var temp_output = mapper.Map<ShoppingCartDisplayDto>(shoppingCart);

                    temp_output.Title = shoppingCart.Prod_Stock?.Prod?.Title ?? "";

                    var pid = shoppingCart.Prod_Stock?.Prod?.Id;
                    temp_output.PId = pid ?? 0;
                    var imagepath = await (from fu in db.FileUploads
                                           join fb in db.FileBinds on fu.Id equals fb.FK_FileUploadId
                                           where fb.Sid == pid && fb.type == (int)FileBindTypeEnum.產品
                                           where fu.FK_WebsiteId == WebsiteId
                                           orderby fb.SerNo
                                           orderby fb.CreationTime
                                           select fu.DownloadFileName).FirstOrDefaultAsync();
                    temp_output.ImagePath = imagepath?.ToString() ?? "";

                    var db_sp = await db.Prod_Specs.ToListAsync();
                    if (db_sp.Any())
                    {
                        temp_output.S1Title = shoppingCart.FK_S1id != null ? db_sp.Find(e => e.Id == shoppingCart.FK_S1id)?.Title ?? "" : "";
                        temp_output.S2Title = shoppingCart.FK_S2id != null ? db_sp.Find(e => e.Id == shoppingCart.FK_S2id)?.Title ?? "" : "";
                    }
                    var psid = shoppingCart.Prod_Stock?.Id;
                    var db_price = await db.Prod_Prices.Where(e => e.FK_PSId == psid).ToListAsync();
                    if (db_price.Any())
                    {
                        if (roleid == 1)
                        {
                            var temp_price = db_price[0]?.Price?.ToString();
                            temp_output.DynamicPrice = temp_price ?? "0";
                        }
                        else
                        {
                            var temp_price = db_price.Find(e => e.FK_RId == roleid)?.Price?.ToString();
                            if (temp_price == null)
                            {
                                temp_price = db_price[0]?.Price?.ToString();
                                temp_output.DynamicPrice = temp_price ?? "0";
                            }
                            else temp_output.DynamicPrice = temp_price;
                        }
                    }

                    if (temp_output.Price == "") temp_output.Price = temp_output.DynamicPrice;
                    var subtotal = int.Parse(temp_output.Price) * int.Parse(temp_output.Quantity);

                    temp_output.Price = int.Parse(temp_output.Price).ToString("#,##0");
                    temp_output.Subtotal = subtotal.ToString("#,##0");
                    temp_output.Bonus = (shoppingCart.Bonus ?? 0).ToString("#,##0");

                    temp_output.Describe = shoppingCart.Prod_Stock?.Prod?.Description ?? "";

                    output.Add(temp_output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ShoppingCart=>GetDisplay回傳資料：{ex.Message}");
            }
            return output;
        }
        public async Task<ResponseMessageDto> Reorder(List<long> scids)
        {
            ResponseMessageDto output = new ResponseMessageDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var StockAllNull = true;

            try
            {
                var oldscs = await db.ShoppingCarts.Include(e => e.Prod_Stock).ThenInclude(e => e.Prod).Where(e => scids.Contains(e.Id)).ToListAsync();
                if (oldscs.Any())
                {
                    var tags = await (from ta in db.Tag_Associates
                                      join t in db.Tags on ta.FK_TId equals t.Id
                                      where t.Title == "售完" && t.FK_WebsiteId == WebsiteId && ta.Type == TagAssociateTypeEnum.商品
                                      select ta).ToListAsync();
                    foreach (var oldsc in oldscs)
                    {
                        if (!tags.Any() || (tags.Any() && tags.Find(e => e.FK_AId == oldsc.Prod_Stock.Prod.Id) == null))
                        {
                            if (!oldsc.Prod_Stock.Prod.RemovedFromShelves && oldsc.Prod_Stock.Stock > 0)
                            {
                                ShoppingCartAddUpDto newsc = new ShoppingCartAddUpDto();
                                newsc.Id = null;
                                newsc.FK_PSid = oldsc.FK_PSid;
                                if (newsc.Quantity > oldsc.Prod_Stock.Stock) newsc.Quantity = (int)oldsc.Prod_Stock.Stock;
                                else newsc.Quantity = oldsc.Quantity;
                                var temp_response = await AddUp(newsc);
                                if (temp_response.Success) StockAllNull = false;
                                else throw new Exception(temp_response.Message);
                            }
                        }
                    }
                    if (!StockAllNull) output.Success = true;
                    else throw new Exception("訂單中商品皆已無庫存或已下架");
                }
                else throw new Exception("查無舊購物車資料");
            }
            catch (Exception ex)
            {
                output.Error = "Error";
                output.Message = ex.Message;
            }
            return output;
        }
        public async Task<List<ShoppingCartDisplayDto>> CheckStockPrice(List<long> scids)
        {
            List<ShoppingCartDisplayDto> output = new List<ShoppingCartDisplayDto>();
            try
            {
                var temp_outputs = await GetDisplay(scids);
                if (temp_outputs.Any())
                {
                    output = temp_outputs;
                }
                else throw new Exception("查無訂單資訊");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ShoppingCart=>CheckStockPrice回傳資料：{ex.Message}");
            }
            return output;
        }
        public async Task<ResponseMessageDto> DeleteDrop(long id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var result = await db.ShoppingCarts.Where(e => e.Id == id).FirstOrDefaultAsync();

                if (result != null)
                {
                    var db_ps = await db.Prod_Stocks.Where(e => e.Id == result.FK_PSid).FirstOrDefaultAsync();
                    if (db_ps != null)
                    {
                        result.IsDeleted = true;
                        result.DeletionTime = DateTime.Now;
                        result.DeleterUserId = result.CreatorUserId;

                        db_ps.Stock += result.Quantity;
                        db.SaveChanges();
                        output.Success = true;
                    }
                    else throw new Exception("查無商品資料");
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
