using ChampsRoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

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
	}
}