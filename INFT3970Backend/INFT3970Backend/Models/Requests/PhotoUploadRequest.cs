namespace INFT3970Backend.Models.Requests
{
    public class PhotoUploadRequest
    {
        public string imgUrl { get; set; }
        public string takenByID { get; set; }
        public string photoOfID { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
       
    }
}
