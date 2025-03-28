using System.Diagnostics;
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

      // Push all messages to produce buffer
      var producerTasks = new List<Task>();
      var targetTopics = new List<string>();
      
      foreach (var mappedMessage in mappedMessages)
      {
         lambdaLogger.LogDebug($"Publishing message to Topic: {mappedMessage.Item1}");
         targetTopics.Add(mappedMessage.Item1);
         producerTasks.Add(_producer.ProduceAsync(mappedMessage.Item1, mappedMessage.Item2));
      }

      var sw = new Stopwatch();
      sw.Start();
      try
      {
         foreach (var topic in targetTopics)
         {
            await topicManager.CreateTopicIfNotExists(topic);
         }
      }
      catch (Exception e)
      {
         // TODO: Determine if this is retryable or not!
         lambdaLogger.LogError(e, $"Error ensuring topics: {string.Join(", ", targetTopics)}");
         throw;
      }
      sw.Stop();
      lambdaLogger.LogDebug("Ensured topics in {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);

      try
      {
         sw.Restart();
         Task.WaitAll(producerTasks.ToArray());
         sw.Stop();
         
         lambdaLogger.LogInformation("Published {NumMessages} events to Kafka in {ElapsedMilliseconds}ms}",
            producerTasks.Count,
            sw.ElapsedMilliseconds);
      }
      catch (Exception e)
      {
         lambdaLogger.LogError(e, "Error waiting for producer tasks to complete");
         throw;
      }
   }
}