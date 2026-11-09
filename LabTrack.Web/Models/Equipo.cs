using System.ComponentModel.DataAnnotations;

namespace LabTrack.Web.Models
{
    public class Equipo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código de inventario es obligatorio.")]
        [StringLength(50)]
        public string CodigoInventario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione una categoría.")]
        public string Categoria { get; set; } = string.Empty; // 'Cómputo', 'Electrónica', 'Redes'

        [Required]
        public string Estado { get; set; } = "Disponible"; // Disponible, Prestado, En Mantenimiento, Fuera de Servicio

        [DataType(DataType.Date)]
        public DateTime FechaUltimaCalibracion { get; set; }

        public ICollection<Reserva>? Reservas { get; set; }
    }
}