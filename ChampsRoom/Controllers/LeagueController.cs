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
                .Where(q => q.LeagueId == league.Id)
                .OrderByDescending(q => q.Created)
                .ToListAsync();

            var latestMatches = await db.Matches
                .Take(20)
                .Include(i => i.Sets)
                .Include(i => i.AwayUsers)
                .Include(i => i.HomeUsers)
                .Include(i => i.AwayTeam)
                .Include(i => i.HomeTeam)
                .OrderByDescending(q => q.Created).ToListAsync();

            var rankings = new List<RankingViewModel>();

            foreach (var user in ratings.Select(q => q.User).Distinct())
            {
                var userRatings = ratings.Where(q => q.UserId == user.Id);
                var userLatestRating = userRatings.FirstOrDefault();

                var ranking = new RankingViewModel()
                {
                    Draw = userRatings.Count(q => q.Draw == true),
                    Lost = userRatings.Count(q => q.Lost == true),
                    Won = userRatings.Count(q => q.Won == true),
                    Matches = userRatings.Count(),
                    User = user,
                    Rank = userLatestRating == null ? 0 : userLatestRating.Rank,
                    RankingChange = userLatestRating == null ? 0 : userLatestRating.RankingChange,
                    Rate = userLatestRating == null ? 1000 : userLatestRating.Rate,
                    RatingChange = userLatestRating == null ? 0 : userLatestRating.RatingChange,
                    Score = userRatings.Sum(q => q.Score),
                    Team = null
                };

                rankings.Add(ranking);
            }

            var viewmodel = new LeagueDetailsViewModel()
            {
                LatestMatches = latestMatches,
                League = league,
                Rankings = rankings
                    .OrderByDescending(q => q.Rate)
                    .ThenByDescending(q => q.Won)
                    .ThenByDescending(q => q.Draw)
                    .ThenBy(q => q.Matches)
                    .ThenBy(q => q.User.UserName)
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