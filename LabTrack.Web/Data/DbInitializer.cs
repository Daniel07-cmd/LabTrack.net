using LabTrack.Web.Models;
using LabTrack.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Web.Data
{
    /// <summary>
    /// Aplica las migraciones pendientes y crea los datos mínimos para que el
    /// sistema sea usable tras el primer despliegue.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(LabTrackContext context, ILogger logger)
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Migraciones de base de datos aplicadas correctamente.");

            if (!await context.Usuarios.AnyAsync())
            {
                context.Usuarios.AddRange(
                    new Usuario
                    {
                        CodigoInstitucional = "ENC001",
                        Password = PasswordHasher.Hash("Admin123*"),
                        NombreCompleto = "Encargado de Laboratorio",
                        Rol = "Encargado"
                    },
                    new Usuario
                    {
                        CodigoInstitucional = "EST001",
                        Password = PasswordHasher.Hash("Estudiante123*"),
                        NombreCompleto = "Estudiante de Prueba",
                        Rol = "Estudiante"
                    });

                logger.LogInformation("Usuarios iniciales creados.");
            }

            if (!await context.Equipos.AnyAsync())
            {
                context.Equipos.AddRange(
                    new Equipo
                    {
                        Nombre = "Computadora de Laboratorio 01",
                        CodigoInventario = "LAB-PC-001",
                        Categoria = "Cómputo",
                        Estado = "Disponible",
                        FechaUltimaCalibracion = DateTime.Today.AddMonths(-2)
                    },
                    new Equipo
                    {
                        Nombre = "Osciloscopio Digital",
                        CodigoInventario = "LAB-ELE-001",
                        Categoria = "Electrónica",
                        Estado = "Disponible",
                        FechaUltimaCalibracion = DateTime.Today.AddMonths(-1)
                    },
                    new Equipo
                    {
                        Nombre = "Switch Administrable 24 puertos",
                        CodigoInventario = "LAB-RED-001",
                        Categoria = "Redes",
                        Estado = "En Mantenimiento",
                        FechaUltimaCalibracion = DateTime.Today.AddMonths(-6)
                    });

                logger.LogInformation("Equipos de ejemplo creados.");
            }

            await context.SaveChangesAsync();
        }
    }
}
