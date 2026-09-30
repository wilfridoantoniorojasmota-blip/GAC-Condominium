using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;

namespace PROTOTIPO.Controllers
{
    public class AreaComunsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AreaComunsController(
            AppDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: AreaComuns
        public async Task<IActionResult> Index(string? buscar)
        {
            var query = _context.AreaComuns
                .Include(a => a.IdCondominioNavigation)
                .Include(a => a.ImagenAreas)
                .Include(a => a.ReservaAreaComuns)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(a => a.Nombre!.Contains(buscar));
            }

            var areas = await query
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            var hoy = DateOnly.FromDateTime(DateTime.Today);

            ViewBag.TotalAreas = areas.Count;

            ViewBag.TotalReservas = areas.Sum(a => a.ReservaAreaComuns.Count);

            ViewBag.ReservadasHoy = areas.Count(a =>
                a.ReservaAreaComuns.Any(r => r.Fecha == hoy));

            ViewBag.DisponiblesHoy = ViewBag.TotalAreas - ViewBag.ReservadasHoy;

            ViewData["Buscar"] = buscar;

            return View(areas);
        }
        // GET: AreaComuns/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(
                _context.Condominios,
                "IdCondominio",
                "Nombre");

            return View();
        }

        // POST: AreaComuns/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("IdArea,Nombre,Descripcion,IdCondominio")] AreaComun areaComun,
            IFormFile imagenFile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(areaComun);
                await _context.SaveChangesAsync();

                // GUARDAR IMAGEN
                if (imagenFile != null && imagenFile.Length > 0)
                {
                    string carpeta = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "source");

                    if (!Directory.Exists(carpeta))
                    {
                        Directory.CreateDirectory(carpeta);
                    }

                    string nombreArchivo = Guid.NewGuid().ToString()
                        + Path.GetExtension(imagenFile.FileName);

                    string rutaCompleta = Path.Combine(
                        carpeta,
                        nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await imagenFile.CopyToAsync(stream);
                    }

                    ImagenArea imagenArea = new ImagenArea
                    {
                        Url = "/source/" + nombreArchivo,
                        IdArea = areaComun.IdArea
                    };

                    _context.ImagenAreas.Add(imagenArea);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["IdCondominio"] = new SelectList(
                _context.Condominios,
                "IdCondominio",
                "Nombre",
                areaComun.IdCondominio);

            return View(areaComun);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Index));

            var areaComun = await _context.AreaComuns
                .Include(a => a.IdCondominioNavigation)
                .Include(a => a.ImagenAreas)
                .Include(a => a.ReservaAreaComuns)
                .FirstOrDefaultAsync(a => a.IdArea == id);

            if (areaComun == null)
                return RedirectToAction(nameof(Index));

            return View(areaComun);
        }

        // GET: AreaComuns/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var areaComun = await _context.AreaComuns.FindAsync(id);

            if (areaComun == null)
            {
                return NotFound();
            }

            ViewData["IdCondominio"] = new SelectList(
                _context.Condominios,
                "IdCondominio",
                "IdCondominio",
                areaComun.IdCondominio);

            return View(areaComun);
        }

        // POST: AreaComuns/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdArea,Nombre,Descripcion,IdCondominio")] AreaComun areaComun)
        {
            if (id != areaComun.IdArea)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(areaComun);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AreaComunExists(areaComun.IdArea))
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

            ViewData["IdCondominio"] = new SelectList(
                _context.Condominios,
                "IdCondominio",
                "IdCondominio",
                areaComun.IdCondominio);

            return View(areaComun);
        }

        // GET: AreaComuns/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var areaComun = await _context.AreaComuns
                .Include(a => a.IdCondominioNavigation)
                .Include(a => a.ImagenAreas)
                .FirstOrDefaultAsync(m => m.IdArea == id);

            if (areaComun == null)
            {
                return NotFound();
            }

            return View(areaComun);
        }

        // POST: AreaComuns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var areaComun = await _context.AreaComuns
                .Include(a => a.ImagenAreas)
                .FirstOrDefaultAsync(a => a.IdArea == id);

            if (areaComun != null)
            {
                // ELIMINAR IMÁGENES FÍSICAS
                foreach (var imagen in areaComun.ImagenAreas)
                {
                    if (!string.IsNullOrEmpty(imagen.Url))
                    {
                        string rutaArchivo = Path.Combine(
                            _webHostEnvironment.WebRootPath,
                            imagen.Url.TrimStart('/'));

                        if (System.IO.File.Exists(rutaArchivo))
                        {
                            System.IO.File.Delete(rutaArchivo);
                        }
                    }
                }

                _context.AreaComuns.Remove(areaComun);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool AreaComunExists(int id)
        {
            return _context.AreaComuns.Any(e => e.IdArea == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetReservasCalendario(int idArea)
        {
            var reservas = await _context.ReservaAreaComuns
                .Where(r => r.IdArea == idArea)
                .ToListAsync();

            var eventos = reservas
                .Where(r => r.Fecha.HasValue && r.HoraInicio.HasValue && r.HoraFin.HasValue)
                .Select(r =>
                {
                    var fechaBase = r.Fecha.Value.ToDateTime(TimeOnly.MinValue);

                    return new
                    {
                        title = "Reservado",
                        start = fechaBase.Add(r.HoraInicio.Value),
                        end = fechaBase.Add(r.HoraFin.Value),
                        color = "#dc3545"
                    };
                })
                .ToList();

            return Json(eventos);
        }
    }
}