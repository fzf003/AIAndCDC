using CdcRedisConsumer.Handlers;
using CdcRedisConsumer.Services;

namespace CdcRedisConsumer;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // 注册处理器
                services.AddSingleton<ICdcHandler, ProductHandler>();
                services.AddSingleton<ICdcHandler, DefaultHandler>();

                // 注册消费者服务
                services.AddHostedService<RedisStreamConsumer>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });
}
