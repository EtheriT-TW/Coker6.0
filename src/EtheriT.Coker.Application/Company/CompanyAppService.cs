using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Company;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Company
{
    public class CompanyAppService: ICompanyAppService
    {
        private readonly CokerDbContext db;
        private readonly IMapper mapper;
        private readonly LoginUserData loginUserData;
        private readonly string ApplicationName;
        public CompanyAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper
        ) { 
            this.db = db;
            this.mapper = mapper;
            this.loginUserData = loginUserData;
            ApplicationName = "Company";
        }

        public Task<OutputCompanyDto> Get()
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseMessageDto> Save(CompanyDto dto)
        {
            ResponseMessageDto responseMessageDto = new ResponseMessageDto { Success=false };
            try
            {
                if (dto.Id == 0) throw new Exception("儲存資料錯誤");
                var data = await db.Companies.Where(e => !e.IsDeleted).Where(e => e.Id == dto.Id).FirstOrDefaultAsync();
                if (data != null)
                {
                    mapper.Map(dto, data);
                    await loginUserData.SaveChanges(data);
                    responseMessageDto.Success = true;
                }
                else throw new Exception("資料不存在！");
            }
            catch(Exception ex)
            {
                responseMessageDto.Error = ex.Message;
            }
            await loginUserData.SetLogs(ApplicationName, "Save",JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(responseMessageDto));
            return responseMessageDto;
            
        }
    }
}
