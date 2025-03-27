using Amazon.Lambda.Core;
using Confluent.Kafka;
using DocumentDbExampleLambda.Models.DocumentDb;

namespace DocumentDbExampleLambda;

public interface IKafkaPublisher
{
   Task Publish(IEnumerable<EventData> eventsToPublish, ILambdaLogger lambdaLogger);
}

public class KafkaPublisher(
   IKafkaProducerFactory kafkaProducerFactory, 
   IKafkaTopicManager topicManager,
   IChangeEventMapper eventMapper)
   : IKafkaPublisher
{
   private readonly IProducer<string,string> _producer = kafkaProducerFactory.Create();

   public async Task Publish(
      IEnumerable<EventData> eventsToPublish,
      ILambdaLogger lambdaLogger)
   {
      var mappedMessages = eventsToPublish.Select(eventMapper.Map)
         .ToList();

      bool success = true;

      // Push all messages to produce buffer
      foreach (var mappedMessage in mappedMessages)
      {
         try
         {
            lambdaLogger.LogDebug($"Publishing message to Topic: {mappedMessage.Item1}");
            
            await topicManager.CreateTopicIfNotExists(mappedMessage.Item1);
            
            await _producer.ProduceAsync(mappedMessage.Item1, mappedMessage.Item2);
         }
         catch (ProduceException<string,string> e)
         {
            success = false;
            lambdaLogger.LogError(e, $"Error publishing message to Kafka: {e.Error.Reason}");
         }
      }

      // TODO: Actually good logic :)
      if (!success)
      {
         throw new Exception("Error publishing messages to Kafka");
      }
   }
}