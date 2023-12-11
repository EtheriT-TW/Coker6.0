using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Dto.Article;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.Search;
using DevExtreme.AspNet.Data;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Tag;
using EtheriT.Coker.Application.Shared.Tag;

namespace EtheriT.Coker.Application.Search
{
    public class CustSearchAppService: ICustSearchAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly ITagAppService tagAppService;
        private readonly IWebMenuApplication webMenuApplicationService;
        public CustSearchAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            ITagAppService tagAppService,
            IWebMenuApplication webMenuApplicationService
        ) {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.tagAppService = tagAppService;
            this.webMenuApplicationService = webMenuApplicationService;
        }
        public async Task<JsonResult> GetAll(DataSourceLoadOptions loadOptions) {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.CustSearch;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted && e.FK_WebsiteId == WebsiteID
                                    select new CuseSearchListDto
                                    {
                                        Id = e.Id,
                                        Title = e.Title,
                                        Visible = e.Visible,
                                        SerNO =e.SerNo
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無目錄資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<CuseSearchListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<List<SearchItemDto>> GetSearchList(long sid)
        {
            List<SearchItemDto> list = new List<SearchItemDto> {
                new SearchItemDto{
                    Id = 0,
                    Name = "找全部"
                }
            };
            switch (sid) {
                case 2:
                    list.Add(
                        new SearchItemDto {
                            Id = 1,
                            Name = "找廠商"
                        }
                    );
                    break;
                case 3:
                    list.Add(
                        new SearchItemDto
                        {
                            Id = 2,
                            Name = "最新消息"
                        }
                    );
                    break;
            }
            return list;
        }
    }
}
