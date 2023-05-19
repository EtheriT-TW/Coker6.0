using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Application.Directory
{
    public class DirectoryAppService : IDirectoryAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        public DirectoryAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
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
                                    select new DirectoryGetAllListDto
                                    {
                                        Id = e.Id,
                                        Title = e.Title,
                                        Description = e.Description,
                                        Type = ((DirectoryTypeEnum)e.Type).ToString(),
                                        Visible = e.Visible,
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無目錄資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<DirectoryGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
    }
}