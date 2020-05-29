using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionsForNdcLondon
{
    public static class HelloDurableFunction
    {
        // 2. OrchestrationFunction instance created
        // Steps are outlined and started with first CallActivityAsync call
        // Once complete, returns list of Hello messages
        [FunctionName("OrchestrationFunction")]
        public static async Task<List<string>> Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("ActivityFunction", "Stockholm"));
            outputs.Add(await context.CallActivityAsync<string>("ActivityFunction", "Malmö"));
            outputs.Add(await context.CallActivityAsync<string>("ActivityFunction", "Gothenburg"));

            return outputs;
        }

        // 3. (x number of invocations from OrchestrationFunction)
        // Actual processing work; here, it composes Hello message with name input
        // Returns message to OrchestrationFunction
        [FunctionName("SayHiActivity")]
        public static string SayHi([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        // 1. START HERE! Http call triggers ClientFunction
        // ClientFunction starts new instance of OrchestrationFunction with no seed inputs 
        [FunctionName("HttpStart")]
        public static async Task<IActionResult> ClientStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequest req,
            [DurableClient]IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync("OrchestrationFunction", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}