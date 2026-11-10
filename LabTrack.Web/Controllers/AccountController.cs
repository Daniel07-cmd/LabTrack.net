using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabTrack.Web.Data;
using LabTrack.Web.Models;
using LabTrack.Web.Models.ViewModels;
using LabTrack.Web.Services;

namespace LabTrack.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly LabTrackContext _context;

        public AccountController(LabTrackContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(model);
            }

            if (usuario.EstaBloqueado)
            {
                if (usuario.BloqueadoHasta.HasValue && DateTime.Now < usuario.BloqueadoHasta.Value)
                {
                    ModelState.AddModelError(string.Empty, "Cuenta bloqueada temporalmente por seguridad tras 3 intentos fallidos consecutivos.");
                    return View(model);
                }
                else
                {
                    usuario.EstaBloqueado = false;
                    usuario.IntentosFallidos = 0;
                    usuario.BloqueadoHasta = null;
                }
            }

            if (!PasswordHasher.Verify(model.Password, usuario.Password))
            {
                usuario.IntentosFallidos++;
                if (usuario.IntentosFallidos >= 3)
                {
                    usuario.EstaBloqueado = true;
                    usuario.BloqueadoHasta = DateTime.Now.AddMinutes(5);
                    ModelState.AddModelError(string.Empty, "Alerta de seguridad: Cuenta bloqueada por 3 intentos fallidos.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Credenciales inválidas. Intentos fallidos: {usuario.IntentosFallidos}/3");
                }
                await _context.SaveChangesAsync();
                return View(model);
            }

            usuario.IntentosFallidos = 0;
            usuario.EstaBloqueado = false;
            usuario.BloqueadoHasta = null;

            if (!PasswordHasher.IsHashed(usuario.Password))
            {
                usuario.Password = PasswordHasher.Hash(model.Password);
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNombre", usuario.NombreCompleto);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol);

            if (usuario.Rol == "Encargado")
            {
                return RedirectToAction("Index", "GestionReservas");
            }
            return RedirectToAction("Index", "Catalogo");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Verificar si el correo ya existe
            bool existe = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
            if (existe)
            {
                ModelState.AddModelError("Email", "El correo electrónico ya se encuentra registrado.");
                return View(model);
            }

            // Crear el nuevo usuario
            var nuevoUsuario = new Usuario
            {
                Email = model.Email,
                NombreCompleto = model.NombreCompleto,
                Password = PasswordHasher.Hash(model.Password),
                Rol = "Estudiante",
                IntentosFallidos = 0,
                EstaBloqueado = false,
                EstaPenalizado = false
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // Mensaje de éxito visible en la misma vista de Registro
            ViewBag.SuccessMessage = "¡Cuenta creada exitosamente! Su usuario ha sido registrado como Estudiante.";

            // Limpiar los campos para que el formulario quede listo si desean otro registro
            ModelState.Clear();

            // Retorna a la vista propia de Registro con un modelo limpio
            return View(new RegisterViewModel());
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}