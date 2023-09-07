using System;
using AdventureBot.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AdventureBot.EntityTriggers;

public partial class EntityTriggerDiscordVotingCounter
{
    [FunctionName(Name.Vote)]
    public void Vote([EntityTrigger] IDurableEntityContext ctx)
    {
        var vote = ctx.GetState<DiscordVotingCounter>() ?? new DiscordVotingCounter();
        switch (ctx.OperationName)
        {
            case DiscordVotingCounterOperationNames.SetPriorVote:
                var priorVote = ctx.GetInput<string>();
                vote.SetPriorVote(priorVote);
                ctx.SetState(vote);
                break;
            case DiscordVotingCounterOperationNames.Vote:
                var candidateToAdd = ctx.GetInput<DiscordLoopInput>();
                vote.Vote(candidateToAdd);
                ctx.SetState(vote);
                break;
            case DiscordVotingCounterOperationNames.Get:
                ctx.Return(vote);
                break;
            case DiscordVotingCounterOperationNames.Delete:
                ctx.DeleteState();
                break;
        }
    }
}