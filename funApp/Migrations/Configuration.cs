namespace funApp.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Linq;
    using System.Text;

    internal sealed class Configuration : DbMigrationsConfiguration<funApp.Models.MessengerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());
            CodeGenerator = new MySql.Data.Entity.MySqlMigrationCodeGenerator(); //this line was missing, so now the migrations does not contains the prefix "dbo"

            //AutomaticMigrationsEnabled = false;
            // SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());

            //SetHistoryContextFactory("MySql.Data.MySqlClient", (conn, schema) => new MySqlHistoryContext(conn, schema));
        }
        

        protected override void Seed(funApp.Models.MessengerContext context)
        {
            context.Database.ExecuteSqlCommand("ALTER TABLE messengerbd.mails DROP foreign key FK_Mails_Users_ReceiveID");
            context.Database.ExecuteSqlCommand("ALTER TABLE messengerbd.mails DROP foreign key FK_Mails_Users_SenderID");
            context.Database.ExecuteSqlCommand("ALTER TABLE messengerbd.mails ADD CONSTRAINT FK_Mails_Users_SenderID FOREIGN KEY (SenderID) REFERENCES messengerbd.users(Id) ON UPDATE NO ACTION ON DELETE SET NULL");
            context.Database.ExecuteSqlCommand("ALTER TABLE messengerbd.mails ADD CONSTRAINT FK_Mails_Users_ReceiveID FOREIGN KEY (ReceiveID) REFERENCES messengerbd.users(Id) ON UPDATE NO ACTION ON DELETE SET NULL");

            ////  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
        
    }
}
