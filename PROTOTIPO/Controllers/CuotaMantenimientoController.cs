using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;

namespace PROTOTIPO.Controllers
{
    public class CuotaMantenimientoController : Controller
    {
        private readonly AppDbContext _context;

        public CuotaMantenimientoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CuotaMantenimiento
        public async Task<IActionResult> Index()
        {
            var cuotas = await _context.CuotaMantenimientos
                .Include(c => c.IdUnidadNavigation)
                    .ThenInclude(u => u.IdCondominioNavigation)
                .OrderByDescending(c => c.IdCuota)
                .ToListAsync();

            return View(cuotas);
        }

        // GET: CuotaMantenimiento/Create
        public IActionResult Create()
        {
            ViewData["IdUnidad"] = new SelectList(
                _context.UnidadHabitacionals.OrderBy(u => u.Numero),
                "IdUnidad",
                "Numero"
            );

            return View();
        }

        // POST: CuotaMantenimiento/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCuota,Periodo,Monto,Estado,IdUnidad")] CuotaMantenimiento cuota)
        {
            if (ModelState.IsValid)
            {
                // 🔥 FORZAR ESTADO INICIAL
                cuota.Estado = "Pendiente";

                _context.Add(cuota);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["IdUnidad"] = new SelectList(
                _context.UnidadHabitacionals,
                "IdUnidad",
                "Numero",
                cuota.IdUnidad
            );

            return View(cuota);
        }

        // GET: CuotaMantenimiento/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cuota = await _context.CuotaMantenimientos.FindAsync(id);
            if (cuota == null)
                return NotFound();

            ViewData["IdUnidad"] = new SelectList(
                _context.UnidadHabitacionals,
                "IdUnidad",
                "Numero",
                cuota.IdUnidad
            );

            return View(cuota);
        }

        // POST: CuotaMantenimiento/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCuota,Periodo,Monto,Estado,IdUnidad")] CuotaMantenimiento cuota)
        {
            if (id != cuota.IdCuota)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuota);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuotaExists(cuota.IdCuota))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["IdUnidad"] = new SelectList(_context.UnidadHabitacionals, "IdUnidad", "Numero", cuota.IdUnidad);
            return View(cuota);
        }

        // GET: CuotaMantenimiento/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var cuota = await _context.CuotaMantenimientos
                .Include(c => c.IdUnidadNavigation)
                .FirstOrDefaultAsync(m => m.IdCuota == id);

            if (cuota == null)
                return NotFound();

            return View(cuota);
        }

        // POST: CuotaMantenimiento/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuota = await _context.CuotaMantenimientos.FindAsync(id);

            if (cuota != null)
            {
                _context.CuotaMantenimientos.Remove(cuota);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CuotaExists(int id)
        {
            return _context.CuotaMantenimientos.Any(e => e.IdCuota == id);
        }
    }
}