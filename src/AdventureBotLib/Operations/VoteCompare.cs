namespace AdventureBot.Operations;
public class VoteCompare : IComparer<Tuple<string, int>>
{
    public int Compare(Tuple<string, int> x, Tuple<string, int> y)
    {
        return x.Item2 - y.Item2;
    }
}