using System;
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
    public static class ProperLondonGreeting
    {
        [FunctionName("OrchestratorFunction")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var outputs = new List<string>();

            outputs.Add(await context.CallActivityAsync<string>("ActivityFunction", "Proper Etiquette"));
            outputs.Add(await context.CallActivityAsync<string>("ActivityFunction", "Your Majesty"));
            outputs.Add(await context.CallActivityAsync<string>("ActivityFunction", "London"));

            log.LogInformation($"COMPLETE: Orchestrator Function and subsequent tasks. Results:");
            foreach (string output in outputs)
            {
                log.LogInformation(output);
            };
            return outputs;
        }

        [FunctionName("ActivityFunction")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Kumusta {name}!";
        }

        [FunctionName("ClientFunction")]
        public static async Task<IActionResult> ClientFunctionStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequest req,
            [DurableClient]IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync("OrchestratorFunction", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}