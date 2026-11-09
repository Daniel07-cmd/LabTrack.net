using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabTrack.Web.Data;
using LabTrack.Web.Models;

namespace LabTrack.Web.Controllers
{
    public class ReservasController : Controller
    {
        private readonly LabTrackContext _context;

        public ReservasController(LabTrackContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Solicitar(int equipoId, DateTime fechaUso, TimeSpan horaInicio, TimeSpan horaFin)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            if (horaFin <= horaInicio)
            {
                TempData["Error"] = "La hora de fin debe ser posterior a la hora de inicio.";
                return RedirectToAction("Index", "Catalogo");
            }

            if (fechaUso.Date < DateTime.Today)
            {
                TempData["Error"] = "No puede solicitar una reserva para una fecha pasada.";
                return RedirectToAction("Index", "Catalogo");
            }

            var equipo = await _context.Equipos.FindAsync(equipoId);
            if (equipo == null) return NotFound();

            if (equipo.Estado != "Disponible")
            {
                TempData["Error"] = "El equipo seleccionado no está disponible para reserva.";
                return RedirectToAction("Index", "Catalogo");
            }

            // Validar estado de penalización del estudiante (CU-05)
            var estudiante = await _context.Usuarios.FindAsync(usuarioId.Value);
            if (estudiante != null && estudiante.EstaPenalizado)
            {
                TempData["Error"] = "No puede solicitar reservas. Su cuenta se encuentra penalizada por entregas fuera de plazo.";
                return RedirectToAction("Index", "Catalogo");
            }

            // CU-03 (Excepción 4a): Validar si el estudiante ya posee una reserva activa o simultánea
            bool tieneReservaActiva = await _context.Reservas
                .AnyAsync(r => r.UsuarioId == usuarioId.Value && (r.Estado == "Pendiente" || r.Estado == "Aprobada"));

            if (tieneReservaActiva)
            {
                TempData["Error"] = "Ya cuenta con una reserva activa o simultánea en curso.";
                return RedirectToAction("Index", "Catalogo");
            }

            // CU-03 (Excepción 5a): Validar solapamiento / cruce de horarios con otra reserva aprobada
            bool cruceHorarios = await _context.Reservas
                .AnyAsync(r => r.EquipoId == equipoId && r.FechaUso.Date == fechaUso.Date && r.Estado == "Aprobada" &&
                               ((horaInicio >= r.HoraInicio && horaInicio < r.HoraFin) ||
                                (horaFin > r.HoraInicio && horaFin <= r.HoraFin)));

            if (cruceHorarios)
            {
                TempData["Error"] = "El equipo no está disponible en el rango horario seleccionado (cruce con otra reserva aprobada).";
                return RedirectToAction("Index", "Catalogo");
            }

            // Registrar nueva solicitud en estado 'Pendiente'
            var nuevaReserva = new Reserva
            {
                UsuarioId = usuarioId.Value,
                EquipoId = equipoId,
                FechaUso = fechaUso,
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                Estado = "Pendiente"
            };

            _context.Reservas.Add(nuevaReserva);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Solicitud de reserva registrada con éxito en estado Pendiente.";
            return RedirectToAction("Index", "Catalogo");
        }
    }
}