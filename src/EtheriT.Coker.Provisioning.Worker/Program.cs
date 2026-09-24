using EtheriT.Coker.Provisioning.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
    options.ServiceName = "EtheriT Coker Provisioning Worker");
var workerOptions = builder.Configuration
    .GetSection("ProvisioningWorker")
    .Get<ProvisioningWorkerOptions>() ?? new ProvisioningWorkerOptions();
builder.Services.AddSingleton(workerOptions);
builder.Services.AddSingleton<PlatformProvisioningClient>();
builder.Services.AddSingleton<ProcessRunner>();
builder.Services.AddSingleton<WindowsSystemMetricsCollector>();
builder.Services.AddSingleton<IisMetricsCollector>();
builder.Services.AddSingleton<IProvisioningTaskHandler, IisTaskHandler>();
builder.Services.AddSingleton<IProvisioningTaskHandler, DnsTaskHandler>();
builder.Services.AddSingleton<IProvisioningTaskHandler, SslTaskHandler>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<SystemMonitoringWorker>();

var host = builder.Build();
host.Run();
