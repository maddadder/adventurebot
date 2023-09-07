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
    public class GetDiscordLoopOrchestrationStatus
    {
        [FunctionName(nameof(GetDiscordLoopOrchestrationStatus))]
        public async Task<DiscordVotingCounter> GetStatus(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var InstanceId = context.GetInput<string>();

            var entityId = new EntityId(EntityTriggerDiscordVotingCounter.Name.Vote, $"{EntityTriggerDiscordVotingCounter.Name.Vote},{InstanceId}");

            DiscordVotingCounter votingCounter = await context.CallEntityAsync<DiscordVotingCounter>(entityId, DiscordVotingCounterOperationNames.DiscordGet);
            return votingCounter;
        }
    }
}