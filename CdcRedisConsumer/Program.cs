using CdcRedisConsumer.Handlers;
using CdcRedisConsumer.Services;

namespace CdcRedisConsumer;

public class Program
{
    public static void Main(string[] args)
    {
        DotEnvLoader.Load();
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ICdcHandler, ProductHandler>();
                services.AddSingleton<ICdcHandler, DefaultHandler>();
                services.AddHostedService<RedisStreamConsumer>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });
}
