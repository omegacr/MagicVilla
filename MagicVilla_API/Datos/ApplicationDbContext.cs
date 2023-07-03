using MagicVilla_API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_API.Datos
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Villa> Villas { get; set; }
        public DbSet<NumeroVilla> NumeroVillas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Villa>().HasData(
                new Villa() { 
                    Id = 1,
                    Nombre="Villa Real",
                    Detalle="Detalle de la villa...",
                    ImagenURL="",
                    Ocupantes=5,
                    MetrosCuadrados=50,
                    Tarifa=200,
                    Amenidad="",
                    FechaCreacion=DateTime.Now,
                    FechaActualizacion=DateTime.Now,
                },
                new Villa()
                {
                    Id = 2,
                    Nombre = "Villa Vista a la Piscina",
                    Detalle = "Detalle de la villa...",
                    ImagenURL = "",
                    Ocupantes = 8,
                    MetrosCuadrados = 80,
                    Tarifa = 400,
                    Amenidad = "",
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now,
                }
                );
        }
    }
}
