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
        switch (ctx.OperationName.ToLowerInvariant())
        {
            case "add":
                var candidateToAdd = ctx.GetInput<GameLoopInput>();
                vote.Add(candidateToAdd);
                ctx.SetState(vote);
                break;
            case "get":
                ctx.Return(vote);
                break;
            case "delete":
                ctx.DeleteState();
                break;
        }
    }
}