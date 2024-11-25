using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
                var tempsc = await db.ShoppingCarts.Where(e => e.UUID == TempUUID && !e.IsOrder).ToListAsync();
                var usersc = await db.ShoppingCarts.Where(e => e.UUID == UserUUID && !e.IsOrder).ToListAsync();
                if (tempsc.Any())
                {
                    for (var i = 0; i < tempsc.Count; i++)
                    {
                        tempsc[i].UUID = UserUUID;
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
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var Token = await tokenAppService.CheckToken();

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
                        if (db_ps.Stock >= dto.Quantity)
                        {
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

                                db_ps.Stock -= dto.Quantity;

                                db.SaveChanges();
                                output.Message = "N" + sc.Id.ToString();
                            }
                            else
                            {
                                db_shoppingcart.Quantity += dto.Quantity;
                                db_shoppingcart.LastModificationTime = DateTime.Now;
                                db_shoppingcart.LastModifierUserId = db_shoppingcart.CreatorUserId;
                                db_ps.Stock -= dto.Quantity;

                                db.SaveChanges();
                                output.Message = "U" + db_shoppingcart.Id.ToString();
                            }
                        }
                        else throw new Exception("商品庫存不足");

                        output.Success = true;
                    }
                }
                else throw new Exception("查無商品資料");
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
                var token = await tokenAppService.CheckToken();
                var db_shoppingcart = db.ShoppingCarts.Where(e => e.Id == dto.Id && !e.IsOrder).FirstOrDefault();
                var db_token = db.Tokens.Where(e => e.id == token.RefreshToken).FirstOrDefault();

                if (db_shoppingcart != null && db_token != null)
                {
                    var db_ps = await db.Prod_Stocks.Where(e => e.Id == db_shoppingcart.FK_PSid).FirstOrDefaultAsync();
                    if (db_ps != null)
                    {
                        db_ps.Stock += db_shoppingcart.Quantity - dto.Quantity;
                        db_shoppingcart.Quantity = dto.Quantity;
                        db_shoppingcart.LastModificationTime = DateTime.Now;
                        db_shoppingcart.LastModifierUserId = db_shoppingcart.CreatorUserId;
                        output.Message = db_shoppingcart.Id.ToString();
                        db.SaveChanges();
                        output.Success = true;
                    }
                    else throw new Exception("查無商品資訊");
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
            List<ShoppingCartGetAllDto> output = new List<ShoppingCartGetAllDto>();
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var Token = await tokenAppService.CheckToken();

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

                var token = await tokenAppService.CheckToken();
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
                    var checkToken = await tokenAppService.CheckToken();
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
                        Price = db_price == null ? 0 : db_price[0].Price ?? 0,
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
            var token = await tokenAppService.CheckToken();
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
                            temp_output.Price = temp_price ?? "0";
                        }
                        else
                        {
                            var temp_price = db_price.Find(e => e.FK_RId == roleid)?.Price?.ToString();
                            if (temp_price == null)
                            {
                                temp_price = db_price[0]?.Price?.ToString();
                                temp_output.Price = temp_price ?? "0";
                            }
                            else temp_output.Price = temp_price;
                        }

                        var subtotal = int.Parse(temp_output.Price) * int.Parse(temp_output.Quantity);

                        temp_output.Price = int.Parse(temp_output.Price).ToString("#,##0");
                        temp_output.Subtotal = subtotal.ToString("#,##0");
                        temp_output.Bonus = (shoppingCart.Bonus ?? 0).ToString("#,##0");

                        temp_output.Describe = shoppingCart.Prod_Stock?.Prod?.Description ?? "";

                        output.Add(temp_output);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ShoppingCart=>GetDisplay回傳資料：{ex.Message}");
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
