using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

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
        public string Url { get; set; }
        public int Sets { get; set; }
        public int SetsNeededToWin { get; set; }
        public int MaxScore { get; set; }
        public int MinScore { get; set; }
        public string Rules { get; set; }

        public ICollection<Match> Matches { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<Team> Teams { get; set; }
    }

    public class Match
    {
        public Match()
        {
            this.Id = System.Guid.NewGuid();
            this.Created = DateTime.Now;

            this.HomeUsers = new HashSet<User>();
            this.AwayUsers = new HashSet<User>();
            this.Ratings = new HashSet<Rating>();
            this.Sets = new HashSet<Set>();
        }

        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public bool Draw { get; set; }
        public bool HomeWon { get; set; }
        public bool AwayWon { get; set; }
        public Guid LeagueId { get; set; }
        public League League { get; set; }
        public Guid HomeTeamId { get; set; }
        public Team HomeTeam { get; set; }
        public Guid AwayTeamId { get; set; }
        public Team AwayTeam { get; set; }
        public ICollection<User> HomeUsers { get; set; }
        public ICollection<User> AwayUsers { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Set> Sets { get; set; }

        public int GetHomeSetScore()
        {
            var score = 0;

            if (this.Sets == null)
                return score;

            foreach (var item in this.Sets)
                if (item.HomeScore > item.AwayScore)
                    score++;

            return score;
        }

        public int GetAwaySetScore()
        {
            var score = 0;

            if (this.Sets == null)
                return score;

            foreach (var item in this.Sets)
                if (item.AwayScore > item.HomeScore)
                    score++;

            return score;
        }

        public HtmlString GetSetScoresTooltip()
        {
            var result = "Scores: <br />";
            var count = 1;

            foreach (var item in this.Sets)
                result += String.Format("{0} - {1}<br />", item.HomeScore, item.AwayScore, count++);

            return new HtmlString(result.Substring(0, result.Length - 6));
        }

        public string GetOpponent(User user)
        {
            var result = String.Empty;

            if (this.HomeUsers.Contains(user))
                return String.Join(", ", this.AwayUsers.Select(x => x.UserName));

            if (this.AwayUsers.Contains(user))
                return String.Join(", ", this.HomeUsers.Select(x => x.UserName));

            return result;
        }

        public IQueryable<User> Opponent(User user)
        {
            if (this.HomeUsers.Contains(user))
                return this.AwayUsers.AsQueryable();

            if (this.AwayUsers.Contains(user))
                return this.HomeUsers.AsQueryable();

            return new List<User>().AsQueryable();
        }

        public Rating GetRating(User user)
        {
            var rating = new Rating() { Rank = 0, RankingChange = 0, Rate = 0, RatingChange = 0 };

            if (this.Ratings == null)
                return rating;

            var result = this.Ratings.FirstOrDefault(q => q.UserId == user.Id);

            if (result == null)
                return rating;

            return result;
        }

        public bool UserWon(User user)
        {
            if (this.Draw)
                return false;

            if (this.HomeUsers.Contains(user) && this.HomeWon)
                return true;

            if (this.AwayUsers.Contains(user) && this.AwayWon)
                return true;

            return false;
        }
    }

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
        public string Url { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<League> Leagues { get; set; }
        public ICollection<Match> AwayMatches { get; set; }
        public ICollection<Match> HomeMatches { get; set; }
    }
}