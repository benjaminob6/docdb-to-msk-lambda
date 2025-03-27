using Confluent.Kafka;
using DocumentDbExampleLambda.Configuration;
using Microsoft.Extensions.Options;

namespace DocumentDbExampleLambda;

public interface IKafkaProducerFactory
{
    IProducer<string, string> Create();
}

public class KafkaProducerFactory(IOptions<KafkaOptions> options) : IKafkaProducerFactory
{
    private readonly KafkaOptions _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    private static readonly AWSMSKAuthTokenGenerator MskAuthTokenGenerator = new();
    
    public IProducer<string, string> Create()
    {
        var producerBuilder = new ProducerBuilder<string, string>(_options.ClientConfig);
        
      if (_options.IamEnabled)
      {
          producerBuilder.SetOAuthBearerTokenRefreshHandler(AuthCallBack);
      }
      
      return producerBuilder.Build();
    }

    private static void AuthCallBack(IClient client, string cfg)
    {
        try
        {
            var (token, expiry) = MskAuthTokenGenerator.GenerateAuthToken(
                Amazon.RegionEndpoint.USEast1 // TODO: Configurable from env
            );
            
            client.OAuthBearerSetToken(token, expiry, "DummyPrincipal");
        }
        catch (Exception e)
        {
            client.OAuthBearerSetTokenFailure(e.ToString());
        }
    }
}