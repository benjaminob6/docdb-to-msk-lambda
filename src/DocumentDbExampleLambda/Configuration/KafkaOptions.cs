using Confluent.Kafka;

namespace DocumentDbExampleLambda.Configuration;

public class KafkaOptions
{
    public string[] BootstrapServerHosts { get; set; } = ["localhost"];
    
    public uint Port { get; set; } = 9092;

    public bool IamEnabled => Port == 9098;
    
    public ClientConfig ClientConfig
    {
        get
        {
            var bootStrapServers = string.Join($":{Port},", BootstrapServerHosts);
            
            if (IamEnabled)
            {
                return new ClientConfig
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