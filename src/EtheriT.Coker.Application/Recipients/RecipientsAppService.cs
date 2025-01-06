using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Recipients;
using EtheriT.Coker.Application.Shared.Recipients;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Token;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.Application.Shared.Dto.Member;

namespace EtheriT.Coker.Application.Recipients
{
    public class RecipientsAppService : IRecipientsAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly ITokenAppService tokenAppService;
        public RecipientsAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            IConfiguration configuration,
            ITokenAppService tokenAppService
            )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.configuration = configuration;
            this.tokenAppService = tokenAppService;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            var websiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();
            Guid UUID = await tokenAppService.GetUUID();
            string error = string.Empty;
            try
            {
                var dataQuery = from r in db.Recipients
                                where r.UUID == UUID && r.FK_WebsiteId == websiteId
                                select new RecipientsDto
                                {
                                    Id = r.Id,
                                    UUID = r.UUID,
                                    Name = r.Name,
                                    Email = r.Email,
                                    Address = r.Address,
                                    Cellphone = r.Cellphone,
                                    Telephone = r.Telephone,
                                    Sex = r.Sex,
                                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            return new JsonResult(new { error }, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> RecipientsAddUp(RecipientsDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                if (dto.Id == null)
                {
                    var recipients = await db.Recipients.Where(e => e.UUID == dto.UUID && e.Name == dto.Name && e.Email == dto.Email && e.Address == dto.Address && e.Cellphone == dto.Cellphone && e.Telephone == dto.Telephone && e.Sex == dto.Sex).FirstOrDefaultAsync();
                    if (recipients == null)
                    {
                        recipients = mapper.Map<Core.Models.Recipient>(dto);
                        db.Recipients.Add(recipients);
                        await loginUserData.SaveChanges(recipients);
                    }
                }
                else
                {
                    var recipients = await db.Recipients.Where(e => e.Id == dto.Id).FirstOrDefaultAsync();
                    if (recipients != null)
                    {
                        recipients = mapper.Map(dto, recipients);
                        recipients.LastModifierUserId = recipients.CreatorUserId;
                        recipients.LastModificationTime = DateTime.Now;
                    }
                    else throw new Exception("查無收件人資訊");
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
