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
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;

namespace EtheriT.Coker.Application.TechnicalCertificate
{
	public class TechnicalCertificateAppService : ITechnicalCertificateAppService
	{
		private readonly CokerDbContext db;
		private readonly LoginUserData loginUserData;
        private readonly StringHandler stringHandler;
        private readonly IFileUploadAppService fileUploadAppService;
		private readonly IConfiguration configuration;
		private readonly IMapper mapper;
		private readonly string ServiceName;

        public TechnicalCertificateAppService(
			CokerDbContext db,
			LoginUserData loginUserData,
            StringHandler stringHandler,
            IFileUploadAppService fileUploadAppService,
			IConfiguration configuration,
			IMapper mapper
		)
		{
			this.db = db;
			this.loginUserData = loginUserData;
			this.stringHandler = stringHandler;
			this.fileUploadAppService = fileUploadAppService;
			this.configuration = configuration;
			this.mapper = mapper;
			ServiceName = "TechnicalCertificate";

        }
		public async Task<ResponseMessageDto> AddUp(TechCertDto dto)
		{
			ResponseMessageDto output = new ResponseMessageDto() { Success = false };
			try
			{
				var asoid = dto.Id;
				Core.Models.TechnicalCertificate? tc;

                if (dto.Id == 0)
				{
					long WebsiteID = await loginUserData.GetWebsiteId();
					tc = new Core.Models.TechnicalCertificate
					{
						FK_WebsiteId = WebsiteID,
						Disp_opt = dto.Disp_opt,
						Img = dto.Img,
						Title = dto.Title,
						Description = dto.Description,
						Ser_no = dto.Ser_no,
						StartDate = dto.StartDate,
						EndDate = dto.EndDate,
						Permanent = dto.Permanent
					};
					db.TechnicalCertificates.Add(tc);
				}
				else
				{
					tc = db.TechnicalCertificates.Where(e => e.Id == dto.Id).FirstOrDefault();
					if (tc != null)
					{
                        tc.Disp_opt = dto.Disp_opt;
						tc.Img = dto.Img;
						tc.Title = dto.Title;
						tc.Description = dto.Description;
						tc.Ser_no = dto.Ser_no;
						tc.StartDate = dto.StartDate;
						tc.EndDate = dto.EndDate;
                        tc.Permanent = dto.Permanent;
					}
					else throw new Exception("查無資料");
                }
				await loginUserData.SaveChanges(tc);
                asoid = tc.Id;
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
								Html = "",
								Css = ""
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
							Prod_TechCert ptc = new Prod_TechCert
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
		public async Task<GetTechnicalCertificateContenDto> GetConten(SearchIDDto dto) {
            GetTechnicalCertificateContenDto results = new GetTechnicalCertificateContenDto();
            try
            {
                long siteId = await loginUserData.GetWebsiteId();
                var tecCer = await db.TechnicalCertificates.Where(e => e.FK_WebsiteId == siteId)
                                    .Where(e => e.Id == dto.Id)
                                    .Where(e => !e.IsDeleted)
                                    .FirstOrDefaultAsync();
                if (tecCer != null)
                {
                    results.Title = tecCer.Title;
                    results.Conten = new TechnicalCertificateSaveContenDto
                    {
                        SaveHtml = tecCer.Html,
                        SaveCss = tecCer.Css
                    };
                    results.Conten.SaveHtml = stringHandler.HtmlEncode(results.Conten.SaveHtml);
                    results.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                results.Success = false;
                results.Error = ex.Message;
            }
            return results;
        }
		public async Task<ResponseMessageDto> SaveConten(TechnicalCertificateSaveContenDto dto) {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                dto.SaveHtml = stringHandler.HtmlEncode(dto.SaveHtml);
                var tecCer = await db.TechnicalCertificates.FirstOrDefaultAsync(e => e.Id == dto.Id);

                if (tecCer != null)
                {
                    tecCer.Html = dto.SaveHtml;
                    tecCer.Css = dto.SaveCss;
                    await loginUserData.SaveChanges(tecCer);
                    response.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            finally
            {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
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
        public async Task<GetFrontContenOutputDto> GetFrontConten(TechCertGetFrontContenInputDto dto)
        {
            if (dto.siteId == null)
            {
                dto.siteId = configuration.GetValue<long>("WebConfig:SiteId");
            }
            GetFrontContenOutputDto result = new GetFrontContenOutputDto();
            try
            {
                var side = await db.Websites.Where(e => e.Id == dto.siteId).FirstOrDefaultAsync();
                var TechCert = await db.TechnicalCertificates.Where(e => e.Id == dto.TechCertId).Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == dto.siteId).FirstOrDefaultAsync();
                if (side != null)
                {
                    result.SiteName = side.Title;
                    if (TechCert != null)
                    {
						var img = await fileUploadAppService.getImgFiles(new FileGetImgInputDto { 
							Sid = dto.TechCertId,
							Size =1,
							Type = (int) FileBindTypeEnum.技術證照
						});
                        result.Id = (int)TechCert.Id;
                        result.Title = TechCert.Title;
                        result.Description = TechCert.Description;
                        result.Html = TechCert.Html;
                        result.Css = TechCert.Css;
						result.Html.Replace($"upload/{side.OrgName}/", "upload/");
                        result.Html = $@"{result.Html}
							<div class='container'>
								{(img.Count > 0? $@"
								<div class=""row imageTitle"">
									<img src=""{img[0].Link}"" alt="" "" />
									{result.Description}
								</div>
								":"")}
								<div class=""catalog_frame type_change_frame mt-3"" data-dirid=""{result.Id}"" data-type=""TechCert"" data-ShowNum=""24"" data-search-text="""">
									<div class=""d-flex justify-content-end switch_control text-black-50"">
										<div class=""justify-content-center align-items-center d-none"">
											<p>
												發布日期
											</p>
											<input type=""date"" name=""startDate"" class=""text-black-50"" />
											<p class=""px-2 fs-3""> ~ </p>
											<input type=""date"" name=""endDate"" class=""text-black-50"" />
										</div>
										<button class=""btn_prod_grid d-flex bg-transparent border-0 align-items-center mx-1"">
											<span class=""material-symbols-outlined fs-5 me-1"">grid_on</span>圖片
										</button>
										<button class=""btn_prod_list d-flex bg-transparent border-0 align-items-center mx-1 text-black-50"">
											<span class=""material-symbols-outlined fs-5 me-1"">view_list</span>圖文
										</button>
										<button class=""btn_text d-flex bg-transparent border-0 align-items-center mx-1 text-black-50"">
											<span class=""material-symbols-outlined fs-5 me-1"">list</span>文字
										</button>
									</div>
									<div class=""catalog content gx-0 rounded-lg type4 row row-cols-lg-4 row-cols-md-2 bg-light px-2"">
										<div class=""templatecontent d-none"">
											<div class=""template p-2 py-2"">
												<div class=""col bg-white p-2 position-sticky p-1 type4 rounded-lg h-100"">
													<a href="""" title="""" target=""_self"" class=""text-black"">
														<figure class=""d-flex justify-content-center mb-0 h-100 max-h flex-column"">
															<div class=""image_frame d-flex flex-grow-1 justify-content-center align-items-center type4-image-frame w-100"">
																<img src=""/upload/Product/Photo/C656NA.jpg"" alt="""" class=""image gjs-plh-image img-fluid"" />
															</div>
															<figcaption class=""w-100 position-relative pb1 type4-caption d-flex flex-column justify-content-center"">
																<div class=""item-header d-flex"">
																	<div class=""itemNo m-0 p-0 align-itmes-center type4-title d-inline fw-bold"">
																		{{{{產品編號}}}}
																	</div>
																</div>
																<div class=""item-title"">
																	<div class=""catalog-number itemNo m-0 p-2 align-itmes-center type4-title d-inline"">
																		{{{{產品編號}}}}
																	</div>
																	<div class=""title m-0 fs-5 align-itmes-center type4-title d-inline fs-6"">
																	</div>
																	<div class=""like-and-share top_line d-none"">
																		<div class=""btn_favorites bg-transparent border-0 d-none"">
																			<i class=""fs-5 fa-regular fa-heart"">
																			</i><span class=""d-none"">關注</span>
																		</div>
																	</div>
																</div>
																<p class=""description mt-1 overflow-hidden p-2 d-none type2-content"">
																</p>
																<div class=""more-btn position-absolute d-flex justify-content-center align-items-center d-none"">
																	<div class=""fas fa-angle-right"">
																	</div>
																</div>
																<div class=""date d-flex justify-content-end align-items-center p-2 d-none"">
																</div>
																<div class=""more text-end mt-1 d-none"">
																	詳細介紹
																</div>
																<div class=""price price-grid mt-auto type2-title d-inline fw-bold fs-7"">{{{{price}}}}</div>
																<div class=""bottom-row d-flex align-text-bottom"">
																	<div class=""tags""></div>
																	<div class=""purchase d-none"">
																		<div class=""price price-discount me-auto p-2 align-itmes-center type2-title d-none"">
																			{{{{min price}}}}
																		</div>
																		<div class=""price normal-price me-auto p-2 align-itmes-center type2-title d-inline fw-bold"">
																			{{{{max price}}}}
																		</div>
																		<span class=""cart badge rounded-pill bg-secondary text-white me-auto p-2 px-4 align-itmes-center type2-title d-none fw-normal"">
																			<i class=""fa-solid fa-cart-shopping fa-inverse""></i>
																			放入購物車
																		</span>
																	</div>
																</div>
															</figcaption>
														</figure>
													</a>
													<div class=""shareBlock"">
														<button class=""btn_share bg-transparent border-0"">
															<i class=""fs-5 fa-solid fa-share"">
															</i><span class=""d-none"">分享</span>
														</button>
													</div>
												</div>
											</div>
										</div>
										<div class=""templatecontent-tag d-none"">
											<span class=""badge rounded-pill bg-light text-secondary fw-normal me-1 px-2"">{{{{Tag Name}}}}</span>
										</div>
									</div>
									<nav draggable=""true"" aria-label=""Page"">
										<ul draggable=""true"" class=""page_btn d-flex justify-content-center my-5 pagination"">
											<li draggable=""true"" class=""page-item btn_prev"">
												<button draggable=""true"" class=""page-link text-black"">
													<i class=""fa-solid fa-angle-left""></i>
												</button>
											</li>
											<li draggable=""true"" class=""page-item btn_next"">
												<button draggable=""true"" class=""page-link text-black"">
													<i class=""fa-solid fa-angle-right""></i>
												</button>
											</li>
										</ul>
									</nav>
								</div>
							</div>
						";
                        result.Html = result.Html == null ? "" : result.Html.Replace("&lt;body&gt;", "").Replace("&lt;/body&gt;", "");
                        result.Html = stringHandler.HtmlEncode(stringHandler.HtmlDecode(result.Html));
                    }
                }
            }
            catch (Exception e)
            {
            }
            return result;
        }
    }
}
