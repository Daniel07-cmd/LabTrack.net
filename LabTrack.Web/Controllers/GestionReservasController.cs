using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabTrack.Web.Data;

namespace LabTrack.Web.Controllers
{
    public class GestionReservasController : Controller
    {
        private readonly LabTrackContext _context;

        public GestionReservasController(LabTrackContext context)
        {
            _context = context;
        }

        // GET: Bandeja del Encargado (CU-04)
        public async Task<IActionResult> Index()
        {
            string? rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Encargado") return RedirectToAction("Login", "Account");

            var reservas = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Equipo)
                .Where(r => r.Estado == "Pendiente" || r.Estado == "Aprobada")
                .ToListAsync();

            return View(reservas);
        }

        // POST: Aprobar Solicitud (CU-04)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            if (!EsEncargado()) return RedirectToAction("Login", "Account");

            var reserva = await _context.Reservas.Include(r => r.Equipo).FirstOrDefaultAsync(r => r.Id == id);
            if (reserva == null) return NotFound();

            if (reserva.Estado != "Pendiente")
            {
                TempData["Error"] = "Solo se pueden aprobar reservas en estado Pendiente.";
                return RedirectToAction(nameof(Index));
            }

            reserva.Estado = "Aprobada";
            if (reserva.Equipo != null)
            {
                reserva.Equipo.Estado = "Prestado"; // Actualiza el estado físico del equipo
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Solicitud aprobada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Rechazar Solicitud con Justificación Obligatoria (CU-04 - Excepción 3a)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string justificacion)
        {
            if (!EsEncargado()) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(justificacion))
            {
                TempData["Error"] = "Debe ingresar obligatoriamente un motivo o justificación para rechazar la solicitud.";
                return RedirectToAction(nameof(Index));
            }

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();

            if (reserva.Estado != "Pendiente")
            {
                TempData["Error"] = "Solo se pueden rechazar reservas en estado Pendiente.";
                return RedirectToAction(nameof(Index));
            }

            reserva.Estado = "Rechazada";
            reserva.JustificacionRechazo = justificacion;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Solicitud rechazada con su respectiva justificación.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Registrar Devolución y Penalización Automática (CU-05)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarDevolucion(int id)
        {
            if (!EsEncargado()) return RedirectToAction("Login", "Account");

            var reserva = await _context.Reservas
                .Include(r => r.Equipo)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null) return NotFound();

            if (reserva.Estado != "Aprobada")
            {
                TempData["Error"] = "Solo se puede registrar la devolución de reservas aprobadas.";
                return RedirectToAction(nameof(Index));
            }

            DateTime horaActualServidor = DateTime.Now;
            TimeSpan horaActualTime = horaActualServidor.TimeOfDay;

            reserva.FechaDevolucionReal = horaActualServidor;
            reserva.Estado = "Completada";
            if (reserva.Equipo != null)
            {
                reserva.Equipo.Estado = "Disponible";
            }

            // CU-05 (Excepción 4a): Comparar hora actual del servidor con la hora límite pactada
            if (reserva.FechaUso.Date < horaActualServidor.Date ||
                (reserva.FechaUso.Date == horaActualServidor.Date && horaActualTime > reserva.HoraFin))
            {
                reserva.PenalizacionAplicada = true;
                if (reserva.Usuario != null)
                {
                    reserva.Usuario.EstaPenalizado = true; // Sanción automática al estudiante
                }
                TempData["Warning"] = "Equipo devuelto fuera del horario límite. Penalización automática aplicada al estudiante.";
            }
            else
            {
                TempData["Success"] = "Devolución registrada puntualmente con éxito.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EsEncargado()
            => HttpContext.Session.GetString("UsuarioRol") == "Encargado";
    }
}