using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.StoreSet
{
    public class StoreSetAppService : IStoreSetAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly MarkdownPipeline _mdPipeline;
        public StoreSetAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            _mdPipeline = new MarkdownPipelineBuilder()
                .DisableHtml()            // 建議：避免有人在 Markdown 偷塞 HTML/XSS
                .UseAdvancedExtensions()  // 可選：要表格/清單等更完整支援就留著
                .Build();
        }

        public async Task<StoreSetResponseMessageDto> find(string key)
        {
            StoreSetResponseMessageDto output = new StoreSetResponseMessageDto();
            var result = await db.StoreSet.Where(e => !e.IsDeleted)
                                .Where(e => e.key == key)
                                .FirstOrDefaultAsync();
            if (result != null)
            {
                output.item = mapper.Map<StoreSetOutputDto>(result);
            }
            else output.Message = "查無資料";

            return output;
        }

        public async Task<StoreSetResponseMessageDto> getAll(List<long> StoreSetGroupId)
        {
            StoreSetResponseMessageDto output = new StoreSetResponseMessageDto();
            var level = await loginUserData.GetWebsiteLevel();
            var result = from g in db.StoreSetGroup.Where(e => !e.IsDeleted && StoreSetGroupId.Contains(e.Id))
                         select new StoreSetGroupOutputDto
                         {
                             Image = g.Image,
                             Description = g.Description!,
                             Title = g.Title,
                             storeSets = (from s in db.StoreSet.Where(e => (e.Level == null || level >= e.Level) && e.FK_StoreSetGroupId == g.Id).OrderBy(e => e.jobID)
                                          select new StoreSetOutputDto
                                          {
                                              key = s.key,
                                              name = s.name,
                                              maxlength = s.maxlength,
                                              memo = s.memo,
                                              pattern = s.pattern,
                                              type = s.type!,
                                              storeSetItems = (
                                                 from item in db.StoreSetItems.Where(e => (e.Level == null || level >= e.Level) && e.FK_StoreSetId == s.Id)
                                                 select new StoreSetItemOutputDto
                                                 {
                                                     Key = item.Key,
                                                     Value = item.Value,
                                                 }
                                             ).ToList()
                                          }).ToList()
                         };
            if (result != null)
            {
                output.storeGroups = mapper.Map<List<StoreSetGroupOutputDto>>(result);
                output.Success = true;
            }
            else output.Message = "資料為空";

            return output;
        }
        public async Task<StoreSetResponseMessageDto> getValues(StoreSetGetValueInput dto)
        {
            StoreSetResponseMessageDto output;
            long websiteId;
            if (dto.SiteId == null || dto.SiteId == 0) websiteId = await loginUserData.GetWebsiteId();
            else websiteId = dto.SiteId.Value;
            if (dto.StoreSetGroupId != null) output = await getValuesByGroupId(dto.StoreSetGroupId.Value, websiteId, dto.RenderTextareaAsHtml);
            else if (dto.keys != null && dto.keys.Count > 0) output = await getValuesByKeys(dto.keys, websiteId, dto.RenderTextareaAsHtml);
            else if (!string.IsNullOrEmpty(dto.key)) output = await getValueByKey(dto.key, websiteId, dto.RenderTextareaAsHtml);
            else
            {
                output = new StoreSetResponseMessageDto { Message = "缺少搜尋條件" };
            }
            return output;
        }
        public async Task<StoreSetResponseMessageDto> getGroupStructure(StoreSetGetValueInput dto) {
            StoreSetResponseMessageDto output = new StoreSetResponseMessageDto();
            try {
                if (dto.StoreSetGroupId != null)
                {
                    var result = db.StoreSetGroup.Include(e => e.StoreSets).ThenInclude(e => e.storeSetItems)
                        .Where(e => e.Id == dto.StoreSetGroupId.Value);

                    output.storeGroups = mapper.Map<List<StoreSetGroupOutputDto>>(await result.ToListAsync());
                    output.Success = true;
                }
                else throw new Exception("缺少搜尋條件");
            }
            catch(Exception ex)
            {
                output.Message = ex.Message;
            }
            return output;
        }
        private async Task<StoreSetResponseMessageDto> getValuesByKeys(List<string> keys, long websiteId, bool renderTextareaAsHtml)
        {
            StoreSetResponseMessageDto output = new StoreSetResponseMessageDto();

            try
            {
                // 1) 先抓 StoreSet（確保「沒 detail 也能回 default」）
                var storeSets = await db.StoreSet
                    .Include(s => s.storeSetItems)
                    .Where(s => !s.IsDeleted)
                    .Where(s => keys.Contains(s.key))
                    .ToListAsync();

                var storeSetByKey = storeSets.ToDictionary(s => s.key, s => s);

                // 2) 再抓 detail（有存值的才會有）
                var details = await db.StoreSetDetail
                    .Include(d => d.StoreSet)
                    .Where(d => !d.IsDeleted)
                    .Where(d => d.FK_WebsiteId == websiteId)
                    .Where(d => keys.Contains(d.StoreSet.key))
                    .ToListAsync();

                // 若同 key 有多筆 detail，保留最新一筆（依你 CreationTime）
                var detailByKey = details
                    .GroupBy(d => d.StoreSet.key)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreationTime).First());

                // 3) 依 keys 的順序輸出（前端通常期待順序一致）
                var list = new List<StoreSetDetailOutputDto>();

                foreach (var key in keys)
                {
                    storeSetByKey.TryGetValue(key, out var s);
                    detailByKey.TryGetValue(key, out var d);

                    // 若 key 根本不存在 StoreSet：仍回一筆避免前端炸
                    if (s == null)
                    {
                        list.Add(new StoreSetDetailOutputDto { key = key, value = new List<string> { string.Empty } });
                        continue;
                    }

                    var values = ResolveEffectiveValues(s, d, renderTextareaAsHtml);
                    list.Add(new StoreSetDetailOutputDto { key = s.key, value = values });
                }

                output.storeSetDetails = list;
                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }

            return output;
        }
        private async Task<StoreSetResponseMessageDto> getValueByKey(
            string key,
            long websiteId,
            bool renderTextareaAsHtml
        )
        {
            // 直接走通用邏輯
            var result = await getValuesByKeys(
                new List<string> { key },
                websiteId,
                renderTextareaAsHtml
            );

            if (result.Success && result.storeSetDetails != null)
            {
                result.detailItem = result.storeSetDetails.FirstOrDefault();
                result.storeSetDetails = null; // 可選：避免前端誤用
            }

            return result;
        }
        private async Task<StoreSetResponseMessageDto> getValuesByGroupId(long storeSetGroupId, long websiteId, bool renderTextareaAsHtml)
        {
            StoreSetResponseMessageDto output = new StoreSetResponseMessageDto();

            try
            {
                // 依網站等級過濾（讓回傳 key 和 getAll/getGroupStructure 的結構一致）
                var level = await loginUserData.GetWebsiteLevel();

                // 先拿該群組的所有欄位 key（這一步是 DefaultValue 能生效的核心）
                var keys = await db.StoreSet
                    .Where(s => !s.IsDeleted)
                    .Where(s => s.FK_StoreSetGroupId == storeSetGroupId)
                    .Where(s => s.Level == null || level >= s.Level)
                    .OrderBy(s => s.jobID)
                    .Select(s => s.key)
                    .ToListAsync();

                // 沒有欄位就直接回空（仍 Success=true）
                if (keys.Count == 0)
                {
                    output.storeSetDetails = new List<StoreSetDetailOutputDto>();
                    output.Success = true;
                    return output;
                }

                // 交給唯一的值解析邏輯（含：detail > isDefault > DefaultValue；textarea 可選 markdown->html）
                return await getValuesByKeys(keys, websiteId, renderTextareaAsHtml);
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                return output;
            }
        }

        public async Task<ResponseMessageDto> CreateOrUpdate(List<StoreSetDetailOutputDto> datas)
        {
            ResponseMessageDto output = new ResponseMessageDto();
            long websiteId = await loginUserData.GetWebsiteId();
            long userId = await loginUserData.GetUserId();
            var keys = datas.Select(e => e.key);
            var updateItems = await db.StoreSetDetail.Include(e => e.StoreSet)
                .Where(e => !e.IsDeleted)
                .Where(e => keys.Contains(e.StoreSet.key))
                .Where(e => e.FK_WebsiteId == websiteId)
                .ToListAsync();
            try
            {
                if (updateItems.Count != 0)
                {
                    for (int i = 0; i < updateItems.Count; i++)
                    {
                        StoreSetDetailOutputDto? item = datas.Find(e => e.key == updateItems[i].StoreSet.key);
                        if (item != null)
                        {
                            mapper.Map(item, updateItems[i]);
                            updateItems[i].value = String.Join(", ", item.value!.ToArray());
                        }
                    }
                    await db.SaveChangesAsync();
                }
                if (updateItems.Count != datas.Count)
                {
                    var hasKeys = updateItems.Select(e => e.StoreSet.key).ToList();
                    var notHas = datas.Where(e => !hasKeys.Contains(e.key)).ToList();
                    var notHasKeys = notHas.Select(e => e.key).ToList();
                    if (notHas.Count != 0)
                    {
                        var data = await (from a in db.StoreSet
                                          where notHasKeys.Contains(a.key)
                                          select new StoreSetDetail
                                          {
                                              FK_WebsiteId = websiteId,
                                              CreatorUserId = userId,
                                              CreationTime = DateTime.Now,
                                              FK_StoreSetId = a.Id,
                                              IsDeleted = false,
                                              StoreSet = a
                                          }).ToListAsync();
                        if (data.Any())
                        {
                            data.ForEach(e =>
                            {
                                StoreSetDetailOutputDto? item = notHas.Find(n => n.key == e.StoreSet.key);
                                if (item != null && item.value != null)
                                {
                                    e.value = String.Join(", ", item.value.ToArray());
                                }
                            });
                            db.StoreSetDetail.AddRange(data);
                            await db.SaveChangesAsync();
                        }
                    }
                }
                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }
            return output;
        }
        // 依你的系統語意：
        // - 有 detail 且有值：用 detail
        // - 沒 detail / 無有效值：用 StoreSet.DefaultValue 或 storeSetItems.isDefault
        private List<string> ResolveEffectiveValues(Core.Models.StoreSet s, StoreSetDetail? d, bool renderTextareaAsHtml)
        {
            // 1) 先嘗試拿 detail 值
            var fromDetail = ParseDetailValues(s, d?.value);
            if (HasMeaningfulValue(fromDetail))
            {
                // textarea + 前台顯示 → Markdown 轉 HTML（不回寫 DB）
                if (s.type == SeoSetDataTypeEnum.textarea && renderTextareaAsHtml)
                {
                    var normalized = NormalizeMarkdownForDisplay(fromDetail![0]);
                    return new List<string> { Markdown.ToHtml(normalized, _mdPipeline) };
                }

                return fromDetail!;
            }

            // 2) detail 沒值 → 用 default
            // 2.1 選項類：用 storeSetItems.isDefault
            if (s.storeSetItems != null && s.storeSetItems.Count > 0)
            {
                var defKeys = s.storeSetItems
                    .Where(i => i.IsDefault)
                    .Select(i => i.Key?.ToString() ?? "")
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (defKeys.Count > 0)
                    return defKeys; // checkbox 多選會回多個；radio/select 通常只會有一個
            }

            // 2.2 非選項類：用 StoreSet.DefaultValue
            // 注意：你已經加了 DefaultValue，這裡直接用即可
            var dv = (s.DefaultValue ?? "").Trim();
            if (!string.IsNullOrEmpty(dv))
            {
                if (s.type == SeoSetDataTypeEnum.textarea && renderTextareaAsHtml)
                {
                    var normalized = NormalizeMarkdownForDisplay(dv);
                    return new List<string> { Markdown.ToHtml(normalized, _mdPipeline) };
                }

                return new List<string> { dv };
            }

            // 3) 仍然沒有 → 保底回 [""]（維持你既有前端習慣，不讓 null/[] 炸）
            return new List<string> { string.Empty };
        }
        private static List<string>? ParseDetailValues(Core.Models.StoreSet s, string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            // 非 textarea 依你原邏輯 split
            if (s.type != SeoSetDataTypeEnum.textarea)
            {
                var parts = raw.Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                return parts.Count > 0 ? parts : null;
            }

            // textarea：保留原字串（含換行/Markdown）
            return new List<string> { raw };
        }

        private static bool HasMeaningfulValue(List<string>? v)
        {
            if (v == null || v.Count == 0) return false;
            if (v.Count == 1 && string.IsNullOrWhiteSpace(v[0])) return false;
            return true;
        }
        private static string NormalizeMarkdownForDisplay(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // 統一換行
            text = text.Replace("\r\n", "\n");

            // 以「空白行」切段（保留段落語意）
            var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.None);

            for (int i = 0; i < paragraphs.Length; i++)
            {
                // 段內：把單換行轉成 Markdown 的 hard line break（兩個空白 + \n）
                // 這樣同段內每行都會變 <br>
                var lines = paragraphs[i].Split('\n');
                paragraphs[i] = string.Join("  \n", lines);
            }

            // 段落之間用空白行連回去（讓 Markdown 產生 <p>）
            return string.Join("\n\n", paragraphs);
        }

    }
}
