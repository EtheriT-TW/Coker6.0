using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
	public class FileUploadAppService : IFileUploadAppService
	{
		private readonly CokerDbContext db;
		public readonly FileAllow fileAllow;
		private readonly LoginUserData loginUserData;
		private readonly string _folder;
		private readonly string AppName;
		public FileUploadAppService(
			IOptions<VirtualDirectory> fileAllow,
			LoginUserData loginUserData,
			CokerDbContext db
		)
		{
			this.fileAllow = fileAllow.Value.FileAllow;
			this.db = db;
			this.loginUserData = loginUserData;
			_folder = fileAllow.Value.upload;
			AppName = "FileUpload";
		}
		public async Task<UploadFileOutputDto> uploadTempFiles(IList<IFormFile> files)
		{
			UploadFileOutputDto response = await uploadFiles(files, "temp", isTemp: true);
			return response;
		}
		public async Task<UploadFileOutputDto> uploadHtmlContentFiles(IList<IFormFile> files)
		{
			UploadFileOutputDto response = await uploadFiles(files, "htmlConten");
			await loginUserData.SetLogs(AppName, "uploadProdtFiles", "FileBinary...", JsonConvert.SerializeObject(response));
			return response;
		}
		public async Task<UploadFileOutputDto> uploadProdtFiles(IList<IFormFile> files, long id)
		{
			UploadFileOutputDto response = await uploadFiles(files, "Prod");
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
		public async Task<UploadFileOutputDto> uploadTechnicalCertificateFiles(IList<IFormFile> files, int type, long sid)
		{
			UploadFileOutputDto response = new UploadFileOutputDto
			{
				Files = new List<FileItemDto>(),
				ErrorFiles = new List<string>()
			};
			try
			{
				List<FileItemDto> items = await SaveImage(files, type, "TechnicalCertificate", sid);
				foreach (var item in items)
				{
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
		public async Task<ResponseMessageDto> deleteImg(long? imgid)
		{
			ResponseMessageDto response = new ResponseMessageDto();

			try
			{
				long websiteId = await loginUserData.GetWebsiteId();
				long usetId = await loginUserData.GetUserId();

				var faimg = await (db.FileUploads.Where(e => e.FK_WebsiteId == websiteId).Where(e => e.Id == imgid).Where(e => !e.IsDeleted).FirstOrDefaultAsync());
				if (faimg != null)
				{
					faimg.IsDeleted = true;
					faimg.DeletionTime = DateTime.Now;
					faimg.DeleterUserId = usetId;

					var chimg_binds = await (db.FileBindMores.Where(e => e.FK_FileBindGuid == faimg.GuidKey).Where(e => !e.IsDeleted).ToListAsync());
					if (chimg_binds != null)
					{
						foreach (var chimg_bind in chimg_binds)
						{
							var chimg = await (db.FileUploads.Where(e => e.FK_WebsiteId == websiteId).Where(e => e.Id == chimg_bind.FK_FileUploadId).Where(e => !e.IsDeleted).FirstOrDefaultAsync());
							if (chimg != null)
							{
								chimg.IsDeleted = true;
								chimg.DeletionTime = DateTime.Now;
								chimg.DeleterUserId = usetId;
								db.SaveChanges();
							}
							chimg_bind.IsDeleted = true;
							chimg_bind.DeletionTime = DateTime.Now;
							chimg_bind.DeleterUserId = usetId;
						}
						db.SaveChanges();
					}
				}

				db.SaveChanges();
				response.Success = true;

			}
			catch (Exception e)
			{
				response.Error = e.Message;
			}

			await loginUserData.SetLogs(AppName, "deleteImgFile", imgid.ToString(), JsonConvert.SerializeObject(response));
			return response;
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
		public async Task<List<ImgGetDto>> getImgThumbnail(long? tid)
		{
			try
			{
				long websiteId = await loginUserData.GetWebsiteId();
				string orgName = await loginUserData.GetWebsiteOrgName();
				var result = new List<ImgGetDto>();

				var faids = await (db.FileBinds.Where(e => e.Sid == tid).Where(e => !e.IsDeleted).Select(e => e.FK_FileUploadId).ToListAsync());

				if (faids != null)
				{
					foreach (var faid in faids)
					{
						var faguid = await (db.FileUploads.Where(e => e.Id == faid).Select(e => e.GuidKey).FirstOrDefaultAsync());
						if (faguid != Guid.Empty)
						{
							var chimg_ids = await (db.FileBindMores.Where(e => e.FK_FileBindGuid == faguid).Where(e => !e.IsDeleted).Select(e => e.FK_FileUploadId).ToListAsync());
							if (chimg_ids.Count > 0)
							{
								var chimg = new List<FileUpload>();
								foreach (var chimg_id in chimg_ids)
								{
									chimg.Add(await (db.FileUploads.Where(e => e.Id == chimg_id).Where(e => !e.IsDeleted).FirstOrDefaultAsync()));

								}
								chimg = chimg.OrderBy(e => e.Size).ToList();
								result.Add(new ImgGetDto
								{
									Id = faid.Value,
									Name = chimg[0].OriginalFileName,
									Link = chimg[0].DownloadFileName.Replace("upload", $"upload/{orgName}")
								});
							}
						}
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				return null;
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
				if (!fileAllow.Ext.Contains(file.ContentType)) throw new Exception();
				if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
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
		private async Task<List<FileItemDto>> SaveImage(IList<IFormFile> files, int type, string directory, long sid)
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
						if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
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
									type = type,
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
									type = type,
									Sid = sid,
									num = 1,
									SerNo = 1,
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
