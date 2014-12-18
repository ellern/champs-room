using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChampsRoom.Models
{
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

        public int GetHomeScore()
        {
            return this.Sets == null ? 0 : this.Sets.Sum(item => item.HomeScore);
        }

        public int GetAwayScore()
        {
            return this.Sets == null ? 0 : this.Sets.Sum(item => item.AwayScore);
        }

        public int GetScore(User user)
        {
            if (this.HomeUsers == null)
                return 0;

            if (this.HomeUsers.Contains(user))
                return GetHomeScore();

            if (this.AwayUsers == null)
                return 0;

            if (this.AwayUsers.Contains(user))
                return GetAwayScore();

            return 0;
        }

        public int GetScoreDiff()
        {
            return this.GetHomeScore() - this.GetAwayScore();
        }

        public int GetOpponentScore(User user)
        {
            if (this.HomeUsers == null)
                return 0;

            if (this.HomeUsers.Contains(user))
                return GetAwayScore();

            if (this.AwayUsers == null)
                return 0;

            if (this.AwayUsers.Contains(user))
                return GetHomeScore();

            return 0;
        }

        public int GetHomeSetScore()
        {
            return this.Sets == null ? 0 : this.Sets.Count(item => item.HomeScore > item.AwayScore);
        }

        public int GetAwaySetScore()
        {
            return this.Sets == null ? 0 : this.Sets.Count(item => item.AwayScore > item.HomeScore);
        }

        public HtmlString GetSetScoresTooltip()
        {
            var result = "Scores: <br />";
            var count = 1;

            foreach (var item in this.Sets.OrderByDescending(q => q.Id))
                result += String.Format("<em>#{2}</em>: {0} - {1}<br />", item.HomeScore, item.AwayScore, count++);

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

        public IQueryable<User> Opponents(User user)
        {
            if (this.HomeUsers.Contains(user))
                return this.AwayUsers.AsQueryable();

            if (this.AwayUsers.Contains(user))
                return this.HomeUsers.AsQueryable();

            return new List<User>().AsQueryable();
        }

        public IQueryable<User> Teammates(User user)
        {
            if (this.HomeUsers.Contains(user))
                return this.HomeUsers.AsQueryable();

            if (this.AwayUsers.Contains(user))
                return this.AwayUsers.AsQueryable();

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

        public bool UserLost(User user)
        {
            if (this.Draw)
                return false;

            if (this.HomeUsers.Contains(user) && this.AwayWon)
                return true;

            if (this.AwayUsers.Contains(user) && this.HomeWon)
                return true;

            return false;
        }

        public bool IsHome(User user)
        {
            if (this.HomeUsers.Contains(user))
                return true;

            return false;
        }

        public bool IsAway(User user)
        {
            if (this.AwayUsers.Contains(user))
                return true;

            return false;
        }
    }
}