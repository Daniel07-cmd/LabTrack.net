using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabTrack.Web.Data;

namespace LabTrack.Web.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly LabTrackContext _context;

        public CatalogoController(LabTrackContext context)
        {
            _context = context;
        }

        // GET: Catalogo (CU-02)
        public async Task<IActionResult> Index(string categoria)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            // Si el usuario es Encargado, redirigirlo a su panel principal de gestión
            string? rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol == "Encargado")
            {
                return RedirectToAction("Index", "GestionReservas");
            }

            var equiposQuery = _context.Equipos.AsQueryable();

            if (!string.IsNullOrEmpty(categoria))
            {
                equiposQuery = equiposQuery.Where(e => e.Categoria == categoria);
            }

            ViewData["CategoriaActual"] = categoria;
            return View(await equiposQuery.ToListAsync());
        }
    }
}