using AutoMapper;
using EtheriT.Coker.Application.Shared.Dto.SeoSet;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.SeoSet
{
    public class SeoSetAppService : ISeoSetAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        public SeoSetAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper
        ) {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
        }

        public async Task<SeoSetOutputDto?> find(string key)
        {
            var result = await db.SeoSet.Where(e => !e.IsDeleted)
                                .Where(e => e.key == key)
                                .FirstOrDefaultAsync();
            if (result != null)
            {

            }
            else return null;
            throw new NotImplementedException();
        }

        public async Task<List<SeoSetOutputDto>> getAll()
        {
            throw new NotImplementedException();
        }
    }
}
