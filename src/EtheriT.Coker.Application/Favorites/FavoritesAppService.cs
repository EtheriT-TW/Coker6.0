using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Favorites;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EtheriT.Coker.Application.Favorites
{
    public class FavoritesAppService : IFavoritesAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITokenAppService tokenAppService;
        public FavoritesAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService)
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tokenAppService = tokenAppService;
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
                    Core.Models.Favorites favorites = new Core.Models.Favorites()
                    {
                        UUID = UUID,
                        FK_PId = Pid,
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
                else throw new Exception("查無商品資料");
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
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

                    Core.Models.Prod_Log prod_log = new Core.Models.Prod_Log
                    {
                        FK_Pid = favorites.FK_PId,
                        FK_UserId = userid,
                        UUID = UUID,
                        Action = (int)LogActionEnum.移除收藏,
                        Db_Name = "Favorites"
                    };
                    db.Prod_Logs.Add(prod_log);
                    await loginUserData.SaveChanges(prod_log);

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
    }
}
