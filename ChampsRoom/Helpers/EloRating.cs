using ChampsRoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChampsRoom.Helpers
{
    public static class EloRating
    {
        private static DataContext db = new DataContext();

        public static int CalculateChange(double ratingA, double ratingB, double scoreA, double scoreB)
        {
            var r1 = scoreA > scoreB ? ratingA : ratingB;
            var r2 = scoreA < scoreB ? ratingA : ratingB;

            var calc = 120 * (1 / (1 + Math.Pow(10, (r1-r2) / 400)));

            return Convert.ToInt32(Math.Round(calc, MidpointRounding.AwayFromZero));
        }

        public static int CalculateChange(Guid leagueId, ICollection<string> home, ICollection<string> away, int scoreHome = 1, int scoreAway = 0)
        {
            if (home == null || away == null || home.Count == 0 || away.Count == 0)
                return 0;

            var ratingHome = 0;
            var ratingAway = 0;

            foreach (var item in home)
            {
                var lastRating = db.Ratings.OrderByDescending(q => q.Created).FirstOrDefault(q => q.UserId == item && q.LeagueId == leagueId);
                ratingHome += lastRating == null ? 1000 : lastRating.Rate;
            }

            foreach (var item in away)
            {
                var lastRating = db.Ratings.OrderByDescending(q => q.Created).FirstOrDefault(q => q.UserId == item && q.LeagueId == leagueId);
                ratingAway += lastRating == null ? 1000 : lastRating.Rate;
            }

            var avgHome = ratingHome / home.Count;
            var avgAway = ratingAway / away.Count;

            return EloRating.CalculateChange(avgHome, avgAway, scoreHome, scoreAway);
        }
    }
}