namespace ChampsRoom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlayerToUserRefactor : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PlayerLeagues", "Player_Id", "dbo.Players");
            DropForeignKey("dbo.PlayerLeagues", "League_Id", "dbo.Leagues");
            DropForeignKey("dbo.Ratings", "PlayerId", "dbo.Players");
            DropForeignKey("dbo.TeamPlayers", "Team_Id", "dbo.Teams");
            DropForeignKey("dbo.TeamPlayers", "Player_Id", "dbo.Players");
            DropForeignKey("dbo.MatchesAwayPlayers", "MatchId", "dbo.Matches");
            DropForeignKey("dbo.MatchesAwayPlayers", "PlayerId", "dbo.Players");
            DropForeignKey("dbo.MatchesHomePlayers", "MatchId", "dbo.Matches");
            DropForeignKey("dbo.MatchesHomePlayers", "PlayerId", "dbo.Players");
            DropForeignKey("dbo.AspNetUsers", "Player_Id", "dbo.Players");
            DropIndex("dbo.PlayerLeagues", new[] { "Player_Id" });
            DropIndex("dbo.PlayerLeagues", new[] { "League_Id" });
            DropIndex("dbo.Ratings", new[] { "PlayerId" });
            DropIndex("dbo.TeamPlayers", new[] { "Team_Id" });
            DropIndex("dbo.TeamPlayers", new[] { "Player_Id" });
            DropIndex("dbo.MatchesAwayPlayers", new[] { "MatchId" });
            DropIndex("dbo.MatchesAwayPlayers", new[] { "PlayerId" });
            DropIndex("dbo.MatchesHomePlayers", new[] { "MatchId" });
            DropIndex("dbo.MatchesHomePlayers", new[] { "PlayerId" });
            DropIndex("dbo.AspNetUsers", new[] { "Player_Id" });
            CreateTable(
                "dbo.UserLeagues",
                c => new
                    {
                        User_Id = c.String(nullable: false, maxLength: 128),
                        League_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.League_Id })
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .ForeignKey("dbo.Leagues", t => t.League_Id)
                .Index(t => t.User_Id)
                .Index(t => t.League_Id);
            
            CreateTable(
                "dbo.UserTeams",
                c => new
                    {
                        User_Id = c.String(nullable: false, maxLength: 128),
                        Team_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.Team_Id })
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .ForeignKey("dbo.Teams", t => t.Team_Id)
                .Index(t => t.User_Id)
                .Index(t => t.Team_Id);
            
            CreateTable(
                "dbo.MatchesAwayUsers",
                c => new
                    {
                        MatchId = c.Guid(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.MatchId, t.UserId })
                .ForeignKey("dbo.Matches", t => t.MatchId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.MatchId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.MatchesHomeUsers",
                c => new
                    {
                        MatchId = c.Guid(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.MatchId, t.UserId })
                .ForeignKey("dbo.Matches", t => t.MatchId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.MatchId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Ratings", "UserId", c => c.String(maxLength: 128));
            AddColumn("dbo.AspNetUsers", "Url", c => c.String());
            CreateIndex("dbo.Ratings", "UserId");
            AddForeignKey("dbo.Ratings", "UserId", "dbo.AspNetUsers", "Id");
            DropColumn("dbo.Ratings", "PlayerId");
            DropColumn("dbo.AspNetUsers", "Player_Id");
            DropTable("dbo.Players");
            DropTable("dbo.PlayerLeagues");
            DropTable("dbo.TeamPlayers");
            DropTable("dbo.MatchesAwayPlayers");
            DropTable("dbo.MatchesHomePlayers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MatchesHomePlayers",
                c => new
                    {
                        MatchId = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.MatchId, t.PlayerId });
            
            CreateTable(
                "dbo.MatchesAwayPlayers",
                c => new
                    {
                        MatchId = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.MatchId, t.PlayerId });
            
            CreateTable(
                "dbo.TeamPlayers",
                c => new
                    {
                        Team_Id = c.Guid(nullable: false),
                        Player_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Team_Id, t.Player_Id });
            
            CreateTable(
                "dbo.PlayerLeagues",
                c => new
                    {
                        Player_Id = c.Guid(nullable: false),
                        League_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Player_Id, t.League_Id });
            
            CreateTable(
                "dbo.Players",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Url = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AspNetUsers", "Player_Id", c => c.Guid());
            AddColumn("dbo.Ratings", "PlayerId", c => c.Guid(nullable: false));
            DropForeignKey("dbo.MatchesHomeUsers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MatchesHomeUsers", "MatchId", "dbo.Matches");
            DropForeignKey("dbo.MatchesAwayUsers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MatchesAwayUsers", "MatchId", "dbo.Matches");
            DropForeignKey("dbo.UserTeams", "Team_Id", "dbo.Teams");
            DropForeignKey("dbo.UserTeams", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Ratings", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserLeagues", "League_Id", "dbo.Leagues");
            DropForeignKey("dbo.UserLeagues", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.MatchesHomeUsers", new[] { "UserId" });
            DropIndex("dbo.MatchesHomeUsers", new[] { "MatchId" });
            DropIndex("dbo.MatchesAwayUsers", new[] { "UserId" });
            DropIndex("dbo.MatchesAwayUsers", new[] { "MatchId" });
            DropIndex("dbo.UserTeams", new[] { "Team_Id" });
            DropIndex("dbo.UserTeams", new[] { "User_Id" });
            DropIndex("dbo.Ratings", new[] { "UserId" });
            DropIndex("dbo.UserLeagues", new[] { "League_Id" });
            DropIndex("dbo.UserLeagues", new[] { "User_Id" });
            DropColumn("dbo.AspNetUsers", "Url");
            DropColumn("dbo.Ratings", "UserId");
            DropTable("dbo.MatchesHomeUsers");
            DropTable("dbo.MatchesAwayUsers");
            DropTable("dbo.UserTeams");
            DropTable("dbo.UserLeagues");
            CreateIndex("dbo.AspNetUsers", "Player_Id");
            CreateIndex("dbo.MatchesHomePlayers", "PlayerId");
            CreateIndex("dbo.MatchesHomePlayers", "MatchId");
            CreateIndex("dbo.MatchesAwayPlayers", "PlayerId");
            CreateIndex("dbo.MatchesAwayPlayers", "MatchId");
            CreateIndex("dbo.TeamPlayers", "Player_Id");
            CreateIndex("dbo.TeamPlayers", "Team_Id");
            CreateIndex("dbo.Ratings", "PlayerId");
            CreateIndex("dbo.PlayerLeagues", "League_Id");
            CreateIndex("dbo.PlayerLeagues", "Player_Id");
            AddForeignKey("dbo.AspNetUsers", "Player_Id", "dbo.Players", "Id");
            AddForeignKey("dbo.MatchesHomePlayers", "PlayerId", "dbo.Players", "Id");
            AddForeignKey("dbo.MatchesHomePlayers", "MatchId", "dbo.Matches", "Id");
            AddForeignKey("dbo.MatchesAwayPlayers", "PlayerId", "dbo.Players", "Id");
            AddForeignKey("dbo.MatchesAwayPlayers", "MatchId", "dbo.Matches", "Id");
            AddForeignKey("dbo.TeamPlayers", "Player_Id", "dbo.Players", "Id");
            AddForeignKey("dbo.TeamPlayers", "Team_Id", "dbo.Teams", "Id");
            AddForeignKey("dbo.Ratings", "PlayerId", "dbo.Players", "Id");
            AddForeignKey("dbo.PlayerLeagues", "League_Id", "dbo.Leagues", "Id");
            AddForeignKey("dbo.PlayerLeagues", "Player_Id", "dbo.Players", "Id");
        }
    }
}
