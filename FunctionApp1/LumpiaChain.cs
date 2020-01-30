using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FunctionsForNdcLondon.Classes;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionsForNdcLondon
{
    public static class LumpiaChain
    {
        [FunctionName("LumpiaAssemblyChain")]
        public static async Task<List<Lumpia>> Run(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            try
            {
                var potentialLumpia = await context.CallActivityAsync<object>("ScoopFillingOntoWrapper", null);
                var rolledLumpia = await context.CallActivityAsync<object>("FoldAndRoll", potentialLumpia);
                var sealedLumpia = await context.CallActivityAsync<object>("AddEggWhiteSeal", rolledLumpia);
                
                return await context.CallActivityAsync<List<Lumpia>>("FryThoseSuckers", sealedLumpia);
            }
            catch (FormatException imperfectRoll)
            {
                var disapprovalFromMom = 
                    new FormatException($"What is this?! Don't you know how to roll these by now? {imperfectRoll.InnerException}");

                throw disapprovalFromMom;
            }
            catch (ArgumentOutOfRangeException tooMuchFilling)
            {
                var structuralIntegrityWarning = 
                    new ArgumentOutOfRangeException($"Warning: This much filling will prevent proper rolling and closure of lumpia. {tooMuchFilling.InnerException}");

                throw structuralIntegrityWarning;
            }
        }
    }
}