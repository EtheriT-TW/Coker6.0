using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.ConsoleApp.DbContextSet;
using Microsoft.Extensions.Configuration;

// load the configuration file.
var configBuilder = new ConfigurationBuilder().
   AddJsonFile("appsettings.json").Build();

// get the section to read
var configSection = configBuilder.GetSection("ConnectionStrings");


using (var dbContext = new NewDbContext(configSection["source"]??""))
{
    var a = dbContext.Articles.Where(e => !e.IsDeleted);
    Console.WriteLine(a.Count());
}