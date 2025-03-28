using System.Collections.Concurrent;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace DocumentDbExampleLambda;

public interface IKafkaTopicManager
{ 
    Task CreateTopicIfNotExists(string topic);
}

public class KafkaTopicManager(IKafkaAdminClientFactory adminClientFactory): IKafkaTopicManager
{
    private readonly IAdminClient _adminClient = adminClientFactory.Create();
    private readonly Dictionary<string, bool> _topics = new(); // BAD

    public async Task CreateTopicIfNotExists(string topic)
    {
        if (!_topics.Any())
        {
            await GetExistingTopics();
        }

        if (_topics.ContainsKey(topic))
        {
            return;
        }

        try
        {
            await _adminClient.CreateTopicsAsync([
                // TODO: Make Configurable
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 1,
                    ReplicationFactor = 2,
                }
            ]);
            
            _topics.Add(topic, true);
        }
        catch (CreateTopicsException e)
        {
            if (e.Results.Any(r => r.Error.Code != ErrorCode.TopicAlreadyExists))
            {
                throw;
            }
            
            _topics.Add(topic, true);
        }
    }

    private Task GetExistingTopics()
    {
        var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        
        metadata.Topics.Select(t => t.Topic).ToList().ForEach(t => _topics.Add(t, true));
        
        Console.WriteLine($"Found {_topics.Count} topics");
        
        return Task.CompletedTask;
    }
}