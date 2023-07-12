using EtheriT.Coker.Web.ConsoleApp.Models;
using EtheriT.Coker.Web.ConsoleApp.Controllers;
using EtheriT.Coker.Web.ConsoleApp.DbContextSet;
using Microsoft.Extensions.Configuration;

// load the configuration file.
var configBuilder = new ConfigurationBuilder().
   AddJsonFile("appsettings.json").Build();

// get the section to read
var configSection = configBuilder.GetSection("ConnectionStrings");

OldDataApplicaation old = new OldDataApplicaation(configSection["source"] ?? "", configBuilder.GetSection("SideID").Get<int>());
List<Tag> tags;
List<Article> articles = old.loadData(
    configBuilder.GetSection("LoadMenuSubAuId").Get<List<int>>() ?? new List<int>(), 
    configBuilder.GetSection("LoadMenuSubId").Get<List<int>>()??new List<int>(), 
    configBuilder.GetSection("LoadShopingSubId").Get<List<int>>() ?? new List<int>(),
    out tags
);

NewDataApplicaation newDB = new NewDataApplicaation(configSection["destination"] ?? "");
newDB.saveData(articles, tags);