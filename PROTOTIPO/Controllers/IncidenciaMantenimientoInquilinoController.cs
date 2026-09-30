using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;
using System.Security.Claims;

namespace PROTOTIPO.Controllers
{
    [Authorize(Roles = "Inquilino")]
    public class IncidenciaMantenimientoInquilinoController : Controller
    {
        private readonly AppDbContext _context;

        public IncidenciaMantenimientoInquilinoController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // MIS INCIDENCIAS
        // =========================
        public async Task<IActionResult> Index(string estado)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var query = _context.IncidenciaMantenimientos
                .Where(i => i.IdUsuario == userId);

            // 🔹 FILTRO
            if (!string.IsNullOrEmpty(estado))
            {
                if (estado == "Activas")
                {
                    query = query.Where(i => i.Estado != "Resuelta" && i.Estado != "Cerrada");
                }
                else if (estado == "Resueltas")
                {
                    query = query.Where(i => i.Estado == "Resuelta");
                }
            }

            var incidencias = await query.ToListAsync();

            // 🔹 ESTADOS (cards)
            var porEstado = incidencias
                .GroupBy(i => i.Estado)
                .Select(g => new { Estado = g.Key, Total = g.Count() })
                .ToList();

            ViewBag.PorEstado = porEstado;

            // 🔹 POR MES (gráfico)
            var porMes = incidencias
                .Where(i => i.FechaSolicitud != null)
                .GroupBy(i => new { i.FechaSolicitud.Value.Year, i.FechaSolicitud.Value.Month })
                .Select(g => new
                {
                    Mes = $"{g.Key.Month}/{g.Key.Year}",
                    Total = g.Count()
                })
                .ToList();

            ViewBag.PorMes = porMes;

            // 🔹 estado actual (para UI)
            ViewBag.EstadoActual = estado;

            return View(incidencias);
        }

        // =========================
        // CREAR (GET)
        // =========================
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // CREAR (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [FromForm] string descripcion,
    [FromForm] List<IFormFile> imagenes) // <-- [FromForm] asegura el binding correcto
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var contrato = await _context.ContratoAlquilers
                .FirstOrDefaultAsync(c => c.IdUsuario == userId);

            if (contrato == null)
            {
                ModelState.AddModelError("", "No tiene una unidad asignada.");
                return View();
            }

            var incidencia = new IncidenciaMantenimiento
            {
                Descripcion = descripcion,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.Now,
                IdUsuario = userId,
                IdUnidad = contrato.IdUnidad
            };

            _context.IncidenciaMantenimientos.Add(incidencia);
            await _context.SaveChangesAsync(); // Aquí se genera el IdIncidencia

            // Validamos estrictamente que la lista contenga elementos y que no pesen 0 bytes
            if (imagenes != null && imagenes.Count > 0)
            {
                // Definir y asegurar la ruta del directorio
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var imagen in imagenes)
                {
                    if (imagen.Length > 0) // Validar que el archivo no esté corrupto/vacío
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(imagen.FileName);
                        var path = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await imagen.CopyToAsync(stream);
                        }

                        _context.ImagenIncidencia.Add(new ImagenIncidencium
                        {
                            Url = "/uploads/" + fileName,
                            IdIncidencia = incidencia.IdIncidencia // Usamos el ID generado arriba
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        // =========================
        // DETALLE
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var incidencia = await _context.IncidenciaMantenimientos
                .Include(i => i.ImagenIncidencia)
                .FirstOrDefaultAsync(i =>
                    i.IdIncidencia == id &&
                    i.IdUsuario == userId);

            if (incidencia == null)
                return NotFound();

            return View(incidencia);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var incidencia = await _context.IncidenciaMantenimientos
                .FirstOrDefaultAsync(i => i.IdIncidencia == id && i.IdUsuario == userId);

            if (incidencia == null)
                return NotFound();

            return View(incidencia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string descripcion)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var incidencia = await _context.IncidenciaMantenimientos
                .FirstOrDefaultAsync(i => i.IdIncidencia == id && i.IdUsuario == userId);

            if (incidencia == null)
                return NotFound();

            incidencia.Descripcion = descripcion;

            _context.Update(incidencia);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Incidencia actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var incidencia = await _context.IncidenciaMantenimientos
                .FirstOrDefaultAsync(i => i.IdIncidencia == id && i.IdUsuario == userId);

            if (incidencia == null)
                return NotFound();

            _context.IncidenciaMantenimientos.Remove(incidencia);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Incidencia eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}