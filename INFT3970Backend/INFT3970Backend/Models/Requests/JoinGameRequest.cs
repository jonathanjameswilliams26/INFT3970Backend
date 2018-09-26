namespace INFT3970Backend.Models.Requests
{
    public class JoinGameRequest
    {
        public string gameCode { get; set; }
        public string nickname { get; set; }
        public string contact { get; set; }
        public string imgUrl { get; set; }
    }
}
