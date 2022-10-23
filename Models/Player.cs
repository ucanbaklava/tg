namespace CustomerAPI.Models
{
    public class Player
    {
        public string player { get; set; }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int ping { get; set; }
        public string uuid { get; set; }
        public bool isadmin { get; set; }
        public bool ismvp { get; set; }
        public bool istoxic { get; set; }
        public DateTime createdAt { get; set; }

    }
}
