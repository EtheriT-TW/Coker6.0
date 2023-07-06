using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.Application.Shared.Dto.Import;
using EtheriT.Coker.Core.Models;
using AutoMapper;

namespace EtheriT.Coker.Application.TechnicalCertificate
{
	public class TechnicalCertificateAppService : ITechnicalCertificateAppService
	{
		private readonly CokerDbContext db;
		private readonly LoginUserData loginUserData;
		private readonly IFileUploadAppService fileUploadAppService;
		private readonly IConfiguration configuration;
		private readonly IMapper mapper;
		public TechnicalCertificateAppService(
			CokerDbContext db,
			LoginUserData loginUserData,
			IFileUploadAppService fileUploadAppService,
			IConfiguration configuration,
			IMapper mapper
		)
		{
			this.db = db;
			this.loginUserData = loginUserData;
			this.fileUploadAppService = fileUploadAppService;
			this.configuration = configuration;
			this.mapper = mapper;
		}
		public async Task<ResponseMessageDto> AddUp(TechCertDto dto)
		{
			ResponseMessageDto output = new ResponseMessageDto() { Success = false };
			try
			{
				var asoid = dto.Id;
				if (dto.Id == 0)
				{
					var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
					if (db_t != null)
					{
						long WebsiteID = await loginUserData.GetWebsiteId();
						Core.Models.TechnicalCertificate tc = new Core.Models.TechnicalCertificate
						{
							FK_WebsiteId = WebsiteID,
							Disp_opt = dto.Disp_opt,
							Img = dto.Img,
							Title = dto.Title,
							Description = dto.Description,
							Ser_no = dto.Ser_no,
							StartDate = dto.StartDate,
							EndDate = dto.EndDate,
							Permanent = dto.Permanent,
							CreatorUserId = (long)db_t.UserID
						};
						db.TechnicalCertificates.Add(tc);
						db.SaveChanges();
						asoid = tc.Id;
					}
					else throw new Exception("查無資料");
				}
				else
				{
					var db_tc = db.TechnicalCertificates.Where(e => e.Id == dto.Id).FirstOrDefault();
					var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
					if (db_tc != null && db_t != null)
					{
						db_tc.Disp_opt = dto.Disp_opt;
						db_tc.Img = dto.Img;
						db_tc.Title = dto.Title;
						db_tc.Description = dto.Description;
						db_tc.Ser_no = dto.Ser_no;
						db_tc.StartDate = dto.StartDate;
						db_tc.EndDate = dto.EndDate;
						db_tc.Permanent = dto.Permanent;
						db_tc.CreatorUserId = (long)db_t.UserID;

						db_tc.LastModificationTime = DateTime.Now;
						db_tc.LastModifierUserId = db_t.UserID;
						db.SaveChanges();
					}
					else throw new Exception("查無資料");
				}
				output.Success = true;
				output.Message = asoid.ToString();
			}
			catch (Exception e)
			{
				output.Success = false;
				output.Error = e.Message;
			}
			return output;
		}
		public async Task<ImportOutputDto> AddAll(List<TechCertDto> dto)
		{
			long WebsiteID = await loginUserData.GetWebsiteId();
			long userId = await loginUserData.GetUserId();
			ImportOutputDto output = new ImportOutputDto() { Success = false, ErrorList = new List<ImportMassageItem>() };
			try
			{
				var datas = dto.GroupBy(x => new { x.Title, x.Description, x.Ser_no })
								.Select(e => new { e.Key.Title, e.Key.Description, e.Key.Ser_no }).ToList();
				var tecTitle = datas.Select(e => e.Title).ToList();
				var files = dto.Where(e => !string.IsNullOrEmpty(e.Img)).GroupBy(x => x.Img).Select(e => e.Key).ToList();
				var currently = db.TechnicalCertificates.Where(e => tecTitle.Contains(e.Title));
				List<Core.Models.TechnicalCertificate> tech = new List<Core.Models.TechnicalCertificate>();
				for (int i = 0; i < datas.Count; i++)
				{
					var cert = datas[i];
					var theDate = currently.Where(e => cert.Title == e.Title).FirstOrDefault();
					if (theDate != null)
					{
						theDate.Description = cert.Description;
						theDate.Ser_no = cert.Ser_no;
					}
					else
					{
						var d = tech.Find(e => e.Title == cert.Title);
						if (d != null)
						{
							d.Description = cert.Description;
							d.Ser_no = cert.Ser_no;
						}
						else
						{
							tech.Add(new Core.Models.TechnicalCertificate
							{
								Title = cert.Title,
								Description = cert.Description,
								Ser_no = cert.Ser_no,
								FK_WebsiteId = WebsiteID,
								CreatorUserId = userId,
								Permanent = true,
							});
						}
					}
				}
				db.TechnicalCertificates.AddRange(tech);
				await db.SaveChangesAsync();
				var newTech = db.TechnicalCertificates.Where(e => tecTitle.Contains(e.Title)).Select(e => new { e.Id, e.Title }).ToList();

				List<FileImageImportDto> uploadFiles = new List<FileImageImportDto>();
				foreach (var file in files)
				{
					if (!string.IsNullOrEmpty(file))
					{
						var items = dto.FindAll(e => e.Img == file);
						if (items != null)
						{
							for (int i = 0; i < items.Count; i++)
							{
								var tec = newTech.Find(e => e.Title == items[i].Title);
								if (tec != null)
								{
									uploadFiles.Add(new FileImageImportDto
									{
										mediaLink = file,
										Type = FileBindTypeEnum.技術證照,
										SerNo = 500,
										SId = tec.Id,
									});
								}
							}
						}
					}
				}
				var response = await fileUploadAppService.uploadImageLink(uploadFiles);
				output.Success = true;
			}
			catch
			{

			}
			return output;
		}

