using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdventureBot.Models;
using AdventureBot.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AdventureBot.ActivityFunctions
{
    public class TallyVoteActivity
    {

        public TallyVoteActivity()
        {

        }

        [FunctionName(nameof(TallyVoteActivity))]
        public async Task<string> Run(          
            [ActivityTrigger] Dictionary<string, int> votes, ILogger log)
        {
            var list =  from entry in votes 
                        orderby entry.Value descending 
                        select entry;
            
            if(!list.Any())
            {
                log.LogWarning("There are not votes to tally. This should not happen.");
                return await Task.FromResult<string>(null);
            }

            var winner = list.First();

            var hasRunnerUp = list.Skip(1).Any();
            
            if (hasRunnerUp)
            {
                var runnerUp = list.Skip(1).First();
                if(winner.Value == runnerUp.Value)
                {
                    log.LogInformation("The top two are tied, there is no consensus.");
                    return await Task.FromResult<string>(null);
                }
                else
                {
                    log.LogInformation($"found winner: {winner.Key}");
                    return await Task.FromResult<string>(winner.Key);
                }
            }
            else
            {
                log.LogInformation($"found winner by default: {winner.Key}");
                return await Task.FromResult<string>(winner.Key);
            }
        }
    }
}