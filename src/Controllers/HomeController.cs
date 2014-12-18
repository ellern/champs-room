using ChampsRoom.Models;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChampsRoom.Controllers
{
    [RoutePrefix("")]
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