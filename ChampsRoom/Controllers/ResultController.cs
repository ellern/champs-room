﻿using ChampsRoom.Helpers;
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
    [RoutePrefix("results")]
    public class ResultController : Controller
    {
        private DataContext db = new DataContext();

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

            var viewmodel = new ResultDetailsViewModel()
            {
                League = league,
                Matches = user.Matches.Where(q => q.LeagueId == league.Id).OrderByDescending(q => q.Created).ToList(),
                User = user
            };

            var maxRank = league.Users.Count + 1;
            var chartRanking = "";
            var chartRating = "";
            var counter = 1;

            foreach (var item in viewmodel.Matches.Take(50).Reverse())
            {
                chartRanking += String.Format("[{0},{1}],", counter, System.Math.Abs(item.GetRating(viewmodel.User).Rank - maxRank));
                chartRating += String.Format("[{0},{1}],", counter++, item.GetRating(viewmodel.User).Rate);
            }

            ViewBag.ChartRanking = chartRanking.Trim(',');
            ViewBag.ChartRating = chartRating.Trim(',');

            return View(viewmodel);
        }

        [Authorize]
        [Route("~/leagues/{slug}/result")]
        public async Task<ActionResult> Create(string slug)
        {
            var leagues = await db.Leagues.Include(q => q.Users).FirstOrDefaultAsync(q => q.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase));

            if (leagues == null)
                return HttpNotFound();

            var viewmodel = new ResultCreateViewModel()
            {
                Match = new Match(),
                League = leagues,
                Users = db.Users.OrderBy(q => q.UserName).ToList()

            };

            return View(viewmodel);
        }

        [Authorize]
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

            #region Find Users and Teams

            var userId = User.Identity.GetUserId();

            if (home == null)
                home = new List<string>();

            if (away == null)
                return View();

            home.Add(userId);

            var homeUsers = db.Users.Include(i => i.Teams).Where(q => home.Contains(q.Id));
            var awayUsers = db.Users.Include(i => i.Teams).Where(q => away.Contains(q.Id));
            var allTeams = db.Teams.Include(i => i.Users);
            var homeTeam = allTeams.FirstOrDefault(p => !p.Users.Select(u => u.Id).Except(home).Union(home.Except(p.Users.Select(u => u.Id))).Any());
            var awayTeam = allTeams.FirstOrDefault(p => !p.Users.Select(u => u.Id).Except(away).Union(away.Except(p.Users.Select(u => u.Id))).Any());

            //var homeTeam = allTeams.Where(p => !p.Users.Select(c => c.Id).Except(home).Union(home.Except(p.Users.Select(c => c.Id))).Any()).FirstOrDefault();
            //var awayTeam = allTeams.Where(p => !p.Users.Select(c => c.Id).Except(away).Union(away.Except(p.Users.Select(c => c.Id))).Any()).FirstOrDefault();

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

            var homeWins = sets.Count(q => q.HomeScore > q.AwayScore);
            var awayWins = sets.Count(q => q.HomeScore < q.AwayScore);

            #endregion

            #region Ratings

            var ratingHome = new List<Tuple<string, int, int, int, int>>();
            var ratingAway = new List<Tuple<string, int, int, int, int>>();

            foreach (var item in homeTeam.Users)
            {
                var latestRating = db.Ratings.OrderByDescending(q => q.Created).FirstOrDefault(q => q.League.Id == league.Id && q.User.Id == item.Id);
                var rating = latestRating == null ? 1000 : latestRating.Rate;
                var rank = latestRating == null ? league.Users.Count : latestRating.Rank;
                ratingHome.Add(new Tuple<string, int, int, int, int>(item.Id, rating, rank, 0, 0));
            }

            foreach (var item in awayTeam.Users)
            {
                var latestRating = db.Ratings.OrderByDescending(q => q.Created).FirstOrDefault(q => q.League.Id == league.Id && q.User.Id == item.Id);
                var rating = latestRating == null ? 1000 : latestRating.Rate;
                var rank = latestRating == null ? league.Users.Count : latestRating.Rank;
                ratingAway.Add(new Tuple<string, int, int, int, int>(item.Id, rating, rank, 0, 0));
            }

            var avgHome = ratingHome.Sum(q => q.Item2) / homeTeam.Users.Count;
            var avgAway = ratingAway.Sum(q => q.Item2) / awayTeam.Users.Count;
            var homeWon = homeWins > awayWins;
            var awayWon = awayWins > homeWins;
            var draw = homeWins == awayWins;
            var elorating = EloRating.CalculateChange(avgHome, avgAway, homeWins, awayWins);
            var ratings = new List<Rating>();

            foreach (var item in homeTeam.Users)
            {
                var latestRating = db.Ratings.OrderByDescending(q => q.Created).FirstOrDefault(q => q.League.Id == league.Id && q.User.Id == item.Id);
                var ratingChange = homeWon ? System.Math.Abs(elorating) : System.Math.Abs(elorating) * -1;

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

            #endregion

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

            #region Update ranking post match

            foreach (var item in homeTeam.Users)
            {
                var rank = Statistics.GetRank(league.Id, item.Id);
                var rating = ratings.FirstOrDefault(q => q.UserId == item.Id);
                rating.Rank = rank;
                rating.RankingChange = ratingHome.FirstOrDefault(q => q.Item1 == item.Id).Item3 - rank;

                db.Entry(rating).State = EntityState.Modified;
            }

            foreach (var item in awayTeam.Users)
            {
                var rank = Statistics.GetRank(league.Id, item.Id);
                var rating = ratings.FirstOrDefault(q => q.UserId == item.Id);
                rating.Rank = rank;
                rating.RankingChange = ratingAway.FirstOrDefault(q => q.Item1 == item.Id).Item3 - rank;

                db.Entry(rating).State = EntityState.Modified;
            }

            #endregion

            await db.SaveChangesAsync();

            return RedirectToAction("Details", "League", new { slug = league.Slug });
        }
    }
}