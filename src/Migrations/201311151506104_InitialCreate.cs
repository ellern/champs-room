namespace ChampsRoom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Leagues",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Url = c.String(),
                        Sets = c.Int(nullable: false),
                        SetsNeededToWin = c.Int(nullable: false),
                        MaxScore = c.Int(nullable: false),
                        MinScore = c.Int(nullable: false),
                        Rules = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Matches",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Draw = c.Boolean(nullable: false),
                        HomeWon = c.Boolean(nullable: false),
                        AwayWon = c.Boolean(nullable: false),
                        LeagueId = c.Guid(nullable: false),
                        HomeTeamId = c.Guid(nullable: false),
                        AwayTeamId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Teams", t => t.AwayTeamId)
                .ForeignKey("dbo.Teams", t => t.HomeTeamId)
                .ForeignKey("dbo.Leagues", t => t.LeagueId)
                .Index(t => t.AwayTeamId)
                .Index(t => t.HomeTeamId)
                .Index(t => t.LeagueId);
            
            CreateTable(
                "dbo.Players",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Url = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Ratings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Won = c.Boolean(nullable: false),
                        Lost = c.Boolean(nullable: false),
                        Draw = c.Boolean(nullable: false),
                        Rate = c.Int(nullable: false),
                        RatingChange = c.Int(nullable: false),
                        Rank = c.Int(nullable: false),
                        RankingChange = c.Int(nullable: false),
                        RankedLast = c.Boolean(nullable: false),
                        LeagueId = c.Guid(nullable: false),
                        MatchId = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                        TeamId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Leagues", t => t.LeagueId)
                .ForeignKey("dbo.Matches", t => t.MatchId)
                .ForeignKey("dbo.Players", t => t.PlayerId)
                .ForeignKey("dbo.Teams", t => t.TeamId)
                .Index(t => t.LeagueId)
                .Index(t => t.MatchId)
                .Index(t => t.PlayerId)
                .Index(t => t.TeamId);
            
            CreateTable(
                "dbo.Teams",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Url = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Sets",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        HomeScore = c.Int(nullable: false),
                        AwayScore = c.Int(nullable: false),
                        MatchId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Matches", t => t.MatchId)
                .Index(t => t.MatchId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Player_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Players", t => t.Player_Id)
                .Index(t => t.Player_Id);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.LoginProvider, t.ProviderKey })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PlayerLeagues",
                c => new
                    {
                        Player_Id = c.Guid(nullable: false),
                        League_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Player_Id, t.League_Id })
                .ForeignKey("dbo.Players", t => t.Player_Id)
                .ForeignKey("dbo.Leagues", t => t.League_Id)
                .Index(t => t.Player_Id)
                .Index(t => t.League_Id);
            
            CreateTable(
                "dbo.TeamLeagues",
                c => new
                    {
                        Team_Id = c.Guid(nullable: false),
                        League_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Team_Id, t.League_Id })
                .ForeignKey("dbo.Teams", t => t.Team_Id)
                .ForeignKey("dbo.Leagues", t => t.League_Id)
                .Index(t => t.Team_Id)
                .Index(t => t.League_Id);
            
            CreateTable(
                "dbo.TeamPlayers",
                c => new
                    {
                        Team_Id = c.Guid(nullable: false),
                        Player_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Team_Id, t.Player_Id })
                .ForeignKey("dbo.Teams", t => t.Team_Id)
                .ForeignKey("dbo.Players", t => t.Player_Id)
                .Index(t => t.Team_Id)
                .Index(t => t.Player_Id);
            
            CreateTable(
                "dbo.MatchesAwayPlayers",
                c => new
                    {
                        MatchId = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.MatchId, t.PlayerId })
                .ForeignKey("dbo.Matches", t => t.MatchId)
                .ForeignKey("dbo.Players", t => t.PlayerId)
                .Index(t => t.MatchId)
                .Index(t => t.PlayerId);
            
            CreateTable(
                "dbo.MatchesHomePlayers",
                c => new
                    {
                        MatchId = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.MatchId, t.PlayerId })
                .ForeignKey("dbo.Matches", t => t.MatchId)
                .ForeignKey("dbo.Players", t => t.PlayerId)
                .Index(t => t.MatchId)
                .Index(t => t.PlayerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "Player_Id", "dbo.Players");
            DropForeignKey("dbo.AspNetUserClaims", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Sets", "MatchId", "dbo.Matches");
            DropForeignKey("dbo.Matches", "LeagueId", "dbo.Leagues");
            DropForeignKey("dbo.MatchesHomePlayers", "PlayerId", "dbo.Players");
            DropForeignKey("dbo.MatchesHomePlayers", "MatchId", "dbo.Matches");
            DropForeignKey("dbo.MatchesAwayPlayers", "PlayerId", "dbo.Players");
            DropForeignKey("dbo.MatchesAwayPlayers", "MatchId", "dbo.Matches");
            DropForeignKey("dbo.Ratings", "TeamId", "dbo.Teams");
            DropForeignKey("dbo.TeamPlayers", "Player_Id", "dbo.Players");
            DropForeignKey("dbo.TeamPlayers", "Team_Id", "dbo.Teams");
            DropForeignKey("dbo.TeamLeagues", "League_Id", "dbo.Leagues");
            DropForeignKey("dbo.TeamLeagues", "Team_Id", "dbo.Teams");
            DropForeignKey("dbo.Matches", "HomeTeamId", "dbo.Teams");
            DropForeignKey("dbo.Matches", "AwayTeamId", "dbo.Teams");
            DropForeignKey("dbo.Ratings", "PlayerId", "dbo.Players");
            DropForeignKey("dbo.Ratings", "MatchId", "dbo.Matches");
            DropForeignKey("dbo.Ratings", "LeagueId", "dbo.Leagues");
            DropForeignKey("dbo.PlayerLeagues", "League_Id", "dbo.Leagues");
            DropForeignKey("dbo.PlayerLeagues", "Player_Id", "dbo.Players");
            DropIndex("dbo.AspNetUsers", new[] { "Player_Id" });
            DropIndex("dbo.AspNetUserClaims", new[] { "User_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.Sets", new[] { "MatchId" });
            DropIndex("dbo.Matches", new[] { "LeagueId" });
            DropIndex("dbo.MatchesHomePlayers", new[] { "PlayerId" });
            DropIndex("dbo.MatchesHomePlayers", new[] { "MatchId" });
            DropIndex("dbo.MatchesAwayPlayers", new[] { "PlayerId" });
            DropIndex("dbo.MatchesAwayPlayers", new[] { "MatchId" });
            DropIndex("dbo.Ratings", new[] { "TeamId" });
            DropIndex("dbo.TeamPlayers", new[] { "Player_Id" });
            DropIndex("dbo.TeamPlayers", new[] { "Team_Id" });
            DropIndex("dbo.TeamLeagues", new[] { "League_Id" });
            DropIndex("dbo.TeamLeagues", new[] { "Team_Id" });
            DropIndex("dbo.Matches", new[] { "HomeTeamId" });
            DropIndex("dbo.Matches", new[] { "AwayTeamId" });
            DropIndex("dbo.Ratings", new[] { "PlayerId" });
            DropIndex("dbo.Ratings", new[] { "MatchId" });
            DropIndex("dbo.Ratings", new[] { "LeagueId" });
            DropIndex("dbo.PlayerLeagues", new[] { "League_Id" });
            DropIndex("dbo.PlayerLeagues", new[] { "Player_Id" });
            DropTable("dbo.MatchesHomePlayers");
            DropTable("dbo.MatchesAwayPlayers");
            DropTable("dbo.TeamPlayers");
            DropTable("dbo.TeamLeagues");
            DropTable("dbo.PlayerLeagues");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Sets");
            DropTable("dbo.Teams");
            DropTable("dbo.Ratings");
            DropTable("dbo.Players");
            DropTable("dbo.Matches");
            DropTable("dbo.Leagues");
        }
    }
}
