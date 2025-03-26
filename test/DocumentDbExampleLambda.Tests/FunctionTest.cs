using Xunit;
using Amazon.Lambda.TestUtilities;
using DocumentDbExampleLambda.Models.DocumentDb;
using NSubstitute;
using Shouldly;

namespace DocumentDbExampleLambda.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestToUpperFunction()
    {
        var kafkaSub = Substitute.For<IKafkaPublisher>();
        var function = new Function(kafkaSub);
        var context = new TestLambdaContext();
        var messageBodyText = await File.ReadAllTextAsync("payloads/awsExampleEvent.json");
        var messageObject = System.Text.Json.JsonSerializer.Deserialize<Event>(messageBodyText)!;
        
        var result = await function.FunctionHandler(messageObject, context);
        
        result.ShouldBe("OK");
        await kafkaSub.Received(1).Publish(Arg.Any<IEnumerable<EventData>>());
    }
}
