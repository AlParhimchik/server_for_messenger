namespace funApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ondeletesetnull : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Mails", "ReceiveID", "Users");
            DropForeignKey("Mails", "SenderID", "Users");
            DropIndex("Mails", new[] { "ReceiveID" });
            DropIndex("Mails", new[] { "SenderID" });
            AlterColumn("Mails", "ReceiveID", c => c.Guid());
            AlterColumn("Mails", "SenderID", c => c.Guid());
            CreateIndex("Mails", "ReceiveID");
            CreateIndex("Mails", "SenderID");
            AddForeignKey("Mails", "ReceiveID", "Users", "Id");
            AddForeignKey("Mails", "SenderID", "Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("Mails", "SenderID", "Users");
            DropForeignKey("Mails", "ReceiveID", "Users");
            DropIndex("Mails", new[] { "SenderID" });
            DropIndex("Mails", new[] { "ReceiveID" });
            AlterColumn("Mails", "SenderID", c => c.Guid(nullable: false));
            AlterColumn("Mails", "ReceiveID", c => c.Guid(nullable: false));
            CreateIndex("Mails", "SenderID");
            CreateIndex("Mails", "ReceiveID");
            AddForeignKey("Mails", "SenderID", "Users", "Id", cascadeDelete: true);
            AddForeignKey("Mails", "ReceiveID", "Users", "Id", cascadeDelete: true);
        }
    }
}
