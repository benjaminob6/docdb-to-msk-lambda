using Xunit;
using Amazon.Lambda.TestUtilities;
using DocumentDbExampleLambda.Models.DocumentDb;
using NSubstitute;

namespace DocumentDbExampleLambda.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestToUpperFunction()
    {
        var function = new Function(Substitute.For<IKafkaPublisher>());
        var context = new TestLambdaContext();
        var messageBodyText = await File.ReadAllTextAsync("payloads/awsExampleEvent.json");
        var messageObject = System.Text.Json.JsonSerializer.Deserialize<Event>(messageBodyText)!;
        
        var result = await function.FunctionHandler(messageObject, context);
        
        Assert.Equal("OK", "OK");
    }
}
