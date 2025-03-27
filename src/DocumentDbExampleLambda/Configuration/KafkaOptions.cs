using Confluent.Kafka;

namespace DocumentDbExampleLambda.Configuration;

public class KafkaOptions
{
    public required string[] BootstrapServerHosts { get; init; }
    
    public int Port { get; init; }

    public bool IamEnabled => Port == 9098;
    
    public ClientConfig ClientConfig
    {
        get
        {
            var hostStrings = BootstrapServerHosts.Select(host => $"{host}:{Port}");
            
            var bootStrapServers = string.Join(",", hostStrings);
            
            if (IamEnabled)
            {
                return new ProducerConfig
                {
                    BootstrapServers = bootStrapServers,
                    SecurityProtocol = SecurityProtocol.SaslSsl,
                    SaslMechanism = SaslMechanism.OAuthBearer,
                };
            }

            return new ClientConfig { BootstrapServers = bootStrapServers };
        }
    }
}