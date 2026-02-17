using FoodDash.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDash.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Menu> Menu { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Orders> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("DataSource=app.db;Cache=Shared");
    }
}
