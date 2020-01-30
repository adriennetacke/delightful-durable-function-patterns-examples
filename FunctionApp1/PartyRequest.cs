using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using System.Threading;

namespace FunctionsForNdcLondon
{
    public static class PartyRequest
    {
        [FunctionName("RunOrchestration")]
        public static async Task<List<string>> Run(
            [OrchestrationTrigger]IDurableOrchestrationContext context,
            ILogger log)
        {
            try
            {
                var answers = new List<string>();

                answers.Add(await context.CallActivityAsync<string>("AskMom", null));
                context.SetCustomStatus("Well, first time didn't work. Let's try again.");

                answers.Add(await context.CallActivityAsync<string>("AskMom", ", pretty please?"));
                context.SetCustomStatus("Not looking good. Maybe one more?");

                answers.Add(await context.CallActivityAsync<string>("AskMom", ", this won't affect my grades, I promise!"));
                context.SetCustomStatus("She's getting agitated. ABORT!");

                return answers;
            }
            catch (Exception)
            {
               throw;
            }
        }

        [FunctionName("AskMom")]
        public static string Ask([ActivityTrigger] string incentive, ILogger log)
        {
            log.LogInformation($"Asking mom now: Mom, can I go to this party{incentive ?? "?!"}");
            Thread.Sleep(15000); // simulate longer processing delay

            return "No";
        }

        [FunctionName("HttpPartyRequest")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")]HttpRequestMessage req,
            [DurableClient]IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync("RunOrchestration", null);

            log.LogInformation($"*GULP* Asking mom if we can go to this party, ID = {instanceId}");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
