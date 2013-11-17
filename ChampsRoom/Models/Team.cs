using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChampsRoom.Models
{
    public class Team
    {
        public Team()
        {
            this.Id = System.Guid.NewGuid();
            this.Users = new HashSet<User>();
            this.Ratings = new HashSet<Rating>();
            this.Leagues = new HashSet<League>();
            this.AwayMatches = new HashSet<Match>();
            this.HomeMatches = new HashSet<Match>();
        }

        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<League> Leagues { get; set; }
        public ICollection<Match> AwayMatches { get; set; }
        public ICollection<Match> HomeMatches { get; set; }
    }
}