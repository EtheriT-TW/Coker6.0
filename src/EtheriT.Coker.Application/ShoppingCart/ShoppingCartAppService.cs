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
                var userid = await db.FrontUsers.Where(e => e.UUID == UserUUID).Select(e => e.FK_User).FirstOrDefaultAsync();
                var oldsc = await db.ShoppingCarts.Where(e => e.UUID == TempUUID && e.IsOrder).ToListAsync();
                var tempsc = await db.ShoppingCarts.Where(e => e.UUID == TempUUID && !e.IsOrder).ToListAsync();
                var usersc = await db.ShoppingCarts.Where(e => e.UUID == UserUUID && !e.IsOrder).ToListAsync();
                var timenow = DateTime.Now;
                if (oldsc.Any())
                {
                    for (var i = 0; i < oldsc.Count; i++)
                    {
                        oldsc[i].UUID = UserUUID;
                        oldsc[i].FK_Uid = userid ?? 0;
                        oldsc[i].LastModifierUserId = userid ?? 0;
                        oldsc[i].LastModificationTime = timenow;
                    }
                }
                if (tempsc.Any())
                {
                    for (var i = 0; i < tempsc.Count; i++)
                    {
                        tempsc[i].UUID = UserUUID;
                        tempsc[i].FK_Uid = userid ?? 0;
                        tempsc[i].LastModifierUserId = userid ?? 0;
                        tempsc[i].LastModificationTime = timenow;
                    }
                    if (usersc.Any())
                    {
                        for (var i = 0; i < tempsc.Count; i++)
                        {
                            if (usersc.Find(e => e.FK_PSid == tempsc[i].FK_PSid) != null)
                            {
                                usersc[usersc.FindIndex(e => e.FK_PSid == tempsc[i].FK_PSid)].Quantity += tempsc[i].Quantity;
                                usersc[i].LastModifierUserId = userid ?? 0;
                                usersc[i].LastModificationTime = timenow;
                                tempsc[i].IsDeleted = true;
                                tempsc[i].DeletionTime = timenow;
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
                        if (pro_stock.Stock > 0)
                        {
                            Core.Models.ShoppingCart? sc = new Core.Models.ShoppingCart();

                            if (dto.Id != null) sc = await db.ShoppingCarts.Where(e => e.Id == dto.Id).FirstOrDefaultAsync();
                            else sc = await db.ShoppingCarts.Where(e => e.UUID == UUID && e.FK_PSid == pro_stock.Id && !e.IsOrder).FirstOrDefaultAsync();

                            if (sc == null)
                            {
                                sc = mapper.Map<Core.Models.ShoppingCart>(pro_stock);
                                sc.Id = 0;
                                sc.Price = 0;
                                if (pro_stock.Stock >= dto.Quantity)
                                {
                                    sc.Quantity = dto.Quantity;
                                    sc.OldQuantity = dto.Quantity;
                                    pro_stock.Stock -= dto.Quantity;
                                }
                                else
                                {
                                    sc.Quantity = (int)pro_stock.Stock;
                                    sc.OldQuantity = (int)pro_stock.Stock;
                                    pro_stock.Stock = 0;
                                }
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

                                db.ShoppingCarts.Add(sc);
                                db.SaveChanges();
                                response.Message = "N" + sc.Id.ToString();
                            }
                            else
                            {
                                if (sc != null)
                                {
                                    if (pro_stock.Stock >= dto.Quantity)
                                    {
                                        sc.Quantity += dto.Quantity;
                                        sc.OldQuantity += sc.Quantity;
                                        pro_stock.Stock -= dto.Quantity;
                                    }
                                    else
                                    {
                                        sc.Quantity += (int)pro_stock.Stock;
                                        sc.OldQuantity += (int)pro_stock.Stock;
                                        pro_stock.Stock = 0;
                                    }
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
                    }
                    else throw new Exception("查無商品規格");
                }
                else throw new Exception("取得Token或UUID發生錯誤");
            }
            catch (Exception ex)
            {
                response.Error = "Erro";
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> QuantityUpdate(ShoppingQuantityUpdateDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var sc = await db.ShoppingCarts.Where(e => e.Id == dto.Id && !e.IsOrder).FirstOrDefaultAsync();

                if (sc != null)
                {
                    var pro_stock = await db.Prod_Stocks.Where(e => e.Id == sc.FK_PSid).FirstOrDefaultAsync();
                    if (pro_stock != null)
                    {
                        var quantity = dto.Quantity - sc.Quantity;
                        if (quantity > pro_stock.Stock) throw new Exception("庫存不足");
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
        public async Task<List<ShoppingCartDisplayDto>> GetAll()
        {
            List<ShoppingCartDisplayDto> output = new List<ShoppingCartDisplayDto>();
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var scs = await db.ShoppingCarts.Where(e => e.UUID == UUID && !e.IsOrder).ToListAsync();
                if (scs.Any())
                {
                    var scids = scs.Select(e => e.Id).ToList();
                    output = await GetDisplay(scids);
                }
                else throw new Exception("查無購物車資料");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ShoppingCart=>GetDropOne回傳資料：{ex.Message}");
            }
            return output;
        }
        public async Task<ShoppingCartDisplayDto> GetDropOne(long id, bool isorder)
        {
            ShoppingCartDisplayDto output = new ShoppingCartDisplayDto();
            try
            {
                var shoppingcart = await db.ShoppingCarts.Where(e => e.Id == id && e.IsOrder == isorder).FirstOrDefaultAsync();
                if (shoppingcart != null)
                {
                    var temp_output = await GetDisplay(new List<long> { shoppingcart.Id });
                    if (temp_output.Any())
                    {
                        output = temp_output[0];
                    }
                    else throw new Exception("查無購物車資料");
                }
                else throw new Exception("查無購物車資料");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ShoppingCart=>GetDropOne回傳資料：{ex.Message}");
            }
            return output;
        }
        public async Task<List<ShoppingCartDisplayDto>> GetDisplay(List<long> scids)
        {
            List<ShoppingCartDisplayDto> output = new List<ShoppingCartDisplayDto>();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();
            var Token = await tokenAppService.CheckToken(null);
            Guid UUID = await tokenAppService.GetUUID();
            long roleid = 1;
            try
            {
                if (Token != null && Token.IsLogin)
                {
                    var temp_roleid = await db.MappingUserAndRoles.Where(e => e.UUID == UUID).Select(e => e.Id).FirstOrDefaultAsync();
                    if (temp_roleid != 0) roleid = temp_roleid;
                }

                var shoppingCarts = await db.ShoppingCarts.Include(e => e.Prod_Stock).ThenInclude(e => e.Prod).Where(e => scids.Contains(e.Id)).ToListAsync();

                foreach (var shoppingCart in shoppingCarts)
                {
                    var temp_output = mapper.Map<ShoppingCartDisplayDto>(shoppingCart);

                    temp_output.Stock = shoppingCart.Prod_Stock?.Stock ?? 0;

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
                    if (temp_output.ImagePath != "") temp_output.ImagePath = $"{temp_output.ImagePath}";

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
                        if (roleid == 1) temp_output.DynamicPrice = (int)(db_price[0]?.Price);
                        else
                        {
                            var temp_price = db_price.Find(e => e.FK_RId == roleid)?.Price;
                            if (temp_price == null) temp_output.DynamicPrice = (int)(db_price[0]?.Price ?? 0);
                            else temp_output.DynamicPrice = (int)(temp_price ?? 0);
                        }
                    }

                    if (temp_output.Price == 0) temp_output.Price = temp_output.DynamicPrice;
                    temp_output.Subtotal = temp_output.Price * temp_output.Quantity;

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
                    foreach (var oldsc in oldscs)
                    {
                        if (oldsc.Prod_Stock.Prod.Status != ProdStatusEnum.售完)
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
