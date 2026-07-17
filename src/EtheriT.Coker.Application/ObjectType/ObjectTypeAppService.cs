using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.ObjectType;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EtheriT.Coker.Application
{
    public class ObjectTypeAppService : IObjectTypeAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly string ApplicationName;
        private long websiteId;
        public ObjectTypeAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            IFileUploadAppService fileUploadAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.fileUploadAppService = fileUploadAppService;
            ApplicationName = "ObjectType";
        }
        public async Task<ObjectTypeGetAlldto> GetAll()
        {
            ObjectTypeGetAlldto response = new ObjectTypeGetAlldto();
            try
            {
                bool othersOnly = await loginUserData.isSystemUser();
                var user = await loginUserData.GetUser();
                if (user == null) throw new Exception("會員尚未登入");
                var result = db.ObjectTypes.Where(e => !e.IsDeleted);
                if (!othersOnly) result = result.Where(e => e.Id == 999);
                websiteId = await loginUserData.GetWebsiteId();
                result = result.OrderBy(e => e.SerNo);
                response.List = mapper.Map<List<ObjectTypeItemDto>>(result);
                foreach (var e in response.List) {
                    e.Children = await GetChild(e.Id);
                }
                response.Purposes = await db.ComponentPurposes
                    .AsNoTracking()
                    .Where(e => e.Visible)
                    .OrderBy(e => e.SerNo)
                    .ThenBy(e => e.Id)
                    .Select(e => new ComponentPurposeDto
                    {
                        Id = e.Id,
                        Code = e.Code,
                        Name = e.Name
                    })
                    .ToListAsync();

                var components = response.List
                    .SelectMany(e => e.Children ?? new List<ObjectTypeItemDto>())
                    .ToList();
                if (components.Count > 0)
                {
                    var images = await fileUploadAppService.getImgsFiles(new FileGetImgsInputDto
                    {
                        Sid = components.Select(e => e.Id).ToList(),
                        Type = (int)FileBindTypeEnum.元件圖片,
                        Size = 1
                    });
                    var imageByComponentId = images
                        .GroupBy(e => e.Sid)
                        .ToDictionary(e => e.Key, e => e.First());
                    foreach (var component in components)
                    {
                        if (!imageByComponentId.TryGetValue(component.Id, out var image)) continue;
                        component.ImgId = image.Id;
                        component.ImgUrl = image.Link;
                        component.ImgName = image.Name;
                    }
                }
                response.Success = true;
            }
            catch( Exception ex )
            {
                response.Success= false;
                response.Message= ex.Message;
            }
            return response;
        }
        private async Task<List<ObjectTypeItemDto>> GetChild(long type) {
            bool isSystemUser = await loginUserData.isSystemUser();
            var reg = await db.Html_Contents
                        .Include(e => e.HtmlContentPurposes)
                        .Where(e => e.Type == type)
                        .Where(e => isSystemUser || (e.Type == (int)ObjectTypeEnum.自訂 && e.FK_WebsiteId == websiteId))
                        .OrderBy(e => e.Ser_no)
                        .ToListAsync();
            var result = mapper.Map<List<ObjectTypeItemDto>>(reg);
            foreach (var item in result)
            {
                item.PurposeIds = reg
                    .First(e => e.Id == item.Id)
                    .HtmlContentPurposes
                    .Select(e => e.FK_ComponentPurposeId)
                    .ToList();
            }
            return result;
        }
        public async Task<ResponseMessageDto> CreateOrEdit(ObjectTypeItemDto dto)
        {
            var response = new ResponseMessageDto();
            try {
                if (dto.FK_TopNodeId == 0)
                {
                    if (dto.Id == 0) response.Message = (await CreateObjectType(dto)).ToString();
                    else response.Message = (await UpdatObjectType(dto)).ToString();
                }
                else { 
                    if(dto.Id==0) response.Message = (await CreateHtmlContent(dto)).ToString();
                    else response.Message = (await UpdateHtmlContent(dto)).ToString();
                }
                response.Success = true;
            }
            catch (Exception e) {
                response.Message= e.Message;
            }
            finally {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }

        private async Task<long> CreateObjectType(ObjectTypeItemDto dto) {
            ObjectType objectType = mapper.Map<ObjectType>(dto);
            db.ObjectTypes.Add(objectType);
            await loginUserData.SaveChanges(objectType);
            return objectType.Id;
        }
        private async Task<long> UpdatObjectType(ObjectTypeItemDto dto) {
            ObjectType? objectType = db.ObjectTypes.Where(e => e.Id == dto.Id).FirstOrDefault();
            if (objectType != null)
            {
                mapper.Map(dto, objectType);
                await loginUserData.SaveChanges(objectType);
                return objectType.Id;
            }
            else throw new Exception("資料不存在");
        }
        private async Task<long> CreateHtmlContent(ObjectTypeItemDto dto)
        {
            Html_Content HtmlContent = mapper.Map<Html_Content>(dto);
            db.Html_Contents.Add(HtmlContent);
            await loginUserData.SaveChanges(HtmlContent);
            await SyncPurposes(HtmlContent, dto.PurposeIds);
            return HtmlContent.Id;
        }
        private async Task<long> UpdateHtmlContent(ObjectTypeItemDto dto)
        {
            Html_Content? HtmlContent = db.Html_Contents.Where(e => e.Id == dto.Id).FirstOrDefault();
            if (HtmlContent != null)
            {
                mapper.Map(dto, HtmlContent);
                await loginUserData.SaveChanges(HtmlContent);
                await SyncPurposes(HtmlContent, dto.PurposeIds);
                return HtmlContent.Id;
            }
            else throw new Exception("資料不存在");
        }
        public async Task<ResponseMessageDto> DeleteHtmlContent(DataDelectDto dto) {
            ResponseMessageDto response = new ResponseMessageDto { };
            try
            {
                var user = await loginUserData.GetUser();
                var item = await db.Html_Contents
                        .Where(e => e.Id == dto.Id)
                        .FirstOrDefaultAsync();
                if (item == null) throw new Exception("資料不存在");
                else
                {
                    item.IsDeleted = true;
                    await loginUserData.SaveChanges(item);
                    var purposes = await db.HtmlContentPurposes
                        .Where(e => e.FK_HtmlContentId == item.Id)
                        .ToListAsync();
                    foreach (var purpose in purposes) purpose.IsDeleted = true;
                    if (purposes.Count > 0) await loginUserData.SaveChanges(purposes);

                    await fileUploadAppService.deleteFileById(new FileDeleteDto
                    {
                        Sid = item.Id,
                        Type = (int)FileBindTypeEnum.元件圖片
                    });
                }
                response.Success = true;
            }
            catch (Exception e)
            {
                response.Message = e.Message;
            }
            finally {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }

        private async Task SyncPurposes(Html_Content htmlContent, IEnumerable<long>? requestedPurposeIds)
        {
            var desiredIds = htmlContent.Type == (int)ObjectTypeEnum.自訂
                ? new HashSet<long>()
                : (requestedPurposeIds ?? Enumerable.Empty<long>()).Where(e => e > 0).ToHashSet();

            if (desiredIds.Count > 0)
            {
                var validIds = await db.ComponentPurposes
                    .Where(e => e.Visible && desiredIds.Contains(e.Id))
                    .Select(e => e.Id)
                    .ToListAsync();
                desiredIds.IntersectWith(validIds);
            }

            var existing = await db.HtmlContentPurposes
                .IgnoreQueryFilters()
                .Where(e => e.FK_HtmlContentId == htmlContent.Id)
                .ToListAsync();
            var changed = new List<object>();

            foreach (var relation in existing)
            {
                var shouldExist = desiredIds.Remove(relation.FK_ComponentPurposeId);
                if (relation.IsDeleted == !shouldExist) continue;
                relation.IsDeleted = !shouldExist;
                changed.Add(relation);
            }

            foreach (var purposeId in desiredIds)
            {
                var relation = new HtmlContentPurpose
                {
                    FK_HtmlContentId = htmlContent.Id,
                    FK_ComponentPurposeId = purposeId
                };
                db.HtmlContentPurposes.Add(relation);
                changed.Add(relation);
            }

            if (changed.Count > 0) await loginUserData.SaveChanges(changed);
        }
        public async Task<ResponseMessageDto> UpdateSerNo(UpdateSerNoListDto dto) {
            ResponseMessageDto response = new ResponseMessageDto { Success = true };
            try
            {
                var o = (from s in dto.list select s.Id).ToList();
                foreach (var e in dto.list)
                {
                    if (e.FK_TopNodeId == null || e.FK_TopNodeId==0) {
                        var item = await db.ObjectTypes.Where(m => m.Id == e.Id).FirstOrDefaultAsync();
                        if (item != null)
                        {
                            mapper.Map(e, item);
                            await loginUserData.SaveChanges(item);
                        }
                    }
                    else
                    {
                        var item = await db.Html_Contents.Where(m => m.Id == e.Id).FirstOrDefaultAsync();
                        if (item != null)
                        {
                            mapper.Map(e, item);
                            await loginUserData.SaveChanges(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.ToString();
            }
            finally {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }
        public async Task<HtmlContentGetHtmlDto> GetConten(SearchIDDto dto) {
            HtmlContentGetHtmlDto results = new HtmlContentGetHtmlDto();
            try
            {
                var content = await db.Html_Contents
                                    .Where(e => e.Id == dto.Id)
                                    .Where(e => !e.IsDeleted)
                                    .FirstOrDefaultAsync();
                if (content != null)
                {
                    results.Conten = mapper.Map<HtmlContentDetailDto>(content);
                    results.Conten.Html = HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(results.Conten.Html));
                    results.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                results.Success = false;
                results.Error = ex.Message;
            }
            return results;
        }
        public async Task<HtmlContentGetHtmlDto> GetNewsletterConten()
        {
            HtmlContentGetHtmlDto results = new HtmlContentGetHtmlDto();
            try
            {
                var content = await db.Html_Contents
                                    .Where(e => e.Type == 4)
                                    .Where(e => !(e.Title??"").Contains("email"))
                                    .Where (e => !e.IsDeleted)
                                    .FirstOrDefaultAsync();
                if (content != null)
                {
                    results.Conten = mapper.Map<HtmlContentDetailDto>(content);
                    results.Conten.Html = HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(results.Conten.Html));
                    results.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                results.Success = false;
                results.Error = ex.Message;
            }
            return results;
        }
        public async Task<HtmlContentGetHtmlDto> GetNewsletterMailConten() {
            HtmlContentGetHtmlDto results = new HtmlContentGetHtmlDto();
            try
            {
                var content = await db.Html_Contents
                                    .Where(e => e.Type == 4)
                                    .Where(e => !(e.Title ?? "").Contains("email"))
                                    .Where(e => !e.IsDeleted)
                                    .FirstOrDefaultAsync();
                if (content != null)
                {
                    results.Conten = mapper.Map<HtmlContentDetailDto>(content);
                    results.Conten.Html = HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(results.Conten.Html));
                    results.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                results.Success = false;
                results.Error = ex.Message;
            }
            return results;
        }
        public async Task<HtmlContentGetHtmlListDto> GetNewsletterAllConten() {
            HtmlContentGetHtmlListDto results = new HtmlContentGetHtmlListDto();
            try
            {
                var content = await db.Html_Contents
                                    .Where(e => e.Type == 4)
                                    .Where(e => !e.IsDeleted)
                                    .ToListAsync();
                if (content != null)
                {
                    results.Conten = mapper.Map<List<HtmlContentDetailDto>>(content);
                    results.Conten.ForEach(e => {
                        e.Html = HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(e.Html));
                    });
                    results.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                results.Success = false;
                results.Error = ex.Message;
            }
            return results;
        }
        public async Task<ResponseMessageDto> SaveConten(HtmlContentDetailDto dto) {
            ResponseMessageDto response = new ResponseMessageDto();
            try {
                var data = await db.Html_Contents.Where(e => e.Id == dto.Id).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                if (data == null) throw new Exception("查無資料");
                dto.Html = HttpUtility.HtmlEncode(dto.Html);
                mapper.Map(dto, data);
                response.Success=true;
            }
            catch (Exception e) {
                response.Message= e.Message;
            }
            finally
            {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }
    }
}
