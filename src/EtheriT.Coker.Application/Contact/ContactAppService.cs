using EtheriT.Coker.Application.Dto.Contact;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.i18n;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using EtheriT.Coker.Application.Shared.Dto.Article;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.Contact;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Data;
using AutoMapper;

namespace EtheriT.Coker.Application.Contact
{
	public class ContactAppService: IContactAppService
	{
		private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ICaptchaAppService captchaAppService;
		private readonly MailAppService mailAppService;
		private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        public ContactAppService(
			CokerDbContext db,
            LoginUserData loginUserData,
            ICaptchaAppService captchaAppService, 
			MailAppService mailAppService,
			IConfiguration configuration,
            IMapper mapper
        ) { 
			this.db = db;
            this.mapper = mapper;
            this.captchaAppService = captchaAppService;
			this.mailAppService = mailAppService;
			this.configuration = configuration;
			this.loginUserData = loginUserData;

        }
		public async Task<ResponseMessageDto> submit(FormSubmitDto dto) {
			ResponseMessageDto response=new ResponseMessageDto();
			long siteId = configuration.GetValue<long>("WebConfig:SiteId");
			try {
				var code = dto.forms.Find(e => e.Name == "captcha");
				var codeId = dto.forms.Find(e => e.Name == "captchaId");
				if (codeId == null || code == null || !captchaAppService.Validate(codeId.Value, code.Value).Success) throw new Exception(L.get("VerificationCodeError"));
                else
                {
					var site = await db.Websites.Where(e => !e.IsDeleted).Where(e => e.Id == siteId).FirstOrDefaultAsync();
					if (site == null) throw new Exception(L.get("WebsiteDataError"));
                    var menu = await db.WebMenus.Where(e => !e.IsDeleted && e.FK_WebsiteId == siteId && e.RouterName == dto.RouterName).FirstOrDefaultAsync();
                    if (menu == null) throw new Exception(L.get("UnknownSource"));
                    MailUserDataDto recipient = new MailUserDataDto();
					string html = "<table class='table'>";
					dto.forms.ForEach(e => {
						if (!string.IsNullOrEmpty(e.Title)) {
							html += $@"<tr>
								<td class='title'>{e.Title}</td>
								<td>{e.Value.Replace(Environment.NewLine, "<br/>")}</td>
							<tr>";
						}
						if(e.Name== "email") recipient.Email = e.Value;
						else if(e.Name == "name") recipient.Name = e.Value;
					});
					html += "<table>";
					SenderDto senderDto = new SenderDto
					{
						Recipients = new List<MailUserDataDto> { recipient },
						CC = new List<MailUserDataDto> { dto.Sender },
						Subject = $"{site.Title}-{L.get("ServiceCenter")}",
						Body = html,
						Css = ".table{width:800px;} .table td{border-bottom: #ececec solid 1px; padding: 6px 3px;} .table td:last-child{padding-left: 9px;} .title{background-color: #ececec; width:22%; text-align: center; font-weight: bold;}"
                    };
					senderDto.Sender.Name = site.Contact?? site.Title??"";
					await mailAppService.sendMail(senderDto);
					Core.Models.Contact contact = new Core.Models.Contact
					{
						FK_WebMenuId = menu.Id,
						Email = recipient.Email,
						Name = menu.Title ?? "",
						TargetEmail = $"{dto.Sender.Name}({dto.Sender.Email})",
						Html = html
					};
					loginUserData.setOptionParameter(contact, 0);
                    db.Contacts.Add(contact);
					await db.SaveChangesAsync();
					response.Success = true;
				}
			}
			catch(Exception ex)
			{
				response.Error = ex.Message;
			}
			return response;
		}
		public async Task<JsonResult> GetContactListAll(DataSourceLoadOptions loadOptions) {
            long WebsiteID = await loginUserData.GetWebsiteId();
            var dataQuery = from c in db.Contacts.Where(e => !e.IsDeleted)
							join m in db.WebMenus.Where(e => e.FK_WebsiteId == WebsiteID && !e.IsDeleted) on c.FK_WebMenuId equals m.Id
                            select new ContactListDto
                            { 
								Id=c.Id,
								Name = c.Name,
                                TargetEmail = c.TargetEmail,
								CreationTime = c.CreationTime
							};
			if (dataQuery.Any())
			{
                var output = DataSourceLoader.Load(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            else
            {
                return new JsonResult(new List<ContactListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
        }
		public async Task<ResponseMessageDto> GetDataOne(long id) {
            ResponseMessageDto response = new ResponseMessageDto();
			try
			{
				var websiteId = await loginUserData.GetWebsiteId();
				var dataQuery = await db.Contacts.Include(e => e.WebMenu).Where(e => e.WebMenu != null && e.WebMenu.FK_WebsiteId == websiteId && e.Id == id).FirstOrDefaultAsync();
                response.Object = mapper.Map<AsrFormDataDto>(dataQuery);
                response.Success = true;
            }
            catch (Exception ex)
			{
				response.Error = ex.Message;
            }
			return response;
        }
    }
}
