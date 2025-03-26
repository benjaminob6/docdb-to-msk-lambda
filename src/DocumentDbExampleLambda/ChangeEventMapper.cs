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
            Key = eventToPublish.DocumentKey.Id.Oid,
            Value = JsonSerializer.Serialize(eventToPublish.FullDocument),
            Headers = [
                new Header("x-documentdb-operation-type", Encoding.UTF8.GetBytes(eventToPublish.OperationType))
            ]
        });
    }
}