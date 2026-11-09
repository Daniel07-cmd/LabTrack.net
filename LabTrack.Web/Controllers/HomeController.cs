using LabTrack.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LabTrack.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly LabTrackContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(LabTrackContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Home/Index (Dashboard Ejecutivo)
        public async Task<IActionResult> Index()
        {
            // Carga la lista de equipos para calcular los KPIs dinámicos en la vista
            var equipos = await _context.Equipos.ToListAsync();
            return View(equipos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new LabTrack.Web.Models.ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}