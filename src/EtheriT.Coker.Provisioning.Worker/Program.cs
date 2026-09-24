using EtheriT.Coker.Provisioning.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
    options.ServiceName = "EtheriT Coker Provisioning Worker");
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
