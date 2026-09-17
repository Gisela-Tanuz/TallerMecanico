using Microsoft.EntityFrameworkCore;

namespace TallerMecanico.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Repuesto> Repuestos { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<RepServ> RepServs { get; set; }
    }
}
