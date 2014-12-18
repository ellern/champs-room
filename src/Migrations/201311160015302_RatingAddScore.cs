namespace ChampsRoom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RatingAddScore : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Ratings", "Score", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Ratings", "Score");
        }
    }
}
