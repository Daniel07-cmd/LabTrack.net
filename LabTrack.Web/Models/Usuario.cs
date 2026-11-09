using System.ComponentModel.DataAnnotations;

namespace LabTrack.Web.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código institucional es obligatorio.")]
        [StringLength(50)]
        public string CodigoInstitucional { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = string.Empty; // 'Estudiante' o 'Encargado'

        // Atributos requeridos por CU-01 (Seguridad e Intentos Fallidos)
        public int IntentosFallidos { get; set; } = 0;
        public bool EstaBloqueado { get; set; } = false;
        public DateTime? BloqueadoHasta { get; set; }

        // Atributo requerido por CU-05 (Control de Penalización)
        public bool EstaPenalizado { get; set; } = false;

        public ICollection<Reserva>? Reservas { get; set; }
    }
}