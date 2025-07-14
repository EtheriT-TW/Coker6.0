using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.ObjectType;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
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
        private readonly string ApplicationName;
        private long websiteId;
        public ObjectTypeAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
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
            var reg = db.Html_Contents
                        .Where(e => e.Type == type)
                        .Where(e => isSystemUser || (e.Type == (int)ObjectTypeEnum.自訂 && e.FK_WebsiteId == websiteId))
                        .OrderBy(e => e.Ser_no);
            return mapper.Map<List<ObjectTypeItemDto>>(reg);
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
            return HtmlContent.Id;
        }
        private async Task<long> UpdateHtmlContent(ObjectTypeItemDto dto)
        {
            Html_Content? HtmlContent = db.Html_Contents.Where(e => e.Id == dto.Id).FirstOrDefault();
            if (HtmlContent != null)
            {
                mapper.Map(dto, HtmlContent);
                await loginUserData.SaveChanges(HtmlContent);
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
