using Confluent.Kafka;
using DocumentDbExampleLambda.Configuration;
using DocumentDbExampleLambda.Models.DocumentDb;
using Microsoft.Extensions.Options;

namespace DocumentDbExampleLambda;

public interface IKafkaPublisher
{
   Task Publish(IEnumerable<EventData> eventsToPublish);
}

public class KafkaPublisher(IOptions<KafkaOptions> kafkaOptions, IChangeEventMapper eventMapper)
   : IKafkaPublisher
{
   private readonly IProducer<string,string> _producer = new ProducerBuilder<string, string>(kafkaOptions.Value.ClientConfig)
      .Build();

   public Task Publish(IEnumerable<EventData> eventsToPublish)
   {
      var mappedMessages = eventsToPublish.Select(eventMapper.Map)
         .ToList();

      // Push all messages to produce buffer
      foreach (var mappedMessage in mappedMessages)
      {
         _producer.Produce(mappedMessage.Item1, mappedMessage.Item2);
      }

      // Force publish regardless of linger.ms , etc
      _producer.Flush(TimeSpan.FromSeconds(10));
      
      // TODO: Ignoring Delivery Receipt and Retry-able or Fatal errors
      // For durability we should ensure there are delivery receipt
      // This would play into potential DLQ / Retry schemes based on nature
      // of the failure. Where possible, push the retry logic down to lambda trigger
      
      return Task.CompletedTask;
   }
}