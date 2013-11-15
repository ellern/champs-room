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
                .Include(i => i.Players)
                .Include(i => i.Leagues)
                .ToListAsync();

            ViewBag.PlayerId = GetPlayerId();

            var viewmodel = new OverviewTeamPlayersViewModel()
            {
                League = null,
                Teams = teams.Where(q => q.Players.Count > 1).ToList(),
                Players = teams.Where(q => q.Players.Count == 1).SelectMany(q => q.Players).ToList()
            };

            return View(viewmodel);
        }

        [Route("{teamUrl}")]
        public async Task<ActionResult> Details(string teamUrl)
        {
            var team = await db.Teams
                .Include(i => i.Players)
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
            var team = await db.Teams.Include(q => q.Players).Include(q => q.Leagues).Include(q => q.Players).FirstOrDefaultAsync(q => q.Url.Equals(teamUrl, StringComparison.InvariantCultureIgnoreCase));

            if (team == null || !CanEditTeam(team))
                return HttpNotFound();

            return View(team);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{teamUrl}/edit")]
        public async Task<ActionResult> Edit(string teamUrl, Team model)
        {
            var team = await db.Teams.Include(i => i.Players).FirstOrDefaultAsync(q => q.Id == model.Id);

            if (team == null || !CanEditTeam(team))
                return HttpNotFound();

            if (!team.Name.Trim().Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))
            {
                var name = model.Name.ToFriendlyUrl().ToString();
                var teamExists = db.Teams.FirstOrDefault(q => q.Url.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                if (teamExists != null)
                    ModelState.AddModelError(String.Empty, "Team name is not available");
            }

            if (ModelState.IsValid)
            {
                team.Name = model.Name.Trim();
                team.Url = model.Name.ToFriendlyUrl();

                db.Entry(team).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        private Guid GetPlayerId()
        {
            if (!Request.IsAuthenticated)
                return Guid.Empty;

            var userId = User.Identity.GetUserId();
            var player = db.Users.AsNoTracking().Include(i => i.Player).FirstOrDefault(q => q.Id == userId);

            if (player != null)
                return player.Player.Id;

            return Guid.Empty;
        }

        private bool CanEditTeam(Team team)
        {
            if (team == null || !Request.IsAuthenticated)
                return false;

            var userId = User.Identity.GetUserId();
            var user = db.Users.Include(i => i.Player).FirstOrDefault(q => q.Id == userId);

            if (team.Players.FirstOrDefault(q => q.Id == user.Player.Id) == null)
                return false;

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}