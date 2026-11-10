using System.ComponentModel.DataAnnotations;

namespace LabTrack.Web.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = string.Empty; // 'Estudiante' o 'Encargado'

        public int IntentosFallidos { get; set; } = 0;
        public bool EstaBloqueado { get; set; } = false;
        public DateTime? BloqueadoHasta { get; set; }

        public bool EstaPenalizado { get; set; } = false;

        public ICollection<Reserva>? Reservas { get; set; }
    }
}