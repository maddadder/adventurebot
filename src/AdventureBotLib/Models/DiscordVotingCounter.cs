using Newtonsoft.Json;

namespace AdventureBot.Models;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class DiscordVotingCounter : IDiscordVotingCounter
{
    [JsonProperty("targetChannelId")]
    public string TargetChannelId { get; set; }

    [JsonProperty("voteInstanceId")]
    public string VoteInstanceId { get; set; }

    [JsonProperty("priorVote")]
    public string PriorVote { get; set; }

    [JsonProperty("voteCount")]
    public Dictionary<string, int> VoteCount { get; set; } = new();

    [JsonProperty("voterList")]
    public Dictionary<string, string> VoterList { get; set; } = new();

    public void SetTargetChannelId(string targetChannelId)
    {
        TargetChannelId = targetChannelId;
    }

    public void SetVoteInstanceId(string voteInstanceId)
    {
        VoteInstanceId = voteInstanceId;
    }

    public void SetPriorVote(string priorVote)
    {
        PriorVote = priorVote;
    }
    public void Vote(DiscordLoopInput voter_candidate)
    {
        string voter = voter_candidate.SubscriberId;
        string candidate = voter_candidate.GameState;
        if (!VoteCount.ContainsKey(candidate)) {
            VoteCount[candidate] = 0;
        }
        if (VoterList.ContainsKey(voter)) 
        {
            var oldCandidate = VoterList[voter];
            VoterList[voter] = candidate;
            VoteCount[oldCandidate]--;
            VoteCount[candidate]++;
        }
        else
        {
            VoterList[voter] = candidate;
            VoteCount[candidate]++;
        }
    }
}