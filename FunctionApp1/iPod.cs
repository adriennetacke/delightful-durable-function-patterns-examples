using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionsForNdcLondon
{
    public static class iPod
    {
        [FunctionName("Counter")]
        public static void Counter([EntityTrigger] IDurableEntityContext ctx)
        {
            int currentValue = ctx.GetState<int>();
            switch (ctx.OperationName.ToLowerInvariant())
            {
                case "add":
                    int amount = ctx.GetInput<int>();
                    ctx.SetState(currentValue + amount);
                    break;
                case "reset":
                    ctx.SetState(0);
                    break;
                case "get":
                    ctx.Return(currentValue);
                    break;
            }
        }
    }
}