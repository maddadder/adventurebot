namespace AdventureBot.Models
{
    public class CheckStatusResponse
    {
        public string id { get; set; }
        public string statusQueryGetUri { get; set; }
        public string sendEventPostUri { get; set; }
        public string terminatePostUri { get; set; }
        public string purgeHistoryDeleteUri { get; set; }
        public string restartPostUri { get; set; }
        public string suspendPostUri { get; set; }
        public string resumePostUri { get; set; }
    }
}
