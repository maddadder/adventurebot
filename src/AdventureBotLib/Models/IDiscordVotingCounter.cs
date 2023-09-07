namespace AdventureBot.Models;

public interface IDiscordVotingCounter
{
    string PriorVote { get; set; }
    Dictionary<string, int> VoteCount { get; set; }

    Dictionary<string, string> VoterList { get; set; }

    void SetPriorVote(string priorVote);

    void Vote(DiscordLoopInput voter_candidate);
}