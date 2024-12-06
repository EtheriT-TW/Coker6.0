using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public class BackgroundJobService
    {
        private readonly IRecurringJobManager _recurringJobManager;
        public BackgroundJobService(IRecurringJobManager recurringJobManager)
        {
            _recurringJobManager = recurringJobManager;
        }
        public void InitializeJobs()
        {
            _recurringJobManager.AddOrUpdate<UserHabitsWorking>("UserHabits", job => job.HabitCollection(), Cron.Daily(18,30));
        }
    }
}
