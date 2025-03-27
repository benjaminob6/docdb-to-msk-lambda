using System.Text.Json;
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
            .AddEnvironmentVariables()
            .Build();
        
        var test = new KafkaOptions { BootstrapServerHosts = []};
        config.GetSection("Kafka").Bind(test);
        
        services.Configure<KafkaOptions>(
            config.GetSection("Kafka")
        );
        
        services.AddSingleton<IKafkaProducerFactory, KafkaProducerFactory>();
        services.AddSingleton<IKafkaAdminClientFactory, KafkaAdminClientFactory>();
        services.AddSingleton<IChangeEventMapper, ChangeEventMapper>();
        services.AddSingleton<IKafkaTopicManager, KafkaTopicManager>();
        services.AddSingleton<IKafkaPublisher, KafkaPublisher>();
    }
}