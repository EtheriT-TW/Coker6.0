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
            Core.Models.JsonObject? header = await db.JsonObjects
                .IgnoreQueryFilters()
                .Where(e => e.CacheKey == dto.CacheKey)
                .Where(e => e.FK_WebsiteId == dto.FK_WebsiteId)
                .Where(e => e.FK_AId == dto.FK_AId)
                .FirstOrDefaultAsync();
            string jsonStr = dto.Json;
            if (header == null)
            {
                header = new Core.Models.JsonObject
                {
                    CacheKey = dto.CacheKey,
                    FK_WebsiteId = dto.FK_WebsiteId.Value,
                    FK_AId = dto.FK_AId
                };
                db.JsonObjects.Add(header);
            }
            else if (header.IsDeleted)
            {
                header.IsDeleted = false;
                header.DeletionTime = null;
                header.DeleterUserId = null;
            }
            header.Json = jsonStr;
            header.Version = dto.CacheVersion;
            try
            {
                await loginUserData.SaveChanges(header);
            }
            catch (DbUpdateException) when (header.Id == 0)
            {
                // 多台主機同時建立同一份快照時，唯一索引只允許一筆；
                // 失敗的一方改為讀取已建立資料並更新，避免快取重建請求直接失敗。
                db.Entry(header).State = EntityState.Detached;
                var existing = await db.JsonObjects
                    .IgnoreQueryFilters()
                    .Where(e => e.CacheKey == dto.CacheKey
                        && e.FK_WebsiteId == dto.FK_WebsiteId
                        && e.FK_AId == dto.FK_AId)
                    .FirstAsync();
                existing.IsDeleted = false;
                existing.DeletionTime = null;
                existing.DeleterUserId = null;
                existing.Json = jsonStr;
                existing.Version = dto.CacheVersion;
                await loginUserData.SaveChanges(existing);
            }
            return response;
        }

        public async Task RemoveAsync(long websiteId, string cacheKey, long? fkAId = null)
        {
            var header = await db.JsonObjects
                .Where(e => e.FK_WebsiteId == websiteId
                    && e.CacheKey == cacheKey
                    && e.FK_AId == fkAId)
                .FirstOrDefaultAsync();

            if (header == null) return;

            header.IsDeleted = true;
            await loginUserData.SaveChanges(header);
        }
    }
}
