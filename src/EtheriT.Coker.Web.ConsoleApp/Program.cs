using Microsoft.Extensions.Configuration;

// load the configuration file.
var configBuilder = new ConfigurationBuilder().
   AddJsonFile("appsettings.json").Build();

// get the section to read
var configSection = configBuilder.GetSection("ConnectionStrings");

// get the configuration values in the section.
var source = configSection["source"] ?? null;
Console.WriteLine(source);