using System;
using System.Net;
using System.Net.Http;
using AdventureBot.Models;
using Microsoft.OpenApi.Models;

namespace AdventureBot.EntityTriggers
{
    public partial class EntityTriggerVotingCounter
    {

        public static class Name
        {
            private const string prefix = nameof(VotingCounter);
            public const string Vote = prefix + nameof(Vote);
        }

    }
}

