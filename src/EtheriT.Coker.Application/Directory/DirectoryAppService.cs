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
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.Application.Shared.Dto;

namespace EtheriT.Coker.Application.Directory
{
    public class DirectoryAppService : IDirectoryAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITagAppService tagAppService;
        private readonly IMapper mapper;
        private readonly IArticleAppService articleAppService;
        private readonly IProductAppService productAppService;
        private readonly IWebMenuApplication webMenuApplicationService;
        public DirectoryAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            ITagAppService tagAppService,
            IArticleAppService articleAppService,
            IProductAppService productAppService,
             IWebMenuApplication webMenuApplicationService,
             IConfiguration configuration
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.tagAppService = tagAppService;
            this.articleAppService = articleAppService;
            this.productAppService = productAppService;
            this.webMenuApplicationService = webMenuApplicationService;
        }
        public async Task<ResponseMessageDto> AddUp(DirectoryAddUpDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tag_response = new ResponseMessageDto() { Success = true };

            try
            {
                long userid = await loginUserData.GetUserId();
                var asoid = dto.Id;

                if (dto.Id == 0)
                {
                    if (userid != 0)
                    {
                        long WebsiteID = await loginUserData.GetWebsiteId();
                        Core.Models.Directory newItem = mapper.Map<Core.Models.Directory>(dto);
                        newItem.FK_WebsiteId = WebsiteID;
                        if (dto.Type == 3) newItem.FK_Mid = dto.FK_Mid;
                        db.Directory.Add(newItem);
                        await loginUserData.SaveChanges(newItem);
                        asoid = newItem.Id;
                    }
                    else throw new Exception("查無資料");
                }
                else
                {
                    var db_d = db.Directory.Where(e => e.Id == dto.Id).FirstOrDefault();
                    if (db_d != null)
                    {
                        db_d = mapper.Map(dto, db_d);
                        if (dto.Type == 3) db_d.FK_Mid = dto.FK_Mid;
                        await loginUserData.SaveChanges(db_d);
                    }
                    else throw new Exception("查無資料");
                }

                if (asoid != null && (dto.Type == (int)DirectoryTypeEnum.商品 || dto.Type == (int)DirectoryTypeEnum.文章))
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
                                            FK_MId = e.FK_Mid
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
        public async Task<DirectoryReleInfoGetDto> GetReleInfo(DirectoryReleInfoInputDto dto)
        {
            var DataIds = new List<long>();
            long WebsiteID = dto.SiteId == 0 ? await loginUserData.GetWebsiteId() : (long)dto.SiteId;
            var output = new DirectoryReleInfoGetDto();

            var db_d = db.Directory.Where(e => e.Id == dto.Ids[0] && e.FK_WebsiteId == WebsiteID && !e.IsDeleted).FirstOrDefault();

            if (db_d != null)
            {
                var tags = await (db.Tag_Associates.Where(e => e.FK_AId == dto.Ids[0] && e.Type == (int)TagAssociateTypeEnum.目錄 && !e.IsDeleted)).ToListAsync();

                if (tags != null)
                {
                    switch ((DirectoryTypeEnum)db_d.Type)
                    {
                        case DirectoryTypeEnum.商品:
                            var product_datas = new List<ProdGetDataDto>();
                            foreach (var tag in tags)
                            {
                                var db_ps = await (db.Tag_Associates.Where(e => e.FK_TId == tag.FK_TId && e.Type == (int)TagAssociateTypeEnum.商品 && !e.IsDeleted)).ToListAsync();
                                if (db_ps != null)
                                {
                                    foreach (var db_p in db_ps)
                                    {
                                        product_datas.Add(await productAppService.GetProdDataOne(db_p.FK_AId));
                                    }
                                }
                            }
                            foreach (var product_data in product_datas)
                            {
                                if (!DataIds.Contains(product_data.Id))
                                {
                                    DataIds.Add(product_data.Id);
                                }
                            }
                            break;
                        case DirectoryTypeEnum.文章:
                            var article_datas = new List<ArticleGetDataDto>();
                            foreach (var tag in tags)
                            {
                                var db_as = await (db.Tag_Associates.Where(e => e.FK_TId == tag.FK_TId && e.Type == (int)TagAssociateTypeEnum.文章 && !e.IsDeleted)).ToListAsync();
                                if (db_as != null)
                                {
                                    foreach (var db_a in db_as)
                                    {
                                        article_datas.Add(await articleAppService.GetDataOne(db_a.FK_AId));
                                    }
                                }
                            }
                            foreach (var article_data in article_datas)
                            {
                                if (!DataIds.Contains(article_data.Id))
                                {
                                    DataIds.Add(article_data.Id);
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    var page = (int)dto.Page;
                    var shownum = (int)dto.ShowNum;
                    output.TotalPage = (int)Math.Ceiling(DataIds.Count / (double)shownum);
                    switch ((DirectoryTypeEnum)db_d.Type)
                    {
                        case DirectoryTypeEnum.商品:
                            var tempproddata = await productAppService.GetDirectoryReleInfo(new DirectoryReleInfoInputDto
                            {
                                Ids = DataIds.Skip((page - 1) * shownum - 1).Take(shownum).ToList<long>(),
                                SiteId = WebsiteID
                            });
                            if (tempproddata != null)
                            {
                                output.ReleInfos = tempproddata;
                            }

                            break;
                        case DirectoryTypeEnum.文章:
                            var temparticledata = await articleAppService.GetDirectoryReleInfo(new DirectoryReleInfoInputDto
                            {
                                Ids = DataIds,
                                Page = page,
                                ShowNum = shownum, 
                                SiteId = WebsiteID
                            });
                            if (temparticledata != null)
                            {
                                output.ReleInfos = temparticledata;
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
            output.ReleInfos = output.ReleInfos.OrderBy(e => e.NodeDate).ThenBy(e => e.SerNo).ToList();
            return output;
        }
        public async Task<MenuItemDto> GetReleMenu(DataIdWebsiteIdDto dto)
        {
            var websiteid = dto.WebsiteId;
            if (websiteid == 0) websiteid = await loginUserData.GetWebsiteId();
            var output = await (from e in db.Directory where e.Id == dto.Id && !e.IsDeleted select e.FK_Mid).FirstOrDefaultAsync();
            if (output != null) return await webMenuApplicationService.GetDisplayOne(new DataIdWebsiteIdDto()
            {
                Id = (long)output,
                WebsiteId = websiteid
            });
            return null;
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
                                        Items = "",
                                        FK_Mid = e.FK_Mid,
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    if (output != null)
                    {
                        foreach (var data in output.data)
                        {
                            switch (data.GetType().GetProperty("Type").GetValue(data, null))
                            {
                                case "商品":
                                case "文章":
                                    var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                                    {
                                        Fk_Aid = (long)data.GetType().GetProperty("Id").GetValue(data, null),
                                        Type = (int)TagAssociateTypeEnum.目錄
                                    });
                                    var tag_text = "";
                                    foreach (var tagData in tagDatas) tag_text += tag_text == "" ? tagData.Tag_Name : $"、{tagData.Tag_Name}";
                                    data.GetType().GetProperty("Items").SetValue(data, tag_text == "" ? "無" : tag_text);
                                    break;
                                case "選單":
                                    var mid = data.GetType().GetProperty("FK_Mid").GetValue(data, null);
                                    MenuGetAllListDto webmenu_data;
                                    var webmenu = "";
                                    if (mid != null)
                                    {
                                        webmenu_data = await webMenuApplicationService.GetSelectData((long)mid);
                                        if (webmenu_data != null) webmenu = webmenu_data.Items == null ? webmenu_data.Title : $"{webmenu_data.Title}({webmenu_data.Items})";
                                    }
                                    data.GetType().GetProperty("Items").SetValue(data, webmenu);
                                    break;
                            }
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