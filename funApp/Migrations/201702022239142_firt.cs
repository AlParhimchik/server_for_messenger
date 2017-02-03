namespace funApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class firt : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Mails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ReceiveID = c.Guid(nullable: false),
                        SenderID = c.Guid(nullable: false),
                        Text = c.String(nullable: false, unicode: false),
                        Time = c.DateTime(nullable: false, precision: 0),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ReceiveID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.SenderID, cascadeDelete: true)
                .Index(t => t.ReceiveID)
                .Index(t => t.SenderID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FirstName = c.String(unicode: false),
                        LastName = c.String(unicode: false),
                        PhoneNumber = c.String(unicode: false),
                        Login = c.String(nullable: false, unicode: false),
                        Password = c.String(nullable: false, unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Mails", "SenderID", "dbo.Users");
            DropForeignKey("dbo.Mails", "ReceiveID", "dbo.Users");
            DropIndex("dbo.Mails", new[] { "SenderID" });
            DropIndex("dbo.Mails", new[] { "ReceiveID" });
            DropTable("dbo.Users");
            DropTable("dbo.Mails");
        }
    }
}
