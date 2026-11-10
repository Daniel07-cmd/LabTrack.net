using LabTrack.Web.Models;
using LabTrack.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Web.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(LabTrackContext context, ILogger logger)
        {
            // Aplica migraciones pendientes automáticamente
            await context.Database.MigrateAsync();
            logger.LogInformation("Migraciones de base de datos aplicadas correctamente.");

            // 1. Verificar o actualizar usuario Encargado
            var encargadoEmail = "encargado@gmail.com";
            var encargado = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == encargadoEmail);

            if (encargado == null)
            {
                context.Usuarios.Add(new Usuario
                {
                    Email = encargadoEmail,
                    Password = PasswordHasher.Hash("Admin123*"),
                    NombreCompleto = "Encargado de Laboratorio",
                    Rol = "Encargado",
                    IntentosFallidos = 0,
                    EstaBloqueado = false,
                    EstaPenalizado = false
                });
                logger.LogInformation("Usuario encargado creado.");
            }
            else
            {
                // Forzar actualización de contraseña y desbloqueo para evitar bloqueos por pruebas
                encargado.Password = PasswordHasher.Hash("Admin123*");
                encargado.IntentosFallidos = 0;
                encargado.EstaBloqueado = false;
                encargado.BloqueadoHasta = null;
                logger.LogInformation("Usuario encargado actualizado y desbloqueado.");
            }

            // 2. Verificar o actualizar usuario Estudiante de prueba
            var estudianteEmail = "estudiante@gmail.com";
            var estudiante = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == estudianteEmail);

            if (estudiante == null)
            {
                context.Usuarios.Add(new Usuario
                {
                    Email = estudianteEmail,
                    Password = PasswordHasher.Hash("Estudiante123*"),
                    NombreCompleto = "Estudiante de Prueba",
                    Rol = "Estudiante",
                    IntentosFallidos = 0,
                    EstaBloqueado = false,
                    EstaPenalizado = false
                });
                logger.LogInformation("Usuario estudiante creado.");
            }
            else
            {
                estudiante.Password = PasswordHasher.Hash("Estudiante123*");
                estudiante.IntentosFallidos = 0;
                estudiante.EstaBloqueado = false;
                estudiante.BloqueadoHasta = null;
                logger.LogInformation("Usuario estudiante actualizado y desbloqueado.");
            }

            // 3. Inicializar equipos de ejemplo si la tabla está vacía
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