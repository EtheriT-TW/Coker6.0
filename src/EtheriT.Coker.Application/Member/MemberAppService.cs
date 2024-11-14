using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Member;
using EtheriT.Coker.Application.Shared.Dto.Member;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Application.Shared.Dto;
using System.Diagnostics;
using System.Xml.Linq;

namespace EtheriT.Coker.Application.Member
{
    public class MemberAppService : IMemberAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITokenAppService tokenAppService;
        private readonly IMapper mapper;
        public MemberAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService,
            IMapper mapper
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tokenAppService = tokenAppService;
            this.mapper = mapper;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var result = db.Users;

                if (result != null)
                {
                    var dataQuery = from e in db.Users
                                    where !e.IsDeleted
                                    select new MemberGetAllListDto
                                    {
                                        Id = e.Id,
                                        Name = e.Name.Substring(0, 1) + "○" + e.Name.Substring(e.Name.Length - 1),
                                        CellPhone = e.CellPhone.Substring(0, 3) + "****" + e.CellPhone.Substring(7),
                                        TelPhone = e.TelPhone == "" ? "" : e.TelPhone.Substring(0, e.TelPhone.IndexOf("-") + 3) + "***" + e.TelPhone.Substring(e.TelPhone.IndexOf("-") + 6),
                                        Address = (e.Address == null || e.Address == "") ? "" : e.Address.Substring(0, e.Address.LastIndexOf(" ")).Replace(" ", "") + "***",
                                        Email = e.Email.Substring(0, 2) + "***" + e.Email.Substring(e.Email.IndexOf("@") - 1),
                                        Total = e.Total,
                                        Level = e.Level,
                                        CreationTime = e.CreationTime,
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<MemberGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetAllFrontList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long websideId = await loginUserData.GetWebsiteId();
                var result = from f in db.FrontUsers
                             join s in db.MappingFrontUserAndWebsite on f.Id equals s.FK_UserId
                             where s.FK_WebsiteId == websideId
                             select f;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    select new MemberGetAllListDto
                                    {
                                        Id = e.Id,
                                        Name = e.Name.Substring(0, 1) + "○" + e.Name.Substring(e.Name.Length - 1),
                                        CellPhone = e.CellPhone.Substring(0, 3) + "****" + e.CellPhone.Substring(7),
                                        TelPhone = e.TelPhone == "" ? "" : e.TelPhone.Substring(0, e.TelPhone.IndexOf("-") + 3) + "***" + e.TelPhone.Substring(e.TelPhone.IndexOf("-") + 6),
                                        Address = (e.Address == null || e.Address == "") ? "" : e.Address.Substring(0, e.Address.LastIndexOf(" ")).Replace(" ", "") + "***",
                                        Email = e.Email.Substring(0, 2) + "***" + e.Email.Substring(e.Email.IndexOf("@") - 1),
                                        Total = (
                                            from order in db.Order_Headers
                                            where order.State == OrderStatusEnum.已完成 &&
                                                (
                                                    (
                                                        from m in db.MappingOldNewUUID
                                                        where m.UserUUID == e.UUID
                                                        select m.TempUUID
                                                    ).Contains(order.FK_UUID) ||
                                                    order.FK_UUID == e.UUID
                                                )
                                            select order
                                        ).Sum(e => e.Payment),
                                        Level = e.Level,
                                        CreationTime = e.CreationTime,
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<MemberGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetAllManagerList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var result = db.Users;

                if (result != null)
                {
                    var dataQuery = from e in db.Users.Include(n => n.Roles)
                                    where !e.IsDeleted
                                    select new ManagerAllListDto
                                    {
                                        Id = e.Id,
                                        Name = e.Name.Substring(0, 1) + "○" + e.Name.Substring(e.Name.Length - 1),
                                        TelPhone = e.TelPhone == "" ? "" : e.TelPhone.Substring(0, e.TelPhone.IndexOf("-") + 3) + "***" + e.TelPhone.Substring(e.TelPhone.IndexOf("-") + 6),
                                        Email = e.Email.Substring(0, 2) + "***" + e.Email.Substring(e.Email.IndexOf("@") - 1),
                                        Status = (UserStatusEnum)(e.Status ?? 0),
                                        Roles = String.Join("、", (
                                            from o in db.Roles
                                            join ur in e.Roles on o.Id equals ur.RoleId
                                            select o.Name
                                        ).ToList())
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<MemberGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<MemberGetAllDataDto> GetAllData(long id)
        {
            try
            {
                long websideId = await loginUserData.GetWebsiteId();
                var result = await (from user in db.FrontUsers.Where(e => e.Id == id)
                                    join map in db.MappingFrontUserAndWebsite on user.Id equals map.FK_UserId
                                    where map.FK_WebsiteId == websideId
                                    select user).FirstOrDefaultAsync();


                if (result != null)
                {
                    MemberGetAllDataDto output = mapper.Map<MemberGetAllDataDto>(result);
                    output.RoleId = await db.MappingUserAndRoles.Where(e => e.UUID == result.UUID).Select(e => e.RoleId).FirstOrDefaultAsync();
                    output.Id = ("000000000" + result.Id).Substring(result.Id.ToString().Length);
                    return output;
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<MemberGetAllDataDto> GetSelfData()
        {
            long userId = await loginUserData.GetUserId();
            return await GetAllData(userId);
        }
        public async Task<ResponseMessageDto> Update(MemberUpdateDto dto)
        {
            long usetId = await loginUserData.GetUserId();
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var result = db.FrontUsers.Where(e => e.Id == dto.Id).FirstOrDefault();

                if (result != null)
                {
                    if (dto.Status == null) dto.Status = result.Status;
                    if (dto.TelPhone == null) dto.TelPhone = result.TelPhone;
                    
                    mapper.Map(dto, result);
                    await loginUserData.SaveChanges(result);

                    var role = await db.MappingUserAndRoles.Where(e => e.UUID == result.UUID).FirstOrDefaultAsync();
                    if(dto.RoleId != null && role != null && role.RoleId != dto.RoleId) {
                        role.RoleId = (long)dto.RoleId;
                        await loginUserData.SaveChanges(role);
                    }
                    
                    output.Success = true;
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<List<SelectDto>> GetAllRole()
        {
            List<SelectDto> output = new List<SelectDto>();
            long websideId = await loginUserData.GetWebsiteId();
            try
            {
                output = await (from role in db.Roles
                                where role.FK_WebsiteId == websideId
                                where role.Type == (int)RoleTypeEnum.前台
                                select new SelectDto()
                                {
                                    Id = role.Id,
                                    Name = role.Name
                                }).ToListAsync();
            }
            catch (Exception e)
            {

            }
            return output;
        }

        public async Task<JsonResult> GetDevAllRole(DataSourceLoadOptions loadOptions)
        {
            var dataQuery = await GetAllRole();
            var output = DataSourceLoader.Load(dataQuery, loadOptions);
            return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        /*public async Task<ResponseMessageDto> RoleAddUp([FromForm] DevExpressDto dto) { 
            
        }*/
    }
}

