using Microsoft.EntityFrameworkCore;
using Postamat.Models;

namespace Postamat.DataBases
{
    /// <summary>
    /// Контекст БД зарегестированных пользователей.
    /// </summary>
    public class IdentityContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // устанавливаем уникальность логина
            modelBuilder.Entity<User>().HasIndex(p => p.Login).IsUnique(true);
        }
    }
}
