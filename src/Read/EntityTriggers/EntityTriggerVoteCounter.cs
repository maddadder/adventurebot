using System;
using AdventureBot.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AdventureBot.EntityTriggers;

public partial class EntityTriggerVotingCounter
{
    [FunctionName(Name.Vote)]
    public void Vote([EntityTrigger] IDurableEntityContext ctx)
    {
        var vote = ctx.GetState<VotingCounter>() ?? new VotingCounter();
        switch (ctx.OperationName)
        {
            case VotingCounterOperationNames.SetPriorVote:
                var priorVote = ctx.GetInput<string>();
                vote.SetPriorVote(priorVote);
                ctx.SetState(vote);
                break;
            case VotingCounterOperationNames.Vote:
                var candidateToAdd = ctx.GetInput<GameLoopInput>();
                vote.Vote(candidateToAdd);
                ctx.SetState(vote);
                break;
            case VotingCounterOperationNames.Get:
                ctx.Return(vote);
                break;
            case VotingCounterOperationNames.Delete:
                ctx.DeleteState();
                break;
        }
    }
}