﻿using ChampsRoom.Models;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Web;

namespace ChampsRoom.Helpers
{
    public class DbHelper
    {
        static readonly DataContext db = new DataContext();

        public static bool CanEditTeam(Team team)
        {
            if (team == null || !HttpContext.Current.Request.IsAuthenticated)
                return false;

            var userId = HttpContext.Current.User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(q => q.Id == userId);

            if (team.Users.FirstOrDefault(q => q.Id == user.Id) == null)
                return false;

            return true;
        }

        public static bool CanEditUser(User user)
        {
            if (user == null || !HttpContext.Current.Request.IsAuthenticated)
                return false;

            return user.Id == HttpContext.Current.User.Identity.GetUserId();
        }
    }
}