using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;

namespace PROTOTIPO.Controllers
{
    public class ContratoAlquilerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ContratoAlquilerController> _logger;

        public ContratoAlquilerController(
            AppDbContext context,
            ILogger<ContratoAlquilerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //====================================================
        // INDEX
        //====================================================

        
public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 6;

            // ==============================
            // CONSULTA BASE
            // ==============================

            var query = _context.ContratoAlquilers
                .Include(c => c.IdUsuarioNavigation)
                .Include(c => c.IdUnidadNavigation)
                .OrderByDescending(c => c.IdContrato);


            // ==============================
            // ESTADÍSTICAS GLOBALES
            // ==============================

            // Total de contratos existentes en toda la BD
            var totalContratos = await _context.ContratoAlquilers
                .CountAsync();


            // Contratos activos en toda la BD
            var contratosActivos = await _context.ContratoAlquilers
                .CountAsync(c =>
                    c.Estado != null &&
                    c.Estado.ToLower() == "activo"
                );


            // Contratos finalizados en toda la BD
            var contratosFinalizados = await _context.ContratoAlquilers
                .CountAsync(c =>
                    c.Estado != null &&
                    c.Estado.ToLower() == "finalizado"
                );


            // Ingresos mensuales de contratos activos
            var ingresosMensuales = await _context.ContratoAlquilers
                .Where(c =>
                    c.Estado != null &&
                    c.Estado.ToLower() == "activo"
                )
                .SumAsync(c => c.MontoRenta ?? 0);


            // ==============================
            // PAGINACIÓN
            // ==============================

            var totalPaginas = (int)Math.Ceiling(
                totalContratos / (double)pageSize
            );

            if (page < 1)
                page = 1;

            if (page > totalPaginas && totalPaginas > 0)
                page = totalPaginas;


            // ==============================
            // CONTRATOS DE LA PÁGINA ACTUAL
            // ==============================

            var contratos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // ==============================
            // DATOS PARA LA VISTA
            // ==============================

            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas = totalPaginas;

            // Estadísticas globales
            ViewBag.TotalContratos = totalContratos;
            ViewBag.ContratosActivos = contratosActivos;
            ViewBag.ContratosFinalizados = contratosFinalizados;
            ViewBag.IngresosMensuales = ingresosMensuales;


            // ==============================
            // RETORNAR VISTA
            // ==============================

            return View(contratos);
        }
        //====================================================
        // DETAILS
        //====================================================

        // GET: ContratoAlquiler/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.ContratoAlquilers
                .Include(c => c.IdUsuarioNavigation)
                .Include(c => c.IdUnidadNavigation)
                .FirstOrDefaultAsync(c => c.IdContrato == id);

            if (contrato == null)
            {
                return NotFound();
            }

