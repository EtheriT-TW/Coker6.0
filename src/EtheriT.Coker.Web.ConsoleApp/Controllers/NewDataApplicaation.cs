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
        public NewDataApplicaation(string connectionStr)
        {
            newDb = connectionStr;
        }
        public bool saveData(List<Article> articles, List<Tag> tags)
        {
            var oldTag = tags.Select(e => new { e.Id,e.Title}).ToList();
            saveTags(tags, articles[0].FK_WebsiteId);
            using (var dbContext = new NewDbContext(newDb))
            {
                foreach(var article in articles)
                {
                    if (article.Associates != null) {
                        foreach (var associate in article.Associates)
                        {
                            var t1 = oldTag.Find(e => e.Id == associate.FK_TId);
                            if (t1 != null)
                            {
                                var t2 = tags.Find(e => e.Title == t1.Title);
                                if (t2 != null) {
                                    associate.FK_TId = t2.Id;
                                }
                            }
                        }
                    }
                }
                dbContext.AddRange(articles);
                dbContext.SaveChanges();
            }
            return true;
        }
        private void saveTags(List<Tag> tags,long siteId) {
            if(tags.Count==0) return;
            using (var dbContext = new NewDbContext(newDb)) {
                long WebsiteId = siteId;
                for (int i = 0; i < tags.Count; i++)
                {
                    tags[i].Id = 0;
                }
                if (tags.Count > 0)
                {
                    dbContext.AddRange(tags);
                    dbContext.SaveChanges();
                }
            }
        }
    }
}
