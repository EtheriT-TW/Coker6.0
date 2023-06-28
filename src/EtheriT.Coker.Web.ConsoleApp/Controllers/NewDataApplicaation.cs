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
        public bool saveData(List<Article> articles)
        {
            using (var dbContext = new NewDbContext(newDb))
            {
                dbContext.AddRange(articles);
                dbContext.SaveChanges();

                var ArtTags = dbContext.Articles.GroupBy(e => e.Description).Select(e => e.Key).ToList();
                if (ArtTags.FindIndex(e => e == "全部") >= 0)
                {
                    ArtTags[ArtTags.FindIndex(e => e == "全部")] = "園區廠商";
                }

                var TotalTags = dbContext.Tags.Select(e => e.Title).ToList();
                List<string?> addTagStringList = ArtTags.FindAll(e => !TotalTags.Contains(e ?? ""));
                List<Tag> addTagsList = new List<Tag>();

                for (int i = 0; i < addTagStringList.Count; i++)
                {
                    var tag = addTagStringList[i];
                    if (tag != null)
                    {
                        addTagsList.Add(new Tag
                        {
                            Title = tag,
                            CreationTime = DateTime.Now,
                            CreatorUserId = 1,
                            IsDeleted = false,
                            FK_WebsiteId = articles[0].FK_WebsiteId
                        });
                    }
                }
                if (addTagsList.Count > 0)
                {
                    dbContext.AddRange(addTagsList);
                    dbContext.SaveChanges();
                }
            }
            return true;
        }
    }
}
