using CDRB.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.InMemory;

namespace CDRB;
class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.InMemory()
            .WriteTo.Console()
            .WriteTo.File("logs/.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        await Host.CreateDefaultBuilder()
            .UseSerilog()
            .UseConsoleLifetime()
            .ConfigureServices((hostContext, services) => 
            {
                var configurationRoot = hostContext.Configuration;
                services.Configure<BotSettings>(configurationRoot);
                services.AddDbContextFactory<BotDatabaseContext>();
                services.AddLogging(logging => logging.ClearProviders().AddSerilog());
                services.AddHostedService<BotService>();
            })
            .RunConsoleAsync();

        await Log.CloseAndFlushAsync();
    }
}
