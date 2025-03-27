using System.Text.RegularExpressions;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using DocumentDbExampleLambda.Models.DocumentDb;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DocumentDbExampleLambda;

public class Function(IKafkaPublisher kafkaPublisher)
{
    private static readonly string[] ValidOperationTypes =
    [
        "insert",
        "update",
        "replace"
    ];
    
    private static readonly Regex EndsWithLatestRegex = new(
        ".*_LATEST$", 
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    [LambdaFunction(ResourceName = "DocumentDbEventFunction")]
    public async Task<string> FunctionHandler(
        Event @event, 
        ILambdaContext context
    )
    {
        try
        {
            context.Logger.LogDebug($"Processing trigger with {@event.Events} events from {@event.EventSource}");
            
            // Only get events that are of operation types we care about from the _LATEST collection(s)
            var eventsOfInterest = @event
                .Events
                .Where(e =>
                    ValidOperationTypes.Contains(e.Event.OperationType) &&
                    EndsWithLatestRegex.IsMatch(e.Event.Ns.Coll)
                )
                .ToList();

            if (eventsOfInterest.Any())
            {
                context.Logger.LogDebug($"Preparing to public {eventsOfInterest.Count} events of interest");

                await kafkaPublisher.Publish(
                    eventsOfInterest.Select(e => e.Event),
                    context.Logger
                );
                
                context.Logger.LogDebug($"All events of interest published to Kafka");
            }
            else
            {
                context.Logger.LogDebug("No events of interest found");
            }

            return "OK";
        }
        catch (Exception e)
        {
            context.Logger.LogError(e, "Error processing event");
            throw;
        }
    }

}
