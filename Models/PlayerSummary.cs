namespace CustomerAPI.Models
{
    public class PlayerSummary
    {
        public string uuid { get; set; }
        public string history { get; set; }
        public string player { get; set; }
        public bool isAdmin { get; set; }
        public bool isToxic { get; set; }
        public bool isMVP { get; set; }
        public string createdData { get; set; }


    }
}
