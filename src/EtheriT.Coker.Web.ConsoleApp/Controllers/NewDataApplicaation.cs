using EtheriT.Coker.Web.ConsoleApp.DbContextSet;
using EtheriT.Coker.Web.ConsoleApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Controllers
{
    public class NewDataApplicaation
    {
        private string newDb { get; set; }
        public NewDataApplicaation(string connectionStr) {
            newDb = connectionStr;
        }
        public bool saveData(List<Article> articles) {
            using (var dbContext = new NewDbContext(newDb)) {
                dbContext.AddRange(articles);
                dbContext.SaveChanges();
            }
            return true;
        }
    }
}
