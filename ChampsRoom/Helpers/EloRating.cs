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

            var result = Convert.ToInt32(Math.Round(calc, MidpointRounding.AwayFromZero));

            if (result == 0)
                result = 1;

            if (result == 120)
                result = 119;

            return result;
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

            if (home.Count == 1 && away.Count > 1)
                avgHome = Convert.ToInt32(ratingHome * 0.70);

            if (away.Count == 1 && home.Count > 1)
                avgAway = Convert.ToInt32(ratingAway * 0.70);

            return EloRating.CalculateChange(avgHome, avgAway, scoreHome, scoreAway);
        }
    }
}