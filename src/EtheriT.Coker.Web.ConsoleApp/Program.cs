using EtheriT.Coker.Web.ConsoleApp.Models;
using EtheriT.Coker.Web.ConsoleApp.Controllers;
using EtheriT.Coker.Web.ConsoleApp.DbContextSet;
using Microsoft.Extensions.Configuration;
using System;

// load the configuration file.
var configBuilder = new ConfigurationBuilder().
   AddJsonFile("appsettings.json").Build();

// get the section to read
var configSection = configBuilder.GetSection("ConnectionStrings");

List<OldDataApplicaation> oldData = configBuilder.GetSection("Side").GetChildren().Select(clientConfig => new OldDataApplicaation {
    source = clientConfig["source"]??"",
    SiteID = int.Parse(clientConfig["id"]??"0"),
    orgName= clientConfig["orgName"] ?? "",
    auIds = clientConfig.GetSection("LoadMenuSubAuId").Get<List<int>>()??new List<int>(),
    subIds = clientConfig.GetSection("LoadMenuSubId").Get<List<int>>() ?? new List<int>(),
    shopSubId = clientConfig.GetSection("LoadShopingSubId").Get<List<int>>() ?? new List<int>()
}).ToList();

oldData.ForEach(e => {
    List<Tag> tags;
    List<Article> articles = e.loadData(out tags);
    NewDataApplicaation newDB = new NewDataApplicaation(configSection["destination"] ?? "");
    newDB.saveData(articles, tags);
});