using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChampsRoom.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class User : IdentityUser
    {
        public string ImageUrl { get; set; }
        public string Slug { get; set; }

        public ICollection<League> Leagues { get; set; }
        public ICollection<Match> AwayMatches { get; set; }
        public ICollection<Match> HomeMatches { get; set; }
        public ICollection<Team> Teams { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Match> Matches
        {
            get
            {
                var matches = new List<Match>();
                if (this.AwayMatches != null && this.AwayMatches.Count > 0)
                    matches.AddRange(this.AwayMatches);
                if (this.HomeMatches != null && this.HomeMatches.Count > 0)
                    matches.AddRange(this.HomeMatches);

                return matches;
            }
        }
        public string GetImageUrl()
        {
            return String.IsNullOrWhiteSpace(this.ImageUrl) ? "http://placehold.it/400x400&text=No+image" : this.ImageUrl;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}