using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
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
        private readonly ITokenAppService tokenAppService;

        private readonly double decayFactor = 0.95; // 衰減因子
        private readonly TimeSpan timeRange = TimeSpan.FromDays(30); //衰減區間

        public UserHabitsWorking(CokerDbContext db, ITokenAppService tokenAppService) {
            this.db = db;
            this.tokenAppService = tokenAppService;
        }
        public void HabitCollection() {
            var currentDateTime = DateTime.Now;
            var cutoffDate = currentDateTime.AddDays(-timeRange.TotalDays);
            db.Database.ExecuteSqlRaw("UPDATE Remotes SET State = {0} WHERE TimeOnPage = 0", (int)RemoteStateEnum.資料不完整);
            //針對舊資料做衰減
            var outdatedStatistics = db.UserTagStatistics.Where(stat => stat.LastModificationTime < cutoffDate).ToList();
            foreach (var existingStatistic in outdatedStatistics)
            {
                // 計算時間已經過多久
                var timeElapsed = (currentDateTime - existingStatistic.LastModificationTime).TotalDays;

                // 計算衰減的比例，根據時間過去的天數來調整
                var timeDecay = Math.Pow(decayFactor, timeElapsed / timeRange.TotalDays);

                // 應用衰減比例
                existingStatistic.Weight *= timeDecay;
                existingStatistic.LastModificationTime = currentDateTime; // 更新時間戳
            }

            var data = db.UserActivityTags.Include(e => e.Remote).Where(e => e.Remote.State == RemoteStateEnum.未處理 && e.Remote.TimeOnPage != 0)
                .ToList();

            // 快取現有的統計資料
            var uuids = data.Select(d => tokenAppService.GetUUID(d.Remote.UUID)).ToHashSet();
            var tagIds = data.Select(d => d.FK_TId).ToHashSet();

            var existingStatistics = db.UserTagStatistics
                .Where(stat => uuids.Contains(stat.UUID) && tagIds.Contains(stat.FK_TagId))
                .ToList()
                .ToDictionary(stat => (stat.UUID, stat.FK_TagId));

            List<UserTagStatistic> UserTagStatisticAdds = new List<UserTagStatistic>();
            foreach (var tag in data)
            {
                var UUID = tokenAppService.GetUUID(tag.Remote.UUID);
                var key = (UUID, tag.FK_TId);

                if (existingStatistics.TryGetValue(key, out var existingStatistic))
                {
                    // 如果已存在的資料，應用當前的計算結果
                    existingStatistic.Weight += tag.Weight; // 假設這是從資料中計算出來的權重
                    existingStatistic.TotalTimes += tag.Remote.TimeOnPage;
                    existingStatistic.LastActivityTime = currentDateTime;
                    existingStatistic.LastModificationTime = currentDateTime;
                } else {
                    // 如果是新的標籤資料，則新增
                    var item = UserTagStatisticAdds.Find(e => e.UUID == UUID && e.FK_TagId == tag.FK_TId);
                    if (item == null)
                    {
                        var newStatistic = new UserTagStatistic
                        {
                            UUID = UUID,
                            FK_TagId = tag.FK_TId,
                            Weight = tag.Weight, // 使用計算的權重
                            TotalTimes = tag.Remote.TimeOnPage,
                            LastActivityTime = currentDateTime,
                            LastModificationTime = currentDateTime,
                        };
                        UserTagStatisticAdds.Add(newStatistic);
                    }
                    else { 
                        item.Weight += tag.Weight;
                        item.TotalTimes += tag.Remote.TimeOnPage;
                    }
                }
                tag.Remote.State = RemoteStateEnum.已完成;
            }
            db.UserTagStatistics.AddRange(UserTagStatisticAdds);
            db.SaveChanges();
        }
    }
}
