using Newtonsoft.Json;

namespace AdventureBot.Models;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class VotingCounter : IVotingCounter
{
    [JsonProperty("priorVote")]
    public string PriorVote { get; set; }

    [JsonProperty("voteCount")]
    public Dictionary<string, int> VoteCount { get; set; } = new();

    [JsonProperty("voterList")]
    public Dictionary<string, string> VoterList { get; set; } = new();

    public void SetPriorVote(string priorVote)
    {
        PriorVote = priorVote;
    }
    public void Vote(GameLoopInput voter_candidate)
    {
        string voter = voter_candidate.Subscriber;
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