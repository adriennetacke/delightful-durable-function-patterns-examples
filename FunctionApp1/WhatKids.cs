using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace FunctionsForNdcLondon
{
    public static class WhatKids
    {
        [FunctionName("WhatKids")]
        public static async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            string granddaughter = "Adrienne";

            // since being married
            await context.CallActivityAsync<string>("AskAboutKids", granddaughter);

            var quarterlyAsk = context.CurrentUtcDateTime.AddMonths(3);

            while (true)
            {
                var operationHasTimedOut = context.CurrentUtcDateTime > quarterlyAsk;

                if (operationHasTimedOut)
                {
                    context.SetCustomStatus("So...where's my grandbabies?");
                    break;
                }

                var isWithBunInOven = await context.CallActivityAsync<bool>("CheckForBabies", granddaughter);

                if (isWithBunInOven)
                {
                    context.SetCustomStatus("Finally! Grandkids!");
                    break;
                }

                // If not that time, or just recently ask, wait a bit
                var nextCheckTime = context.CurrentUtcDateTime.AddDays(15);
                await context.CreateTimer(nextCheckTime, CancellationToken.None);
            }
        }
    }
}