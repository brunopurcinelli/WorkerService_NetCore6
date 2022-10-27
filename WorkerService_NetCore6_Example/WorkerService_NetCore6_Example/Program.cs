using WorkerService_NetCore6_Example;
using WorkerService_NetCore6_Example.Application.Interfaces;
using WorkerService_NetCore6_Example.Application.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Example Service";
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IWorkerAppService, WorkerAppService>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();

//sc create Worker_Service binPath=C:\temp\Worker_Service.exe
