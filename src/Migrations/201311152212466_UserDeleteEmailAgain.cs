namespace ChampsRoom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserDeleteEmailAgain : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "Email");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Email", c => c.String());
        }
    }
}
