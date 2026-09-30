using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;
using PROTOTIPO.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace PROTOTIPO.Controllers
{
    public class CondominiosController : Controller
    {
        private readonly AppDbContext _context;

        public CondominiosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Condominios
        public async Task<IActionResult> Index()
        {
            var dbCondominiosContext = _context.Condominios.Include(c => c.IdSectorNavigation);
            return View(await dbCondominiosContext.ToListAsync());
        }

        // GET: Condominios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var condominio = await _context.Condominios
                .Include(c => c.IdSectorNavigation)
                .FirstOrDefaultAsync(m => m.IdCondominio == id);
            if (condominio == null)
            {
                return NotFound();
            }

            return View(condominio);
        }

        // GET: Condominios/Create
        public IActionResult Create()
        {
            ViewData["IdSector"] = new SelectList(_context.Sectors, "IdSector", "Nombre");
            return View();
        }

        // POST: Condominios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCondominio,Nombre,Direccion,Telefono,Email,Tamano,PaginaWeb,Instagram,Facebook,Rnc,IdSector")] Condominio condominio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(condominio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdSector"] = new SelectList(_context.Sectors, "IdSector", "Nombre", condominio.IdSector);
            return View(condominio);
        }

        // GET: Condominios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var condominio = await _context.Condominios.FindAsync(id);
            if (condominio == null)
            {
                return NotFound();
            }
            ViewData["IdSector"] = new SelectList(_context.Sectors, "IdSector", "Nombre", condominio.IdSector);
            return View(condominio);
        }

        // POST: Condominios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCondominio,Nombre,Direccion,Telefono,Email,Tamano,PaginaWeb,Instagram,Facebook,Rnc,IdSector")] Condominio condominio)
        {
            if (id != condominio.IdCondominio)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(condominio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CondominioExists(condominio.IdCondominio))
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
            ViewData["IdSector"] = new SelectList(_context.Sectors, "IdSector", "Nombre", condominio.IdSector);
            return View(condominio);
        }

        // GET: Condominios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var condominio = await _context.Condominios
                .Include(c => c.IdSectorNavigation)
                .FirstOrDefaultAsync(m => m.IdCondominio == id);
            if (condominio == null)
            {
                return NotFound();
            }

            return View(condominio);
        }

        // POST: Condominios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var condominio = await _context.Condominios.FindAsync(id);
            if (condominio != null)
            {
                _context.Condominios.Remove(condominio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CondominioExists(int id)
        {
            return _context.Condominios.Any(e => e.IdCondominio == id);
        }
        // GET: Condominios/Bienvenida/5
        public async Task<IActionResult> Bienvenida(int? id, int page = 1)
        {
            id ??= 1;
            const int pageSize = 6; // Ajusta la cantidad de unidades por página aquí

            var condominio = await _context.Condominios
                .Include(c => c.IdSectorNavigation)
                .FirstOrDefaultAsync(c => c.IdCondominio == id);

            if (condominio == null)
                return NotFound();

            // Query base para unidades de este condominio
            var unidadesQuery = _context.UnidadHabitacionals
                .Where(u => u.IdCondominio == id)
                .Select(u => new UnidadLandingVM
                {
                    IdUnidad = u.IdUnidad,
                    Numero = u.Numero,
                    Tipo = u.Tipo,
                    Estado = u.Estado,
                    Descripcion = u.Descripcion,
                    Imagen = u.ImagenUnidads
                        .OrderByDescending(i => i.IdImagenUnidad)
                        .Select(i => i.Url)
                        .FirstOrDefault(),

                    CantidadHabitaciones = u.CantidadHabitaciones,
                    CantidadBanos = u.CantidadBanos,
                    MetrosCuadrados = u.MetrosCuadrados,
                    SalaEstar = u.SalaEstar,
                    Balcon = u.Balcon,
                    Terraza = u.Terraza,
                    Jacuzzi = u.Jacuzzi,
                    Picuzzi = u.Picuzzi
                })
                .AsNoTracking();

            // 1. Conteos y cálculo de paginación sobre la BD completa
            var totalUnidadesCount = await unidadesQuery.CountAsync();
            var disponiblesCount = await unidadesQuery.CountAsync(u => u.Estado == "Disponible");
            var ocupadasCount = await unidadesQuery.CountAsync(u => u.Estado == "Ocupada");

            var totalPages = (int)Math.Ceiling(totalUnidadesCount / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // 2. Aplicar paginación a las unidades
            var unidadesPaginadas = await unidadesQuery
                .OrderBy(u => u.IdUnidad)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var unidadesDisponibles = await unidadesQuery
                .Where(u => u.Estado == "Disponible")
                .OrderBy(u => u.IdUnidad)
                .ToListAsync();

            var areas = await _context.AreaComuns
                .Include(a => a.ImagenAreas)
                .Where(a => a.IdCondominio == id)
                .AsNoTracking()
                .ToListAsync();

            // 3. Pasar parámetros de paginación en ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CondominioId = id;

            var vm = new CondominioLandingVM
            {
                Condominio = condominio,
                Unidades = unidadesPaginadas,
                UnidadesDisponibles = unidadesDisponibles,
                Areas = areas,
                TotalUnidades = totalUnidadesCount,
                Disponibles = disponiblesCount,
                Ocupadas = ocupadasCount,
                TotalAreas = areas.Count
            };

            return View(vm);
        }

        public async Task<IActionResult> DetallesUnidad(int id)
        {
            var unidad = await _context.UnidadHabitacionals
                .Where(u => u.IdUnidad == id)
                .Select(u => new
                {
                    u.Numero,
                    u.Tipo,
                    u.Estado,
                    u.Descripcion,
                    u.CantidadHabitaciones,
                    u.CantidadBanos,
                    u.MetrosCuadrados,
                    u.SalaEstar,
                    u.Balcon,
                    u.Terraza,
                    u.Jacuzzi,
                    u.Picuzzi,
                    Imagen = u.ImagenUnidads
                        .OrderByDescending(i => i.IdImagenUnidad)
                        .Select(i => i.Url)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (unidad == null)
                return NotFound();

            return Json(unidad);
        }
    }

}
