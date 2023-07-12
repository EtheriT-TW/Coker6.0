using EtheriT.Coker.Web.ConsoleApp.Models;
using Microsoft.EntityFrameworkCore;
namespace EtheriT.Coker.Web.ConsoleApp.DbContextSet
{
    public class NewDbContext : DbContext
    {
        public DbSet<Article> Articles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Tag_Associate> Tag_Associates { get; set; }
        private string connectionString { get; set; }

        public NewDbContext(string connectionString) { 
            this.connectionString = connectionString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Tag_Associate>(o =>
            {
                o.HasOne(u => u.Article).WithMany(u => u.Associates).HasForeignKey(f => f.FK_AId);
                o.HasOne(w => w.Tag).WithMany(w => w.Associates).HasForeignKey(f => f.FK_TId);
            });
        }
    }
}
