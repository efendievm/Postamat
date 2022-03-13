using Microsoft.EntityFrameworkCore;
using Postamat.Models;

namespace Postamat.DataBases
{
    /// <summary>
    /// Контекст базы данных приложения.
    /// </summary>
    public class ApplicationContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Models.Postamat> Postamats { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CartLine> CartLines { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // создаём уникалный ключ для поля номера постамата
            modelBuilder.Entity<Models.Postamat>().HasIndex(p => p.Number).IsUnique(true);
            // создаём уникалный ключ для поля названия товара
            modelBuilder.Entity<Product>().HasIndex(p => p.Name).IsUnique(true);
            // устаналиваем каскадное удаление при удалении заказа
            modelBuilder.Entity<CartLine>()
                .HasOne(p => p.Order)
                .WithMany(t => t.Lines)
                .OnDelete(DeleteBehavior.Cascade);
            // устаналиваем каскадное удаление при удалении товара
            modelBuilder.Entity<CartLine>()
                .HasOne(p => p.Product)
                .WithMany(t => t.Lines)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}