using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FunctionsForNdcLondon.Classes;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace FunctionsForNdcLondon
{
    public static class LumpiaFanOutFanIn
    {
        [FunctionName("TitasFanOutFanIn")]
        public static async Task ParallelLumpiaFunction([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var titas = new List<Task<int>>();

            object[] workBatch = await context.CallActivityAsync<object[]>("MoreLumpia", null);
            for (int i = 0; i < workBatch.Length; i++)
            {
                Task<int> task = context.CallActivityAsync<int>("MakeLumpia", workBatch[i]);
                titas.Add(task);
            }

            // Wait for all Titas to finish making Lumpia!
            await Task.WhenAll(titas);

            int allTheLumpias = titas.Sum(t => t.Result);
            await context.CallActivityAsync("FryThoseSuckers", allTheLumpias);
        }
    }
}