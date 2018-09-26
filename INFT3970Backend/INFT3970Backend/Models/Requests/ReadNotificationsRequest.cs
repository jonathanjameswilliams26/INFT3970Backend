namespace INFT3970Backend.Models
{
    public class ReadNotificationsRequest
    {
        public string PlayerID { get; set; }
        public string[] NotificationArray { get; set; }
    }
}