            return View(contrato);
        }

        // GET: ContratoAlquiler/DetailsPartial/5
        // Útil si quieres mostrar el detalle en un offcanvas/modal en vez de una página completa
        public async Task<IActionResult> DetailsPartial(int id)
        {
            var contrato = await _context.ContratoAlquilers
                .Include(c => c.IdUsuarioNavigation)
                .Include(c => c.IdUnidadNavigation)
                .FirstOrDefaultAsync(c => c.IdContrato == id);

            if (contrato == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", contrato);
        }

        //====================================================
        // EDIT
        //====================================================

        // GET: ContratoAlquiler/Edit/5
        // GET: Contratos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contratoAlquiler = await _context.ContratoAlquilers.FindAsync(id);
            if (contratoAlquiler == null) return NotFound();

            // Cargar listas seleccionando por defecto el Id actual del contrato
            ViewBag.IdUsuario = new SelectList(
                _context.Usuarios.Select(u => new { u.IdUsuario, NombreCompleto = u.Nombre + " " + u.Apellido }),
                "IdUsuario",
                "NombreCompleto",
                contratoAlquiler.IdUsuario
            );

            ViewBag.IdUnidad = new SelectList(
                _context.UnidadHabitacionals.Select(u => new { u.IdUnidad, u.Numero }),
                "IdUnidad",
                "Numero",
                contratoAlquiler.IdUnidad
            );

            return View(contratoAlquiler);
        }

        // POST: ContratoAlquiler/Edit/5
        // NOTA: ajusta la lista del [Bind] a los nombres reales de las
        // propiedades de tu modelo ContratoAlquiler si difieren.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    [Bind("IdContrato,IdUsuario,IdUnidad,FechaInicio,FechaFin,MontoRenta,Estado")] ContratoAlquiler contratoAlquiler)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                         Request.Headers["Accept"].ToString().Contains("application/json");

            _logger.LogInformation("Edit POST contrato id={Id}", id);

            if (id != contratoAlquiler.IdContrato)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    return PartialView("_EditPartial", contratoAlquiler);
                }

                // Si la validación falla en la vista normal, volvemos a llenar los desplegables:
                ViewBag.IdUsuario = new SelectList(
                    _context.Usuarios.Select(u => new { u.IdUsuario, NombreCompleto = u.Nombre + " " + u.Apellido }),
                    "IdUsuario",
                    "NombreCompleto",
                    contratoAlquiler.IdUsuario
                );

                ViewBag.IdUnidad = new SelectList(
                    _context.UnidadHabitacionals.Select(u => new { u.IdUnidad, u.Numero }),
                    "IdUnidad",
                    "Numero",
                    contratoAlquiler.IdUnidad
                );

                return View(contratoAlquiler);
            }

            try
            {
                var contratoDb = await _context.ContratoAlquilers
                    .FirstOrDefaultAsync(c => c.IdContrato == id);

                if (contratoDb == null)
                {
                    return NotFound();
                }

                // UPDATE EXPLÍCITO: Si el formulario envía 0/null, conservamos el valor que ya tenía la BD
                if (contratoAlquiler.IdUsuario != 0)
                {
                    contratoDb.IdUsuario = contratoAlquiler.IdUsuario;
                }

                if (contratoAlquiler.IdUnidad.HasValue && contratoAlquiler.IdUnidad.Value != 0)
                {
                    contratoDb.IdUnidad = contratoAlquiler.IdUnidad;
                }

                contratoDb.FechaInicio = contratoAlquiler.FechaInicio;
                contratoDb.FechaFin = contratoAlquiler.FechaFin;
                contratoDb.MontoRenta = contratoAlquiler.MontoRenta;
                contratoDb.Estado = contratoAlquiler.Estado;

                await _context.SaveChangesAsync();

                if (isAjax)
                {
                    var updated = await _context.ContratoAlquilers
                        .Include(c => c.IdUsuarioNavigation)
                        .Include(c => c.IdUnidadNavigation)
                        .FirstOrDefaultAsync(c => c.IdContrato == id);

                    return Json(new
                    {
                        success = true,
                        id = updated?.IdContrato,
                        idUnidad = updated?.IdUnidad,
                        estado = updated?.Estado,
                        montoRenta = updated?.MontoRenta,
                        fechaInicio = updated?.FechaInicio?.ToString("dd/MM/yyyy"),
                        fechaFin = updated?.FechaFin?.ToString("dd/MM/yyyy"),
                        inquilino = updated?.IdUsuarioNavigation != null
                            ? $"{updated.IdUsuarioNavigation.Nombre} {updated.IdUsuarioNavigation.Apellido}"
                            : null
                    });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editando contrato {Id}", id);

                if (isAjax)
                {
                    return Json(new { success = false, message = "Error interno al editar el contrato" });
                }

                ModelState.AddModelError("", "Error al editar el contrato.");

                ViewBag.IdUsuario = new SelectList(
                    _context.Usuarios.Select(u => new { u.IdUsuario, NombreCompleto = u.Nombre + " " + u.Apellido }),
                    "IdUsuario",
                    "NombreCompleto",
                    contratoAlquiler.IdUsuario
                );

                ViewBag.IdUnidad = new SelectList(
                    _context.UnidadHabitacionals.Select(u => new { u.IdUnidad, u.Numero }),
                    "IdUnidad",
                    "Numero",
                    contratoAlquiler.IdUnidad
                );

                return View(contratoAlquiler);
            }
        }

        //====================================================
        // FINALIZAR CONTRATO
        //====================================================

        // POST: ContratoAlquiler/FinalizarContrato/5
        // A diferencia del método del mismo nombre en UnidadHabitacionalsController
        // (que recibe el id de la UNIDAD), este recibe directamente el id del CONTRATO.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarContrato(int id)
        {
            var isAjax =
                Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Accept"].ToString().Contains("application/json");

            var contrato = await _context.ContratoAlquilers
                .FirstOrDefaultAsync(c => c.IdContrato == id);

            if (contrato == null)
            {
                if (isAjax)
                {
                    return Json(new { success = false, message = "Contrato no encontrado." });
                }

                return NotFound();
            }

            if (contrato.Estado != null && contrato.Estado.ToLower() == "finalizado")
            {
                if (isAjax)
                {
                    return Json(new { success = false, message = "El contrato ya está finalizado." });
                }

                ModelState.AddModelError("", "El contrato ya está finalizado.");
                return RedirectToAction(nameof(Details), new { id });
            }

            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Cerrar contrato
                    contrato.Estado = "finalizado";
                    contrato.FechaFin = DateOnly.FromDateTime(DateTime.Now);

                    _context.ContratoAlquilers.Update(contrato);

                    // Liberar la unidad asociada, si tiene una
                    if (contrato.IdUnidad != null)
                    {
                        var unidad = await _context.UnidadHabitacionals
                            .FirstOrDefaultAsync(u => u.IdUnidad == contrato.IdUnidad);

                        if (unidad != null)
                        {
                            unidad.Estado = "disponible";
                            _context.UnidadHabitacionals.Update(unidad);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    _logger.LogInformation(
                        "Contrato {IdContrato} finalizado correctamente",
                        id);

                    if (isAjax)
                    {
                        return Json(new
                        {
                            success = true,
                            id = contrato.IdContrato,
                            estado = contrato.Estado,
                            fechaFin = contrato.FechaFin?.ToString("dd/MM/yyyy")
                        });
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();

                    _logger.LogError(
                        ex,
                        "Error finalizando contrato {IdContrato}",
                        id);

                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Error al finalizar el contrato." });
                    }

                    ModelState.AddModelError("", "Error al finalizar el contrato.");
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
        }

        private bool ContratoAlquilerExists(int id)
        {
            return _context.ContratoAlquilers.Any(e => e.IdContrato == id);
        }
    }
}