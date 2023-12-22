using EtheriT.Coker.Application.Dto.Contact;
using EtheriT.Coker.Application.Dto;
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

namespace EtheriT.Coker.Application.Contact
{
	public class ContactAppService: IContactAppService
	{
		private readonly CokerDbContext db;
		private readonly ICaptchaAppService captchaAppService;
		private readonly MailAppService mailAppService;
		private readonly IConfiguration configuration;
		public ContactAppService(
			CokerDbContext db,
			ICaptchaAppService captchaAppService, 
			MailAppService mailAppService,
			IConfiguration configuration
		) { 
			this.db = db;
			this.captchaAppService = captchaAppService;
			this.mailAppService = mailAppService;
			this.configuration = configuration;
		}
		public async Task<ResponseMessageDto> submit(FormSubmitDto dto) {
			ResponseMessageDto response=new ResponseMessageDto();
			long siteId = configuration.GetValue<long>("WebConfig:SiteId");
			try {
				var code = dto.forms.Find(e => e.Name == "captcha");
				var codeId = dto.forms.Find(e => e.Name == "captchaId");
				if (codeId == null || code == null || !captchaAppService.Validate(codeId.Value, code.Value).Success) throw new Exception("驗證碼錯誤");
                else
                {
					var site = await db.Websites.Where(e => !e.IsDeleted).Where(e => e.Id == siteId).FirstOrDefaultAsync();
					if (site == null) throw new Exception("網站資料錯誤");
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
						Subject = $"{site.Title}-客服中心",
						Body = html,
						Css = ".table{width:800px;} .table td{border-bottom: #ececec solid 1px; padding: 6px 3px;} .table td:last-child{padding-left: 9px;} .title{background-color: #ececec; width:22%; text-align: center; font-weight: bold;}"
                    };
					senderDto.Sender.Name = site.Contact?? site.Title??"";
					await mailAppService.sendMail(senderDto);
					response.Success = true;
				}
			}
			catch(Exception ex)
			{
				response.Error = ex.Message;
			}
			return response;
		}
	}
}
