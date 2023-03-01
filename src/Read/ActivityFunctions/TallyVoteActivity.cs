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
using AdventureBot.Operations;

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
            List<Tuple<string, int>> list = new List<Tuple<string, int>>();
            foreach(var entry in votes){
                list.Add(new Tuple<string, int>(entry.Key, entry.Value));
            }
            
            if(!list.Any())
            {
                log.LogWarning("There are not votes to tally. This should not happen.");
                return await Task.FromResult<string>(null);
            }
            //VoteCompare uses a - b to compare
            list.Sort(new VoteCompare());

            var winner = list.First();

            var runnerUp = list.Skip(1).FirstOrDefault();
            if (runnerUp != null && winner.Item2 == runnerUp.Item2)
            {
                log.LogInformation("The top two are tied, there is no consensus.");
                return await Task.FromResult<string>(null);
            }
            log.LogInformation($"found winner:{winner.Item1}");
            return await Task.FromResult<string>(winner.Item1);
        }

    }
}