using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ChampsRoom.Models
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static DataContext Create()
        {
            return new DataContext();
        }

        public DbSet<Match> Matches { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Entity<User>()
                .Ignore(x => x.Matches);

            modelBuilder.Entity<Team>()
                .HasMany(x => x.HomeMatches)
                .WithRequired(x => x.HomeTeam)
                .HasForeignKey(x => x.HomeTeamId);

            modelBuilder.Entity<Team>()
                .HasMany(x => x.AwayMatches)
                .WithRequired(x => x.AwayTeam)
                .HasForeignKey(x => x.AwayTeamId);

            modelBuilder.Entity<User>()
                .HasMany(x => x.AwayMatches)
                .WithMany(x => x.AwayUsers)
                .Map(map =>
                {
                    map.MapLeftKey("UserId");
                    map.MapRightKey("MatchId");
                    map.ToTable("MatchesAwayUsers");
                });

            modelBuilder.Entity<User>()
                .HasMany(x => x.HomeMatches)
                .WithMany(x => x.HomeUsers)
                .Map(map =>
                {
                    map.MapLeftKey("UserId");
                    map.MapRightKey("MatchId");
                    map.ToTable("MatchesHomeUsers");
                });

            modelBuilder.Entity<Match>()
                .HasMany(x => x.AwayUsers)
                .WithMany(x => x.AwayMatches)
                .Map(map =>
                {
                    map.MapLeftKey("UserId");
                    map.MapRightKey("MatchId");
                    map.ToTable("MatchesAwayUsers");
                });

            modelBuilder.Entity<Match>()
                .HasMany(x => x.HomeUsers)
                .WithMany(x => x.HomeMatches)
                .Map(map =>
                {
                    map.MapLeftKey("UserId");
                    map.MapRightKey("MatchId");
                    map.ToTable("MatchesHomeUsers");
                });

            base.OnModelCreating(modelBuilder);
        }
    }
}