using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public class UserHabitsWorking
    {
        private readonly CokerDbContext db;
        public UserHabitsWorking(CokerDbContext db) {
            this.db = db;
        }
        public void HabitCollection() {
            db.Database.ExecuteSqlRaw("UPDATE Remotes SET State = {0} WHERE TimeOnPage = 0", (int)RemoteStateEnum.資料不完整);
            var data = db.Remotes.Where(e => e.State != RemoteStateEnum.已完成).ToList();
            foreach (var item in data)
            {
                
            }
        }
    }
}
