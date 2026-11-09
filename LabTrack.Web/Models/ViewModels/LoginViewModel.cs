using System.ComponentModel.DataAnnotations;

namespace LabTrack.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El código institucional es obligatorio.")]
        [Display(Name = "Código institucional")]
        [StringLength(50)]
        public string CodigoInstitucional { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;
    }
}
