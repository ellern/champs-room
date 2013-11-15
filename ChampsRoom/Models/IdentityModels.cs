using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ChampsRoom.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class User : IdentityUser
    {
        public Player Player { get; set; }
        public string ImageUrl { get; set; }
    }

    public class DataContext : IdentityDbContext<User>
    {
        public DataContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Match> Matches { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Player>()
                .Ignore(x => x.Matches);

            modelBuilder.Entity<Team>()
                .HasMany(x => x.HomeMatches)
                .WithRequired(x => x.HomeTeam)
                .HasForeignKey(x => x.HomeTeamId);

            modelBuilder.Entity<Team>()
                .HasMany(x => x.AwayMatches)
                .WithRequired(x => x.AwayTeam)
                .HasForeignKey(x => x.AwayTeamId);

            //modelBuilder.Entity<Match>()
            //    .HasRequired(x => x.AwayTeam)
            //    .WithMany(x => x.AwayMatches)
            //    .HasForeignKey(x => x.AwayTeamId);

            //modelBuilder.Entity<Match>()
            //    .HasRequired(x => x.HomeTeam)
            //    .WithMany(x => x.HomeMatches)
            //    .HasForeignKey(x => x.HomeTeamId);

            modelBuilder.Entity<Player>()
                .HasMany(x => x.AwayMatches)
                .WithMany(x => x.AwayPlayers)
                .Map(map =>
                {
                    map.MapLeftKey("PlayerId");
                    map.MapRightKey("MatchId");
                    map.ToTable("MatchesAwayPlayers");
                });

            modelBuilder.Entity<Player>()
                .HasMany(x => x.HomeMatches)
                .WithMany(x => x.HomePlayers)
                .Map(map =>
                {
                    map.MapLeftKey("PlayerId");
                    map.MapRightKey("MatchId");
                    map.ToTable("MatchesHomePlayers");
                });

            modelBuilder.Entity<Match>()
                .HasMany(x => x.AwayPlayers)
                .WithMany(x => x.AwayMatches)
                .Map(map =>
                {
                    map.MapLeftKey("PlayerId");
                    map.MapRightKey("MatchId");
                    map.ToTable("MatchesAwayPlayers");
                });

            modelBuilder.Entity<Match>()
                .HasMany(x => x.HomePlayers)
                .WithMany(x => x.HomeMatches)
                .Map(map =>
                {
                    map.MapLeftKey("PlayerId");
                    map.MapRightKey("MatchId");
                    map.ToTable("MatchesHomePlayers");
                });
           
            base.OnModelCreating(modelBuilder);
        }
    }
}