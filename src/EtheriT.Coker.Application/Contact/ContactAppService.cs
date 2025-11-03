using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Contact;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Contact;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.i18n;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Contact
{
    public class ContactAppService : IContactAppService
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
        )
        {
            this.db = db;
            this.mapper = mapper;
            this.captchaAppService = captchaAppService;
            this.mailAppService = mailAppService;
            this.configuration = configuration;
            this.loginUserData = loginUserData;

        }
        public async Task<ResponseMessageDto> submit(FormSubmitDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            long siteId = configuration.GetValue<long>("WebConfig:SiteId");
            try
            {
                var code = dto.forms.Find(e => e.Name == "captcha");
                var StoreSetId = (await db.StoreSet.Where(e => e.key == "EmailNotificationType").FirstOrDefaultAsync())?.Id;
                var StoreSet = StoreSetId != null ? await db.StoreSetDetail.Where(e => e.FK_WebsiteId == siteId && e.FK_StoreSetId == StoreSetId).FirstOrDefaultAsync() : null;
                var EmailNotificationType = StoreSet?.value?.ToString() ?? "Detailed";

                var codeId = dto.forms.Find(e => e.Name == "captchaId");
                if (codeId == null || code == null || !captchaAppService.Validate(codeId.Value, code.Value).Success) throw new Exception(L.get("VerificationCodeError"));
                else
                {
                    var site = await db.Websites.Where(e => !e.IsDeleted).Where(e => e.Id == siteId).FirstOrDefaultAsync();
                    if (site == null) throw new Exception(L.get("WebsiteDataError"));
                    var menu = await db.WebMenus.Where(e => !e.IsDeleted && e.FK_WebsiteId == siteId && e.RouterName == dto.RouterName).FirstOrDefaultAsync();
                    if (menu == null) throw new Exception(L.get("UnknownSource"));
                    MailUserDataDto recipient = new MailUserDataDto();

                    string html = "";
                    switch (EmailNotificationType)
                    {
                        case "Detailed":
                            html = "<p>您好，感謝您的聯繫。此信件為系統通知，請勿直接回覆，感謝您的理解與配合~謝謝<br></p><table class='table'>";
                            dto.forms.ForEach(e =>
                            {
                                if (!string.IsNullOrEmpty(e.Title))
                                {
                                    html += $@"<tr>
										<td class='title'>{e.Title}</td>
										<td>{e.Value.Replace(Environment.NewLine, "<br/>")}</td>
									</tr>";
                                }
                                if (e.Name == "email") recipient.Email = e.Value;
                                else if (e.Name == "name") recipient.Name = e.Value;
                            });
                            html += "</table>";
                            break;
                        case "Simple":
                            dto.forms.ForEach(e =>
                            {
                                if (!string.IsNullOrEmpty(e.Title))
                                {
                                    html += $@"<tr>
										<td class='title'>{e.Title}</td>
										<td>{e.Value.Replace(Environment.NewLine, "<br/>")}</td>
									</tr>";
                                }
                                if (e.Name == "email") recipient.Email = e.Value;
                                else if (e.Name == "name") recipient.Name = e.Value;
                            });

                            var name = dto.forms.FirstOrDefault(f => f.Name == "name")?.Value ?? "";
                            if (name.Length <= 1) name = "親愛的會員";
                            else if (name.Length == 2) name = name.Substring(0, 1) + "○";
                            else name = $"{name[0]}○{name[name.Length - 1]}";

                            html = $@"<div>{name} 您好</div>
                                                    <p>感謝您透過【{site.Title}】提交 {menu.Title} 表單，我們已經成功收到您的資料。<br>
                                                    我們將盡快進行後續處理，並於需要時與您聯繫。<br><br>
                                                    謝謝您的耐心與支持。</p>";
                            break;
                    }
                    SenderDto senderDto = new SenderDto
                    {
                        Recipients = new List<MailUserDataDto> { recipient },
                        CC = new List<MailUserDataDto> { dto.Sender },
                        Subject = $"{site.Title}-{L.get("ServiceCenter")}",
                        Body = html,
                        Css = ".table{width:800px;} .table td{border-bottom: #ececec solid 1px; padding: 6px 3px;} .table td:last-child{padding-left: 9px;} .title{background-color: #ececec; width:22%; text-align: center; font-weight: bold;}"
                    };
                    senderDto.Sender.Name = string.IsNullOrEmpty(site.Contact) ? site.Title ?? "" : site.Contact;
                    await mailAppService.sendMail(senderDto);
                    var dict = dto.forms
                        .Where(f => !string.IsNullOrWhiteSpace(f.Title))
                        .ToDictionary(
                            f => f.Name,
                            f => new
                            {
                                title = string.IsNullOrWhiteSpace(f.Title) ? "" : f.Title.Trim().Replace(" ", "").Replace("　", ""),
                                value = f.Value
                            }
                        );
                    string result = JsonConvert.SerializeObject(dict);
                    var parts = new[]
                    {
                        dto.forms.Find(e => e.Name == "name")?.Value,
                        dto.forms.Find(e => e.Name == "Gender")?.Value
                    };
                    Core.Models.Contact contact = new Core.Models.Contact
                    {
                        FK_WebMenuId = menu.Id,
                        Email = recipient.Email,
                        Name = menu.Title ?? "",
                        TargetEmail = $"{dto.Sender.Name}({dto.Sender.Email})",
                        Html = html,
                        FromDate = result,
                        UserName = string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)))
                    };
                    loginUserData.setOptionParameter(contact, 0);
                    db.Contacts.Add(contact);
                    await db.SaveChangesAsync();
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<JsonResult> GetContactListAll(DataSourceLoadOptions loadOptions)
        {
            long WebsiteID = await loginUserData.GetWebsiteId();
            var dataQuery = from c in db.Contacts.Where(e => !e.IsDeleted)
                            join m in db.WebMenus.Where(e => e.FK_WebsiteId == WebsiteID && !e.IsDeleted) on c.FK_WebMenuId equals m.Id
                            select new ContactListDto
                            {
                                Id = c.Id,
                                Name = c.Name,
                                UserName = c.UserName,
                                TargetEmail = c.TargetEmail,
                                Email = c.Email,
                                Status = c.Status.ToString().Replace("_", "/"),
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
        public async Task<ResponseMessageDto> GetDataOne(long id)
        {
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
        public async Task<ResponseMessageDto> ReplyContact(ContactReplyDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var contact = await db.Contacts.Include(e => e.WebMenu).Where(e => e.WebMenu != null && e.WebMenu.FK_WebsiteId == websiteId && e.Id == dto.Id).FirstOrDefaultAsync();
                if (contact != null)
                {
                    contact.Status = dto.Status;
                    if (!string.IsNullOrWhiteSpace(dto.Reply))
                    {
                        contact.Reply = dto.Reply;
                        contact.ReplyTime = DateTime.Now;
                    }
                    await loginUserData.SaveChanges(contact);
                    response.Success = true;
                }
                else throw new Exception(L.get("DataNotFound"));
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));

            return response;
        }
    }
}
