using ChampsRoom.Models;
using ChampsRoom.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;

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

            var viewmodel = new OverviewTeamUsersViewModel()
            {
                League = null,
                Teams = teams.Where(q => q.Users.Count > 1).ToList(),
                Users = teams.Where(q => q.Users.Count == 1).SelectMany(q => q.Users).ToList()
            };

            return View(viewmodel);
        }

        [Route("{teamUrl}")]
        public async Task<ActionResult> Details(string teamUrl)
        {
            var team = await db.Teams
                .Include(i => i.Users)
                .Include(i => i.Leagues)
                .FirstOrDefaultAsync(q => q.Url.Equals(teamUrl, StringComparison.InvariantCultureIgnoreCase));

            if (team == null)
                return HttpNotFound();

            return View(team);
        }

        [Authorize]
        [Route("{teamUrl}/edit")]
        public async Task<ActionResult> Edit(string teamUrl)
        {
            var team = await db.Teams.Include(q => q.Users).Include(q => q.Leagues).Include(q => q.Users).FirstOrDefaultAsync(q => q.Url.Equals(teamUrl, StringComparison.InvariantCultureIgnoreCase));

            if (team == null || !Helpers.DbHelper.CanEditTeam(team))
                return HttpNotFound();

            return View(team);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{teamUrl}/edit")]
        public async Task<ActionResult> Edit(string teamUrl, Team model)
        {
            var team = await db.Teams.Include(i => i.Users).FirstOrDefaultAsync(q => q.Id == model.Id);

            if (team == null || !Helpers.DbHelper.CanEditTeam(team))
                return HttpNotFound();

            if (!team.Name.Trim().Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))
            {
                var name = model.Name.ToFriendlyUrl().ToString();
                var teamExists = await db.Teams.FirstOrDefaultAsync(q => q.Url.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                if (teamExists != null)
                    ModelState.AddModelError(String.Empty, "Team name is not available");
            }

            if (ModelState.IsValid)
            {
                team.Name = model.Name.Trim();
                team.Url = model.Name.ToFriendlyUrl();

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