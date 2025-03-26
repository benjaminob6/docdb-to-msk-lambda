using System.Text;
using DocumentDbExampleLambda.Models.DocumentDb;
using Shouldly;
using Xunit;
using Timestamp = DocumentDbExampleLambda.Models.DocumentDb.Timestamp;

namespace DocumentDbExampleLambda.Tests;

public class ChangeEventMapperTest
{

    [Theory]
    [InlineData("test_LATEST", "test.latest")]
    [InlineData("wumbo_LATEST", "wumbo.latest")]
    public void StuffMapsRight(string collectionName, string expectedTopic)
    {
       // arrange
       var testEventData = new EventData
       {
           Id = new IdData() { Data = string.Empty },
           ClusterTime = new ClusterTime { 
               Timestamp = new Timestamp
               {
                   T = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
               }
           },
           Ns = new Namespace
           {
               Db = "test",
               Coll = collectionName
           },
           DocumentKey = new DocumentKey
           {
               Id = new Id
               {
                   Oid = Guid.NewGuid().ToString()
               }
           },
           OperationType = "fake",
           FullDocument = new ()
       };
       
       // act
       var (topic, msg) = new ChangeEventMapper().Map(testEventData);

       // assert
       topic.ShouldBe(expectedTopic);
       msg.Key.ShouldBe(testEventData.DocumentKey.Id.Oid);
       msg.Timestamp.UnixTimestampMs.ShouldBe(testEventData.ClusterTime.Timestamp.T);

       var headers = msg.Headers;
       headers.Count.ShouldBe(1);
       headers[0].Key.ShouldBe("x-documentdb-operation-type");
       headers[0].GetValueBytes().ShouldBe(Encoding.UTF8.GetBytes(testEventData.OperationType));
    }
}