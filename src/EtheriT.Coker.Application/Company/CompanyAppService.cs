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
using EtheriT.Coker.Core.Models;

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

        public async Task<OutputCompanyDto> GetByTaxId(string taxId)
        {
            return await GetByIdentity(taxId, null);
        }

        public async Task<OutputCompanyDto> GetByIdentity(string? taxId, string? name)
        {
            var output = new OutputCompanyDto
            {
                Success = false,
                Company = new CompanyDto()
            };
            var normalizedTaxId = NormalizeTaxId(taxId);
            var normalizedName = NormalizeCompanyName(name);
            if (!string.IsNullOrEmpty(normalizedTaxId) && !IsValidTaxId(normalizedTaxId))
            {
                output.Error = "請輸入 8 碼統一編號或 10 碼醫療機構代碼";
                return output;
            }

            Core.Models.Company? company;
            if (!string.IsNullOrEmpty(normalizedTaxId))
            {
                var companies = await db.Companies
                    .AsNoTracking()
                    .Where(e => !e.IsDeleted && e.TaxID == normalizedTaxId)
                    .OrderBy(e => e.Id)
                    .Take(2)
                    .ToListAsync();
                if (companies.Count > 1)
                {
                    output.MatchStatus = "Ambiguous";
                    output.Error = "找到多筆相同統編的公司資料，請由系統管理者確認";
                    return output;
                }

                company = companies.SingleOrDefault();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    output.Error = "請輸入統編或公司／機構名稱";
                    return output;
                }

                var companies = await db.Companies
                    .AsNoTracking()
                    .Where(e => !e.IsDeleted && e.Name == normalizedName)
                    .OrderBy(e => e.Id)
                    .Take(2)
                    .ToListAsync();
                if (companies.Count > 1)
                {
                    output.MatchStatus = "Ambiguous";
                    output.Error = "找到多筆同名公司／機構資料，無法自動判斷，請改用統編或由系統管理者確認";
                    return output;
                }

                company = companies.SingleOrDefault();
            }

            if (company == null)
            {
                output.MatchStatus = "NotFound";
                output.Error = "查無既有公司資料，可繼續建立新資料";
                return output;
            }

            mapper.Map(company, output.Company);
            // 查詢結果只用於帶入畫面；Save 會在伺服器端重新依識別資料確認並建立關聯。
            output.Company.Id = 0;
            output.MatchStatus = "Found";
            output.Success = true;
            return output;
        }

        public async Task<ResponseMessageDto> Save(CompanyDto dto)
        {
            ResponseMessageDto responseMessageDto = new ResponseMessageDto { Success=false };
            try
            {
                dto.TaxID = NormalizeTaxId(dto.TaxID);
                dto.Name = NormalizeCompanyName(dto.Name);
                if (!string.IsNullOrEmpty(dto.TaxID) && !IsValidTaxId(dto.TaxID))
                    throw new Exception("請輸入 8 碼統一編號或 10 碼醫療機構代碼");
                if (string.IsNullOrWhiteSpace(dto.Name))
                    throw new Exception("請輸入公司或機構名稱");
                if (dto.Id == 0) return await Insert(dto);
                var siteId = await loginUserData.GetWebsiteId();
                if (siteId <= 0) throw new Exception("登入狀態異常");
                var belongsToCurrentWebsite = await db.MappingCompanyAndWebsites.AnyAsync(e =>
                    !e.IsDeleted &&
                    e.FK_WebsiteId == siteId &&
                    e.FK_CompanyId == dto.Id);
                if (!belongsToCurrentWebsite)
                    throw new Exception("公司資料與目前網站不符，請重新整理後再試");

                var data = await db.Companies.Where(e => !e.IsDeleted).Where(e => e.Id == dto.Id).FirstOrDefaultAsync();
                if (data != null)
                {
                    if (!string.Equals(data.TaxID, dto.TaxID, StringComparison.Ordinal))
                        throw new Exception("統編不可在公司資訊編輯中變更");
                    mapper.Map(dto, data);
                    await loginUserData.SaveChanges(data);
                    responseMessageDto.Success = true;
                    responseMessageDto.Message = data.Id.ToString();
                }
                else throw new Exception("資料不存在！");
            }
            catch(Exception ex)
            {
                responseMessageDto.Error = ex.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(responseMessageDto));
            return responseMessageDto;
        }
        private async Task<ResponseMessageDto> Insert(CompanyDto dto) {
			ResponseMessageDto responseMessageDto = new ResponseMessageDto ();
            try {
				var siteId = await loginUserData.GetWebsiteId();
				if (siteId <= 0) throw new Exception("登入狀態異常");

				await using var transaction = await db.Database.BeginTransactionAsync(
					System.Data.IsolationLevel.Serializable);
				var websiteAlreadyHasCompany = await db.MappingCompanyAndWebsites.AnyAsync(e =>
					!e.IsDeleted && e.FK_WebsiteId == siteId);
				if (websiteAlreadyHasCompany)
					throw new Exception("目前網站已綁定公司資料，請重新整理後再編輯");

				Core.Models.Company? existingCompany;
				if (!string.IsNullOrEmpty(dto.TaxID))
				{
					var sameTaxIdCompanies = await db.Companies
						.Where(e => !e.IsDeleted && e.TaxID == dto.TaxID)
						.OrderBy(e => e.Id)
						.Take(2)
						.ToListAsync();
					if (sameTaxIdCompanies.Count > 1)
						throw new Exception("找到多筆相同統編的公司資料，無法自動建立關聯，請由系統管理者確認");
					existingCompany = sameTaxIdCompanies.SingleOrDefault();
				}
				else
				{
					var sameNameCompanies = await db.Companies
						.Where(e => !e.IsDeleted && e.Name == dto.Name)
						.OrderBy(e => e.Id)
						.Take(2)
						.ToListAsync();
					if (sameNameCompanies.Count > 1)
						throw new Exception("找到多筆同名公司／機構資料，無法自動建立關聯，請由系統管理者確認");
					existingCompany = sameNameCompanies.SingleOrDefault();
				}
				if (existingCompany != null)
				{
					await WebsiteMapping(existingCompany.Id, siteId);
					responseMessageDto.Success = true;
					responseMessageDto.Message = existingCompany.Id.ToString();
				}
				else
				{
					if (string.IsNullOrWhiteSpace(dto.Name) ||
						string.IsNullOrWhiteSpace(dto.Contact) ||
						string.IsNullOrWhiteSpace(dto.Email) ||
						string.IsNullOrWhiteSpace(dto.Address))
					{
						throw new Exception("請填寫完整公司資訊");
					}

                    Core.Models.Company company = new Core.Models.Company();
					mapper.Map(dto, company);
					db.Companies.Add(company);
					await loginUserData.SaveChanges(company);
					await WebsiteMapping(company.Id, siteId);
					responseMessageDto.Success = true;
					responseMessageDto.Message = company.Id.ToString();
				}

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				responseMessageDto.Error = ex.Message;
			}
			await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(responseMessageDto));
			return responseMessageDto;
		}
		private async Task WebsiteMapping(long cid, long siteId) {
			var mappingExists = await db.MappingCompanyAndWebsites.AnyAsync(e =>
				!e.IsDeleted && e.FK_CompanyId == cid && e.FK_WebsiteId == siteId);
			if (mappingExists) return;
			Core.Models.MappingCompanyAndWebsites mapping = new Core.Models.MappingCompanyAndWebsites { 
                FK_CompanyId = cid,
                FK_WebsiteId = siteId,
            };
            db.MappingCompanyAndWebsites.Add(mapping);
			await loginUserData.SaveChanges(mapping);
		}

		private static string NormalizeTaxId(string? taxId)
		{
			return new string((taxId ?? string.Empty).Where(char.IsDigit).ToArray());
		}

		private static string NormalizeCompanyName(string? name)
		{
			return string.Join(" ", (name ?? string.Empty)
				.Trim()
				.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
		}

		private static bool IsValidTaxId(string taxId)
		{
			return taxId.Length == 8 || taxId.Length == 10;
		}
	}
}
