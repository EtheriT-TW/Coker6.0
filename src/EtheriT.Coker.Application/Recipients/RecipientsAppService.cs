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
                                where r.UUID == UUID && r.FK_WebsiteId == websiteId && !r.IsDeleted
                                select new RecipientsDto
                                {
                                    Id = r.Id,
                                    UUID = r.UUID,
                                    Name = r.Name,
                                    Email = r.Email,
                                    Address = r.Address,
                                    CellPhone = r.CellPhone,
                                    TelePhone = r.TelePhone,
                                    Sex = r.Sex,
                                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            return new JsonResult(new { error }, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<List<RecipientsDto>> GetCheckoutList()
        {
            var websiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0
                ? configuration.GetValue<long>("WebConfig:SiteId")
                : await loginUserData.GetWebsiteId();
            var uuid = await tokenAppService.GetUUID();

            if (uuid == Guid.Empty) return new List<RecipientsDto>();

            var recipients = await db.Recipients
                .AsNoTracking()
                .Where(r => r.UUID == uuid && r.FK_WebsiteId == websiteId && !r.IsDeleted)
                .OrderByDescending(r => r.CreationTime)
                .Select(r => new RecipientsDto
                {
                    Id = r.Id,
                    UUID = r.UUID,
                    Name = r.Name,
                    Email = r.Email,
                    Address = r.Address,
                    ZipCode = r.ZipCode,
                    CellPhone = r.CellPhone,
                    TelePhone = r.TelePhone,
                    Sex = r.Sex,
                    LogisticsType = r.LogisticsType,
                    CVSStoreID = r.CVSStoreID,
                    CVSStoreName = r.CVSStoreName,
                    CVSAddress = r.CVSAddress,
                    CVSTelephone = r.CVSTelephone,
                    CVSOutSide = r.CVSOutSide,
                    FK_WebsiteId = r.FK_WebsiteId
                })
                .ToListAsync();

            return recipients
                .GroupBy(r => new
                {
                    Name = r.Name?.Trim() ?? string.Empty,
                    Email = r.Email?.Trim() ?? string.Empty,
                    Address = r.Address?.Trim() ?? string.Empty,
                    ZipCode = r.ZipCode?.Trim() ?? string.Empty,
                    CellPhone = r.CellPhone?.Trim() ?? string.Empty,
                    TelePhone = r.TelePhone?.Trim() ?? string.Empty,
                    r.Sex,
                    r.LogisticsType,
                    CVSStoreID = r.CVSStoreID?.Trim() ?? string.Empty,
                    CVSStoreName = r.CVSStoreName?.Trim() ?? string.Empty,
                    CVSAddress = r.CVSAddress?.Trim() ?? string.Empty,
                    CVSTelephone = r.CVSTelephone?.Trim() ?? string.Empty,
                    CVSOutSide = r.CVSOutSide?.Trim() ?? string.Empty
                })
                .Select(group => group.First())
                .ToList();
        }

        public async Task<ResponseMessageDto> SaveCheckoutRecipient(RecipientsDto dto)
        {
            var response = new ResponseMessageDto();

            try
            {
                var websiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0
                    ? configuration.GetValue<long>("WebConfig:SiteId")
                    : await loginUserData.GetWebsiteId();
                var uuid = await tokenAppService.GetUUID();

                if (uuid == Guid.Empty) throw new Exception("請先登入會員後再儲存常用收件資訊");
                if (string.IsNullOrWhiteSpace(dto.Name)) throw new Exception("請填寫收件人姓名");
                if (string.IsNullOrWhiteSpace(dto.CellPhone)) throw new Exception("請填寫收件人手機號碼");

                var name = dto.Name.Trim();
                var email = dto.Email?.Trim() ?? string.Empty;
                var address = dto.Address?.Trim() ?? string.Empty;
                var zipCode = dto.ZipCode?.Trim() ?? string.Empty;
                var cellPhone = dto.CellPhone.Trim();
                var telePhone = dto.TelePhone?.Trim() ?? string.Empty;
                var cvsStoreId = dto.CVSStoreID?.Trim() ?? string.Empty;
                var cvsStoreName = dto.CVSStoreName?.Trim() ?? string.Empty;
                var cvsAddress = dto.CVSAddress?.Trim() ?? string.Empty;
                var cvsTelephone = dto.CVSTelephone?.Trim() ?? string.Empty;
                var cvsOutSide = dto.CVSOutSide?.Trim() ?? string.Empty;

                var entity = await db.Recipients.FirstOrDefaultAsync(r =>
                    r.UUID == uuid &&
                    r.FK_WebsiteId == websiteId &&
                    !r.IsDeleted &&
                    r.Name == name &&
                    (r.Email ?? string.Empty) == email &&
                    (r.Address ?? string.Empty) == address &&
                    (r.ZipCode ?? string.Empty) == zipCode &&
                    r.CellPhone == cellPhone &&
                    (r.TelePhone ?? string.Empty) == telePhone &&
                    r.Sex == dto.Sex &&
                    r.LogisticsType == dto.LogisticsType &&
                    (r.CVSStoreID ?? string.Empty) == cvsStoreId &&
                    (r.CVSStoreName ?? string.Empty) == cvsStoreName &&
                    (r.CVSAddress ?? string.Empty) == cvsAddress &&
                    (r.CVSTelephone ?? string.Empty) == cvsTelephone &&
                    (r.CVSOutSide ?? string.Empty) == cvsOutSide);

                if (entity == null)
                {
                    entity = new Core.Models.Recipient
                    {
                        UUID = uuid,
                        FK_WebsiteId = websiteId,
                        Name = name,
                        Email = email,
                        Address = address,
                        ZipCode = zipCode,
                        CellPhone = cellPhone,
                        TelePhone = telePhone,
                        Sex = dto.Sex,
                        LogisticsType = dto.LogisticsType,
                        CVSStoreID = cvsStoreId,
                        CVSStoreName = cvsStoreName,
                        CVSAddress = cvsAddress,
                        CVSTelephone = cvsTelephone,
                        CVSOutSide = cvsOutSide
                    };

                    db.Recipients.Add(entity);
                    await loginUserData.SaveChanges(entity);
                }

                dto.Id = entity.Id;
                dto.UUID = entity.UUID;
                dto.FK_WebsiteId = entity.FK_WebsiteId;
                dto.Name = name;
                dto.Email = email;
                dto.Address = address;
                dto.ZipCode = zipCode;
                dto.CellPhone = cellPhone;
                dto.TelePhone = telePhone;
                dto.CVSStoreID = cvsStoreId;
                dto.CVSStoreName = cvsStoreName;
                dto.CVSAddress = cvsAddress;
                dto.CVSTelephone = cvsTelephone;
                dto.CVSOutSide = cvsOutSide;
                response.Success = true;
                response.Object = dto;
                response.Message = "常用收件資訊已同步";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Error = ex.Message;
            }

            return response;
        }

        public async Task<ResponseMessageDto> RecipientsAddUp(RecipientsDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                if (dto.Id == null)
                {
                    var recipients = await db.Recipients.Where(e => e.UUID == dto.UUID && e.Name == dto.Name && e.Email == dto.Email && e.Address == dto.Address && e.CellPhone == dto.CellPhone && e.TelePhone == dto.TelePhone && e.Sex == dto.Sex).FirstOrDefaultAsync();
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
