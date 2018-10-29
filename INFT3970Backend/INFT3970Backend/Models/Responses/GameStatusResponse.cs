namespace INFT3970Backend.Models.Responses
{
    public class GameStatusResponse
    {
        public bool HasVotesToComplete { get; set; }
        public bool HasNotifications { get; set; }
        public Player Player { get; set; }


        public GameStatusResponse(bool hasVotesToComplete, bool hasNotifications, Player player)
        {
            HasVotesToComplete = hasVotesToComplete;
            HasNotifications = hasNotifications;
            Player = player;
        }

        public void Compress()
        {
            if (Player != null)
                Player.Compress(true, true, true);
        }
    }
}
