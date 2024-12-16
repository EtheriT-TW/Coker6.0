using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Favorites;
using EtheriT.Coker.Application.Token;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.Favorites;
using EtheriT.Coker.Core.Models;

namespace EtheriT.Coker.Application.Favorites
{
    public class FavoritesAppService : IFavoritesAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITokenAppService tokenAppService;
        private readonly IConfiguration configuration;
        public FavoritesAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService,
            IConfiguration configuration)
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tokenAppService = tokenAppService;
            this.configuration = configuration;
        }

        public async Task<ResponseMessageDto> Add(long Pid)
        {
            ResponseMessageDto response = new ResponseMessageDto() { Success = false };

            try
            {
                Guid UUID = await tokenAppService.GetUUID();

                var prod = await db.Prods.Where(e => e.Id == Pid).FirstOrDefaultAsync();
                if (prod != null)
                {
                    var favorite = await db.Favorites.Where(e => e.FK_AssocId == Pid && e.UUID == UUID && e.Type == (int)FavoritesTypeEnum.商品).FirstOrDefaultAsync();
                    if (favorite == null)
                    {
                        Core.Models.Favorites favorites = new Core.Models.Favorites()
                        {
                            UUID = UUID,
                            FK_AssocId = Pid,
                            Type = (int)FavoritesTypeEnum.商品
                        };
                        db.Favorites.Add(favorites);
                        await loginUserData.SaveChanges(favorites);

                        Core.Models.Prod_Log prod_log = new Core.Models.Prod_Log
                        {
                            FK_Pid = Pid,
                            FK_UserId = await db.FrontUsers.Where(e => e.UUID == UUID).Select(e => e.FK_User).FirstOrDefaultAsync(),
                            UUID = UUID,
                            Action = (int)LogActionEnum.加入收藏,
                            Db_Name = "Favorites"
                        };
                        db.Prod_Logs.Add(prod_log);
                        await loginUserData.SaveChanges(prod_log);

                        response.Success = true;
                        response.Message = favorites.Id.ToString();
                    }
                    else throw new Exception("商品已加入收藏");
                }
                else throw new Exception("查無商品資料");
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<FavoritesGetDisplayAllDto> GetDisplay(int page)
        {
            var output = new FavoritesGetDisplayAllDto();
            Guid UUID = await tokenAppService.GetUUID();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

            try
            {
                var favorites = await db.Favorites.Where(e => e.UUID == UUID).ToListAsync();
                if (favorites != null)
                {
                    var favorites_data = (from favorite in favorites
                                          join prod in db.Prods on favorite.FK_AssocId equals prod.Id
                                          where favorite.UUID == UUID && favorite.Type == (int)FavoritesTypeEnum.商品
                                          orderby favorite.Id descending
                                          select new FavoritesGetDisplayOneDto()
                                          {
                                              FId = favorite.Id,
                                              PId = prod.Id,
                                              Title = prod.Title,
                                              Introduction = prod.Introduction,
                                              Description = prod.Description,
                                              Link = "/product/" + prod.Id,
                                              Image = ((from f in db.FileBinds.Include(e => e.fileUpload)
                                                      .Where(e => e.fileUpload != null && e.fileUpload.FK_WebsiteId == WebsiteId)
                                                      .Where(e => e.fileUpload != null)
                                                      .Where(e => e.Sid == prod.Id && e.type == (int)FileBindTypeEnum.產品)
                                                      .OrderBy(e => e.SerNo).ThenBy(e => e.CreationTime)
                                                        select new DirectoryReleInfoDto
                                                        {
                                                            Link = f.fileUpload != null ? f.fileUpload.DownloadFileName ?? "" : ""
                                                        }).FirstOrDefault() ?? new DirectoryReleInfoDto()).Link,
                                              Price = null,
                                              ItemNo = prod.ItemNo,
                                          }).ToList();
                    output.Page_Total = (int)Math.Ceiling(favorites_data.Count / 8.0);
                    favorites_data = favorites_data.Skip((page - 1) * 8).Take(8).ToList();

                    var sotreset = await (from sd in db.StoreSetDetail
                                          join ss in db.StoreSet on sd.FK_StoreSetId equals ss.Id
                                          where sd.FK_WebsiteId == WebsiteId
                                          where ss.key == "storeBuyState"
                                          select sd.value).FirstOrDefaultAsync();

                    var showprice = !(sotreset == "noPayNoShow");

                    if (showprice)
                    {
                        for (var i = 0; i < favorites_data.Count; i++)
                        {
                            favorites_data[i].Price = new List<double>() { };
                            var prod_prices = await (from prod_stock in db.Prod_Stocks
                                                     join prod_price in db.Prod_Prices on prod_stock.Id equals prod_price.FK_PSId
                                                     where prod_stock.FK_Pid == favorites_data[i].PId
                                                     where prod_price.Price > 0
                                                     orderby prod_price descending
                                                     select prod_price).ToListAsync();
                            if (prod_prices.Count > 1)
                            {
                                favorites_data[i].Price.Add((double)prod_prices[0].Price);
                                favorites_data[i].Price.Add((double)prod_prices[prod_prices.Count - 1].Price);
                            }
                            else if (prod_prices.Count == 1)
                            {
                                favorites_data[i].Price.Add((double)prod_prices[0].Price);
                            }
                            else favorites_data[i].Price.Add(0);
                        }
                    }

                    output.Data = favorites_data;
                    output.Success = true;
                }
            }
            catch (Exception ex)
            {
                output.Error = ex.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> Delete(long Fid)
        {
            ResponseMessageDto response = new ResponseMessageDto() { Success = false };

            try
            {
                Guid UUID = await tokenAppService.GetUUID();

                var favorites = await db.Favorites.Where(e => e.Id == Fid).FirstOrDefaultAsync();
                if (favorites != null)
                {
                    var userid = await db.FrontUsers.Where(e => e.UUID == UUID).Select(e => e.FK_User).FirstOrDefaultAsync();

                    favorites.IsDeleted = true;
                    favorites.DeleterUserId = userid;
                    favorites.DeletionTime = DateTime.Now;
                    await loginUserData.SaveChanges(favorites);

                    if (favorites.Type == (int)FavoritesTypeEnum.商品)
                    {
                        Core.Models.Prod_Log prod_log = new Core.Models.Prod_Log
                        {
                            FK_Pid = favorites.FK_AssocId,
                            FK_UserId = userid,
                            UUID = UUID,
                            Action = (int)LogActionEnum.移除收藏,
                            Db_Name = "Favorites"
                        };
                        db.Prod_Logs.Add(prod_log);
                        await loginUserData.SaveChanges(prod_log);
                    }

                    response.Success = true;
                }
                else throw new Exception("查無收藏資料");
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> CheckIsFavorites(long Pid)
        {
            var response = new ResponseMessageDto();
            Guid UUID = await tokenAppService.GetUUID();

            try
            {
                var favorites = await db.Favorites.Where(e => e.UUID == UUID && e.FK_AssocId == Pid && e.Type == (int)FavoritesTypeEnum.商品).FirstOrDefaultAsync();
                if (favorites != null)
                {
                    response.Message = favorites.Id.ToString();
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
