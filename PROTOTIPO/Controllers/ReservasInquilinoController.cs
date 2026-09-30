using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;
using PROTOTIPO.Models.ViewModel;
using System.Security.Claims;

namespace PROTOTIPO.Controllers
{
    [Authorize(Roles = "Inquilino")]
    public class ReservasInquilinoController : Controller
    {
        private readonly AppDbContext _context;

        public ReservasInquilinoController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // LISTADO ÁREAS
        // =========================
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


            var areas = await _context.AreaComuns
                .Select(a => new AreaComunTenantVM
                {
                    IdArea = a.IdArea,
                    Nombre = a.Nombre,
                    Descripcion = a.Descripcion,
                    ImagenUrl = _context.ImagenAreas
                        .Where(i => i.IdArea == a.IdArea)
                        .Select(i => i.Url)
                        .FirstOrDefault()
                })
                .ToListAsync();



            var reservasAbiertas = await _context.ReservaAreaComuns
                .Include(r => r.IdAreaNavigation)
                .Where(r =>
                    r.IdUsuario == userId &&
                    r.Estado != "Cancelada" &&
                    r.Fecha >= DateOnly.FromDateTime(DateTime.Today)
                )
                .OrderBy(r => r.Fecha)
                .ToListAsync();



            ViewBag.ReservasAbiertas = reservasAbiertas;


            return View(areas);
        }
        // =========================
        // DETALLE ÁREA
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var area = await _context.AreaComuns
                .FirstOrDefaultAsync(x => x.IdArea == id);

            if (area == null)
                return NotFound();

            var reservas = await _context.ReservaAreaComuns
                .Where(r => r.IdArea == id)
                .ToListAsync();

            ViewBag.Area = area;
            ViewBag.Reservas = reservas;

            return View();
        }

        // =========================
        // CREAR RESERVA (GET)
        // =========================
        public IActionResult Create(int areaId)
        {
            var area = _context.AreaComuns.FirstOrDefault(x => x.IdArea == areaId);

            ViewBag.AreaNombre = area?.Nombre;

            var model = new ReservaAreaComun
            {
                IdArea = areaId
            };
            var reservas = _context.ReservaAreaComuns
                .Where(r => r.IdArea == areaId &&
                            r.Fecha >= DateOnly.FromDateTime(DateTime.Today) &&
                            r.Estado != "Cancelada")
                .OrderBy(r => r.Fecha)
                .ToList();

            ViewBag.Reservas = reservas;

            return View(model);
        }

        // =========================
        // CREAR RESERVA (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int areaId, DateOnly fecha, TimeSpan horaInicio, TimeSpan horaFin)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            if (fecha < DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("", "No puede reservar una fecha pasada.");
                return View(new ReservaAreaComun { IdArea = areaId });
            }

            if (horaFin <= horaInicio)
            {
                ModelState.AddModelError("", "La hora de fin debe ser mayor a la de inicio.");
                ViewBag.AreaId = areaId;
                return View(new ReservaAreaComun { IdArea = areaId });
            }



            var conflicto = await _context.ReservaAreaComuns.AnyAsync(r =>
                r.IdArea == areaId &&
                r.Fecha == fecha &&
                r.Estado != "Cancelada" &&
                r.HoraInicio != null &&
                r.HoraFin != null &&
                (
                    (horaInicio >= r.HoraInicio && horaInicio < r.HoraFin) ||
                    (horaFin > r.HoraInicio && horaFin <= r.HoraFin) ||
                    (horaInicio <= r.HoraInicio && horaFin >= r.HoraFin)
                )
            );

            var reserva = new ReservaAreaComun
            {
                IdArea = areaId,
                IdUsuario = userId,
                Fecha = fecha,
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                Estado = "Pendiente"
            };

            _context.ReservaAreaComuns.Add(reserva);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reserva creada correctamente.";

            return RedirectToAction(nameof(MisReservas));
        }
        // =========================
        // MIS RESERVAS
        // =========================
        public async Task<IActionResult> MisReservas()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var reservas = await _context.ReservaAreaComuns
                .Include(r => r.IdAreaNavigation)
                .Where(r => r.IdUsuario == userId)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            return View(reservas);
        }

        // =========================
        // CANCELAR
        // =========================
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var reserva = await _context.ReservaAreaComuns
                .FirstOrDefaultAsync(r => r.IdReserva == id && r.IdUsuario == userId);

            if (reserva == null)
                return NotFound();

            reserva.Estado = "Cancelada";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MisReservas));
        }
    }
}