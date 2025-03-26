using Xunit;
using Amazon.Lambda.TestUtilities;
using DocumentDbExampleLambda.Models.DocumentDb;

namespace DocumentDbExampleLambda.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestToUpperFunction()
    {
        // Invoke the lambda function and confirm the string was upper cased.
        var function = new Function();
        var context = new TestLambdaContext();
        var messageBodyText = await File.ReadAllTextAsync("payloads/awsExampleEvent.json");
        var messageObject = System.Text.Json.JsonSerializer.Deserialize<Event>(messageBodyText)!;
        
        var result = await function.FunctionHandler(messageObject, context);
        
        Assert.Equal("OK", result);
    }
}
