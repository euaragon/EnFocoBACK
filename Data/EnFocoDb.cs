using EnFoco_new.Models;
using Microsoft.EntityFrameworkCore;

namespace EnFoco_new.Data
{
    public class EnFocoDb : DbContext
    {
        public EnFocoDb(DbContextOptions<EnFocoDb> options) : base(options) { }

        // Definir las tablas de la base de datos
        public DbSet<Notice> Notices => Set<Notice>();
        public DbSet<Newsletter> Newsletters => Set<Newsletter>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}