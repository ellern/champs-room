using ChampsRoom.Models;

namespace ChampsRoom.ViewModels
{
    public class RatingsViewModel
    {
        public int Rank { get; set; }
        public int RankingChange { get; set; }
        public Team Team { get; set; }
        public User User { get; set; }
        public int Matches { get; set; }
        public int Won { get; set; }
        public int Draw { get; set; }
        public int Lost { get; set; }
        public int Score { get; set; }
        public int Rate { get; set; }
        public int RatingChange { get; set; }
    }
}