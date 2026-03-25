using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.JsonObject;
using EtheriT.Coker.Application.Shared.JsonObject;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.JsonObject
{
    public class JsonObjectAppService: IJsonObjectAppService
    {
        private readonly string ApplicationName;
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        public JsonObjectAppService(CokerDbContext db, LoginUserData loginUserData)
        {
            this.db = db;
            this.ApplicationName = "JsonObjectAppService";
            this.loginUserData = loginUserData;
        }
        public async Task<ResponseMessageDto> AddUp(JsonObjectAddDto dto) {
            ResponseMessageDto response = new ResponseMessageDto();
            if (dto.FK_WebsiteId == null)
            {
                dto.FK_WebsiteId = await loginUserData.GetWebsiteId();
            }
            Core.Models.JsonObject? header = await db.JsonObjects.Where(e => e.CacheKey == dto.CacheKey).Where(e => e.FK_WebsiteId == dto.FK_WebsiteId).FirstOrDefaultAsync();
            string jsonStr = dto.Json;
            if (header == null)
            {
                header = new Core.Models.JsonObject
                {
                    CacheKey = dto.CacheKey,
                    FK_WebsiteId = dto.FK_WebsiteId.Value
                };
                db.JsonObjects.Add(header);
            }
            header.Json = jsonStr;
            header.Version = dto.CacheVersion;
            await loginUserData.SaveChanges(header);
            return response;
        }
    }
}
