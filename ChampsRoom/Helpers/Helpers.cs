using ChampsRoom.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Web;

namespace ChampsRoom.Helpers
{
    public class TeamHelper
    {
        static DataContext db = new DataContext();

        public static bool CanEditTeam(Team team)
        {
            if (team == null || !HttpContext.Current.Request.IsAuthenticated)
                return false;

            var userId = HttpContext.Current.User.Identity.GetUserId();
            var user = db.Users.Include(i => i.Player).FirstOrDefault(q => q.Id == userId);

            if (team.Players.FirstOrDefault(q => q.Id == user.Player.Id) == null)
                return false;

            return true;
        }
    }
}