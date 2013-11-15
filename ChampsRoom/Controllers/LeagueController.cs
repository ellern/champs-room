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
            var ratings = await db.Ratings
                .Include(i => i.Player)
                .Include(i => i.Team)
                .Include(i => i.League)
                .OrderByDescending(q => q.Created)
                .Where(q => q.League.Url.Equals(id, StringComparison.InvariantCultureIgnoreCase)).ToListAsync();

            var league = await db.Leagues
                .Include(i => i.Players)
                .FirstOrDefaultAsync(q => q.Url.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            if (league == null)
                return HttpNotFound();

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
                    RankingChange = latestRating == null ? 0 : latestRating.RankingChange,
                    Rating = latestRating == null ? 1000 : latestRating.Rate,
                    RatingChange = latestRating == null ? 0 : latestRating.RatingChange,
                    Team = null
                };

                rankings.Add(ranking);
            }

            var rank = 1;

            var viewmodel = new LeagueDetailsViewModel()
            {
                League = league,
                Rankings = rankings
                    .OrderByDescending(q => q.Rating)
                    .ThenByDescending(q => q.Won)
                    .ThenBy(q => q.Played)
                    .ThenBy(q => q.Player.Name)
                    .ToList()
            };

            foreach (var item in viewmodel.Rankings)
                item.Rank = rank++;

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