using ChampsRoom.Helpers;
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
    public class HomeController : Controller
    {
        private DataContext db = new DataContext();

        [Route("")]
        public async Task<ActionResult> Index()
        {
            var leagues = await db.Leagues.ToListAsync();

            return View(leagues);
        }
    }
}