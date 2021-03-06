﻿using ChampsRoom.Helpers;
using ChampsRoom.Models;
using ChampsRoom.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChampsRoom.Controllers
{
    [RoutePrefix("teams")]
    public class TeamController : Controller
    {
        DataContext db = new DataContext();

        [Route("")]
        public async Task<ActionResult> Index()
        {
            var teams = await db.Teams
                .Include(i => i.Users)
                .Include(i => i.Leagues)
                .ToListAsync();

            var viewmodel = new TeamsIndexViewModel()
            {
                League = null,
                Teams = teams.Where(q => q.Users.Count > 1).ToList(),
                Users = teams.Where(q => q.Users.Count == 1).SelectMany(q => q.Users).ToList()
            };

            return View(viewmodel);
        }

        [Route("{slug}")]
        public async Task<ActionResult> Details(string slug)
        {
            var team = await db.Teams
                .Include(i => i.Users)
                .Include(i => i.Leagues)
                .FirstOrDefaultAsync(q => q.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase));

            if (team == null)
                return HttpNotFound();

            return View(team);
        }

        [Authorize]
        [Route("{slug}/edit")]
        public async Task<ActionResult> Edit(string slug)
        {
            var team = await db.Teams.Include(q => q.Users).Include(q => q.Leagues).Include(q => q.Users).FirstOrDefaultAsync(q => q.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase));

            if (team == null || !Helpers.DbHelper.CanEditTeam(team))
                return HttpNotFound();

            return View(team);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{slug}/edit")]
        public async Task<ActionResult> Edit(string slug, Team model)
        {
            var team = await db.Teams.Include(i => i.Users).FirstOrDefaultAsync(q => q.Id == model.Id);

            if (team == null || !Helpers.DbHelper.CanEditTeam(team))
                return HttpNotFound();

            if (!team.Name.Trim().Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))
            {
                var name = model.Name.ToFriendlyUrl().ToString();
                var teamExists = await db.Teams.FirstOrDefaultAsync(q => q.Slug.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                if (teamExists != null)
                    ModelState.AddModelError(String.Empty, "Team name is not available");
            }

            if (ModelState.IsValid)
            {
                team.Name = model.Name.Trim();
                team.Slug = model.Name.ToFriendlyUrl();

                db.Entry(team).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}