using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Tag;

namespace EtheriT.Coker.Application.Directory
{
    public class DirectoryAppService : IDirectoryAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITagAppService tagAppService;
        private readonly IMapper mapper;
        public DirectoryAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            ITagAppService tagAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.tagAppService = tagAppService;
        }
        public async Task<ResponseMessageDto> AddUp(DirectoryAddUpDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tag_response = new ResponseMessageDto() { Success = false };

            try
            {
                long userid = await loginUserData.GetUserId();
                var asoid = dto.Id;

                if (dto.Id == 0)
                {
                    if (userid != 0)
                    {
                        long WebsiteID = await loginUserData.GetWebsiteId();
                        Core.Models.Directory newItem = new Core.Models.Directory
                        {
                            FK_WebsiteId = WebsiteID,
                            Title = dto.Title,
                            Description = dto.Description,
                            Visible = dto.Visible,
                            Type = dto.Type,
                        };
                        db.Directory.Add(newItem);
                        await loginUserData.SaveChanges(newItem);
                        asoid = newItem.Id;
                    }
                    else throw new Exception("查無資料");
                }
                else
                {
                    var db_d = db.Directory.Where(e => e.Id == dto.Id).FirstOrDefault();
                    if (db_d != null && userid != 0)
                    {
                        db_d.Title = dto.Title;
                        db_d.Description = dto.Description;
                        db_d.Visible = dto.Visible;
                        db_d.Type = dto.Type;
                        db_d.LastModifierUserId = userid;
                        db_d.LastModificationTime = DateTime.Now;
                        await loginUserData.SaveChanges(db_d);
                    }
                    else throw new Exception("查無資料");
                }

                if (asoid != null)
                {
                    var tagitem = new List<TagAssociateDto>();
                    foreach (var data in dto.TagSelected)
                    {
                        tagitem.Add(new TagAssociateDto()
                        {
                            Id = data.Id,
                            FK_AId = (long)asoid,
                            FK_TId = data.FK_TId,
                            Type = (int)TagAssociateTypeEnum.目錄,
                            IsDeleted = data.IsDeleted
                        });
                    }

                    tag_response = await tagAppService.TagAssociateAddDelect(tagitem);
                }

                output.Success = tag_response.Success;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            return output;
        }
        public async Task<DirectoryGetDataDto> GetDataOne(long Id)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.Directory.Where(e => e.Id == Id && !e.IsDeleted && e.FK_WebsiteId == WebsiteID);

                if (result != null)
                {
                    var output = await (from e in result
                                        select new DirectoryGetDataDto
                                        {
                                            Id = e.Id,
                                            Title = e.Title,
                                            Description = e.Description,
                                            Visible = e.Visible,
                                            Type = e.Type,
                                            TagDatas = new List<TagGetSelectedDto>(),
                                        }).FirstOrDefaultAsync();

                    var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                    {
                        Fk_Aid = output.Id,
                        Type = (int)TagAssociateTypeEnum.目錄,
                    }
                    );

                    if (tagDatas != null)
                    {
                        output.TagDatas = tagDatas;
                    }

                    return output;
                }
                else throw new Exception("查無文章資料");
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.Directory;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted && e.FK_WebsiteId == WebsiteID
                                    select new DirectoryGetListDto
                                    {
                                        Id = e.Id,
                                        Title = e.Title,
                                        Description = e.Description,
                                        Type = ((DirectoryTypeEnum)e.Type).ToString(),
                                        Visible = e.Visible,
                                        Tags = "",
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    if (output != null)
                    {
                        foreach (var data in output.data)
                        {
                            var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                            {
                                Fk_Aid = (long)data.GetType().GetProperty("Id").GetValue(data, null),
                                Type = 3
                            });

                            var tag_text = "";
                            foreach (var tagData in tagDatas)
                            {
                                tag_text += tag_text == "" ? tagData.Tag_Name : $"、{tagData.Tag_Name}";
                            }

                            data.GetType().GetProperty("Tags").SetValue(data, tag_text == "" ? "無" : tag_text);
                        }
                    }
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無目錄資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<DirectoryGetListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tagdeleteresponse = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var result = db.Directory.Where(e => e.Id == Id).FirstOrDefault();

                if (result != null)
                {
                    var tagids = await db.Tag_Associates.Where(e => e.FK_AId == Id && e.Type == (int)TagAssociateTypeEnum.目錄 && !e.IsDeleted).ToListAsync();

                    if (tagids != null)
                    {
                        foreach (var tagid in tagids)
                        {

                            tagdeleteresponse = await tagAppService.TagAssociateDelete(tagid.Id);
                        }
                    }

                    result.IsDeleted = true;
                    result.DeletionTime = DateTime.Now;
                    result.DeleterUserId = usetId;
                    db.SaveChanges();

                    output.Success = tagdeleteresponse.Success;
                }
                else throw new Exception("查無目錄資料");
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