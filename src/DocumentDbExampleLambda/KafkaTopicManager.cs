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
    private readonly ConcurrentBag<string> _topics = new();

    public async Task CreateTopicIfNotExists(string topic)
    {
        if (!_topics.Any())
        {
            await GetExistingTopics();
        }

        if (_topics.Contains(topic))
        {
            return;
        }

        await _adminClient.CreateTopicsAsync(new[]
        {
            // TODO: Make Configurable
            new TopicSpecification
            {
                Name = topic,
                NumPartitions = 1,
                ReplicationFactor = 2,
            }
        });
    }

    private Task GetExistingTopics()
    {
        var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        
        metadata.Topics.Select(t => t.Topic).ToList().ForEach(t => _topics.Add(t));
        
        return Task.CompletedTask;
    }
}