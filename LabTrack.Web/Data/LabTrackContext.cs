using Microsoft.EntityFrameworkCore;
using LabTrack.Web.Models;

namespace LabTrack.Web.Data
{
    public class LabTrackContext : DbContext
    {
        public LabTrackContext(DbContextOptions<LabTrackContext> options) : base(options)
        {
        }

        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
    }
}