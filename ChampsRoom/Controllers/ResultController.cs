using ChampsRoom.Helpers;
using ChampsRoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ChampsRoom.Controllers
{
    [RoutePrefix("results")]
    public class ResultController : Controller
    {
        private DataContext db = new DataContext();

        [Route("~/leagues/{leagueUrl}/{playerUrl}/results")]
        public async Task<ActionResult> Details(string leagueUrl, string playerUrl)
        {
            var player = await db.Players
                .Include(i => i.AwayMatches.Select(y => y.Sets))
                .Include(i => i.AwayMatches.Select(y => y.Ratings))
                .Include(i => i.AwayMatches.Select(y => y.AwayPlayers))
                .Include(i => i.AwayMatches.Select(y => y.HomePlayers))
                .Include(i => i.HomeMatches.Select(y => y.Sets))
                .Include(i => i.HomeMatches.Select(y => y.Ratings))
                .Include(i => i.HomeMatches.Select(y => y.AwayPlayers))
                .Include(i => i.HomeMatches.Select(y => y.HomePlayers))
                .FirstOrDefaultAsync(q => q.Url.Equals(playerUrl, StringComparison.InvariantCultureIgnoreCase));
                               
            if (player == null)
                return HttpNotFound();

            var league = await db.Leagues.FirstOrDefaultAsync(q => q.Url.Equals(leagueUrl, StringComparison.InvariantCultureIgnoreCase));

            var viewmodel = new LeagueMatchesViewModel()
            {
                League = league,
                Matches = player.Matches.Where(q => q.LeagueId == league.Id).OrderByDescending(q => q.Created).ToList(),
                Player = player
            };

            return View(viewmodel);
        }

        [Route("~/leagues/{leagueUrl}/result")]
        public async Task<ActionResult> Create(string leagueUrl)
        {
            var leagues = await db.Leagues.Include(q => q.Players).FirstOrDefaultAsync(q => q.Url.Equals(leagueUrl, StringComparison.InvariantCultureIgnoreCase));

            if (leagues == null)
                return HttpNotFound();

            var viewmodel = new MatchCreateViewModel()
            {
                Match = new Match(),
                League = leagues,
                Players = db.Players.OrderBy(q => q.Name).ToList()

            };

            return View(viewmodel);
        }

        [HttpPost]
        [Route("~/leagues/{leagueUrl}/result")]
        public async Task<ActionResult> Create(Guid leagueId, List<Guid> home, List<Guid> away, List<int?> homeScore, List<int?> awayScore)
        {
            var league = await db.Leagues
                .Include(i => i.Players)
                .Include(i => i.Teams.Select(u => u.Players))
                .FirstOrDefaultAsync(q => q.Id == leagueId);

            if (league == null)
                return HttpNotFound();

            #region Find Players and Teams

            var homePlayers = db.Players.Include(i => i.Teams).Where(q => home.Contains(q.Id));
            var awayPlayers = db.Players.Include(i => i.Teams).Where(q => away.Contains(q.Id));
            var allTeams = db.Teams.Include(i => i.Players);

            var homeTeam = allTeams.Where(p => !p.Players.Select(c => c.Id).Except(home).Union(home.Except(p.Players.Select(c => c.Id))).Any()).FirstOrDefault();
            var awayTeam = allTeams.Where(p => !p.Players.Select(c => c.Id).Except(away).Union(away.Except(p.Players.Select(c => c.Id))).Any()).FirstOrDefault();

            if (homeTeam == null)
            {
                homeTeam = new Team();

                foreach (var player in db.Players.Where(q => home.Contains(q.Id)))
                    homeTeam.Players.Add(player);

                homeTeam.Name = String.Join("+", homeTeam.Players.Select(q => q.Name));
                homeTeam.Url = homeTeam.Name.ToFriendlyUrl();

                db.Teams.Add(homeTeam);
            }

            if (awayTeam == null)
            {
                awayTeam = new Team();

                foreach (var player in db.Players.Where(q => away.Contains(q.Id)))
                    awayTeam.Players.Add(player);

                awayTeam.Name = String.Join("+", awayTeam.Players.Select(q => q.Name));
                awayTeam.Url = awayTeam.Name.ToFriendlyUrl();

                db.Teams.Add(awayTeam);
            }

            foreach (var player in awayTeam.Players)
                if (!league.Players.Contains(player))
                    league.Players.Add(player);

            foreach (var player in homeTeam.Players)
                if (!league.Players.Contains(player))
                    league.Players.Add(player);

            if (!league.Teams.Contains(awayTeam))
                league.Teams.Add(awayTeam);

            if (!league.Teams.Contains(homeTeam))
                league.Teams.Add(homeTeam);

            #endregion

            #region Sets

            var sets = new List<Set>();

            for (int i = 0; i < homeScore.Count; i++)
                if (homeScore[i].HasValue && awayScore[i].HasValue)
                    sets.Add(new Set() { HomeScore = homeScore[i].Value, AwayScore = awayScore[i].Value });

            var homeWins = sets.Count(q => q.HomeScore > q.AwayScore);
            var awayWins = sets.Count(q => q.HomeScore < q.AwayScore);

            #endregion

            #region Ratings

            var ratingHome = new List<Tuple<Guid, int, int, int, int>>();
            var ratingAway = new List<Tuple<Guid, int, int, int, int>>();

            foreach (var item in homeTeam.Players)
            {
                var latestRating = db.Ratings.OrderByDescending(q => q.Created).FirstOrDefault(q => q.League.Id == leagueId && q.Player.Id == item.Id);
                var rating = latestRating == null ? 1000 : latestRating.Rate;
                var rank = latestRating == null ? league.Players.Count : latestRating.Rank;
                ratingHome.Add(new Tuple<Guid, int, int, int, int>(item.Id, rating, rank, 0, 0));
            }

            foreach (var item in awayTeam.Players)
            {
                var latestRating = db.Ratings.OrderByDescending(q => q.Created).FirstOrDefault(q => q.League.Id == leagueId && q.Player.Id == item.Id);
                var rating = latestRating == null ? 1000 : latestRating.Rate;
                var rank = latestRating == null ? league.Players.Count : latestRating.Rank;
                ratingAway.Add(new Tuple<Guid, int, int, int, int>(item.Id, rating, rank, 0, 0));
            }

            var avgHome = ratingHome.Sum(q => q.Item2) / homeTeam.Players.Count;
            var avgAway = ratingAway.Sum(q => q.Item2) / awayTeam.Players.Count;
            var homeWon = homeWins > awayWins;
            var awayWon = awayWins > homeWins;
            var draw = homeWins == awayWins;
            var elo = new EloRating(avgHome, avgAway, homeWins, awayWins);
            var ratings = new List<Rating>();

            foreach (var player in homeTeam.Players)
                ratings.Add(new Rating()
                {
                    Draw = draw,
                    Lost = awayWon,
                    Won = homeWon,
                    League = league,
                    Player = player,
                    Team = homeTeam,
                    Rank = 0,
                    RankingChange = 0,
                    RankedLast = false,
                    Rate = Convert.ToInt32(elo.FinalResult1),
                    RatingChange = Convert.ToInt32(elo.FinalResult1) - avgHome
                });

            foreach (var player in awayTeam.Players)
                ratings.Add(new Rating()
                {
                    Draw = draw,
                    Lost = homeWon,
                    Won = awayWon,
                    League = league,
                    Player = player,
                    Team = awayTeam,
                    Rank = 0,
                    RankingChange = 0,
                    RankedLast = false,
                    Rate = Convert.ToInt32(elo.FinalResult2),
                    RatingChange = Convert.ToInt32(elo.FinalResult2) - avgAway
                });

            #endregion

            var match = new Match()
            {
                AwayPlayers = awayTeam.Players,
                AwayTeam = awayTeam,
                AwayWon = awayWon,
                Draw = draw,
                HomePlayers = homeTeam.Players,
                HomeTeam = homeTeam,
                HomeWon = homeWon,
                League = league,
                Ratings = ratings,
                Sets = sets
            };

            db.Matches.Add(match);

            await db.SaveChangesAsync();

            #region Update rank

            foreach (var player in homeTeam.Players)
            {
                var rank = Stats.GetRank(league.Id, player.Id);
                var rating = ratings.FirstOrDefault(q => q.PlayerId == player.Id);
                rating.Rank = rank;
                rating.RankingChange = ratingHome.FirstOrDefault(q => q.Item1 == player.Id).Item3 - rank;

                db.Entry(rating).State = EntityState.Modified;
            }

            foreach (var player in awayTeam.Players)
            {
                var rank = Stats.GetRank(league.Id, player.Id);
                var rating = ratings.FirstOrDefault(q => q.PlayerId == player.Id);
                rating.Rank = rank;
                rating.RankingChange = ratingAway.FirstOrDefault(q => q.Item1 == player.Id).Item3 - rank;

                db.Entry(rating).State = EntityState.Modified;
            }

            #endregion

            await db.SaveChangesAsync();

            return RedirectToAction("Details", "League", new { id = league.Url });
        }
	}
}