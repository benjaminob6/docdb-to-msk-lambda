using Confluent.Kafka;
using DocumentDbExampleLambda.Configuration;
using Microsoft.Extensions.Options;

namespace DocumentDbExampleLambda;

public interface IKafkaAdminClientFactory
{
    IAdminClient Create();
}

public class KafkaAdminClientFactory(IOptions<KafkaOptions> options) : IKafkaAdminClientFactory
{
    private static readonly AWSMSKAuthTokenGenerator MskAuthTokenGenerator = new();
    
    public IAdminClient Create()
    {
        var builder = new AdminClientBuilder(options.Value.ClientConfig);
        
      if (options.Value.IamEnabled)
      {
          builder.SetOAuthBearerTokenRefreshHandler(AuthCallBack);
      }
      
      return builder.Build();
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