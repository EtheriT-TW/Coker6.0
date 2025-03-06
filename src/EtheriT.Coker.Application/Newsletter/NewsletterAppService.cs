using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto.Newsletter;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using System.Xml.Linq;
using EtheriT.Coker.Application.Shared.Dto.Newsletter;
using System.Security.Cryptography;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.Newsletter
{
    public class NewsletterAppService: INewsletterAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly string controllerName;
        private readonly MailAppService mailAppService;
        public NewsletterAppService(
            CokerDbContext db, 
            LoginUserData loginUserData,
            IMapper mapper,
            MailAppService mailAppService
        ) { 
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            controllerName = "Newsletter";
            this.mailAppService = mailAppService;
        }
        public async Task<JsonResult> GetRecipients(DataSourceLoadOptions loadOptions)
        {
            long sideId = await loginUserData.GetWebsiteId();
            var data = db.Recipients.Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == sideId);
            var dataQuery = mapper.Map<List<RecipientsListDto>>(data);
            var output = DataSourceLoader.Load(dataQuery, loadOptions);
            return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> RecipientAddUp(DevExpressDto dto) {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            var data = JsonConvert.DeserializeObject<RecipientsListDto>(dto.Values);

            try
            {
                long webid = await loginUserData.GetWebsiteId();

                if (data != null)
                {
                    if (dto.Key == null)
                    {
                        Core.Models.Recipient r = mapper.Map<Recipient>(data);
                        r.FK_WebsiteId = webid;
                        db.Recipients.Add(r);
                        await loginUserData.SaveChanges(r);
                    }
                    else
                    {
                        var theRec = await db.Recipients.Where(e => e.Id == dto.Key).FirstOrDefaultAsync();

                        if (theRec != null)
                        {
                            if(!string.IsNullOrEmpty(data.Email)) theRec.Email = data.Email;
                            if (!string.IsNullOrEmpty(data.Name)) theRec.Name = data.Name;
                            await loginUserData.SaveChanges(theRec);
                        }
                    }

                    output.Success = true;
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            await loginUserData.SetLogs(
                JsonConvert.SerializeObject(dto),
                JsonConvert.SerializeObject(output)
            );
            return output;
        }
        public async Task<ResponseMessageDto> DeleteRecipients(long Id) {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                long userId = await loginUserData.GetUserId();

                var data = db.Recipients.Where(e => e.Id == Id).FirstOrDefault();
                if (data != null)
                {
                    data.IsDeleted = true;
                    data.DeletionTime = DateTime.Now;
                    data.DeleterUserId = userId;
                    await loginUserData.SaveChanges(data);
                    output.Success = true;
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            await loginUserData.SetLogs(
                JsonConvert.SerializeObject(new { Id }),
                JsonConvert.SerializeObject(output)
            );
            return output;
        }
        public async Task<ResponseMessageDto> Send(long Id) {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                long userId = await loginUserData.GetUserId();
                var data = await db.Article.Where(e => !e.IsDeleted).Where(e => e.Id == Id).FirstOrDefaultAsync();
                if (data != null)
                {
                    await mailAppService.sendMail(new SenderDto
                    {
                        Recipients = await getRecipients(),
                        Subject = data.Title??"",
                        Body = data.NewsletterHtml ?? "",
                        Css = data.NewsletterCss?? ""
                    });
                    output.Success=true;
                }
                else throw new Exception("資料不存在");
                
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            await loginUserData.SetLogs(
                JsonConvert.SerializeObject(new { Id }),
                JsonConvert.SerializeObject(output)
            );
            return output;
        }
        private async Task<List<MailUserDataDto>> getRecipients() {
            long sideId = await loginUserData.GetWebsiteId();
            var data = db.Recipients.Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == sideId);
            var dataQuery = mapper.Map<List<MailUserDataDto>>(data);
            return dataQuery;
        }
        public async Task<ResponseMessageDto> UpdateJson(NewsletterFrameDto dto) {
            ResponseMessageDto output = new ResponseMessageDto();
            long sideId = await loginUserData.GetWebsiteId();
            try
            {
                if (dto.Id == 0) throw new Exception("資料不存在");
                var orgName = await loginUserData.GetWebsiteOrgName();
                var data = await db.Article
                .Where(e => e.Id == dto.Id)
                .Where(e => e.FK_WebsiteId == sideId)
                .Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                if (data != null)
                {
                    data.DataJson = JsonConvert.SerializeObject(dto);
                    data.DataJson = data.DataJson = Regex.Replace(data.DataJson, $@"/upload/(?:{orgName}/)?", $"/upload/{orgName}/"); ;
                    await loginUserData.SaveChanges(data);
                    output.Success = true;
                }
                else throw new Exception("資料不存在");
            }catch (Exception ex)
            {
                output.Error = ex.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
            return output;
        }
        public async Task<ResponseMessageDto> SaveConten(HtmlContentDetailDto dto) { 
            ResponseMessageDto output = new ResponseMessageDto();
            long sideId = await loginUserData.GetWebsiteId();
            try { 
                var data = await db.Article.Where(e => e.Id == dto.Id).Where(e => !e.IsDeleted).Where( e=> e.FK_WebsiteId==sideId).FirstOrDefaultAsync();
                if (data != null)
                {
                    string Orgname = await loginUserData.GetWebsiteOrgName();
                    data.NewsletterCss = (dto.Css??"").Replace($"/upload/{Orgname}/", "/upload/");
                    data.NewsletterHtml = (dto.Html??"").Replace($"/upload/{Orgname}/", "/upload/");
                    await loginUserData.SaveChanges(data);
                    output.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                output.Error = ex.Message;
            }
            return output;
        }
        public async Task<ResponseMessageDto> UpdateText(NewsletterTextUpdateDto dto) {
			ResponseMessageDto output = new ResponseMessageDto();
			long sideId = await loginUserData.GetWebsiteId();
            try { 
                var art = await db.Article.Where(e => !e.IsDeleted).Where(e => e.Id==dto.Id).FirstOrDefaultAsync();
                if (art != null)
                {
                    if (!string.IsNullOrEmpty(art.DataJson)) { 
                        
                    }
                }
                else throw new Exception("資料不存在");
            }catch (Exception ex)
            {
                output.Error = ex.Message;
            }
			return output;
		}

	}
}
