using ChampsRoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using ChampsRoom.Helpers;

namespace ChampsRoom.Controllers
{
    [RoutePrefix("players")]
    public class PlayerController : Controller
    {
        DataContext db = new DataContext();

        [Route("{playerUrl}")]
        public async Task<ActionResult> Details(string playerUrl)
        {
            var player = await db.Players
                .Include(i => i.Leagues)
                .Include(i => i.Teams.Select(p => p.Players))
                .FirstOrDefaultAsync(q => q.Url.Equals(playerUrl, StringComparison.InvariantCultureIgnoreCase));

            return View(player);
        }

        [Authorize]
        [Route("{playerUrl}/edit")]
        public async Task<ActionResult> Edit(string playerUrl)
        {
            var player = await db.Players.Include(q => q.Teams).Include(q => q.Leagues).FirstOrDefaultAsync(q => q.Url.Equals(playerUrl, StringComparison.InvariantCultureIgnoreCase));

            var userid = User.Identity.GetUserId();

            if (player == null || !Helpers.DbHelper.CanEditPlayer(player))
                return HttpNotFound();

            return View(player);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{playerUrl}/edit")]
        public async Task<ActionResult> Edit(string playerUrl, Player model)
        {
            var player = await db.Players.FirstOrDefaultAsync(q => q.Url.Equals(playerUrl, StringComparison.InvariantCultureIgnoreCase));

            if (player == null || !Helpers.DbHelper.CanEditPlayer(player))
                return HttpNotFound();

            if (!player.Name.Trim().Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))
            {
                var name = model.Name.ToFriendlyUrl().ToString();
                var playerExists = await db.Players.FirstOrDefaultAsync(q => q.Url.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                if (playerExists != null)
                    ModelState.AddModelError(String.Empty, "Player name is not available");
            }

            if (ModelState.IsValid)
            {
                player.Name = model.Name.Trim();
                player.Url = model.Name.ToFriendlyUrl();

                db.Entry(player).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }

	}
}