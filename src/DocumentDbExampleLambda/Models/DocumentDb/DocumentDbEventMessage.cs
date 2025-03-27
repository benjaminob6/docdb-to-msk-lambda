using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace DocumentDbExampleLambda.Models.DocumentDb;

// https://docs.aws.amazon.com/lambda/latest/dg/example_serverless_DocumentDB_Lambda_section.html

public class Event
{
    [JsonPropertyName("eventSourceArn")]
    public required string EventSourceArn { get; set; }

    [JsonPropertyName("events")]
    public required List<DocumentDbEventRecord> Events { get; set; }

    [JsonPropertyName("eventSource")]
    public required string EventSource { get; set; }
}

public class DocumentDbEventRecord
{
    [JsonPropertyName("event")]
    public required EventData Event { get; set; }
}

public class EventData
{
    [JsonPropertyName("_id")]
    public required IdData Id { get; set; }

    [JsonPropertyName("clusterTime")]
    public required ClusterTime ClusterTime { get; set; }

    [JsonPropertyName("documentKey")]
    public required DocumentKey DocumentKey { get; set; }

    [JsonPropertyName("fullDocument")]
    public required Dictionary<string, object> FullDocument { get; set; }
    
    [JsonPropertyName("updateDescription")]
    public Dictionary<string, object>? UpdateDescription { get; set; }

    [JsonPropertyName("ns")]
    public required Namespace Ns { get; set; }

    [JsonPropertyName("operationType")]
    public required string OperationType { get; set; }
}

public class IdData
{
    [JsonPropertyName("_data")]
    public required string Data { get; set; }
}

public class ClusterTime
{
    [JsonPropertyName("$timestamp")]
    public required Timestamp Timestamp { get; set; }
}

public class Timestamp
{
    [JsonPropertyName("t")]
    public long T { get; set; }

    [JsonPropertyName("i")]
    public int I { get; set; }
    
    public DateTime ToUtcTime()
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddSeconds(T);
    }
}

public class DocumentKey
{
    [JsonPropertyName("_id")]
    public required Id Id { get; set; }
}

public class Id
{
    [JsonPropertyName("$oid")]
    public required string Oid { get; set; }
}

public class Namespace
{
    [JsonPropertyName("db")]
    public required string Db { get; set; }

    [JsonPropertyName("coll")]
    public required string Coll { get; set; }
}