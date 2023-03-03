using AdventureBot.ActivityFunctions;
using AdventureBot.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using AdventureBot.EntityTriggers;

namespace AdventureBot.Orchestrators
{
    public class GetGameLoopOrchestrationStatus
    {
        [FunctionName(nameof(GetGameLoopOrchestrationStatus))]
        public async Task<VotingCounter> GetStatus(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var InstanceId = context.GetInput<string>();

            var entityId = new EntityId(EntityTriggerVotingCounter.Name.Vote, $"{EntityTriggerVotingCounter.Name.Vote},{InstanceId}");

            VotingCounter votingCounter = await context.CallEntityAsync<VotingCounter>(entityId, VotingCounterOperationNames.Get);
            return votingCounter;
        }
    }
}