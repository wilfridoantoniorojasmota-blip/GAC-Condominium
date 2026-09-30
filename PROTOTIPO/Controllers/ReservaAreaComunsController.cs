using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;

namespace PROTOTIPO.Controllers
{
    public class ReservaAreaComunsController : Controller
    {
        private readonly AppDbContext _context;

        public ReservaAreaComunsController(AppDbContext context)
        {
            _context = context;
        }

        // =====================
        // API: Obtener reservas por area y fecha (GET)
        // =====================
        [HttpGet]
        public IActionResult GetReservasByAreaAndFecha(int idArea, DateOnly fecha)
        {
            var reservas = _context.ReservaAreaComuns
                .Where(r => r.IdArea == idArea && r.Fecha == fecha)
                .Select(r => new
                {
                    r.IdReserva,
                    Fecha = r.Fecha,
                    HoraInicio = r.HoraInicio,
                    HoraFin = r.HoraFin,
                    r.Estado,
                    Area = r.IdAreaNavigation != null ? r.IdAreaNavigation.Nombre : ""
                })
                .ToList();

            return Json(reservas);
        }

        // =====================
        // INDEX
        // =====================
        public async Task<IActionResult> Index()
        {
            var data = _context.ReservaAreaComuns
                .Include(r => r.IdAreaNavigation)
                .Include(r => r.IdUsuarioNavigation);

            return View(await data.ToListAsync());
        }

        // =====================
        // DETAILS
        // =====================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reserva = await _context.ReservaAreaComuns
                .Include(r => r.IdAreaNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null) return NotFound();

            return View(reserva);
        }

        // =====================
        // CREATE GET
        // =====================
        public IActionResult Create()
        {
            CargarCombos();
            return View();
        }

        // =====================
        // CREATE POST
        // =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservaAreaComun reserva)
        {
            if (ModelState.IsValid)
            {
                // Reglas de negocio:
                // 1) No permitir reservar el mismo día para la misma área si ya existe cualquier reserva (regla solicitada).
                var existeMismoDia = _context.ReservaAreaComuns.Any(r =>
                    r.IdArea == reserva.IdArea &&
                    r.Fecha == reserva.Fecha);

                if (existeMismoDia)
                {
                    ModelState.AddModelError("", "Ya existe una reserva para esa fecha en esta área común.");
                    CargarCombos(reserva.IdArea, reserva.IdUsuario);
                    return View(reserva);
                }

                // 2) Además comprobar solapamiento horario (protección adicional si decide quitar la regla 1 en futuro)
                var conflicto = _context.ReservaAreaComuns.Any(r =>
                    r.IdArea == reserva.IdArea &&
                    r.Fecha == reserva.Fecha &&
                    (
                        (reserva.HoraInicio >= r.HoraInicio && reserva.HoraInicio < r.HoraFin) ||
                        (reserva.HoraFin > r.HoraInicio && reserva.HoraFin <= r.HoraFin) ||
                        (reserva.HoraInicio <= r.HoraInicio && reserva.HoraFin >= r.HoraFin)
                    )
                );

                if (conflicto)
                {
                    ModelState.AddModelError("", "Ya existe una reserva en ese horario para esta área común.");
                    CargarCombos(reserva.IdArea, reserva.IdUsuario);
                    return View(reserva);
                }

                _context.Add(reserva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            CargarCombos(reserva.IdArea, reserva.IdUsuario);
            return View(reserva);
        }

        // =====================
        // EDIT GET  🔥 (ESTO ARREGLA TU 404)
        // =====================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var reserva = await _context.ReservaAreaComuns
                .FindAsync(id);

            if (reserva == null) return NotFound();

            CargarCombos(reserva.IdArea, reserva.IdUsuario);
            return View(reserva);
        }

        // =====================
        // EDIT POST
        // =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReservaAreaComun reserva)
        {
            if (id != reserva.IdReserva)
                return NotFound();

            if (ModelState.IsValid)
            {
                var conflicto = _context.ReservaAreaComuns.Any(r =>
                    r.IdArea == reserva.IdArea &&
                    r.Fecha == reserva.Fecha &&
                    r.IdReserva != reserva.IdReserva &&
                    (
                        (reserva.HoraInicio >= r.HoraInicio && reserva.HoraInicio < r.HoraFin) ||
                        (reserva.HoraFin > r.HoraInicio && reserva.HoraFin <= r.HoraFin) ||
                        (reserva.HoraInicio <= r.HoraInicio && reserva.HoraFin >= r.HoraFin)
                    )
                );

                if (conflicto)
                {
                    ModelState.AddModelError("", "Conflicto: ya existe una reserva en ese horario.");
                    CargarCombos(reserva.IdArea, reserva.IdUsuario);
                    return View(reserva);
                }

                _context.Update(reserva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            CargarCombos(reserva.IdArea, reserva.IdUsuario);
            return View(reserva);
        }

        // =====================
        // DELETE GET
        // =====================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var reserva = await _context.ReservaAreaComuns
                .Include(r => r.IdAreaNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null) return NotFound();

            return View(reserva);
        }

        // =====================
        // DELETE POST
        // =====================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.ReservaAreaComuns.FindAsync(id);

            if (reserva != null)
                _context.ReservaAreaComuns.Remove(reserva);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =====================
        // COMBOS CENTRALIZADOS
        // =====================
        private void CargarCombos(int? idArea = null, int? idUsuario = null)
        {
            ViewData["IdArea"] = new SelectList(
                _context.AreaComuns,
                "IdArea",
                "Nombre",
                idArea);

            ViewData["IdUsuario"] = new SelectList(
                _context.Usuarios,
                "IdUsuario",
                "Nombre",
                idUsuario);
        }
    }
}