using ChampsRoom.Models;
using ChampsRoom.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace ChampsRoom.Controllers
{
    [RoutePrefix("leagues")]
    public class LeagueController : Controller
    {
        private DataContext db = new DataContext();

        [Route("")]
        public async Task<ActionResult> Index()
        {
            var result = await db.Leagues.OrderBy(q => q.Name).ToListAsync();

            if (result.Count == 1)
                return RedirectToAction("Details", new { id = result.FirstOrDefault().Url });

            return View(result);
        }

        [Route("{id}")]
        public async Task<ActionResult> Details(string id)
        {
            var league = await db.Leagues.FirstOrDefaultAsync(q => q.Url.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            if (league == null)
                return HttpNotFound();

            var ratings = await db.Ratings
                .OrderByDescending(q => q.Created)
                .Where(q => q.LeagueId == league.Id)
                .ToListAsync();

            var latestMatches = await db.Matches
                .Take(20)
                .Include(i => i.Sets)
                .Include(i => i.AwayPlayers)
                .Include(i => i.HomePlayers)
                .Include(i => i.AwayTeam)
                .Include(i => i.HomeTeam)
                .OrderByDescending(q => q.Created).ToListAsync();

            var rankings = new List<RankingViewModel>();

            foreach (var player in ratings.Select(q => q.Player).Distinct())
            {
                var playerRatings = ratings.Where(q => q.PlayerId == player.Id);
                var latestRating = playerRatings.FirstOrDefault();

                var ranking = new RankingViewModel()
                {
                    Draw = playerRatings.Count(q => q.Draw == true),
                    Lost = playerRatings.Count(q => q.Lost == true),
                    Won = playerRatings.Count(q => q.Won == true),
                    Played = playerRatings.Count(),
                    Player = player,
                    Rank = latestRating == null ? 0 : latestRating.Rank,
                    RankingChange = latestRating == null ? 0 : latestRating.RankingChange,
                    Rating = latestRating == null ? 1000 : latestRating.Rate,
                    RatingChange = latestRating == null ? 0 : latestRating.RatingChange,
                    Score = playerRatings.Sum(q => q.Score),
                    Team = null
                };

                rankings.Add(ranking);
            }

            var viewmodel = new LeagueDetailsViewModel()
            {
                LatestMatches = latestMatches,
                League = league,
                Rankings = rankings
                    .OrderByDescending(q => q.Rating)
                    .ThenByDescending(q => q.Won)
                    .ThenByDescending(q => q.Draw)
                    .ThenBy(q => q.Played)
                    .ThenBy(q => q.Player.Name)
                    .ToList()
            };

            var rank = 0;

            foreach (var item in viewmodel.Rankings)
                item.Rank = ++rank;

            return View(viewmodel);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(League league)
        {
            if (ModelState.IsValid)
            {
                league.Url = league.Name.ToFriendlyUrl();

                db.Leagues.Add(league);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(league);
        }

        public async Task<ActionResult> Edit(Guid id)
        {
            var league = await db.Leagues.FindAsync(id);

            if (league == null)
                return HttpNotFound();

            return View(league);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(League league)
        {
            if (ModelState.IsValid)
            {
                league.Url = league.Name.ToFriendlyUrl();

                db.Entry(league).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(league);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}