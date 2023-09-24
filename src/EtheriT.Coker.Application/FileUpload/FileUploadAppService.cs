using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application
{
	public class FileUploadAppService : IFileUploadAppService
	{
		private readonly CokerDbContext db;
		public readonly FileAllow fileAllow;
		private readonly LoginUserData loginUserData;
		private readonly string _folder;
		private readonly string AppName;
		private readonly IConfiguration configuration;
		public FileUploadAppService(
			IOptions<VirtualDirectory> fileAllow,
			LoginUserData loginUserData,
			CokerDbContext db,
			IConfiguration configuration
		)
		{
			this.fileAllow = fileAllow.Value.FileAllow;
			this.db = db;
			this.loginUserData = loginUserData;
			_folder = fileAllow.Value.upload;
			AppName = "FileUpload";
			this.configuration = configuration;
		}
		public async Task<UploadFileOutputDto> uploadTempFiles(IList<IFormFile> files)
		{
			UploadFileOutputDto response = await uploadFiles(files, "temp", isTemp: true);
			return response;
		}
		public async Task<UploadFileOutputDto> uploadHtmlContentFiles(IList<IFormFile> files)
		{
			UploadFileOutputDto response = await uploadFiles(files, "htmlConten");
			await loginUserData.SetLogs(AppName, "uploadHtmlFiles", "FileBinary...", JsonConvert.SerializeObject(response));
			return response;
		}
		private async Task<UploadFileOutputDto> uploadFiles(IList<IFormFile> files, string type, bool isTemp = false)
		{
			UploadFileOutputDto response = new UploadFileOutputDto
			{
				Files = new List<FileItemDto>(),
				ErrorFiles = new List<string>()
			};
			try
			{
				foreach (var file in files)
				{
					FileItemDto item = await SaveFile(file, type, isTemp);
					response.Files.Add(item);
				}
				response.Success = true;
			}
			catch (Exception ex)
			{
				response.ErrorFiles.Add(ex.Message);
			}
			return response;
		}
		public async Task<UploadFileOutputDto> uploadMediaFiles(IList<IFormFile> files, int type, long sid, int serno, string page)
		{
			UploadFileOutputDto response = new UploadFileOutputDto
			{
				Files = new List<FileItemDto>(),
				ErrorFiles = new List<string>()
			};
			try
			{
				List<FileItemDto> items = await SaveImage(files, type, (int)FileBindMoreEnum.壓縮圖片, serno, page, sid);
				foreach (var item in items)
				{
					response.Files.Add(item);
				}
				response.Success = true;

				var websiteid = await loginUserData.GetWebsiteId();
				switch (type)
				{
					case (int)FileBindTypeEnum.選單圖:
						var db_bind = await db.WebMenus.Where(e => e.Id == sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
						if (db_bind != null) db_bind.ImgId = response.Files[0].Id;
						db.SaveChanges();
						break;
					case (int)FileBindTypeEnum.選單覆蓋:
						var db_bind_over = await db.WebMenus.Where(e => e.Id == sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
						if (db_bind_over != null) db_bind_over.OverImgId = response.Files[0].Id;
						db.SaveChanges();
						break;
					case (int)FileBindTypeEnum.選單Icon:
						var db_bind_icon = await db.WebMenus.Where(e => e.Id == sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
						if (db_bind_icon != null) db_bind_icon.icon = $"IconId:{response.Files[0].Id}";
						db.SaveChanges();
						break;
				}
				return response;
			}
			catch (Exception ex)
			{
				response.ErrorFiles.Add(ex.Message);
				return response;
			}
		}
		public async Task<UploadFileOutputDto> upload360Files(IList<IFormFile> files, int type, long? sid, string page)
		{
			UploadFileOutputDto response = new UploadFileOutputDto
			{
				Files = new List<FileItemDto>(),
				ErrorFiles = new List<string>()
			};
			return null;
		}
		public async Task<ResponseMessageDto> uploadYTLink(FileYTLinkUploadDto dto)
		{
			ResponseMessageDto response = new ResponseMessageDto() { Success = true };

			try
			{
				long userId = await loginUserData.GetUserId();

				if (dto.Id == 0)
				{
					long WebsiteID = await loginUserData.GetWebsiteId();
					Guid key = Guid.NewGuid();

					Core.Models.FileUpload fu = new Core.Models.FileUpload
					{
						FK_WebsiteId = WebsiteID,
						GuidKey = Guid.NewGuid(),
						ContentType = "youtube",
						OriginalFileName = dto.File,
						DownloadFileName = $"https://www.youtube.com/watch?v={dto.File}",
						Size = 0,
						FileGuid = key,
						CreatorUserId = userId,
						CreationTime = DateTime.Now,
					};
					db.FileUploads.Add(fu);
					db.SaveChanges();

					Core.Models.FileBind fb = new Core.Models.FileBind
					{
						Guid = Guid.NewGuid(),
						Name = fu.OriginalFileName,
						Sid = dto.SId,
						type = dto.Type,
						num = 1,
						SerNo = dto.SerNo,
						MediaLink = fu.DownloadFileName,
						FK_FileUploadId = fu.Id,
						CreatorUserId = userId,
						CreationTime = DateTime.Now,
					};
					db.FileBinds.Add(fb);
					db.SaveChanges();
				}
				else
				{
					var db_fu = db.FileUploads.Where(e => e.Id == dto.Id).FirstOrDefault();
					if (db_fu != null)
					{
						db_fu.OriginalFileName = dto.File;
						db_fu.DownloadFileName = $"https://www.youtube.com/watch?v={dto.File}";
						db_fu.LastModificationTime = DateTime.Now;
						db_fu.LastModifierUserId = userId;

						var db_fb = db.FileBinds.Where(e => e.FK_FileUploadId == dto.Id).FirstOrDefault();
						if (db_fb != null)
						{

							db_fb.SerNo = dto.SerNo;
							db_fb.Name = db_fu.OriginalFileName;
							db_fb.MediaLink = db_fu.DownloadFileName;
							db_fb.LastModifierUserId = userId;
							db_fb.LastModificationTime = DateTime.Now;
						}
						else
						{
							response.Success = false;
						}
					}
					else
					{
						response.Success = false;
					}
				}
				db.SaveChanges();

			}
			catch (Exception e)
			{
				response.Success = false;
				response.Error = e.Message;
			}

			return response;
		}
		public async Task<ResponseMessageDto> uploadImageLink(List<FileImageImportDto> dto)
		{
			ResponseMessageDto response = new ResponseMessageDto() { Success = true };
			if (dto.Count == 0) return response;

			long WebsiteID = await loginUserData.GetWebsiteId();
			long userId = await loginUserData.GetUserId();
			List<FileUpload> files = await AddFileUploads(dto);
			db.FileUploads.AddRange(files);
			await db.SaveChangesAsync();

			List<FileBind> FileBinds = await AddFileBinds(dto);
			db.FileBinds.AddRange(FileBinds);
			await db.SaveChangesAsync();
			return response;
		}
		private async Task<List<FileUpload>> AddFileUploads(List<FileImageImportDto> dto) {
			long WebsiteID = await loginUserData.GetWebsiteId();
			string OrgName = await loginUserData.GetWebsiteOrgName();
			long userId = await loginUserData.GetUserId();
			List<FileUpload> files = new List<FileUpload>();
			List<string> fileNames = dto.Where(e => string.IsNullOrEmpty(e.mediaLink)).Select(e => e.mediaLink).ToList();
			List<FileUpload> nowFiles = db.FileUploads
										.Where(e => !e.IsDeleted)
										.Where(e => e.DownloadFileName!=null)
										.Where(e => fileNames.Contains(e.DownloadFileName))
										.Where(e => e.FK_WebsiteId == WebsiteID)
										.ToList();
			foreach (var file in dto)
			{
				try
				{
					string dir = "";
					switch (file.Type)
					{
						case FileBindTypeEnum.產品:
							dir = "Product";
							break;
						case FileBindTypeEnum.技術證照:
							dir = "TechnicalCertificate";
							break;
						default:
							dir = "Orthers";
							break;
					}
					if (!(new Regex("^http")).IsMatch(file.mediaLink)) file.mediaLink = $"/upload/{dir}/{file.mediaLink}";

					var myFile = nowFiles.Find(e => e.DownloadFileName == file.mediaLink);
					var hasFile = files.Find(e => e.DownloadFileName == file.mediaLink);
					if (myFile == null && hasFile == null) {
						
						
						Guid key = Guid.NewGuid();
						FileUpload fu = new FileUpload
						{
							FK_WebsiteId = WebsiteID,
							GuidKey = Guid.NewGuid(),
							ContentType = "image/",
							OriginalFileName = file.mediaLink,
							DownloadFileName = file.mediaLink,
							Size = 0,
							FileGuid = key,
							CreatorUserId = userId
						};
						files.Add(fu);
					}
				}
				catch (Exception e)
				{

				}
			}
			return files;
		}
		private async Task<List<FileBind>> AddFileBinds(List<FileImageImportDto> dto) {
			long WebsiteID = await loginUserData.GetWebsiteId();
			long userId = await loginUserData.GetUserId();
			var filesName = dto.Select(o => o.mediaLink).ToList();
			var allFile = db.FileUploads
							.Where(e => e.FK_WebsiteId == WebsiteID)
							.Where(f => filesName.Contains(f.DownloadFileName)).ToList();
			List<FileBind> FileBinds = new List<FileBind>();
			foreach (var file in dto)
			{
				long Id = allFile.Find(d => d.DownloadFileName == file.mediaLink).Id;
				var myFileBind = db.FileBinds
						.Where(e => e.FK_FileUploadId == Id)
						.Where(e => e.Sid == file.SId)
						.Where(e => e.type == (int)file.Type)
						.FirstOrDefault();
				var hasBind = FileBinds.Find(e => e.FK_FileUploadId == Id && e.Sid == file.SId && e.type == (int)file.Type);
				if (myFileBind == null && hasBind==null)
				{
					FileBind fb = new FileBind
					{
						Guid = Guid.NewGuid(),
						Name = file.mediaLink,
						Sid = file.SId,
						type = (int)file.Type,
						num = 1,
						SerNo = file.SerNo,
						MediaLink = file.mediaLink,
						FK_FileUploadId = Id,
						CreatorUserId = userId,
						CreationTime = DateTime.Now,
					};
					FileBinds.Add(fb);
				}
				else if(myFileBind != null)
				{
					myFileBind.SerNo = file.SerNo;
					myFileBind.Name = file.mediaLink;
					myFileBind.FK_FileUploadId = Id;
					myFileBind.MediaLink = file.mediaLink;
					myFileBind.LastModifierUserId = userId;
					myFileBind.LastModificationTime = DateTime.Now;
				}
			}
			return FileBinds;
		}
		public async Task<ResponseMessageDto> uploadImageLink(FileImageImportDto dto)
		{
			ResponseMessageDto response = new ResponseMessageDto() { Success = true };

			try
			{
				long WebsiteID = await loginUserData.GetWebsiteId();
				long userId = await loginUserData.GetUserId();
				var myFile = await db.FileUploads.Where(e => e.DownloadFileName == dto.mediaLink).FirstOrDefaultAsync();
				if (myFile != null) dto.Id = myFile.Id;
				else
				{
					Guid key = Guid.NewGuid();
					FileUpload fu = new FileUpload
					{
						FK_WebsiteId = WebsiteID,
						GuidKey = Guid.NewGuid(),
						ContentType = "image/",
						OriginalFileName = dto.mediaLink,
						DownloadFileName = dto.mediaLink,
						Size = 0,
						FileGuid = key,
						CreatorUserId = userId
					};
					db.FileUploads.Add(fu);
					db.SaveChanges();
					dto.Id = fu.Id;
				}
				var myFileBind = db.FileBinds
						.Where(e => e.FK_FileUploadId == dto.Id)
						.Where(e => e.Sid == dto.SId)
						.Where(e => e.type == (int)dto.Type)
						.FirstOrDefault();
				if (myFileBind == null)
				{
					FileBind fb = new FileBind
					{
						Guid = Guid.NewGuid(),
						Name = dto.mediaLink,
						Sid = dto.SId,
						type = (int)dto.Type,
						num = 1,
						SerNo = dto.SerNo,
						MediaLink = dto.mediaLink,
						FK_FileUploadId = dto.Id,
						CreatorUserId = userId,
						CreationTime = DateTime.Now,
					};
					db.FileBinds.Add(fb);
				}
				else
				{
					myFileBind.SerNo = dto.SerNo;
					myFileBind.Name = dto.mediaLink;
					myFileBind.FK_FileUploadId = dto.Id;
					myFileBind.MediaLink = dto.mediaLink;
					myFileBind.LastModifierUserId = userId;
					myFileBind.LastModificationTime = DateTime.Now;
				}
				db.SaveChanges();
			}
			catch (Exception e)
			{
				response.Success = false;
				response.Error = e.Message;
			}

			return response;
		}
		public async Task<UploadFileOutputDto> getHtmlContentFiles()
		{
			UploadFileOutputDto response = new UploadFileOutputDto
			{
				Files = new List<FileItemDto>()
			};
			try
			{
				long websiteId = await loginUserData.GetWebsiteId();
				string orgName = await loginUserData.GetWebsiteOrgName();
				var files = db.FileUploads
							.Where(e => e.FK_WebsiteId == websiteId)
							.Where(e => !e.IsDeleted)
							.Where(e => (e.DownloadFileName ?? "").Contains("htmlConten"));
				var result = from file in files
							 select new FileItemDto
							 {
								 Guid = file.GuidKey,
								 Name = file.OriginalFileName,
								 Path = (file.DownloadFileName ?? "").Replace(@"\", "/").Replace("/upload/", $"/upload/{orgName}/"),
							 };
				response.Files = await result.ToListAsync();
				response.Success = true;
			}
			catch (Exception ex)
			{
				response.Error = ex.Message;
			}
			return response;
		}
		// size = 1 原圖 2中縮圖 3小縮圖
		public async Task<List<FileGetImgDto>> getImgFiles(FileGetImgInputDto dto)
		{
			var result = new List<FileGetImgDto>();
			try
			{
				long websiteId = await loginUserData.GetWebsiteId();
				string orgName = await loginUserData.GetWebsiteOrgName();

				var faids = await (db.FileBinds.Where(e => e.Sid == dto.Sid && e.type == dto.Type).Where(e => !e.IsDeleted).Select(e => e.FK_FileUploadId)).ToListAsync();

				if (faids != null)
				{
					if (dto.Size == 1)
					{
						foreach (var faid in faids)
						{
							var fadata = await (db.FileUploads.Where(e => e.Id == faid)).FirstOrDefaultAsync();

							if (fadata.GuidKey != Guid.Empty)
							{
								orgName = orgName == "" ? "" : $"/{orgName}";
								result.Add(new FileGetImgDto
								{
									Id = fadata.Id,
									Name = fadata.OriginalFileName,
									Link = fadata.DownloadFileName.Replace("upload", $"upload{orgName}")
								});
							}
						}
					}
					else
					{
						foreach (var faid in faids)
						{
							var fadata = await (db.FileUploads.Where(e => e.Id == faid)).FirstOrDefaultAsync();

							if (fadata.GuidKey != Guid.Empty)
							{
								var chimg_ids = await (db.FileBindMores.Where(e => e.FK_FileBindGuid == fadata.GuidKey).Where(e => !e.IsDeleted).Select(e => e.FK_FileUploadId).ToListAsync());
								if (chimg_ids.Count > 0)
								{
									var chimg = new List<FileUpload>();
									foreach (var chimg_id in chimg_ids)
									{
										chimg.Add(await (db.FileUploads.Where(e => e.Id == chimg_id).Where(e => !e.IsDeleted).FirstOrDefaultAsync()));

									}
									chimg = chimg.OrderByDescending(e => e.Size).ToList();
									orgName = orgName == "" ? "" : $"/{orgName}";
									if (chimg_ids.Count == 2)
									{
										result.Add(new FileGetImgDto
										{
											Id = faid.Value,
											Name = chimg[dto.Size - 2].OriginalFileName,
											Link = chimg[dto.Size - 2].DownloadFileName.Replace("upload", $"upload{orgName}")
										});
									}
									else if (chimg_ids.Count == 1)
									{
										result.Add(new FileGetImgDto
										{
											Id = faid.Value,
											Name = chimg[0].OriginalFileName,
											Link = chimg[0].DownloadFileName.Replace("upload", $"upload{orgName}")
										});
									}
								}
								else
								{
									orgName = orgName == "" ? "" : $"/{orgName}";
									result.Add(new FileGetImgDto
									{
										Id = fadata.Id,
										Name = fadata.OriginalFileName,
										Link = fadata.DownloadFileName.Replace("upload", $"upload{orgName}")
									});
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{

			}
			return result;
		}
		// size = 1 原圖 2中縮圖 3小縮圖
		public async Task<List<string>> getImgFilesById(List<long> Ids, int size)
		{
			var result = new List<string>();
			try
			{
				long websiteId = await loginUserData.GetWebsiteId();
				if (websiteId == 0)
				{
					websiteId = configuration.GetValue<long>("WebConfig:SiteId");
				}

				if (size == 1)
				{
					foreach (var id in Ids)
					{
						var file = await db.FileUploads.Where(e => e.Id == id && !e.IsDeleted && e.FK_WebsiteId == websiteId).FirstOrDefaultAsync();
						if (file != null)
						{
							result.Add(file.DownloadFileName == null ? "" : file.DownloadFileName);
						}
					}
				}
				else
				{
					foreach (var id in Ids)
					{
						var fa_file = await db.FileUploads.Where(e => e.Id == id && !e.IsDeleted && e.FK_WebsiteId == websiteId).FirstOrDefaultAsync();

						if (fa_file != null)
						{
							var files = await db.FileBindMores.Where(e => e.FK_FileBindGuid == fa_file.GuidKey && !e.IsDeleted).ToListAsync();

							if (files.Count > 0)
							{
								var temp_files = new List<FileGetImgDto>();
								foreach (var file in files)
								{
									var fu = await db.FileUploads.Where(e => e.Id == file.FK_FileUploadId && !e.IsDeleted && e.FK_WebsiteId == websiteId).FirstOrDefaultAsync();
									if (fu != null)
									{
										temp_files.Add(new FileGetImgDto()
										{
											Id = fu.Id,
											Name = fu.OriginalFileName,
											Link = fu.DownloadFileName,
											Size = fu.Size,
										});
									}
								}

								temp_files.Sort((x, y) => x.Size.CompareTo(y.Size));
								temp_files.Reverse();

								if (files.Count == 2)
								{
									result.Add(temp_files[size - 2].Link);
								}
								else
								{
									result.Add(temp_files[0].Link);
								}
							}
							else
							{
								result.Add(fa_file.DownloadFileName == null ? "" : fa_file.DownloadFileName);
							}
						}
					}

				}
			}
			catch (Exception ex)
			{

			}
			return result;
		}
		public async Task<List<FileGetProdDisplayDto>> getProdDisplayFiles(long Pid, int size)
		{
			var output = new List<FileGetProdDisplayDto>();
			string orgName = await loginUserData.GetWebsiteOrgName();

			try
			{
				var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
				if (websiteId == 0)
				{
					websiteId = await loginUserData.GetWebsiteId();
				}

				var fbs = await (db.FileBinds.Where(e => e.Sid == Pid && e.type == (int)FileBindTypeEnum.產品).Where(e => !e.IsDeleted)).ToListAsync();

				if (fbs != null)
				{
					foreach (var fb in fbs)
					{
						if (fb.MediaLink != "")
						{
							var file_type = 0;
							var fu = await (db.FileUploads.Where(e => e.Id == fb.FK_FileUploadId)).FirstOrDefaultAsync();

							if (Regex.IsMatch(fb.MediaLink, @"^https://www.youtube.com/[\w\W]*", RegexOptions.IgnoreCase))
							{
								file_type = 4;
							}
							else
							{
								if (fu != null)
								{
									if (Regex.IsMatch(fu.ContentType, @"^image/[\w\W]*", RegexOptions.IgnoreCase))
									{
										file_type = fb.num == 1 ? 1 : 2;
									}
									else if (Regex.IsMatch(fu.ContentType, @"^video/[\w\W]*", RegexOptions.IgnoreCase))
									{
										file_type = 3;
									}
								}
							}

							output.Add(new FileGetProdDisplayDto
							{
								Id = fu.Id,
								Name = fb.Name,
								FileType = file_type,
								Link = new List<string> { fb.MediaLink },
								SerNo = fb.SerNo,
							});
						}
						else
						{
							var fu = await (db.FileUploads.Where(e => e.Id == fb.FK_FileUploadId)).FirstOrDefaultAsync();
							if (fu != null)
							{
								if (Regex.IsMatch(fu.ContentType, @"^image/[\w\W]*", RegexOptions.IgnoreCase))
								{
									if (fb.num == 1)
									{
										var ids = new List<long> { fu.Id };
										var file_link = await this.getImgFilesById(ids, size);

										output.Add(new FileGetProdDisplayDto
										{
											Id = fu.Id,
											Name = fb.Name,
											FileType = 1,
											Link = file_link,
											SerNo = fb.SerNo,
										});
									}
									else
									{
										//360抓圖
									}
								}
								else if (Regex.IsMatch(fu.ContentType, @"^video/[\w\W]*", RegexOptions.IgnoreCase))
								{
									output.Add(new FileGetProdDisplayDto
									{
										Id = fu.Id,
										Name = fb.Name,
										FileType = 3,
										Link = new List<string> { fu.DownloadFileName == null ? "" : fu.DownloadFileName },
										SerNo = fb.SerNo,
									});
								}
							}
						}
					}
				}

				output.Sort((x, y) => (x.SerNo.CompareTo(y.SerNo) * 2 + x.Id.CompareTo(y.Id)));
				for (var i = 0; i < output.Count; i++)
				{
					for (var j = 0; j < output[i].Link.Count; j++)
					{
						if (orgName != "")
						{
							output[i].Link[j] = output[i].Link[j].Replace("upload", $"upload/{orgName}");
						}
					}
				}

				return output;
			}
			catch (Exception e)
			{
				return output;
			}
		}
		public async Task<string> getImgUrl(long? imgid, long websiteid)
		{
			try
			{
				var files = await (db.FileUploads
							.Where(e => e.FK_WebsiteId == websiteid)
							.Where(e => e.Id == imgid)
							.Where(e => !e.IsDeleted)
							.Select(e => e.DownloadFileName).FirstOrDefaultAsync());
				return files;
			}
			catch (Exception ex)
			{
				return "";
			}
		}
		public async Task<ResponseMessageDto> fileSortChange(FileChangeSortDto dto)
		{
			ResponseMessageDto response = new ResponseMessageDto();
			try
			{
				var userid = await loginUserData.GetUserId();
				var db_fb = await db.FileBinds.Where(e => e.FK_FileUploadId == dto.id && !e.IsDeleted && e.type == (int)FileBindTypeEnum.產品).FirstOrDefaultAsync();
				if (db_fb != null)
				{
					db_fb.SerNo = dto.SerNo;
					db_fb.LastModifierUserId = userid;
					db_fb.LastModificationTime = DateTime.Now;
				}
				db.SaveChanges();
				response.Success = true;
			}
			catch (Exception ex)
			{
				response.Error = ex.Message;
			}
			return response;

		}
		public async Task<ResponseMessageDto> deleteFile(string path)
		{
			ResponseMessageDto response = new ResponseMessageDto();
			try
			{
				if (File.Exists(path))
				{
					File.Delete($"{path}");
				}
				response.Success = true;
			}
			catch (Exception ex)
			{
				response.Error = ex.Message;
			}
			await loginUserData.SetLogs(AppName, "deleteFile", path, JsonConvert.SerializeObject(response));
			return response;
		}
		public async Task<ResponseMessageDto> deleteFile(Guid key)
		{
			ResponseMessageDto response = new ResponseMessageDto();
			try
			{
				string orgName = await loginUserData.GetWebsiteOrgName();
				long websiteId = await loginUserData.GetWebsiteId();
				string s = "";
				var rootPath = $"{_folder}/{orgName}";
				var files = await db.FileUploads.Where(e => e.GuidKey == key).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
				if (files != null)
				{
					s = files.DownloadFileName.Replace(@"\", "/").Replace("/upload", "");
					File.Delete($"{rootPath}{s}");
					files.IsDeleted = true;
					response.Success = true;
					await loginUserData.SaveChanges(files);
				}
				else throw new Exception("檔案不存在");
			}
			catch (Exception ex)
			{
				response.Error = ex.Message;
			}
			await loginUserData.SetLogs(AppName, "deleteFile", key.ToString(), JsonConvert.SerializeObject(response));
			return response;
		}
		public async Task<ResponseMessageDto> deleteFileById(FileDeleteDto dto)
		{
			ResponseMessageDto response = new ResponseMessageDto();
			response.Success = true;

			try
			{
				long websiteId = await loginUserData.GetWebsiteId();
				long usetId = await loginUserData.GetUserId();
				if (dto.Fid != null && dto.Fid.Count > 0)
				{
					List<FileBind> fafile_other;
					FileBind? fafile_binds;
					for (var i = 0; i < dto.Fid.Count; i++)
					{
						fafile_other = await (db.FileBinds.Where(e => e.FK_FileUploadId == dto.Fid[i] && e.type == dto.Type && e.Sid != dto.Sid)).ToListAsync();
						fafile_binds = await db.FileBinds.Where(e => e.FK_FileUploadId == dto.Fid[i] && e.type == dto.Type && e.Sid == dto.Sid).FirstOrDefaultAsync();

						if (fafile_other.Count > 0 && fafile_binds != null)
						{
							fafile_binds.IsDeleted = true;
							fafile_binds.DeletionTime = DateTime.Now;
							fafile_binds.DeleterUserId = usetId;
							db.SaveChanges();
						}
						else
						{
							var fafile = await (db.FileUploads.Where(e => e.FK_WebsiteId == websiteId).Where(e => e.Id == dto.Fid[i]).Where(e => !e.IsDeleted).FirstOrDefaultAsync());
							if (fafile != null)
							{
								if (fafile_binds != null)
								{
									var chfile_binds = await (db.FileBindMores.Where(e => e.FK_FileBindGuid == fafile.GuidKey && e.type == (int)FileBindMoreEnum.壓縮圖片).Where(e => !e.IsDeleted).ToListAsync());
									if (chfile_binds != null)
									{
										foreach (var chfile_bind in chfile_binds)
										{
											var chfile = await (db.FileUploads.Where(e => e.FK_WebsiteId == websiteId).Where(e => e.Id == chfile_bind.FK_FileUploadId).Where(e => !e.IsDeleted).FirstOrDefaultAsync());
											if (chfile != null && response.Success)
											{
												response = await this.deleteFile(chfile.GuidKey);
												if (response.Success)
												{
													chfile.IsDeleted = true;
													chfile.DeletionTime = DateTime.Now;
													chfile.DeleterUserId = usetId;

													chfile_bind.IsDeleted = true;
													chfile_bind.DeletionTime = DateTime.Now;
													chfile_bind.DeleterUserId = usetId;

													db.SaveChanges();
												}
											}
										}
									}
									fafile_binds.IsDeleted = true;
									fafile_binds.DeletionTime = DateTime.Now;
									fafile_binds.DeleterUserId = usetId;
									db.SaveChanges();
								}
								response = await this.deleteFile(fafile.GuidKey);
								if (response.Success)
								{
									fafile.IsDeleted = true;
									fafile.DeletionTime = DateTime.Now;
									fafile.DeleterUserId = usetId;
									db.SaveChanges();
								}
							}
							else {
                                fafile_binds.IsDeleted = true;
                                fafile_binds.DeletionTime = DateTime.Now;
                                fafile_binds.DeleterUserId = usetId;
                                db.SaveChanges();
                            }
						}
						await loginUserData.SetLogs(AppName, "deleteImgFile", dto.Fid.ToString(), JsonConvert.SerializeObject(response));

						var websiteid = await loginUserData.GetWebsiteId();
						switch (dto.Type)
						{
							case (int)FileBindTypeEnum.選單圖:
								var db_bind = await db.WebMenus.Where(e => e.Id == dto.Sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
								if (db_bind != null)
								{
									db_bind.ImgId = null;
									db.SaveChanges();
								}
								break;
							case (int)FileBindTypeEnum.選單覆蓋:
								var db_bind_over = await db.WebMenus.Where(e => e.Id == dto.Sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
								if (db_bind_over != null)
								{
									db_bind_over.OverImgId = null;
									db.SaveChanges();
								}
								break;
							case (int)FileBindTypeEnum.右側浮動廣告:
							case (int)FileBindTypeEnum.進入廣告:
								var db_html = await db.Html_Contents.Where(e => e.Id == dto.Sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
								if (db_html != null)
								{
									db_html.Img = null;
									db.SaveChanges();
								}
								break;
							case (int)FileBindTypeEnum.選單Icon:
								var db_menuicon = await db.WebMenus.Where(e => e.Id == dto.Sid && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();
								if (db_menuicon != null) db_menuicon.icon = "empty";
                                break;
						}
					}
					return response;
				}
				else
				{
					var result = await (from fb in db.FileBinds
										where fb.Sid == dto.Sid && !fb.IsDeleted
										select fb.FK_FileUploadId).ToListAsync();

					if (result.Count > 0)
					{
						var fid_list = new List<long>();
						result.ForEach(fid =>
						{
							fid_list.Add((long)fid);
						});

						response = await this.deleteFileById(new FileDeleteDto
						{
							Sid = dto.Sid,
							Fid = fid_list,
							Type = dto.Type
						});
						return response;
					}
					else
					{
						response.Error = "Fid為0";
						response.Success = false;
						return response;
					}
				}
			}
			catch (Exception e)
			{
				response.Error = e.Message;
				response.Success = false;
				return response;
			}

		}
		private async Task<FileItemDto> SaveFile(IFormFile file, string directory, bool isTemp = false)
		{
			if (file.Length > 0)
			{
				string orgName = await loginUserData.GetWebsiteOrgName();
				Guid key = Guid.NewGuid();
				string[] sp = file.FileName.Split('.');
				string ext = sp[sp.Length - 1];
				var rootPath = $"{_folder}/{orgName}";
				var directoryPath = $"{rootPath}/{directory}";
				var path = $"/{directory}/{key}.{ext}";
				if (!fileAllow.Ext.Contains(file.ContentType)) throw new Exception("Type Error");
				if (!System.IO.Directory.Exists(directoryPath)) System.IO.Directory.CreateDirectory(directoryPath);
				using (var stream = new FileStream($"{rootPath}{path}", FileMode.Create))
				{
					await file.CopyToAsync(stream);
					if (isTemp)
					{
						return new FileItemDto
						{
							Name = file.FileName,
							Path = $@"/upload{path}".Replace("/upload/", $"/upload/{orgName}/"),
							Guid = Guid.NewGuid()
						};
					}
					else
					{
						FileUpload fileUpload = new FileUpload
						{
							FK_WebsiteId = await loginUserData.GetWebsiteId(),
							FileGuid = key,
							GuidKey = Guid.NewGuid(),
							DownloadFileName = $@"/upload{path}",
							OriginalFileName = file.FileName,
							ContentType = file.ContentType,
							Size = file.Length
						};
						db.FileUploads.Add(fileUpload);
						await loginUserData.SaveChanges(fileUpload);
						return new FileItemDto
						{
							Name = fileUpload.OriginalFileName,
							Path = fileUpload.DownloadFileName.Replace("/upload/", $"/upload/{orgName}/"),
							Guid = fileUpload.GuidKey,
						};
					}
				}
			}
			else throw new Exception("上傳失敗");
		}
		private async Task<List<FileItemDto>> SaveImage(IList<IFormFile> files, int asotype, int bindtype, int serno, string directory, long sid)
		{
			if (files.Count() > 0)
			{
				Guid faimg_id = new Guid();
				string orgName = await loginUserData.GetWebsiteOrgName();
				var return_item = new List<FileItemDto>();
				try
				{
					foreach (var file in files)
					{
						Guid key = Guid.NewGuid();
						string[] sp = file.FileName.Split('.');
						string ext = sp[sp.Length - 1];
						var rootPath = $"{_folder}/{orgName}";
						var directoryPath = $"{rootPath}/{directory}";
						var path = $"/{directory}/{key}.{ext}";
						if (!fileAllow.Ext.Contains(file.ContentType)) throw new Exception();
						if (!System.IO.Directory.Exists(directoryPath)) System.IO.Directory.CreateDirectory(directoryPath);
						using (var stream = new FileStream($"{rootPath}{path}", FileMode.Create))
						{
							FileUpload fileUpload = new FileUpload
							{
								FK_WebsiteId = await loginUserData.GetWebsiteId(),
								FileGuid = key,
								GuidKey = Guid.NewGuid(),
								DownloadFileName = $@"/upload{path}",
								OriginalFileName = file.FileName,
								ContentType = file.ContentType,
								Size = file.Length
							};
							await file.CopyToAsync(stream);
							db.FileUploads.Add(fileUpload);
							await loginUserData.SaveChanges(fileUpload);
							if (faimg_id != Guid.Empty)
							{
								FileBindMore fileBindMore = new FileBindMore
								{
									type = bindtype,
									FK_FileBindGuid = faimg_id,
									FK_FileUploadId = fileUpload.Id
								};
								db.FileBindMores.Add(fileBindMore);
								await loginUserData.SaveChanges(fileBindMore);
							}
							else
							{
								faimg_id = fileUpload.GuidKey;
								long userid = await loginUserData.GetUserId();
								Core.Models.FileBind fb = new Core.Models.FileBind
								{
									Guid = Guid.NewGuid(),
									Name = fileUpload.OriginalFileName,
									type = asotype,
									Sid = sid,
									num = 1,
									SerNo = serno,
									MediaLink = "",
									FK_FileUploadId = fileUpload.Id,
									CreatorUserId = userid,
								};
								db.FileBinds.Add(fb);
								db.SaveChanges();

							}

							return_item.Add(new FileItemDto
							{
								Id = fileUpload.Id,
								Name = fileUpload.OriginalFileName,
								Path = fileUpload.DownloadFileName.Replace("/upload/", $"/upload/{orgName}/"),
								Guid = fileUpload.GuidKey,
							});
						}
					}
					return return_item;
				}
				catch (Exception ex)
				{
					throw new Exception(ex.ToString());
				}
			}
			else throw new Exception("上傳失敗");
		}
	}
}
