﻿using ChampsRoom.Helpers;
using ChampsRoom.Models;
using ChampsRoom.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

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
                return RedirectToAction("Details", new { slug = result.FirstOrDefault().Slug });

            return View(result);
        }

        [Route("{slug}")]
        public async Task<ActionResult> Details(string slug)
        {
            var league = await db.Leagues.FirstOrDefaultAsync(q => q.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase));

            if (league == null)
                return HttpNotFound();

            var ratings = await db.Ratings
                .Include(i => i.User)
                .Where(q => q.LeagueId == league.Id && q.User.Leagues.Any(c => c.Id == league.Id))
                .OrderByDescending(q => q.Created)
                .ToListAsync();

            var latestMatches = await db.Matches
                .Include(i => i.Sets)
                .Include(i => i.AwayUsers)
                .Include(i => i.HomeUsers)
                .Include(i => i.AwayTeam)
                .Include(i => i.HomeTeam)
                .Where(q => q.LeagueId == league.Id)
                .OrderByDescending(q => q.Created)
                .Take(20)
                .ToListAsync();

            var ratingViewModels = new List<RatingsViewModel>();

            foreach (var user in ratings.Select(q => q.User).Distinct())
            {
                var userRatings = ratings.Where(q => q.UserId == user.Id);
                var userLatestRating = userRatings.FirstOrDefault();

                var ratingsViewModel = new RatingsViewModel()
                {
                    Matches = userRatings.Count(),
                    Draw = userRatings.Count(q => q.Draw),
                    Lost = userRatings.Count(q => q.Lost),
                    Won = userRatings.Count(q => q.Won),
                    Rank = userLatestRating == null ? 0 : userLatestRating.Rank,
                    RankingChange = userLatestRating == null ? 0 : userLatestRating.RankingChange,
                    Rate = userLatestRating == null ? 1000 : userLatestRating.Rate,
                    RatingChange = userLatestRating == null ? 0 : userLatestRating.RatingChange,
                    Score = userRatings.Sum(q => q.Score),
                    Team = null,
                    User = user
                };

                ratingViewModels.Add(ratingsViewModel);
            }

            var viewmodel = new LeagueDetailsViewModel()
            {
                LatestMatches = latestMatches,
                League = league,
                Ratings = ratingViewModels.OrderByRanking()
            };

            var rank = 0;

            foreach (var item in viewmodel.Ratings)
                item.Rank = ++rank;

            return View(viewmodel);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult> Create(League league)
        {
            if (!ModelState.IsValid)
                return View(league);

            league.Slug = league.Name.ToFriendlyUrl();

            db.Leagues.Add(league);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Edit(Guid id)
        {
            var league = await db.Leagues.FindAsync(id);

            if (league == null)
                return HttpNotFound();

            return View(league);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult> Edit(League league)
        {
            if (!ModelState.IsValid)
                return View(league);

            league.Slug = league.Name.ToFriendlyUrl();

            db.Entry(league).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}