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

namespace EtheriT.Coker.Application.Member
{
    public class MemberAppService : IMemberAppService
    {
        private readonly CokerDbContext db;
        public MemberAppService(
            CokerDbContext db
        )
        {
            this.db = db;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var result = db.Users;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted
                                    select new MemberGetAllListDto
                                    {
                                        Id = e.Id,
                                        Name = e.Name.Substring(0, 1) + "○" + e.Name.Substring(e.Name.Length - 1),
                                        CellPhone = e.CellPhone.Substring(0, 3) + "****" + e.CellPhone.Substring(7),
                                        TelPhone = e.TelPhone != null ? e.TelPhone.Substring(0, e.TelPhone.IndexOf("-") + 3) + "***" + e.TelPhone.Substring(e.TelPhone.IndexOf("-") + 6) : "-",
                                        Address = e.Address.Replace(" ", ""),
                                        Email = e.Email,
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
        public async Task<MemberGetAllDataDto> GetAllData(long id)
        {
            try
            {
                var result = db.Users.Where(e => e.Id == id && !e.IsDeleted).FirstOrDefault();

                if (result != null)
                {
                    MemberGetAllDataDto output = new MemberGetAllDataDto()
                    {
                        Name = result.Name,
                        Sex = result.Sex,
                        Status = result.Status,
                        Level = result.Level,
                        Email = result.Email,
                        CellPhone = result.CellPhone,
                        TelPhone = result.TelPhone,
                        Address = result.Address,
                        Password = result.Password,
                    };

                    return output;
                }
                else throw new Exception("查無會員資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<ResponseMessageDto> Update(MemberUpdateDto dto)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var result = db.Users.Where(e => e.Id == dto.Id).FirstOrDefault();

                if (result != null)
                {
                    result.Name = dto.Name;
                    result.Sex = dto.Sex;
                    result.Status = dto.Status;
                    result.Level = dto.Level;
                    result.Email = dto.Email;
                    result.CellPhone = dto.CellPhone;
                    result.TelPhone = dto.TelPhone;
                    result.Address = dto.Address;
                    result.LastModificationTime = DateTime.Now;
                    //if (dto.Password != null)
                    //{
                    //    result.Password = dto.Password;
                    //}
                    db.SaveChanges();
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
    }
}

