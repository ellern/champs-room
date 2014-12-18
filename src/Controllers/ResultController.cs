using ChampsRoom.Helpers;
using ChampsRoom.Models;
using ChampsRoom.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChampsRoom.Controllers
{
    [Authorize]
    [RoutePrefix("results")]
    public class ResultController : Controller
    {
        private DataContext db = new DataContext();

        [AllowAnonymous]
        [Route("~/leagues/{slug}/{slugUser}/results")]
        public async Task<ActionResult> Details(string slug, string slugUser)
        {
            var user = await db.Users
                .Include(i => i.AwayMatches.Select(y => y.Sets))
                .Include(i => i.AwayMatches.Select(y => y.Ratings))
                .Include(i => i.AwayMatches.Select(y => y.AwayUsers))
                .Include(i => i.AwayMatches.Select(y => y.HomeUsers))
                .Include(i => i.HomeMatches.Select(y => y.Sets))
                .Include(i => i.HomeMatches.Select(y => y.Ratings))
                .Include(i => i.HomeMatches.Select(y => y.AwayUsers))
                .Include(i => i.HomeMatches.Select(y => y.HomeUsers))
                .FirstOrDefaultAsync(q => q.Slug.Equals(slugUser, StringComparison.InvariantCultureIgnoreCase));

            if (user == null)
                return HttpNotFound();

            var league = await db.Leagues
                .Include(i => i.Users)
                .FirstOrDefaultAsync(q => q.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase));

            var matches = user.Matches.Where(q => q.LeagueId == league.Id).OrderByDescending(q => q.Created).ToList();

            var viewmodel = new ResultDetailsViewModel()
            {
                League = league,
                Matches = matches,
                Statistics = buildStatistics(user, matches),
                User = user
            };

            var maxRank = league.Users.Count + 1;
            var chartRanking = "";
            var chartRating = "";
            var counter = 1;

            foreach (var item in viewmodel.Matches.Take(50).Reverse())
            {
                var rank = System.Math.Abs(item.GetRating(viewmodel.User).Rank - maxRank);
                var rating = item.GetRating(viewmodel.User).Rate;

                chartRanking += String.Format("[{0},{1}],", counter, rank);
                chartRating += String.Format("[{0},{1}],", counter++, rating);
            }

            ViewBag.ChartRanking = chartRanking.Trim(',');
            ViewBag.ChartRating = chartRating.Trim(',');
            ViewBag.ChartRatingMax = user.Ratings.Max(q => q.Rate) + 100;
            ViewBag.ChartRatingMin = Math.Round(user.Ratings.Min(q => q.Rate) / 2m, MidpointRounding.AwayFromZero);

            return View(viewmodel);
        }

        private ResultStatisticsViewModel buildStatistics(User user, IList<Match> matches)
        {
            #region Eggs

            var eggsGiven = 0;
            var eggsConceded = 0;

            foreach (var match in matches.SelectMany(q => q.Sets.Where(x => x.HomeScore == 0 || x.AwayScore == 0)).ToList())
            {
                if (match.Match.IsHome(user))
                {
                    if (match.HomeScore == 0)
                        eggsConceded++;
                    else if (match.AwayScore == 0)
                        eggsGiven++;
                }
                else
                {
                    if (match.AwayScore == 0)
                        eggsConceded++;
                    else if (match.HomeScore == 0)
                        eggsGiven++;
                }
            }

            #endregion

            #region Best/worst streaks

            var winningStreak = 0;
            var bestWinningStreak = 0;
            var loosingStreak = 0;
            var worstLoosingStreak = 0;

            foreach (var match in matches.OrderBy(x => x.Created))
            {
                if (match.UserWon(user))
                {
                    loosingStreak = 0;
                    winningStreak++;
                }
                else if (match.UserLost(user))
                {
                    loosingStreak++;
                    winningStreak = 0;
                }

                if (winningStreak > bestWinningStreak)
                    bestWinningStreak = winningStreak;

                if (loosingStreak > worstLoosingStreak)
                    worstLoosingStreak = loosingStreak;
            }

            #endregion

            #region Best/worst matches

            var bestWin = (Match)null;
            var worstLost = (Match)null;

            foreach (var match in matches.Where(x => x.UserWon(user)).ToList())
            {
                if (bestWin == null)
                    bestWin = match;

                var score = match.GetScoreDiff();
                var bestScore = bestWin.IsHome(user) ? bestWin.GetScoreDiff() : Math.Abs(bestWin.GetScoreDiff());

                if (score > bestScore)
                    bestWin = match;
            }

            foreach (var match in matches.Where(x => x.UserLost(user)).ToList())
            {
                if (worstLost == null)
                    worstLost = match;

                var score = match.GetScoreDiff();
                var worstScore = worstLost.IsAway(user) ? worstLost.GetScoreDiff() : -worstLost.GetScoreDiff();

                if (score < worstScore)
                    worstLost = match;
            }

            #endregion

            var goalsConceded = matches.Sum(x => x.GetOpponentScore(user));
            var goalsScored = matches.Sum(x => x.GetScore(user));

            var result = new ResultStatisticsViewModel()
            {
                WinningStreak = winningStreak,
                BestWinningStreak = bestWinningStreak,
                LoosingStreak = loosingStreak,
                WorstLoosingStreak = worstLoosingStreak,

                BestWin = bestWin,
                WorstLost = worstLost,

                EggsConceded = eggsConceded,
                EggsGiven = eggsGiven,
                GoalsConceded = goalsConceded,
                GoalsConcededPerMatch = goalsConceded / matches.Count,
                GoalsScored = goalsScored,
                GoalsScoredPerMatch = goalsScored / matches.Count,
            };

            return result;
        }

        [Route("~/leagues/{slug}/result")]
        public async Task<ActionResult> Create(string slug)
        {
            var league = await db.Leagues.Include(q => q.Users).FirstOrDefaultAsync(q => q.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase));

            if (league == null)
                return HttpNotFound();

            var ratings = await db.Ratings
                .Include(i => i.User)
                .Where(q => q.LeagueId == league.Id && q.User.Leagues.Any(c => c.Id == league.Id))
                .OrderByDescending(q => q.Created)
                .ToListAsync();

            var usersWithRatings = ratings.Select(q => q.User).Distinct().OrderBy(q => q.UserName).ToList();
            var users = await db.Users.OrderBy(q => q.UserName).ToListAsync();
            var usersInLeague = usersWithRatings.Count;

            foreach (var user in users)
            {
                if (usersWithRatings.Count(q => q.Id == user.Id) == 0)
                    usersWithRatings.Add(user);
            }

            var viewmodel = new ResultCreateViewModel()
            {
                Match = new Match(),
                League = league,
                Users = usersWithRatings,
                UsersInLeague = usersInLeague
            };

            return View(viewmodel);
        }

        [HttpPost]
        [Route("~/leagues/{slug}/result")]
        public async Task<ActionResult> Create(string slug, List<string> home, List<string> away, List<int?> homeScore, List<int?> awayScore)
        {
            var league = await db.Leagues
                .Include(i => i.Users)
                .Include(i => i.Teams.Select(u => u.Users))
                .FirstOrDefaultAsync(q => q.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase));

            if (league == null)
                return HttpNotFound();

            #region Users and Teams

            var userId = User.Identity.GetUserId();

            if (home == null)
                home = new List<string>();

            if (away == null)
                return View();

            home.Add(userId);

            var allTeams = db.Teams.Include(i => i.Users);
            var homeTeam = await allTeams.FirstOrDefaultAsync(p => !p.Users.Select(u => u.Id).Except(home).Union(home.Except(p.Users.Select(u => u.Id))).Any());
            var awayTeam = await allTeams.FirstOrDefaultAsync(p => !p.Users.Select(u => u.Id).Except(away).Union(away.Except(p.Users.Select(u => u.Id))).Any());

            if (homeTeam == null)
            {
                homeTeam = new Team();

                foreach (var item in db.Users.Where(q => home.Contains(q.Id)))
                    homeTeam.Users.Add(item);

                homeTeam.Name = String.Join("+", homeTeam.Users.Select(q => q.UserName));
                homeTeam.Slug = homeTeam.Name.ToFriendlyUrl();

                db.Teams.Add(homeTeam);
            }

            if (awayTeam == null)
            {
                awayTeam = new Team();

                foreach (var user in db.Users.Where(q => away.Contains(q.Id)))
                    awayTeam.Users.Add(user);

                awayTeam.Name = String.Join("+", awayTeam.Users.Select(q => q.UserName));
                awayTeam.Slug = awayTeam.Name.ToFriendlyUrl();

                db.Teams.Add(awayTeam);
            }

            foreach (var item in awayTeam.Users.Where(item => !league.Users.Contains(item)))
                league.Users.Add(item);

            foreach (var item in homeTeam.Users.Where(item => !league.Users.Contains(item)))
                league.Users.Add(item);

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

            #endregion

            #region Ratings

            var homeWins = sets.Count(q => q.HomeScore > q.AwayScore);
            var awayWins = sets.Count(q => q.HomeScore < q.AwayScore);
            var homeWon = homeWins > awayWins;
            var awayWon = awayWins > homeWins;
            var draw = homeWins == awayWins;
            var elorating = EloRating.CalculateChange(league.Id, home, away, homeWins, awayWins);
            var rankingHome = new Dictionary<string, int>();
            var rankingAway = new Dictionary<string, int>();
            var ratings = new List<Rating>();

            foreach (var item in homeTeam.Users)
            {
                var latestRating = db.Ratings.OrderByDescending(q => q.Created).FirstOrDefault(q => q.League.Id == league.Id && q.User.Id == item.Id);
                var ratingChange = homeWon ? System.Math.Abs(elorating) : System.Math.Abs(elorating) * -1;
                var rating = latestRating == null ? 1000 : latestRating.Rate;
                var rank = latestRating == null ? league.Users.Count : latestRating.Rank;
                rankingHome.Add(item.Id, rank);

                ratings.Add(new Rating()
                {
                    Draw = draw,
                    Lost = awayWon,
                    Won = homeWon,
                    League = league,
                    User = item,
                    Team = homeTeam,
                    Rank = 0,
                    RankingChange = 0,
                    RankedLast = false,
                    Rate = (latestRating == null ? 1000 : latestRating.Rate) + ratingChange,
                    RatingChange = ratingChange,
                    Score = sets.Sum(q => q.HomeScore) - sets.Sum(q => q.AwayScore)
                });
            }

            foreach (var item in awayTeam.Users)
            {
                var latestRating = db.Ratings.OrderByDescending(q => q.Created).FirstOrDefault(q => q.League.Id == league.Id && q.User.Id == item.Id);
                var ratingChange = awayWon ? System.Math.Abs(elorating) : System.Math.Abs(elorating) * -1;
                var rating = latestRating == null ? 1000 : latestRating.Rate;
                var rank = latestRating == null ? league.Users.Count : latestRating.Rank;
                rankingAway.Add(item.Id, rank);

                ratings.Add(new Rating()
                {
                    Draw = draw,
                    Lost = homeWon,
                    Won = awayWon,
                    League = league,
                    User = item,
                    Team = awayTeam,
                    Rank = 0,
                    RankingChange = 0,
                    RankedLast = false,
                    Rate = (latestRating == null ? 1000 : latestRating.Rate) + ratingChange,
                    RatingChange = ratingChange,
                    Score = sets.Sum(q => q.AwayScore) - sets.Sum(q => q.HomeScore)
                });
            }

            // TODO: Give all points from loosing team to winning player - when player 1 vs. 2

            #endregion

            #region Match

            var match = new Match()
            {
                AwayUsers = awayTeam.Users,
                AwayTeam = awayTeam,
                AwayWon = awayWon,
                Draw = draw,
                HomeUsers = homeTeam.Users,
                HomeTeam = homeTeam,
                HomeWon = homeWon,
                League = league,
                Ratings = ratings,
                Sets = sets
            };

            db.Matches.Add(match);

            await db.SaveChangesAsync();

            #endregion

            #region Update ranking post match

            foreach (var item in homeTeam.Users)
            {
                var rank = Statistics.GetRank(league.Id, item.Id);
                var rating = ratings.FirstOrDefault(q => q.UserId == item.Id);
                rating.Rank = rank;
                rating.RankingChange = rankingHome[item.Id] - rank;

                db.Entry(rating).State = EntityState.Modified;
            }

            foreach (var item in awayTeam.Users)
            {
                var rank = Statistics.GetRank(league.Id, item.Id);
                var rating = ratings.FirstOrDefault(q => q.UserId == item.Id);
                rating.Rank = rank;
                rating.RankingChange = rankingAway[item.Id] - rank;

                db.Entry(rating).State = EntityState.Modified;
            }

            #endregion

            await db.SaveChangesAsync();

            return RedirectToAction("Details", "League", new { slug = league.Slug });
        }

        [Route("~/leagues/{slug}/result/calc")]
        public async Task<ActionResult> Calc(string slug, List<string> home, List<string> away)
        {
            if (away == null || away.Count == 0)
                return HttpNotFound();

            if (home == null)
                home = new List<string>();

            home.Add(User.Identity.GetUserId());

            var league = await db.Leagues.FirstOrDefaultAsync(q => q.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase));
            var eloHome = EloRating.CalculateChange(league.Id, home, away, 1, 0);
            var eloAway = EloRating.CalculateChange(league.Id, home, away, 0, 1);
            var eloHomeChange = eloHome.DisplayChange();
            var eloAwayChange = eloAway.DisplayChange();

            // TODO: Give all points from loosing team to winning player - when player 1 vs. 2
            //if (home.Count == 1 && away.Count > 1)
            //    eloHome = eloAway * away.Count;
            //if (away.Count == 1 && home.Count > 1)
            //    eloAway = eloHome * home.Count;

            var elo = String.Format("{0} / {1}", eloHomeChange, eloAwayChange);

            return Content(elo, "text/html");
        }
    }
}