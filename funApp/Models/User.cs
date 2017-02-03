using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web;

namespace funApp.Models
{
    [Table("Users",Schema = "dbo")]
    public class User
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
    }
    [Table("Mails", Schema = "dbo")]
    public class Mail
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [ForeignKey("receiver")]
        public Guid? ReceiveID { get; set; }
        [ForeignKey("sender")]
        public Guid? SenderID { get; set; }
        [Required]
        public string Text { get; set; }
        public User receiver { get; set; }
        public User sender { get; set; }
        public DateTime Time { get; set; }
    }
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class MessengerContext : DbContext
    {
        public MessengerContext()
            : base("funmessengerConnection")
        {
            Database.SetInitializer<MessengerContext>(new MySqlInitializer());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Mail> Mails { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<Mail>().HasOptional(m => m.receiver).WithMany().HasForeignKey(m => m.ReceiveID);
            modelBuilder.Entity<Mail>().HasOptional(m => m.sender).WithMany().HasForeignKey(m => m.SenderID);

            //modelBuilder.Entity<Mail>().Property(t => t.Time).HasColumnType("smalldatetime");
            //modelBuilder.Entity<Mail>().HasRequired(c => c.sender).WithMany().WillCascadeOnDelete(false);
            //modelBuilder.Entity<Mail>().HasRequired(c => c.receiver).WithMany().WillCascadeOnDelete(false);
        }
        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                throw new DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" +
                    sb.ToString(), ex
                    ); // Add the original exception as the innerException
            }
        }
    }

    public class MyDbInitializer : CreateDatabaseIfNotExists<MessengerContext>
    {
        protected override void Seed(MessengerContext context)
        {
            // create 3 students to seed the database
            context.Users.Add(new User { Id = Guid.NewGuid(), FirstName = "Mark", LastName = "Richards", Login = "123", Password = "123", PhoneNumber = "1" });

            base.Seed(context);
        }
    }
}