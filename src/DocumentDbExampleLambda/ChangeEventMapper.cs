using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using DocumentDbExampleLambda.Models.DocumentDb;
using Timestamp = Confluent.Kafka.Timestamp;

namespace DocumentDbExampleLambda;

public interface IChangeEventMapper
{
    (string, Message<string, string>) Map(EventData eventToPublish);
}

public class ChangeEventMapper : IChangeEventMapper
{
    public (string, Message<string, string>) Map(EventData eventToPublish)
    {
        var timeStamp = new DateTimeOffset(
                eventToPublish.ClusterTime.Timestamp.ToUtcTime()
            );
        
        // TODO: random choice, let's consider what this should really be
        var topic = eventToPublish.Ns.Coll.Replace("_LATEST", ".latest");
        
        return (topic, new Message<string, string>
        {
            Timestamp = new Timestamp(timeStamp),
            Key = $"{eventToPublish.DocumentKey.Id.Oid}_{eventToPublish.ClusterTime.Timestamp.T}_{eventToPublish.ClusterTime.Timestamp.I}",
            Value = JsonSerializer.Serialize(new
            {
                LatestEvent = eventToPublish.FullDocument,
                Updates = eventToPublish.UpdateDescription
            }),
            Headers = [
                new Header("x-documentdb-operation-type", Encoding.UTF8.GetBytes(eventToPublish.OperationType)),
                new Header("x-lambda-handled-at", Encoding.UTF8.GetBytes(
                    DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"))),
            ]
        });
    }
}