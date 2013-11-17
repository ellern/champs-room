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
    [RoutePrefix("users")]
    public class UserController : Controller
    {
        DataContext db = new DataContext();

        [Route("{slug}")]
        public async Task<ActionResult> Details(string slug)
        {
            var user = await db.Users
                .Include(i => i.Leagues)
                .Include(i => i.Teams.Select(p => p.Users))
                .FirstOrDefaultAsync(q => q.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase));

            return View(user);
        }
	}
}