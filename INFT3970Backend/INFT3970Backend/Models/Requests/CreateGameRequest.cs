namespace INFT3970Backend.Models.Requests
{
    public class CreateGameRequest
    {
        public string nickname { get; set; }
        public string contact { get; set; }
        public string imgUrl { get; set; }
        public int timeLimit { get; set; }
        public int ammoLimit { get; set; }
        public int startDelay { get; set; }
        public int replenishAmmoDelay { get; set; }
        public string gameMode { get; set; }
        public bool isJoinableAtAnytime { get; set; }

        //Game settings for the battle royal game mode
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Radius { get; set; }
    }
}
