using System;
using System.Collections.Generic;

namespace ChampsRoom.Models
{
    public class League
    {
        public League()
        {
            this.Id = System.Guid.NewGuid();
            this.Sets = 7;
            this.SetsNeededToWin = 4;
            this.MaxScore = 9999;
            this.MinScore = 0;

            this.Matches = new HashSet<Match>();
            this.Users = new HashSet<User>();
            this.Teams = new HashSet<Team>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int Sets { get; set; }
        public int SetsNeededToWin { get; set; }
        public int MaxScore { get; set; }
        public int MinScore { get; set; }
        public string Rules { get; set; }

        public ICollection<Match> Matches { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<Team> Teams { get; set; }
    }

}