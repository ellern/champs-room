using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChampsRoom.Models 
{
    public class RankingViewModel
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

    public class LeagueDetailsViewModel
    {
        public League League { get; set; }
        public ICollection<RankingViewModel> Rankings { get; set; }
        public ICollection<Match> LatestMatches { get; set; }
    }

    public class LeagueMatchesViewModel
    {
        public League League { get; set; }
        public User User { get; set; }
        public ICollection<Match> Matches { get; set; }
    }

    public class OverviewTeamUsersViewModel
    {
        public League League { get; set; }
        public ICollection<Team> Teams { get; set; }
        public ICollection<User> Users { get; set; }
    }

    public class MatchCreateViewModel
    {
        public League League { get; set; }
        public Match Match { get; set; }
        public ICollection<User> Users { get; set; }
    }
}