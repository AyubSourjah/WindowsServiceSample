using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using WinService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>)
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .UseWindowsService(options => { options.ServiceName = "Sample Windows Service"; })
    .Build();

host.Run();