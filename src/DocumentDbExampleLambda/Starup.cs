using Amazon.Lambda.Annotations;
using DocumentDbExampleLambda.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentDbExampleLambda;

[LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        services.Configure<KafkaOptions>(
            config.GetSection("Kafka")
        );
        
        services.AddSingleton<IChangeEventMapper, ChangeEventMapper>();
        services.AddSingleton<IKafkaPublisher, KafkaPublisher>();
    }
}