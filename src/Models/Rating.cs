using System;

namespace ChampsRoom.Models
{
    public class Rating
    {
        public Rating()
        {
            this.Id = System.Guid.NewGuid();
            this.Created = DateTime.Now;
        }

        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public bool Won { get; set; }
        public bool Lost { get; set; }
        public bool Draw { get; set; }
        public int Rate { get; set; }
        public int RatingChange { get; set; }
        public int Rank { get; set; }
        public int RankingChange { get; set; }
        public int Score { get; set; }
        public bool RankedLast { get; set; }
        public Guid LeagueId { get; set; }
        public League League { get; set; }
        public Guid MatchId { get; set; }
        public Match Match { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public Guid TeamId { get; set; }
        public Team Team { get; set; }
    }
}