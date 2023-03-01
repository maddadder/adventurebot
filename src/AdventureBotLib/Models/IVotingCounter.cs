namespace AdventureBot.Models;

public interface IVotingCounter
{
    Dictionary<string, int> VoteCount { get; set; }

    Dictionary<string, string> VoterList { get; set; }

    void Add(GameLoopInput voter_candidate);

    Dictionary<string, int> Get();
}