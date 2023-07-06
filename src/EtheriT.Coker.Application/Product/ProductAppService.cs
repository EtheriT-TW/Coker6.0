using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.Dto.Import;
using Microsoft.AspNetCore.Http;
using EtheriT.Coker.Application.Import;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto;
using AutoMapper;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.Specification;
using EtheriT.Coker.Application.Shared.Dto.Specification;
using EtheriT.Coker.Application.Shared.Dto.Article;
using System.Web;
using System.Data;

namespace EtheriT.Coker.Application.Product
{
	public class ProductAppService : IProductAppService
	{
		private readonly CokerDbContext db;
		private readonly LoginUserData loginUserData;
		private readonly ITagAppService tagAppService;
		private readonly IConfiguration configuration;
		private readonly IMapper mapper;
		private readonly ITechnicalCertificateAppService technicalCertificateAppService;
		private readonly IFileUploadAppService fileUploadAppService;
		private readonly ISpecificationAppService specificationAppService;
		private readonly ImportAppService importAppService;
		public ProductAppService(
			CokerDbContext db,
			LoginUserData loginUserData,
			ITagAppService tagAppService,
			IConfiguration configuration,
			IMapper mapper,
			ITechnicalCertificateAppService technicalCertificateAppService,
			IFileUploadAppService fileUploadAppService,
			ISpecificationAppService specificationAppService,
			ImportAppService importAppService
		)
		{
			this.db = db;
			this.loginUserData = loginUserData;
			this.tagAppService = tagAppService;
			this.configuration = configuration;
			this.technicalCertificateAppService = technicalCertificateAppService;
			this.importAppService = importAppService;
			this.fileUploadAppService = fileUploadAppService;
			this.specificationAppService = specificationAppService;
			this.mapper = mapper;
		}
		/* Add & Update */
		public async Task<ResponseMessageDto> ProductAddUp(ProdAddUpDto dto)
		{
			ResponseMessageDto output = new ResponseMessageDto() { Success = false };
			ResponseMessageDto tag_response = new ResponseMessageDto() { Success = true };
			ResponseMessageDto techcert_response = new ResponseMessageDto() { Success = true };
			ResponseMessageDto stock_response = new ResponseMessageDto() { Success = true };
			var asoid = dto.Id;

			try
			{
				long WebsiteID = await loginUserData.GetWebsiteId();
				long userId = await loginUserData.GetUserId();

				if (dto.Id == 0)
				{
					Core.Models.Prod p = new Core.Models.Prod
					{
						FK_WebsiteId = WebsiteID,
						Title = dto.Title,
						Disp_Opt = dto.Disp_Opt,
						Ser_No = dto.Ser_No,
						Introduction = dto.Introduction ?? "",
						Description = dto.Description ?? "",
						StartTime = dto.StartTime,
						EndTime = dto.EndTime,
						permanent = dto.Permanent,
						CreatorUserId = userId
					};
					db.Prods.Add(p);
					db.SaveChanges();
					asoid = p.Id;
				}
				else
				{
					var db_p = db.Prods.Where(e => e.Id == dto.Id).FirstOrDefault();
					if (db_p != null)
					{
						db_p.Title = dto.Title;
						db_p.Disp_Opt = dto.Disp_Opt;
						db_p.Ser_No = dto.Ser_No;
						db_p.Introduction = dto.Introduction;
						db_p.Description = dto.Description;
						db_p.StartTime = dto.StartTime;
						db_p.EndTime = dto.EndTime;
						db_p.permanent = dto.Permanent;
						db_p.LastModificationTime = DateTime.Now;
						db_p.LastModifierUserId = userId;
					}
				}
				db.SaveChanges();

				if (asoid != 0)
				{
					var tagitem = new List<TagAssociateDto>();
					foreach (var data in dto.TagSelected)
					{
						tagitem.Add(new TagAssociateDto()
						{
							Id = data.Id,
							FK_AId = (long)asoid,
							FK_TId = data.FK_TId,
							Type = (int)TagAssociateTypeEnum.商品,
							IsDeleted = data.IsDeleted
						});
					}

					tag_response = await tagAppService.TagAssociateAddDelect(tagitem);

					var techcertitem = new List<TechCertProdAssociateDto>();
					foreach (var data in dto.TechCertSelected)
					{
						techcertitem.Add(new TechCertProdAssociateDto()
						{
							Id = data.Id,
							FK_PId = (long)asoid,
							FK_TCId = data.FK_TCId,
							IsDeleted = data.IsDeleted
						});
					}

					techcert_response = await technicalCertificateAppService.TechCertAssociateAddDelect(techcertitem);

					stock_response = await this.StockAddUp(asoid, dto.Stocks);
				}

				output.Success = tag_response.Success && techcert_response.Success && stock_response.Success;
				output.Message = asoid.ToString();
			}
			catch (Exception e)
			{
				output.Success = false;
				output.Error = e.Message;
			}

			return output;
		}
		public async Task<ResponseMessageDto> StockAddUp(long Pid, List<ProductStockDto> dto)
		{
			ResponseMessageDto output = new ResponseMessageDto() { Success = false };
			ResponseMessageDto priceresponse = new ResponseMessageDto() { Success = false };
			if (dto.Count == 0)
			{
				output.Success = true;
				return output;
			}
			try
			{
				long usetId = await loginUserData.GetUserId();
				output.Message = "";
				for (int i = 0; i < dto.Count; i++)
				{
					var item = dto[i];
					if (item.Id == 0)
					{
						Core.Models.Prod_Stock ps = new Core.Models.Prod_Stock
						{
							FK_Pid = Pid,
							FK_S1id = item.FK_S1id,
							FK_S2id = item.FK_S2id,
							Stock = item.Stock,
							Min_Qty = item.Min_Qty,
							Alert_Qty = item.Alert_Qty,
							Ser_No = item.Ser_No,
							CreatorUserId = usetId,
						};
						db.Prod_Stocks.Add(ps);
						await db.SaveChangesAsync();

						foreach (var price in item.Prices)
						{
							price.FK_PSId = ps.Id;

						}
					}
					else
					{
						var db_ps = await db.Prod_Stocks.Where(e => e.Id == item.Id).FirstOrDefaultAsync();

						if (db_ps != null)
						{
							db_ps.FK_S1id = item.FK_S1id;
							db_ps.FK_S2id = item.FK_S2id;
							db_ps.Stock = item.Stock;
							db_ps.Min_Qty = item.Min_Qty;
							db_ps.Alert_Qty = item.Alert_Qty;
							db_ps.Ser_No = item.Ser_No;
							db_ps.LastModificationTime = DateTime.Now;
							db_ps.LastModifierUserId = usetId;
						}
					}

					priceresponse = await this.PriceAddUp(item.Prices);

				}

				db.SaveChanges();

				output.Success = priceresponse.Success;
			}
			catch (Exception e)
			{
				output.Success = false;
				output.Error = e.Message;
			}

			return output;
		}
		public async Task<ResponseMessageDto> PriceAddUp(List<ProductPriceDto> dto)
		{
			ResponseMessageDto output = new ResponseMessageDto() { Success = false };
			ResponseMessageDto deleteresponse = new ResponseMessageDto() { Success = true };
			try
			{
				long usetId = await loginUserData.GetUserId();
				if (usetId != 0)
				{
					for (int i = 0; i < dto.Count; i++)
					{
						var item = dto[i];
						var thePrice = await db.Prod_Prices
								.Where(e => e.FK_PSId == item.FK_PSId)
								.Where(e => e.FK_RId == item.FK_RId)
								.Where(e => e.Price == item.Price).FirstOrDefaultAsync();
						if (thePrice != null) item.Id = thePrice.Id;

						if (item.Id == 0 && !item.IsDelete)
						{
							Prod_Price pp = new Prod_Price
							{
								FK_PSId = (long)item.FK_PSId,
								FK_RId = item.FK_RId,
								Price = item.Price,
								Bonus = item.Bonus,
								CreatorUserId = usetId
							};
							db.Prod_Prices.Add(pp);
							await db.SaveChangesAsync();
						}
						else if (!item.IsDelete)
						{
							var db_pp = await db.Prod_Prices.Where(e => e.Id == item.Id).FirstOrDefaultAsync();

							if (db_pp != null)
							{
								db_pp.FK_RId = item.FK_RId;
								db_pp.Price = item.Price;
								db_pp.Bonus = item.Bonus;
								db_pp.LastModifierUserId = usetId;
								db_pp.LastModificationTime = DateTime.Now;
							}
						}
						else
						{
							deleteresponse = await this.PriceDelete((long)item.Id);
							if (!deleteresponse.Success)
							{
								output.Success = false;
							}
						}
					}
				}

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
		/* Get Data */
		public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
		{
			try
			{
				long webid = await loginUserData.GetWebsiteId();

				var dataQuery = from p in db.Prods
								where p.FK_WebsiteId == webid && !p.IsDeleted
								select new ProductGetAllListDto
								{
									Id = p.Id,
									Title = p.Title,
									Disp_Opt = p.Disp_Opt,
									Ser_No = p.Ser_No,
									Price = "",
									StartTime = p.StartTime == null ? "-" : string.Format("{0:yyyy-MM-dd hh:mm}", p.StartTime),
									EndTime = p.EndTime == null ? "-" : string.Format("{0:yyyy-MM-dd hh:mm}", p.EndTime),
									Permanent = p.permanent
								};

				var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);

				int min_price = 0, max_price = 0;
				foreach (var data in output.data)
				{
					var pid = (long)data.GetType().GetProperty("Id").GetValue(data, null);
					var db_ps = await db.Prod_Stocks.Where(e => e.FK_Pid == pid && !e.IsDeleted).ToListAsync();
					if (db_ps.Count > 0)
					{
						for (var ps_i = 0; ps_i < db_ps.Count(); ps_i++)
						{
							var db_pp = await db.Prod_Prices.Where(e => e.FK_PSId == db_ps[ps_i].Id && !e.IsDeleted).ToListAsync();
							if (db_pp.Count > 0)
							{
								for (var pp_i = 0; pp_i < db_pp.Count; pp_i++)
								{
									var price = (int)(db_pp[pp_i].Price ?? 0);
									if (min_price == 0 || price < min_price) { min_price = price; }
									if (price > max_price) { max_price = price; }
								}
							}
						}
						var price_text = min_price == max_price ? max_price.ToString("###,###") : $"{min_price}~{max_price}";
						data.GetType().GetProperty("Price").SetValue(data, price_text);
					}
				}
				return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
			}
			catch (Exception e)
			{

			}

			return new JsonResult(new List<ProductGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
		}
		public async Task<ProdGetDataDto> GetProdDataOne(long Id)
		{
			try
			{
				var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
				var db_p = db.Prods.Where(e => e.Id == Id).OrderBy(e => e.Ser_No).FirstOrDefault();

				if (db_p != null)
				{
					ProdGetDataDto output = new ProdGetDataDto()
					{
						Id = db_p.Id,
						Title = db_p.Title,
						Disp_Opt = db_p.Disp_Opt,
						Ser_No = db_p.Ser_No,
						Introduction = db_p.Introduction,
						Description = db_p.Description,
						StartTime = db_p.StartTime,
						EndTime = db_p.EndTime,
						Permanent = db_p.permanent,
						TagDatas = new List<TagGetSelectedDto>(),
						TechCertDatas = new List<TechCertGetSelectedDto>(),
						Stocks = new List<ProductStockDto>(),
						Files = new List<FileGetProdDisplayDto>(),
					};

					var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
					{
						Fk_Aid = output.Id,
						Type = (int)TagAssociateTypeEnum.商品,
					}
					);

					if (tagDatas != null)
					{
						output.TagDatas = tagDatas;
					}

					var techcertDatas = await technicalCertificateAppService.GetTechCertAssociate(db_p.Id);

					if (techcertDatas != null)
					{
						output.TechCertDatas = techcertDatas;
					}

					var stockDatas = await this.GetStockDataAll(output.Id);
					if (stockDatas != null)
					{
						output.Stocks = stockDatas;
					}

					var fileDatas = await fileUploadAppService.getProdDisplayFiles(output.Id, 1);
					if (fileDatas != null)
					{
						output.Files = fileDatas;
					}

					return output;
				}
				else throw new Exception("查無商品資料");
			}
			catch (Exception e)
			{
				return null;
			}
		}
		public async Task<List<ProductStockDto>> GetStockDataAll(long PId)
		{
			try
			{
				var output = await (from ps in db.Prod_Stocks
									where !ps.IsDeleted && ps.FK_Pid == PId
									orderby ps.Id
									select new ProductStockDto
									{
										Pid = PId,
										Id = ps.Id,
										FK_S1id = ps.FK_S1id,
										S1_Title = "",
										FK_S2id = ps.FK_S2id,
										S2_Title = "",
										Price = ps.Price,
										Min_Qty = ps.Min_Qty,
										Stock = ps.Stock,
										Alert_Qty = ps.Alert_Qty,
										Prices = new List<ProductPriceDto>(),
									}).ToListAsync();

				var db_sp = await db.Prod_Specs.Where(e => !e.IsDeleted).ToListAsync();

				if (db_sp.Count > 0)
				{
					foreach (var item in output)
					{
						item.FK_ST1id = (int)item.FK_S1id != 0 ? db_sp.Find(spec => spec.Id == item.FK_S1id).FK_Tid : 0;
						item.S1_Title = (int)item.FK_S1id != 0 ? db_sp.Find(spec => spec.Id == item.FK_S1id).Title : "";
						item.FK_ST2id = (int)item.FK_S2id != 0 ? db_sp.Find(spec => spec.Id == item.FK_S2id).FK_Tid : 0;
						item.S2_Title = (int)item.FK_S2id != 0 ? db_sp.Find(spec => spec.Id == item.FK_S2id).Title : "";
						item.Prices = await this.GetPriceDataAll(item.Id);
					}
				}

				return output;
			}
			catch (Exception e)
			{
				return null;
			}

		}
		public async Task<List<ProductPriceDto>> GetPriceDataAll(long PSId)
		{
			try
			{
				var output = await (from pp in db.Prod_Prices
									where !pp.IsDeleted && pp.FK_PSId == PSId
									orderby pp.FK_PSId
									select new ProductPriceDto
									{
										Id = pp.Id,
										FK_PSId = pp.FK_PSId,
										FK_RId = pp.FK_RId,
										Price = pp.Price,
										Bonus = pp.Bonus ?? 0,
									}).ToListAsync();

				return output;
			}
			catch (Exception e)
			{

			}

			return null;
		}
		public async Task<ProdGetMainDisplayDto> GetMainDisplayOne(long Id)
		{
			try
			{
				var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
				var db_p = db.Prods.Where(e => e.Id == Id).OrderBy(e => e.Ser_No).FirstOrDefault();

				if (db_p != null)
				{
					ProdGetMainDisplayDto output = new ProdGetMainDisplayDto()
					{
						Id = db_p.Id,
						Title = db_p.Title,
						Introduction = db_p.Introduction,
						Description = db_p.Description,
						TagDatas = new List<TagGetSelectedDto>(),
						TechCertDatas = new List<TechCertDisplayDto>(),
						Stocks = new List<ProductStockDto>(),
						Img_Original = new List<FileGetProdDisplayDto>(),
						Img_Medium = new List<FileGetProdDisplayDto>(),
						Img_Small = new List<FileGetProdDisplayDto>(),
					};

					var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
					{
						Fk_Aid = output.Id,
						Type = (int)TagAssociateTypeEnum.商品,
					}
					);

					if (tagDatas != null)
					{
						output.TagDatas = tagDatas;
					}

					var techcertDatas = await technicalCertificateAppService.GetDisplayData(db_p.Id);

					if (techcertDatas != null)
					{
						output.TechCertDatas = techcertDatas;
					}

					var stockDatas = await this.GetStockDataAll(output.Id);
					if (stockDatas != null)
					{
						output.Stocks = stockDatas;
					}

					var Files = await fileUploadAppService.getImgFiles(new FileGetImgInputDto()
					{
						Sid = output.Id,
						Size = 1,
						Type = 8
					});
					if (Files.Count != null) output.Files = Files;

					var Imgs_original = await fileUploadAppService.getProdDisplayFiles(output.Id, 1);
					if (Imgs_original != null)
					{
						output.Img_Original = Imgs_original;
					}

					var Imgs_medium = await fileUploadAppService.getProdDisplayFiles(output.Id, 2);
					if (Imgs_medium != null)
					{
						output.Img_Medium = Imgs_medium;
					}

					var Imgs_small = await fileUploadAppService.getProdDisplayFiles(output.Id, 3);
					if (Imgs_small != null)
					{
						output.Img_Small = Imgs_small;
					}

					return output;
				}
				else throw new Exception("查無商品資料");
			}
			catch (Exception e)
			{
				return null;
			}
		}
		public async Task<List<DirectoryReleInfoDto>> GetDirectoryReleInfo(DirectoryReleInfoInputDto dto)
		{

			try
			{
				long WebsiteID = dto.SiteId == 0 ? await loginUserData.GetWebsiteId() : (long)dto.SiteId;
				var output = new List<DirectoryReleInfoDto>();
				var productData = new List<ProdGetDataDto>();

				foreach (var Id in dto.Ids)
				{
					var result = await db.Prods.Where(e => e.Id == Id && !e.IsDeleted && e.FK_WebsiteId == WebsiteID).FirstOrDefaultAsync();

					if (result != null)
					{
						ProdGetDataDto tempoutput = mapper.Map<ProdGetDataDto>(result);
						productData.Add(tempoutput);
					}
					else throw new Exception("查無商品資料");
				}

				if (productData != null)
				{
					productData.Sort((x, y) => (x.Ser_No.CompareTo(y.Ser_No) * 2 + x.Id.CompareTo(y.Id)));
					for (int i = 0; i < productData.Count; i++)
					{
						var data = productData[i];
						var output_data = new DirectoryReleInfoDto();
						var imagedata = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
						{
							Sid = data.Id,
							Type = (int)FileBindTypeEnum.產品,
							Size = 1
						});
						output_data = mapper.Map(data, output_data);
						output_data.Link = $"/lcb/product/toilet/{data.Id}";
						output_data.MainImage = (imagedata[0] ?? new FileGetImgDto()).Link;

						output.Add(output_data);

						//var imagedata = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
						//{
						//    Sid = data.Id,
						//    Type = (int)FileBindTypeEnum.產品,
						//    Size = 1
						//});
					}
				}

				return output;
			}
			catch (Exception e)
			{
				return null;
			}
		}
		/* Delete */
		public async Task<ResponseMessageDto> ProdDelete(long Id)
		{

			ResponseMessageDto output = new ResponseMessageDto() { Success = true };
			ResponseMessageDto tagdeleteresponse = new ResponseMessageDto() { Success = true };
			ResponseMessageDto techcertdeleteresponse = new ResponseMessageDto() { Success = true };
			ResponseMessageDto stockresponse = new ResponseMessageDto() { Success = true };
			ResponseMessageDto fileresponse = new ResponseMessageDto() { Success = true };

			try
			{
				long usetId = await loginUserData.GetUserId();
				var db_p = db.Prods.Where(e => e.Id == Id).FirstOrDefault();

				if (db_p != null)
				{
					db_p.IsDeleted = true;
					db_p.DeletionTime = DateTime.Now;
					db_p.DeleterUserId = usetId;

					var db_ps = db.Prod_Stocks.Where(e => e.FK_Pid == Id);
					if (db_ps != null)
					{
						foreach (var ps in db_ps)
						{
							ps.IsDeleted = true;
							ps.DeletionTime = DateTime.Now;
							ps.DeleterUserId = usetId;

							var db_pp = db.Prod_Prices.Where(e => e.FK_PSId == ps.Id);
							foreach (var item in db_pp)
							{
								item.IsDeleted = true;
								item.DeleterUserId = usetId;
								item.DeletionTime = DateTime.Now;
							}
						}
					}

					var db_ptc = db.Prod_TechCerts.Where(e => e.FK_PId == Id);
					if (db_ptc != null)
					{
						foreach (var pst in db_ptc)
						{
							pst.IsDeleted = true;
							pst.DeletionTime = DateTime.Now;
							pst.DeleterUserId = usetId;
						}
					}

					var tagids = await db.Tag_Associates.Where(e => e.FK_AId == Id && e.Type == (int)TagAssociateTypeEnum.商品 && !e.IsDeleted).ToListAsync();

					if (tagids != null)
					{
						foreach (var tagid in tagids)
						{

							tagdeleteresponse = await tagAppService.TagAssociateDelete(tagid.Id);
							if (tagdeleteresponse.Success == false)
							{
								output.Success = false;
							}
						}
					}

					var techcertids = await db.Prod_TechCerts.Where(e => e.FK_PId == Id && !e.IsDeleted).ToListAsync();

					if (techcertids != null)
					{
						foreach (var techcertid in techcertids)
						{
							techcertdeleteresponse = await technicalCertificateAppService.TechCertAssociateDelete(techcertid.Id);
							if (techcertdeleteresponse.Success == false)
							{
								output.Success = false;
							}
						}
					}

					var stockids = await db.Prod_Stocks.Where(e => e.FK_Pid == Id && !e.IsDeleted).ToListAsync();

					if (stockids != null)
					{
						foreach (var stockid in stockids)
						{
							stockresponse = await this.StockDelete(stockid.Id);
							if (stockresponse.Success == false)
							{
								output.Success = false;
							}
						}
					}

					fileresponse = await fileUploadAppService.deleteFileById(new FileDeleteDto()
					{
						Sid = Id,
						Type = (int)FileBindTypeEnum.產品,
					});

					output.Success = fileresponse.Success;

					db.SaveChanges();
				}
			}
			catch (Exception e)
			{
				output.Success = false;
				output.Error = e.Message;
			}

			return output;
		}
		public async Task<ResponseMessageDto> StockDelete(long Id)
		{

			ResponseMessageDto output = new ResponseMessageDto() { Success = false };

			try
			{
				long usetId = await loginUserData.GetUserId();
				var db_ps = db.Prod_Stocks.Where(e => e.Id == Id).FirstOrDefault();
				if (db_ps != null)
				{
					db_ps.IsDeleted = true;
					db_ps.DeletionTime = DateTime.Now;
					db_ps.DeleterUserId = usetId;
					db.SaveChanges();
					output.Success = true;
				}

				var db_pp = db.Prod_Prices.Where(e => e.FK_PSId == Id);
				foreach (var item in db_pp)
				{
					item.IsDeleted = true;
					item.DeleterUserId = usetId;
					item.DeletionTime = DateTime.Now;
				}
			}
			catch (Exception e)
			{
				output.Success = false;
				output.Error = e.Message;
			}

			return output;
		}
		public async Task<ResponseMessageDto> PriceDelete(long Id)
		{

			ResponseMessageDto output = new ResponseMessageDto() { Success = false };

			try
			{
				long usetId = await loginUserData.GetUserId();
				var db_pp = db.Prod_Prices.Where(e => e.Id == Id).FirstOrDefault();
				if (db_pp != null)
				{
					db_pp.IsDeleted = true;
					db_pp.DeletionTime = DateTime.Now;
					db_pp.DeleterUserId = usetId;
					db.SaveChanges();
					output.Success = true;
				}
			}
			catch (Exception e)
			{
				output.Success = false;
				output.Error = e.Message;
			}

			return output;
		}
		/* Product Log */
		public async Task<ResponseMessageDto> ClickLog(ProductLogDto dto)
		{
			ResponseMessageDto output = new ResponseMessageDto() { Success = false };

			try
			{
				var db_t = db.Tokens.Where(e => e.id == dto.FK_Tid).FirstOrDefault();

				Core.Models.Prod_Log pl = new Core.Models.Prod_Log
				{
					FK_Pid = dto.FK_Pid,
					FK_Uid = db_t.UserID,
					FK_Tid = dto.FK_Tid,
					Action = dto.Action,
				};
				db.Prod_Logs.Add(pl);
				db.SaveChanges();
				output.Success = true;
			}
			catch (Exception e)
			{
				output.Success = false;
				output.Error = e.Message;
			}

			return output;
		}
		/* Other Get */
		public async Task<ProdGetOneDto> GetDisplayOne(long id)
		{
			try
			{
				var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
				var db_p = await db.Prods.Where(e => e.Id == id && e.FK_WebsiteId == websiteId)
					.Where(e => !e.IsDeleted && (e.permanent || (DateTime.Now >= e.StartTime && DateTime.Now < e.EndTime)))
					.FirstOrDefaultAsync();
				var db_ps = db.Prod_Stocks.Where(e => e.Id == db_p.Id).FirstOrDefault();

				if (db_p != null && db_ps != null)
				{
					ProdGetOneDto output = new ProdGetOneDto()
					{
						Id = db_p.Id,
						Title = db_p.Title,
						Introduction = db_p.Introduction,
						Description = db_p.Description,
						Price = db_ps.Price,
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
		public async Task<List<ProductStockDto>> GetDisplayStock(long id)
		{
			try
			{
				var output = await (from ps in db.Prod_Stocks
									where ps.FK_Pid == id && !ps.IsDeleted
									orderby ps.Price ascending
									select new ProductStockDto
									{
										Id = ps.Id,
										FK_S1id = ps.FK_S1id,
										FK_S2id = ps.FK_S2id,
										Price = ps.Price,
										Stock = ps.Stock,
										Min_Qty = ps.Min_Qty,
									}).ToListAsync();

				var db_spt = db.Prod_Spec_Types.ToList();
				var db_sp = db.Prod_Specs.ToList();

				foreach (var item in output)
				{
					item.S1_Title = db_spt[0].Type;
					item.S1_Name = item.FK_S1id == 0 ? "" : db_sp[(int)item.FK_S1id - 1].Title;
					item.S2_Title = db_spt[1].Type;
					item.S2_Name = item.FK_S2id == 0 ? "" : db_sp[(int)item.FK_S2id - 1].Title;
				}

				return output;
			}
			catch (Exception e)
			{

			}
			return null;
		}
		public async Task<ProdGetDisplayDto> GetDisplaySimple(long id)
		{
			try
			{
				var db_p = db.Prods.Where(e => e.Id == id).FirstOrDefault();
				var db_ps = db.Prod_Stocks.Where(e => e.Id == db_p.Id).FirstOrDefault();

				if (db_p != null && db_ps != null)
				{
					ProdGetDisplayDto output = new ProdGetDisplayDto()
					{
						Id = db_p.Id,
						Title = db_p.Title,
						Introduction = db_p.Introduction,
						Description = db_p.Description,
						Link = "/Toilet/" + db_p.Id,
						Image = "/upload/product/pro_0" + db_p.Id + ".png",
						Price = db_ps.Price.ToString(),
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
		public async Task<JsonResult> GetRandomDIsplay(long webid, int num)
		{
			try
			{
				var result = await (from p in db.Prods
									where !p.IsDeleted && p.Disp_Opt && p.FK_WebsiteId == webid
									where p.permanent || (DateTime.Compare(DateTime.Now, (DateTime)p.StartTime) > 0 && DateTime.Compare(DateTime.Now, (DateTime)p.EndTime) < 0)
									orderby p.Ser_No
									join ps in db.Prod_Stocks.Where(e => !e.IsDeleted) on p.Id equals ps.FK_Pid
									group ps by new { p.Id, p.Title, p.Introduction, p.Description } into r
									orderby Guid.NewGuid()
									select new ProdGetDisplayDto
									{
										Id = r.Key.Id,
										Title = r.Key.Title,
										Introduction = r.Key.Introduction,
										Description = r.Key.Description,
										Link = "/Toilet/" + r.Key.Id,
										Image = "/upload/product/pro_0" + r.Key.Id + ".png",
										Price = r.Min(e => e.Price) == r.Max(e => e.Price) ? r.Min(e => e.Price).ToString() : r.Min(e => e.Price) + " ~ " + r.Max(e => e.Price),
									}).Take(num).ToArrayAsync();

				return new JsonResult(result, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
			}
			catch (Exception e)
			{

			}

			return new JsonResult(new List<ProdGetDisplayDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
		}
		public async Task<List<ProdDisImgDto>> GetHistoryDisplay(Guid TId)
		{
			try
			{
				var output = await (from pl in db.Prod_Logs
									where pl.FK_Tid == TId && pl.Action == 2
									orderby pl.Id descending
									select new ProdDisImgDto
									{
										Id = pl.FK_Pid,
									}).Take(4).ToListAsync();

				return output;
			}
			catch (Exception e)
			{

			}
			return null;
		}
		/* Product Log */
		public async Task<ImportOutputDto> ProdReplace(IList<IFormFile> files)
		{
			ImportOutputDto response = new ImportOutputDto { ErrorList = new List<ImportMassageItem>() };
			List<ProductImportDto> allData = (await importAppService.ProdReplace(files)).FindAll(e => !string.IsNullOrEmpty(e.Title));
			List<ProductImportDto> prods = new List<ProductImportDto>();
			List<string> allTitles = allData.Select(p => p.Title).ToList();
			var updateItems = db.Prods.Where(p => allTitles.Contains(p.Title)).Select(s => new { s.Id, s.Title }).ToList();
			long WebsiteID = await loginUserData.GetWebsiteId();
			ProductImportDto dto = null;
			for (int i = 0; i < allData.Count; i++)
			{
				var item = updateItems.Find(e => e.Title == allData[i].Title);
				allData[i].FK_WebsiteId = WebsiteID;
				if (item != null) allData[i].Id = item.Id;
				var preProds = prods.Find(e => e.Title == allData[i].Title);
				if (preProds == null)
				{
					dto = allData[i];
					dto.stocks = new List<ProductStockDto>();
					prods.Add(allData[i]);
				}
				else dto = preProds;
				if (dto != null && dto.stocks != null) dto.stocks.Add(mapper.Map<ProductStockDto>(allData[i]));
			}
			await InsertOrUpdateProd(prods, response.ErrorList);
			await ImportProdMediaLinks(prods, response.ErrorList);
			await ImportProdTags(prods, response.ErrorList);
			await importTechs(prods, response.ErrorList);
			return response;
		}
		private async Task importTechs(List<ProductImportDto> prods, List<ImportMassageItem> errors) {
			List<TechCertDto> allTech = new List<TechCertDto>();
			for (int i=0;i< prods.Count; i++) {
				var prod = prods[i];
				if(prod.Techs!=null) allTech.AddRange(prod.Techs);
			}
			await technicalCertificateAppService.AddAll(allTech);
			await importProdTech(prods, errors);
		}
		private async Task importProdTech(List<ProductImportDto> prods, List<ImportMassageItem> errors)
		{
			var prodTitle = prods.GroupBy(x => x.Title).Select(e => e.Key);
			var crrenProds = db.Prods.Where(e => !e.IsDeleted).Where(e => prodTitle.Contains(e.Title)).Select(e => new {e.Id,e.Title }).ToList();
			var techs = db.TechnicalCertificates.Where(e => !e.IsDeleted).Select(e => new { e.Id, e.Title }).ToList();

			List<TechCertProdAssociateDto> techCertProdAssociateDtos = new List<TechCertProdAssociateDto>();
			for (int i = 0; i < prods.Count; i++)
			{
				var prod = prods[i];
				var n = crrenProds.Find(e => e.Title == prod.Title);
				for (int j=0;j< prod.Techs.Count;j++) {
					var item = prod.Techs[j];
					var tec = techs.Find(e => e.Title == item.Title);
					if (tec != null)
					{
						techCertProdAssociateDtos.Add(new TechCertProdAssociateDto {
							FK_PId=n.Id,
							FK_TCId= tec.Id,
							IsDeleted=false,
						});
					}
				}
			}
			await technicalCertificateAppService.TechCertAssociateAddDelect(techCertProdAssociateDtos);
		}
		private async Task ImportProdTags(List<ProductImportDto> prods, List<ImportMassageItem> errors)
		{
			long WebsiteId = await loginUserData.GetWebsiteId();
			long userId = await loginUserData.GetUserId();
			List<string?> TagStr = prods.Where(e => !string.IsNullOrEmpty(e.Tag1)).Select(e => e.Tag1).ToList();
			List<string?> TagStr2 = prods.Where(e => !string.IsNullOrEmpty(e.Tag2)).Select(e => e.Tag2).ToList();
			List<string?> TagStr3 = prods.Where(e => !string.IsNullOrEmpty(e.Tag3)).Select(e => e.Tag3).ToList();
			List<string?> TagStr4 = prods.Where(e => !string.IsNullOrEmpty(e.Tag4)).Select(e => e.Tag4).ToList();
			List<string?> TagStr5 = prods.Where(e => !string.IsNullOrEmpty(e.Tag5)).Select(e => e.Tag5).ToList();
			List<string?> ProdStr = prods.Where(e => !string.IsNullOrEmpty(e.Title)).Select(e => e.Title).ToList();
			TagStr.AddRange(TagStr2);
			TagStr.AddRange(TagStr3);
			TagStr.AddRange(TagStr4);
			TagStr.AddRange(TagStr5);
			TagStr = TagStr.Where(e => !string.IsNullOrEmpty(e)).GroupBy(e => e).Select(e => e.Key).ToList();

			List<string> nowTags = db.Tags.Where(e => e.FK_WebsiteId == WebsiteId)
									.Where(e => !e.IsDeleted)
									.Select(e => e.Title).ToList();

			TagStr = TagStr.FindAll(e => !nowTags.Contains(e ?? ""));
			List<Core.Models.Tag> addTads = new List<Core.Models.Tag>();
			for (int i = 0; i < TagStr.Count; i++)
			{
				string? title = TagStr[i];
				if (!string.IsNullOrEmpty(title)) {
					addTads.Add(new Core.Models.Tag
					{
						Title = title,
						FK_WebsiteId = WebsiteId,
						CreatorUserId = userId,
						CreationTime = DateTime.Now,
					});
				}
			}
			db.Tags.AddRange(addTads);
			await db.SaveChangesAsync();

			await ImportProdAssociates(prods, errors);
		}
		private async Task ImportProdAssociates(List<ProductImportDto> prods, List<ImportMassageItem> errors)
		{
			long WebsiteId = await loginUserData.GetWebsiteId();
			var nowTags = db.Tags.Where(e => e.FK_WebsiteId == WebsiteId)
									.Where(e => !e.IsDeleted)
									.Select(e => new { e.Id, e.Title }).ToList();
			var allProd = db.Prods.Where(e => e.FK_WebsiteId == WebsiteId)
									.Where(e => !e.IsDeleted)
									.Select(e => new { e.Id, e.Title }).ToList();
			List<TagAssociateDto> TagAssociates = new List<TagAssociateDto>();
			for (int i = 0; i < prods.Count; i++)
			{
				var item = prods[i];
				item.Id = allProd.Find(e => e.Title == item.Title).Id;
				var tag = nowTags.FindAll(e => e.Title == item.Tag1 || e.Title == item.Tag2 || e.Title == item.Tag3 || e.Title == item.Tag4 || e.Title == item.Tag5);
				if (tag != null)
				{
					for (int j = 0; j < tag.Count; j++)
					{
						TagAssociates.Add(new TagAssociateDto
						{
							Type = (int)TagAssociateTypeEnum.商品,
							FK_TId = tag[j].Id,
							FK_AId = item.Id,
							IsDeleted = false
						});
					}
				}
			}
			await tagAppService.TagAssociateAddDelect(TagAssociates);
		}
		private async Task ImportProdMediaLinks(List<ProductImportDto> prods, List<ImportMassageItem> errors)
		{
			List<string?> ImagStr = prods.Where(e => !string.IsNullOrEmpty(e.Image1)).Select(e => e.Image1).ToList();
			List<string?> ImagStr2 = prods.Where(e => !string.IsNullOrEmpty(e.Image2)).Select(e => e.Image2).ToList();
			List<string?> ImagStr3 = prods.Where(e => !string.IsNullOrEmpty(e.Image3)).Select(e => e.Image3).ToList();
			List<string?> ImagStr4 = prods.Where(e => !string.IsNullOrEmpty(e.Image4)).Select(e => e.Image4).ToList();
			List<string?> ImagStr5 = prods.Where(e => !string.IsNullOrEmpty(e.Image5)).Select(e => e.Image5).ToList();
			List<string?> ProdStr = prods.Where(e => !string.IsNullOrEmpty(e.Title)).Select(e => e.Title).ToList();
			ImagStr.AddRange(ImagStr2);
			ImagStr.AddRange(ImagStr3);
			ImagStr.AddRange(ImagStr4);
			ImagStr.AddRange(ImagStr5);
			ImagStr = ImagStr.Where(e => !string.IsNullOrEmpty(e)).GroupBy(e => e).Select(e => e.Key).ToList();
			List<FileImageImportDto> importDtos = new List<FileImageImportDto>();
			var fileProds = db.Prods.Where(e => !e.IsDeleted).Where(e => ProdStr.Contains(e.Title)).ToList();
			foreach (var prod in prods)
			{
				var myProd = fileProds.Where(e => e.Title == prod.Title).FirstOrDefault();
				if (myProd != null)
				{
					List<string?> fileName =
						ImagStr.FindAll(e => e == prod.Image1 || e == prod.Image2 || e == prod.Image3 || e == prod.Image4 || e == prod.Image5);
					for (int i = 0; i < fileName.Count; i++)
					{
						if (!string.IsNullOrEmpty(fileName[i]))
						{
							importDtos.Add(new FileImageImportDto
							{
								SId = myProd.Id,
								Type = FileBindTypeEnum.產品,
								mediaLink = fileName[i] ?? "",
								SerNo = 500
							});
						}
					}
				}
			}
			await fileUploadAppService.uploadImageLink(importDtos);
		}
		private async Task InsertOrUpdateProd(List<ProductImportDto> prods, List<ImportMassageItem> errors)
		{
			List<ProductImportDto> AddProds = prods.FindAll(e => e.Id == 0);
			List<ProductImportDto> Prods = prods.FindAll(e => e.Id != 0);
			await InsetProdSpecTypes(prods);
			await InsetProdSpec(prods);

			if (AddProds.Count != 0) await InsertProds(AddProds, errors);
			if (Prods.Count != 0) await UpdateProds(Prods, errors);
		}
		private async Task InsertProds(List<ProductImportDto> prods, List<ImportMassageItem> errors)
		{
			long userId = await loginUserData.GetUserId();
			List<Prod> news = mapper.Map<List<Prod>>(prods);
			foreach (Prod prod in news)
			{
				var item = prods.Find(p => p.Title == prod.Title);
				if (item != null && item.stocks != null)
				{
					prod.Prod_Stocks = await InsertOrUpdateStore(item);
				}
				prod.CreatorUserId = userId;
			}
			db.AddRange(news);
			await db.SaveChangesAsync();
		}
		private async Task UpdateProds(List<ProductImportDto> prods, List<ImportMassageItem> errors)
		{
			long userId = await loginUserData.GetUserId();
			List<string> titles = prods.Select(e => e.Title ?? "").ToList();
			var items = db.Prods.Where(e => titles.Contains(e.Title));
			foreach (var prod in items)
			{
				ProductImportDto? item = prods.Find(e => e.Title == prod.Title);
				if (item != null)
				{
					mapper.Map(mapper.Map<ProductImportUpateDto>(item), prod);
					if (item.stocks != null)
					{
						prod.Prod_Stocks = await InsertOrUpdateStore(item);
					}
				}
				prod.LastModifierUserId = userId;
				prod.LastModificationTime = DateTime.Now;
			}
			await db.SaveChangesAsync();
		}
		private async Task<List<Prod_Stock>> InsertOrUpdateStore(ProductImportDto item)
		{
			var ProdSpecTitleList = db.Prod_Specs.Where(e => !e.IsDeleted).Select(e => new { e.Id, e.Title }).ToList();

			List<Prod_Stock> stockDto = new List<Prod_Stock>();
			if (item.stocks == null) item.stocks = new List<ProductStockDto>();

			for (int i = 0; i < item.stocks.Count; i++)
			{
				Prod_Stock? myStore = null;
				var stock = item.stocks[i];
				var r1 = ProdSpecTitleList.Find(e => e.Title == stock.S1_Title);
				var r2 = ProdSpecTitleList.Find(e => e.Title == stock.S2_Title);
				if (r1 != null) stock.FK_S1id = r1.Id;
				else stock.FK_S1id = 0;
				if (r2 != null) stock.FK_S2id = r2.Id;
				else stock.FK_S2id = 0;

				if (item.Id != 0)
				{
					myStore = await db.Prod_Stocks
							.Where(e => !e.IsDeleted)
							.Where(e => e.FK_S1id == stock.FK_S1id)
							.Where(e => e.FK_S2id == stock.FK_S2id)
							.Where(e => e.FK_Pid == item.Id)
							.FirstOrDefaultAsync();

					if (myStore != null)
					{
						myStore.Stock = stock.Stock;
						stockDto.Add(myStore);
					}
					else
					{
						myStore = mapper.Map<Prod_Stock>(stock);
						stockDto.Add(myStore);
					}
				}
				else
				{
					myStore = mapper.Map<Prod_Stock>(stock);
					stockDto.Add(myStore);
				}
				myStore.Prod_Prices = mapper.Map<List<Prod_Price>>(stock.Prices);

			}
			return stockDto;
		}
		private async Task InsetProdSpecTypes(List<ProductImportDto> prods)
		{
			if (prods.Count == 0) return;
			long userId = await loginUserData.GetUserId();
			long WebsiteId = await loginUserData.GetWebsiteId();
			var ProdSpecTitleList = db.Prod_Spec_Types
									.Where(e => !e.IsDeleted)
									.Where(e => e.FK_WebsiteId == prods[0].FK_WebsiteId)
									.Select(e => e.Type).ToList();
			List<Prod_Spec_Type> news = new List<Prod_Spec_Type>();
			for (int i = 0; i < prods.Count; i++)
			{
				var items = prods[i];
				if (items.stocks != null)
				{
					var Adds1 = items.stocks.FindAll(e => !ProdSpecTitleList.Contains(e.S1_Name ?? "")).Select(e => e.S1_Name).ToList();
					var Adds2 = items.stocks.FindAll(e => !ProdSpecTitleList.Contains(e.S2_Name ?? "")).Select(e => e.S2_Name).ToList();
					Adds1.AddRange(Adds2);

					var allAdds = Adds1.GroupBy(o => o ?? "").Select(o => o.Key).ToList();
					var nowTitle = news.Select(e => e.Type);
					var Adds = allAdds.FindAll(e => !nowTitle.Contains(e));

					for (int j = 0; j < Adds.Count; j++)
					{
						var item = Adds[j];
						if (!string.IsNullOrEmpty(item))
						{
							news.Add(new Prod_Spec_Type
							{
								Type = item,
								FK_WebsiteId = items.FK_WebsiteId ?? 0,
								CreationTime = DateTime.Now,
								CreatorUserId = userId
							});
						}
					}
				}
			}
			if (news.Count == 0 && ProdSpecTitleList.Count == 0)
			{
				news.Add(new Prod_Spec_Type
				{
					Type = "規格",
					FK_WebsiteId = WebsiteId,
					CreationTime = DateTime.Now,
					CreatorUserId = userId
				});
			}
			db.Prod_Spec_Types.AddRange(news);
			await db.SaveChangesAsync();
		}
		private async Task InsetProdSpec(List<ProductImportDto> prods)
		{
			long userId = await loginUserData.GetUserId();
			var ProdSpecTitleList = db.Prod_Specs
									.Where(e => !e.IsDeleted)
									.Select(e => e.Title).ToList();
			List<Prod_Spec> news = new List<Prod_Spec>();
			for (int i = 0; i < prods.Count; i++)
			{
				var items = prods[i];
				if (items.stocks != null)
				{
					var Adds1 = items.stocks.FindAll(e => !ProdSpecTitleList.Contains(e.S1_Title ?? "")).Select(e => new { name = e.S1_Name, title = e.S1_Title }).ToList();
					var Adds2 = items.stocks.FindAll(e => !ProdSpecTitleList.Contains(e.S2_Title ?? "")).Select(e => new { name = e.S2_Name, title = e.S2_Title }).ToList();
					Adds1.AddRange(Adds2);

					var allAdds = Adds1
								.Where(e => e.title != "")
								.Where(e => e.title != null)
								.GroupBy(o => o.title).Select(o => o.Key).ToList();

					var nowTitle = news.Select(e => e.Title);
					var Adds = allAdds.FindAll(e => !nowTitle.Contains(e));

					var types = db.Prod_Spec_Types.Where(e => !e.IsDeleted).Select(e => new { e.Id, e.Type }).ToList();
					var specs = db.Prod_Specs.Where(e => !e.IsDeleted).Select(e => new { e.FK_Tid, e.Title }).ToList();

					for (int j = 0; j < Adds.Count; j++)
					{
						var item = Adds[j];
						if (!string.IsNullOrEmpty(item))
						{
							var Spec = Adds1.Find(e => e.title == item);
							string SpecTypeName = "";
							if (Spec != null) SpecTypeName = Spec.name ?? "";
							var SpecType = types.Find(o => o.Type == SpecTypeName);
							long SpecTypeId = types.Count == 0 ? 0 : types.FirstOrDefault().Id;
							if (SpecType == null)
							{
								var c = specs.Find(e => e.Title == item);
								if (c != null) SpecTypeId = c.FK_Tid;
							}
							else SpecTypeId = SpecType.Id;
							if (SpecTypeId != 0)
							{
								news.Add(new Prod_Spec
								{
									Title = item,
									FK_Tid = SpecTypeId,
									CreationTime = DateTime.Now,
									CreatorUserId = userId
								});
							}

						}
					}
				}
			}
			db.Prod_Specs.AddRange(news);
			await db.SaveChangesAsync();
		}


	}
}
