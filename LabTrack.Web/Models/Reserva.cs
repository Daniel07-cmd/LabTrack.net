using System.ComponentModel.DataAnnotations;

namespace LabTrack.Web.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required]
        public int EquipoId { get; set; }
        public Equipo? Equipo { get; set; }

        [Required(ErrorMessage = "La fecha de uso es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaUso { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        public TimeSpan HoraFin { get; set; }

        [Required]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobada, Rechazada, Completada

        // Atributo requerido por CU-04 (Excepción 3a: Justificación obligatoria de rechazo)
        [StringLength(255)]
        public string? JustificacionRechazo { get; set; }

        // Atributos requeridos por CU-05 (Devolución y Penalización Automática)
        [DataType(DataType.DateTime)]
        public DateTime? FechaDevolucionReal { get; set; }
        public bool PenalizacionAplicada { get; set; } = false;
    }
}