		public async Task<ResponseMessageDto> TechCertAssociateAddDelect(List<TechCertProdAssociateDto> dto)
		{
			ResponseMessageDto output = new ResponseMessageDto() { Success = false };

			try
			{
				long usetId = await loginUserData.GetUserId();
				List<Prod_TechCert> techs = new List<Prod_TechCert>();
				var crrent = db.Prod_TechCerts.Where(e => !e.IsDeleted).Select(e => new { e.Id, e.FK_TCId, e.FK_PId }).ToList();
				foreach (var data in dto)
				{
					var item = crrent.Find(e => e.FK_TCId == data.FK_TCId && e.FK_PId == data.FK_PId);
					if (item != null) data.Id = item.Id;
					else data.Id = 0;

					if (data.Id == 0 && !data.IsDeleted)
					{
						if (techs.Find(e => e.FK_TCId == data.FK_TCId && e.FK_PId == data.FK_PId) == null)
						{
							Core.Models.Prod_TechCert ptc = new Core.Models.Prod_TechCert
							{
								FK_PId = data.FK_PId,
								FK_TCId = data.FK_TCId,
								CreatorUserId = usetId,
							};
							techs.Add(ptc);
						}
					}
					else if (data.Id > 0 && data.IsDeleted)
					{
						await this.TechCertAssociateDelete((long)data.Id);
					}
				}
				db.Prod_TechCerts.AddRange(techs);
				await db.SaveChangesAsync();
				output.Success = true;
			}
			catch (Exception e)
			{
				output.Success = false;
				output.Error = e.Message;
			}

			return output;
		}
		public async Task<List<TechCertGetSelectedDto>> GetTechCertAssociate(long Pid)
		{
			try
			{

				long WebsiteID = await loginUserData.GetWebsiteId();
				if (WebsiteID == 0)
				{
					WebsiteID = configuration.GetValue<long>("WebConfig:SiteId");
				}

				var output = from ptc in db.Prod_TechCerts
							 where ptc.FK_PId == Pid && !ptc.IsDeleted
							 join tc in db.TechnicalCertificates on ptc.FK_TCId equals tc.Id
							 where !tc.IsDeleted && tc.FK_WebsiteId == WebsiteID
							 select new TechCertGetSelectedDto
							 {
								 Id = ptc.Id,
								 FK_TCId = ptc.FK_TCId,
								 TechCert_Name = tc.Title
							 };

				return await output.ToListAsync();

			}
			catch (Exception e) { }

			return null;
		}
		public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
		{
			try
			{
				long webid = await loginUserData.GetWebsiteId();

				var dataQuery = from e in db.TechnicalCertificates
								where !e.IsDeleted && e.FK_WebsiteId == webid
								select new TechCertGetAllListDto
								{
									Id = e.Id,
									Disp_opt = e.Disp_opt,
									Title = e.Title,
									Img = new List<string>(),
									Description = e.Description,
									Ser_no = e.Ser_no,
									StartDate = e.StartDate,
									EndDate = e.EndDate,
									Permanent = e.Permanent,
								};
				var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
				if (output != null)
				{
					foreach (var data in output.data)
					{
						var tid = data.GetType().GetProperty("Id").GetValue(data, null);
						var getImgFileInput = new FileGetImgInputDto
						{
							Sid = (long)tid,
							Type = (int)FileBindTypeEnum.技術證照,
							Size = 3
						};
						var image = (await fileUploadAppService.getImgFiles(getImgFileInput));
						if (image.Count > 0)
						{
							var img_list = new List<string>();
							foreach (var img in image)
							{
								img_list.Add(img.Link);
							}
							data.GetType().GetProperty("Img").SetValue(data, img_list);
						}
					}
				}
				return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
			}
			catch (Exception e)
			{
				var expectiontext = e;
				return new JsonResult(new List<TechCertGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
			}
		}
		public async Task<List<TechCertDisplayDto>> GetDisplayData(long pid)
		{
			try
			{

				var tcdatas = await (from ptc in db.Prod_TechCerts
									 where ptc.FK_PId == pid && !ptc.IsDeleted
									 join tc in db.TechnicalCertificates on ptc.FK_TCId equals tc.Id
									 where !tc.IsDeleted
									 select new TechCertDisplayDto
									 {
										 Id = tc.Id,
										 Img_orig = new List<FileGetImgDto>(),
										 Img_small = new List<FileGetImgDto>(),
										 Title = tc.Title,
										 Description = tc.Description
									 }).ToListAsync(); ;

				foreach (var tcdata in tcdatas)
				{
					var imgdatas_orig = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
					{
						Sid = tcdata.Id,
						Type = (int)FileBindTypeEnum.技術證照,
						Size = 1
					});
					if (imgdatas_orig.Count > 0)
					{
						foreach (var imgdata in imgdatas_orig)
						{
							if (imgdata.Link != null)
							{
								tcdata.Img_orig.Add(new FileGetImgDto
								{
									Id = imgdata.Id,
									Link = imgdata.Link,
									Name = imgdata.Name,
								});
							}
						}
					}


					var imgdatas_small = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
					{
						Sid = tcdata.Id,
						Type = (int)FileBindTypeEnum.技術證照,
						Size = 3
					});
					if (imgdatas_small.Count > 0)
					{
						foreach (var imgdata in imgdatas_small)
						{
							if (imgdata.Link != null)
							{
								tcdata.Img_small.Add(new FileGetImgDto
								{
									Id = imgdata.Id,
									Link = imgdata.Link,
									Name = imgdata.Name,
								});
							}
						}
					}
				}
				return tcdatas;
			}
			catch (Exception e)
			{
			}

