using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;

namespace PROTOTIPO.Controllers
{
    public class IncidenciaMantenimientoesController : Controller
    {
        private readonly AppDbContext _context;

        public IncidenciaMantenimientoesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: IncidenciaMantenimientoes
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.IncidenciaMantenimientos.Include(i => i.IdUnidadNavigation).Include(i => i.IdUsuarioNavigation);
            return View(await appDbContext.ToListAsync());
        }

        // GET: IncidenciaMantenimientoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incidenciaMantenimiento = await _context.IncidenciaMantenimientos
                .AsNoTracking()
                .Include(i => i.IdUnidadNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .Include(i => i.ImagenIncidencia)
                .FirstOrDefaultAsync(m => m.IdIncidencia == id);

            if (incidenciaMantenimiento == null)
            {
                return NotFound();
            }

            // Debug: verificar si las imágenes se cargaron
            System.Diagnostics.Debug.WriteLine($"[Details] Incidencia {id}: {incidenciaMantenimiento.ImagenIncidencia?.Count ?? 0} imágenes cargadas");

            if (incidenciaMantenimiento.ImagenIncidencia != null && incidenciaMantenimiento.ImagenIncidencia.Any())
            {
                foreach (var img in incidenciaMantenimiento.ImagenIncidencia)
                {
                    System.Diagnostics.Debug.WriteLine($"  [Details] Imagen ID {img.IdImagen}: {img.Url}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[Details] ImagenIncidencia es null o vacío");

                // Intenta cargar manualmente si el Include falló
                var imagenesDirectas = await _context.ImagenIncidencia
                    .Where(x => x.IdIncidencia == id)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"[Details] Consulta directa encontró {imagenesDirectas.Count} imágenes");

                if (imagenesDirectas.Any())
                {
                    incidenciaMantenimiento.ImagenIncidencia = imagenesDirectas;
                }
            }

            return View(incidenciaMantenimiento);
        }

        // GET: IncidenciaMantenimientoes/Create
        public IActionResult Create()
        {
            ViewData["IdUnidad"] = new SelectList(_context.UnidadHabitacionals, "IdUnidad", "Numero");
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "Nombre");
            return View();
        }

        // POST: IncidenciaMantenimientoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdIncidencia,Descripcion,Estado,FechaSolicitud,FechaCierre,IdUnidad,IdUsuario, TecnicoAsignado")] IncidenciaMantenimiento incidenciaMantenimiento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(incidenciaMantenimiento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUnidad"] = new SelectList(_context.UnidadHabitacionals, "IdUnidad", "IdUnidad", incidenciaMantenimiento.IdUnidad);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "Email", incidenciaMantenimiento.IdUsuario);
            return View(incidenciaMantenimiento);
        }

        // GET: IncidenciaMantenimientoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incidenciaMantenimiento = await _context.IncidenciaMantenimientos.FindAsync(id);
            if (incidenciaMantenimiento == null)
            {
                return NotFound();
            }
            ViewData["IdUnidad"] = new SelectList(_context.UnidadHabitacionals, "IdUnidad", "Numero", incidenciaMantenimiento.IdUnidad);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "Nombre", incidenciaMantenimiento.IdUsuario);
            return View(incidenciaMantenimiento);
        }

        // POST: IncidenciaMantenimientoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdIncidencia,Descripcion,Estado,FechaSolicitud,FechaCierre,IdUnidad,IdUsuario, TecnicoAsignado")] IncidenciaMantenimiento incidenciaMantenimiento)
        {
            if (id != incidenciaMantenimiento.IdIncidencia)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(incidenciaMantenimiento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncidenciaMantenimientoExists(incidenciaMantenimiento.IdIncidencia))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUnidad"] = new SelectList(_context.UnidadHabitacionals, "IdUnidad", "IdUnidad", incidenciaMantenimiento.IdUnidad);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", incidenciaMantenimiento.IdUsuario);
            ViewData["TecnicoAsignado"] = new SelectList(_context.Usuarios, "IdUsuario", "Nombre", incidenciaMantenimiento.TecnicoAsignado);
            return View(incidenciaMantenimiento);
        }

        // GET: IncidenciaMantenimientoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incidenciaMantenimiento = await _context.IncidenciaMantenimientos
                .Include(i => i.IdUnidadNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdIncidencia == id);
            if (incidenciaMantenimiento == null)
            {
                return NotFound();
            }

            return View(incidenciaMantenimiento);
        }

        // POST: IncidenciaMantenimientoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var incidenciaMantenimiento = await _context.IncidenciaMantenimientos.FindAsync(id);
            if (incidenciaMantenimiento != null)
            {
                _context.IncidenciaMantenimientos.Remove(incidenciaMantenimiento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IncidenciaMantenimientoExists(int id)
        {
            return _context.IncidenciaMantenimientos.Any(e => e.IdIncidencia == id);
        }
    }
}
