namespace ChampsRoom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameUrlToSlug : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.AspNetUsers", "Url", "Slug");
            RenameColumn("dbo.Teams", "Url", "Slug");
            RenameColumn("dbo.Leagues", "Url", "Slug");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.AspNetUsers", "Slug", "Url");
            RenameColumn("dbo.Teams", "Slug", "Url");
            RenameColumn("dbo.Leagues", "Slug", "Url");
        }
    }
}
