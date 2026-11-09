using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabTrack.Web.Data;
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
                .FirstOrDefaultAsync(u => u.CodigoInstitucional == model.CodigoInstitucional);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(model);
            }

            // Verificación de bloqueo temporal por seguridad (CU-01 - Excepción 3b)
            if (usuario.EstaBloqueado)
            {
                if (usuario.BloqueadoHasta.HasValue && DateTime.Now < usuario.BloqueadoHasta.Value)
                {
                    ModelState.AddModelError(string.Empty, "Cuenta bloqueada temporalmente por seguridad tras 3 intentos fallidos consecutivos.");
                    return View(model);
                }
                else
                {
                    // Desbloqueo automático tras expirar el tiempo
                    usuario.EstaBloqueado = false;
                    usuario.IntentosFallidos = 0;
                    usuario.BloqueadoHasta = null;
                }
            }

            // Validación de contraseña (hash PBKDF2, con compatibilidad para texto plano)
            if (!PasswordHasher.Verify(model.Password, usuario.Password))
            {
                usuario.IntentosFallidos++;
                if (usuario.IntentosFallidos >= 3)
                {
                    usuario.EstaBloqueado = true;
                    usuario.BloqueadoHasta = DateTime.Now.AddMinutes(5); // Bloqueo por 5 minutos
                    ModelState.AddModelError(string.Empty, "Alerta de seguridad: Cuenta bloqueada por 3 intentos fallidos.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Credenciales inválidas. Intentos fallidos: {usuario.IntentosFallidos}/3");
                }
                await _context.SaveChangesAsync();
                return View(model);
            }

            // Credenciales correctas: Reiniciar contador de intentos (CU-01 - Flujo Principal 5)
            usuario.IntentosFallidos = 0;
            usuario.EstaBloqueado = false;
            usuario.BloqueadoHasta = null;

            // Actualizar contraseñas antiguas en texto plano al nuevo formato hash.
            if (!PasswordHasher.IsHashed(usuario.Password))
            {
                usuario.Password = PasswordHasher.Hash(model.Password);
            }

            await _context.SaveChangesAsync();

            // Configurar sesión de usuario
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNombre", usuario.NombreCompleto);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol);

            // Redirección según rol (CU-01 - Postcondición)
            if (usuario.Rol == "Encargado")
            {
                return RedirectToAction("Index", "GestionReservas");
            }
            return RedirectToAction("Index", "Catalogo");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