			return null;
		}
		public async Task<TechCertDto> GetOne(int id)
		{
			try
			{
				var result = db.TechnicalCertificates.Where(e => e.Id == id && !e.IsDeleted).FirstOrDefault();

				if (result != null)
				{
					TechCertDto output = new TechCertDto()
					{
						Id = result.Id,
						Disp_opt = result.Disp_opt,
						Img = result.Img,
						Title = result.Title,
						Description = result.Description,
						Ser_no = result.Ser_no,
						StartDate = result.StartDate,
						EndDate = result.EndDate,
						Permanent = result.Permanent,
					};

					return output;
				}
				else throw new Exception("查無資料");
			}
			catch (Exception e)
			{

			}

			return null;
		}
		public async Task<ResponseMessageDto> Delete(long Id)
		{
			ResponseMessageDto output = new ResponseMessageDto() { Success = false };

			try
			{
				var db_tc = db.TechnicalCertificates.Where(e => e.Id == Id).FirstOrDefault();
				long usetId = await loginUserData.GetUserId();

				if (db_tc != null)
				{
					var delete_img_dto = new FileDeleteDto
					{
						Sid = db_tc.Id,
						Type = (int)FileBindTypeEnum.技術證照
					};
					var imgdelete_response = await fileUploadAppService.deleteFileById(delete_img_dto);

					db_tc.IsDeleted = true;
					db_tc.DeletionTime = DateTime.Now;
					db_tc.DeleterUserId = usetId;
					db.SaveChanges();
					output.Success = imgdelete_response.Success;
				}
				else throw new Exception("查無資料");
			}
			catch (Exception e)
			{
				output.Success = false;
				output.Error = e.Message;
			}

			return output;
		}
		public async Task<ResponseMessageDto> TechCertAssociateDelete(long Id)
		{

			ResponseMessageDto output = new ResponseMessageDto() { Success = false };

			try
			{
				long usetId = await loginUserData.GetUserId();
				var db_ptc = await db.Prod_TechCerts.Where(e => e.Id == Id).ToListAsync();

				if (db_ptc != null)
				{
					foreach (var item in db_ptc)
					{
						item.IsDeleted = true;
						item.DeletionTime = DateTime.Now;
						item.DeleterUserId = usetId;
						db.SaveChanges();
						output.Success = true;
					}
				}
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
