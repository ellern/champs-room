using ChampsRoom.Models;
using ChampsRoom.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace ChampsRoom.Helpers
{
    public class Stats
    {
        static DataContext db = new DataContext();

        public static int GetRank(Guid leagueId, Guid playerId)
        {
            var ratings = db.Ratings
                .AsNoTracking()
                .OrderByDescending(q => q.Created)
                .Where(q => q.LeagueId == leagueId)
                .ToList();

            if (ratings.Count == 0)
                return 0;

            var league = db.Leagues
                .AsNoTracking()
                .Include(i => i.Players)
                .FirstOrDefault(q => q.Id == leagueId);

            if (league == null)
                return 0;

            var rankings = new List<RankingViewModel>();

            foreach (var player in league.Players.Distinct())
            {
                var userRatings = ratings.Where(q => q.PlayerId == player.Id);
                var latestRating = userRatings.FirstOrDefault();

                var ranking = new RankingViewModel()
                {
                    Draw = userRatings.Count(q => q.Draw == true),
                    Lost = userRatings.Count(q => q.Lost == true),
                    Won = userRatings.Count(q => q.Won == true),
                    Played = userRatings.Count(),
                    Player = player,
                    Rank = latestRating == null ? 0 : latestRating.Rank,
                    Rating = latestRating == null ? 1000 : latestRating.Rate,
                    RatingChange = latestRating == null ? 0 : latestRating.RatingChange,
                    Team = null
                };

                rankings.Add(ranking);
            }

            rankings = rankings
                .OrderByDescending(q => q.Rating)
                .ThenByDescending(q => q.Won)
                .ThenBy(q => q.Played)                
                .ThenBy(q => q.Player.Name)
                .ToList();

            var rank = 1;

            foreach (var item in rankings)
            { 
                if (item.Player.Id == playerId)
                    return rank;

                rank++;
            }

            return 0;
        }
    }
}