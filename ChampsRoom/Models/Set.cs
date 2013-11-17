using System;

namespace ChampsRoom.Models
{
    public class Set
    {
        public Set()
        {
            this.Id = System.Guid.NewGuid();
            this.AwayScore = 0;
            this.HomeScore = 0;
        }

        public Guid Id { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public Guid MatchId { get; set; }
        public Match Match { get; set; }
    }
}