using ChampsRoom.Models;
using ChampsRoom.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ChampsRoom.Helpers
{
    public class Statistics
    {
        static readonly DataContext db = new DataContext();

        public static int GetRank(Guid leagueId, string userId)
        {
            var ratings = db.Ratings
                .OrderByDescending(q => q.Created)
                .Where(q => q.LeagueId == leagueId)
                .ToList();

            if (ratings.Count == 0)
                return 0;

            var league = db.Leagues
                .Include(i => i.Users)
                .FirstOrDefault(q => q.Id == leagueId);

            if (league == null)
                return 0;

            var rankings = new List<RatingsViewModel>();

            foreach (var item in league.Users.Distinct())
            {
                var userRatings = ratings.Where(q => q.UserId == item.Id);
                var latestRating = userRatings.FirstOrDefault();

                var ranking = new RatingsViewModel()
                {
                    Draw = userRatings.Count(q => q.Draw == true),
                    Lost = userRatings.Count(q => q.Lost == true),
                    Won = userRatings.Count(q => q.Won == true),
                    Matches = userRatings.Count(),
                    User = item,
                    Rank = latestRating == null ? 0 : latestRating.Rank,
                    Rate = latestRating == null ? 1000 : latestRating.Rate,
                    RatingChange = latestRating == null ? 0 : latestRating.RatingChange,
                    Team = null
                };

                rankings.Add(ranking);
            }

            rankings = rankings
                .OrderByDescending(q => q.Rate)
                .ThenByDescending(q => q.Won)
                .ThenByDescending(q => q.Draw)
                .ThenBy(q => q.Matches)
                .ThenBy(q => q.User.UserName)
                .ToList();

            var rank = 1;

            foreach (var item in rankings)
            {
                if (item.User.Id == userId)
                    return rank;

                rank++;
            }

            return league.Users.Distinct().Count();
        }
    }
}