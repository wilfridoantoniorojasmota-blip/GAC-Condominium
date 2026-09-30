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
    public class UnidadHabitacionalsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UnidadHabitacionalsController> _logger;

        public UnidadHabitacionalsController(AppDbContext context, IWebHostEnvironment env, ILogger<UnidadHabitacionalsController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        // GET: UnidadHabitacionals/EditPartial/5
        // Returns a partial view with edit form in offcanvas
        public async Task<IActionResult> EditPartial(int id)
        {
            var unidadHabitacional = await _context.UnidadHabitacionals
                .Include(u => u.IdCondominioNavigation)
                .Include(u => u.ImagenUnidads)
                .Include(u => u.ContratoAlquilers)
                    .ThenInclude(c => c.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdUnidad == id);

            if (unidadHabitacional == null)
            {
                return NotFound();
            }

            ViewData["Condominios"] = new SelectList(_context.Condominios.OrderBy(c => c.Nombre).ToList(), "IdCondominio", "Nombre", unidadHabitacional.IdCondominio);
            return PartialView("_EditPartial", unidadHabitacional);
        }

        // GET: UnidadHabitacionals/AssignPartial/5
        // Returns a partial view with a form to assign a tenant to the unidad
        public async Task<IActionResult> AssignPartial(int id)
        {
            var unidadHabitacional = await _context.UnidadHabitacionals
                .Include(u => u.IdCondominioNavigation)
                .Include(u => u.ImagenUnidads)
                .Include(u => u.ContratoAlquilers)
                    .ThenInclude(c => c.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdUnidad == id);


            if (unidadHabitacional == null)
            {
                return NotFound();
            }



            ViewData["Contratos"] = await _context.ContratoAlquilers
                .Include(c => c.IdUsuarioNavigation)
                .Where(c => c.IdUnidad == null)
                .Select(c => new SelectListItem
                {
                    Value = c.IdContrato.ToString(),

                    Text =
                        c.IdUsuarioNavigation.Nombre
                        + " "
                        + c.IdUsuarioNavigation.Apellido
                        + " - $"
                        + c.MontoRenta
                })
                .ToListAsync();



            return PartialView("_AssignPartial", unidadHabitacional);
        }

        public async Task<IActionResult> ObtenerContrato(int id)
        {
            var contrato = await _context.ContratoAlquilers
                .Include(c => c.IdUsuarioNavigation)
                .FirstOrDefaultAsync(c => c.IdContrato == id);

            if (contrato == null)
            {
                return Json(new
                {
                    success = false
                });
            }

            var usuario = contrato.IdUsuarioNavigation;

            return Json(new
            {
                success = true,

                nombre = usuario.Nombre + " " + usuario.Apellido,

                tipoCliente = usuario.TipoCliente,

                cedula = usuario.Cedula,
                pasaporte = usuario.Pasaporte,
                nacionalidad = usuario.Nacionalidad,

                rnc = usuario.RNC,
                nombreEmpresa = usuario.NombreEmpresa,
                personaAlojada = usuario.PersonaAlojada,

                telefono = usuario.Telefono,

                fechaInicio = contrato.FechaInicio?
                    .ToString("dd/MM/yyyy"),

                fechaFin = contrato.FechaFin?
                    .ToString("dd/MM/yyyy"),

                renta = contrato.MontoRenta?
                    .ToString("N2"),

                estado = contrato.Estado
            });
        }

        // POST: UnidadHabitacionals/Assign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(
     int id,
     int IdContrato)
        {
            _logger.LogInformation(
                "Assign called for unidad {Id} with IdContrato={IdContrato}",
                id,
                IdContrato);


            var unidad = await _context.UnidadHabitacionals
                .FirstOrDefaultAsync(u => u.IdUnidad == id);


            if (unidad == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Unidad no encontrada"
                });
            }


            // Buscar contrato pendiente
            var contrato = await _context.ContratoAlquilers
                .Include(c => c.IdUsuarioNavigation)
                .FirstOrDefaultAsync(c => c.IdContrato == IdContrato);


            if (contrato == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Contrato no encontrado"
                });
            }



            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {

                    // Finalizar contratos anteriores de la unidad
                    var activosPrev = await _context.ContratoAlquilers
                        .Where(c =>
                            c.IdUnidad == id &&
                            c.Estado != null &&
                            c.Estado.ToLower() == "activo")
                        .ToListAsync();


                    if (activosPrev.Any())
                    {
                        foreach (var anterior in activosPrev)
                        {
                            anterior.Estado = "finalizado";

                            if (anterior.FechaFin == null)
                            {
                                anterior.FechaFin =
                                    DateOnly.FromDateTime(DateTime.UtcNow);
                            }

                            _context.ContratoAlquilers.Update(anterior);
                        }
                    }



                    // ASIGNAR CONTRATO EXISTENTE A LA UNIDAD

                    contrato.IdUnidad = id;
                    contrato.Estado = "activo";

                    _context.ContratoAlquilers.Update(contrato);



                    // CAMBIAR ESTADO DE LA UNIDAD

                    unidad.Estado = "Ocupada";

                    _context.UnidadHabitacionals.Update(unidad);



                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();

                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Assign transaction failed for unidad {Id}",
                        id);


                    try
                    {
                        await tx.RollbackAsync();
                    }
                    catch { }



                    return Json(new
                    {
                        success = false,
                        message = "Error guardando asignación",
                        detail = ex.Message
                    });
                }
            }



            // Recargar datos actualizados

            var updated = await _context.UnidadHabitacionals
                .Include(u => u.ImagenUnidads)
                .Include(u => u.ContratoAlquilers)
                    .ThenInclude(c => c.IdUsuarioNavigation)
                .FirstOrDefaultAsync(u => u.IdUnidad == id);



            var imgUrl = updated?
                .ImagenUnidads?
                .FirstOrDefault()?
                .Url;


            if (string.IsNullOrEmpty(imgUrl))
            {
                imgUrl = Url.Content("~/img/placeholder-unidad.jpg");
            }



            var contratoActivo = updated?
                .ContratoAlquilers?
                .FirstOrDefault(c =>
                    c.Estado != null &&
                    c.Estado.ToLower() == "activo");



            var inquilinoName =
                contratoActivo?
                .IdUsuarioNavigation?
                .Nombre
                + " "
                +
                contratoActivo?
                .IdUsuarioNavigation?
                .Apellido;



            return Json(new
            {
                success = true,

                id = updated?.IdUnidad,

                numero = updated?.Numero,

                tipo = updated?.Tipo,

                estado = updated?.Estado,

                descripcion = updated?.Descripcion,

                imgUrl,

                inquilino = inquilinoName,

                usuarioName = inquilinoName
            });
        }


        // GET: UnidadHabitacionals
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 8;

            // Conteos totales globales en BD
            var totalCount = await _context.UnidadHabitacionals.CountAsync();
            var disponiblesCount = await _context.UnidadHabitacionals.CountAsync(u => u.Estado.ToLower() == "disponible");
            var ocupadasCount = await _context.UnidadHabitacionals.CountAsync(u => u.Estado.ToLower() == "ocupada");

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var unidades = await _context.UnidadHabitacionals
                .Include(u => u.IdCondominioNavigation)
                .Include(u => u.ContratoAlquilers)
                    .ThenInclude(c => c.IdUsuarioNavigation)
                .Include(u => u.ImagenUnidads)
                .OrderBy(u => u.IdUnidad)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            // Pasamos los totales a la vista
            ViewBag.TotalCount = totalCount;
            ViewBag.DisponiblesCount = disponiblesCount;
            ViewBag.OcupadasCount = ocupadasCount;

            return View(unidades);
        }
        // GET: UnidadHabitacionals/Details/5
        // GET: UnidadHabitacionals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unidadHabitacional = await _context.UnidadHabitacionals
                .Include(u => u.IdCondominioNavigation)
                .Include(u => u.ImagenUnidads)
                .Include(u => u.ContratoAlquilers)
                    .ThenInclude(c => c.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdUnidad == id);

            if (unidadHabitacional == null)
            {
                return NotFound();
            }

            return View(unidadHabitacional);
        }

        // POST: UnidadHabitacionals/Unassign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unassign(int id)
        {
            _logger.LogInformation("Unassign called for unidad {Id}", id);

            try
            {
                var unidad = await _context.UnidadHabitacionals.FindAsync(id);
                if (unidad == null)
                {
                    _logger.LogWarning("Unassign: unidad {Id} not found", id);
                    return Json(new { success = false, message = "Unidad no encontrada" });
                }

                // load active contratos directly to ensure we update tracked entities
                var activos = await _context.ContratoAlquilers
                    .Where(c => c.IdUnidad == id && c.Estado != null && c.Estado.ToLower() == "activo")
                    .ToListAsync();

                if (activos != null && activos.Count > 0)
                {
                    foreach (var c in activos)
                    {
                        c.Estado = "finalizado";
                        if (c.FechaFin == null)
                        {
                            c.FechaFin = DateOnly.FromDateTime(DateTime.UtcNow);
                        }
                        _context.ContratoAlquilers.Update(c);
                    }
                }

                unidad.Estado = "disponible";
                _context.UnidadHabitacionals.Update(unidad);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Unassign completed for unidad {Id}", unidad.IdUnidad);
                return Json(new { success = true, id = unidad.IdUnidad, estado = unidad.Estado });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unassign failed for unidad {Id}", id);
                return Json(new { success = false, message = "Error al desvincular" });
            }
        }

        // GET: UnidadHabitacionals/DetailsPartial/5
        // Returns a partial view with unidad details for offcanvas/modal
        public async Task<IActionResult> DetailsPartial(int id)
        {
            var unidadHabitacional = await _context.UnidadHabitacionals
                .Include(u => u.IdCondominioNavigation)
                .Include(u => u.ImagenUnidads)
                .Include(u => u.ContratoAlquilers)
                    .ThenInclude(c => c.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdUnidad == id);

            if (unidadHabitacional == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", unidadHabitacional);
        }

        // GET: UnidadHabitacionals/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre");
            return View();
        }

        // POST: UnidadHabitacionals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("IdUnidad,Numero,Tipo,Estado,Descripcion,IdCondominio,CantidadHabitaciones,CantidadBanos,MetrosCuadrados,SalaEstar,Balcon,Terraza,Jacuzzi,Picuzzi")]
    UnidadHabitacional unidadHabitacional,
    IFormFile? ImageFile)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                         Request.Headers["Accept"].Any(h => h != null && h.Contains("application/json"));

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Guardar unidad
                    _context.Add(unidadHabitacional);
                    await _context.SaveChangesAsync();

                    // 2. Imagen (si existe)
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uploads = Path.Combine(_env.WebRootPath, "uploads", "unidades");
                        if (!Directory.Exists(uploads))
                            Directory.CreateDirectory(uploads);

                        var fileName = $"unidad_{unidadHabitacional.IdUnidad}_{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                        var filePath = Path.Combine(uploads, fileName);

                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        _logger.LogInformation("Saved unidad image to {Path}", filePath);

                        var img = new ImagenUnidad
                        {
                            Url = $"/uploads/unidades/{fileName}",
                            Descripcion = "Imagen principal",
                            IdUnidad = unidadHabitacional.IdUnidad
                        };

                        _context.ImagenUnidads.Add(img);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Created ImagenUnidad record with Url {Url}", img.Url);
                    }

                    // 3. Si es AJAX → responder JSON
                    if (isAjax)
                    {
                        var updated = await _context.UnidadHabitacionals
                            .Include(u => u.ImagenUnidads)
                            .Include(u => u.ContratoAlquilers)
                                .ThenInclude(c => c.IdUsuarioNavigation)
                            .FirstOrDefaultAsync(u => u.IdUnidad == unidadHabitacional.IdUnidad);

                        var imgUrl = updated?.ImagenUnidads?.FirstOrDefault()?.Url;
                        if (string.IsNullOrEmpty(imgUrl))
                            imgUrl = Url.Content("~/img/placeholder-unidad.jpg");

                        var contrato = updated?.ContratoAlquilers?
                            .OrderByDescending(c => c.IdContrato)
                            .FirstOrDefault();

                        return Json(new
                        {
                            success = true,
                            id = updated?.IdUnidad,
                            numero = updated?.Numero,
                            tipo = updated?.Tipo,
                            estado = updated?.Estado,
                            descripcion = updated?.Descripcion,
                            habitaciones = updated?.CantidadHabitaciones,
                            banos = updated?.CantidadBanos,
                            metros = updated?.MetrosCuadrados,
                            imgUrl,
                            inquilino = contrato?.IdUsuarioNavigation?.Nombre
                        });
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating UnidadHabitacional");
                    ModelState.AddModelError("", "Error al crear la unidad.");
                }
            }

            ViewData["IdCondominio"] = new SelectList(
                _context.Condominios,
                "IdCondominio",
                "Nombre",
                unidadHabitacional.IdCondominio
            );

            return View(unidadHabitacional);
        }

        // GET: UnidadHabitacionals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unidadHabitacional = await _context.UnidadHabitacionals.FindAsync(id);
            if (unidadHabitacional == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", unidadHabitacional.IdCondominio);
            return View(unidadHabitacional);
        }

        // POST: UnidadHabitacionals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
     int id,
     [Bind("IdUnidad,Numero,Tipo,Estado,Descripcion,IdCondominio,CantidadHabitaciones,CantidadBanos,MetrosCuadrados,SalaEstar,Balcon,Terraza,Jacuzzi,Picuzzi")]
    UnidadHabitacional unidadHabitacional,
     IFormFile? ImageFile)
        {
            var isAjax =
                Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Accept"].ToString().Contains("application/json");

            _logger.LogInformation(
                "Edit POST id={Id} Numero={Numero}",
                id,
                unidadHabitacional?.Numero);

            if (id != unidadHabitacional.IdUnidad)
                return NotFound();

            if (!ModelState.IsValid)
            {
                if (isAjax)
                    return PartialView("_EditPartial", unidadHabitacional);

                return View(unidadHabitacional);
            }

            try
            {
                // =====================================================
                // 1. OBTENER ENTIDAD TRACKED
                // =====================================================
                var unidadDb = await _context.UnidadHabitacionals
                    .FirstOrDefaultAsync(u => u.IdUnidad == id);

                if (unidadDb == null)
                    return NotFound();

                // =====================================================
                // 2. UPDATE EXPLÍCITO (INCLUYE NUEVOS CAMPOS)
                // =====================================================
                unidadDb.Numero = unidadHabitacional.Numero?.Trim();
                unidadDb.Tipo = unidadHabitacional.Tipo;
                unidadDb.Estado = unidadHabitacional.Estado;
                unidadDb.Descripcion = unidadHabitacional.Descripcion;
                unidadDb.IdCondominio = unidadHabitacional.IdCondominio;

                // NUEVOS CAMPOS
                unidadDb.CantidadHabitaciones = unidadHabitacional.CantidadHabitaciones;
                unidadDb.CantidadBanos = unidadHabitacional.CantidadBanos;
                unidadDb.MetrosCuadrados = unidadHabitacional.MetrosCuadrados;
                unidadDb.SalaEstar = unidadHabitacional.SalaEstar;
                unidadDb.Balcon = unidadHabitacional.Balcon;
                unidadDb.Terraza = unidadHabitacional.Terraza;
                unidadDb.Jacuzzi = unidadHabitacional.Jacuzzi;
                unidadDb.Picuzzi = unidadHabitacional.Picuzzi;

                await _context.SaveChangesAsync();

                // =====================================================
                // 3. CONTRATOS (MISMA LÓGICA)
                // =====================================================
                var unassignedFlag = Request.Form["Unassigned"].FirstOrDefault();

                if ((!string.IsNullOrEmpty(unassignedFlag) && unassignedFlag == "1") ||
                    string.Equals(unidadHabitacional.Estado, "disponible", StringComparison.OrdinalIgnoreCase))
                {
                    var unidadContratos = await _context.UnidadHabitacionals
                        .Include(u => u.ContratoAlquilers)
                        .FirstOrDefaultAsync(u => u.IdUnidad == id);

                    if (unidadContratos != null)
                    {
                        var activos = unidadContratos.ContratoAlquilers?
                            .Where(c => c.Estado != null &&
                                        c.Estado.ToLower() == "activo")
                            .ToList();

                        if (activos != null && activos.Count > 0)
                        {
                            foreach (var c in activos)
                            {
                                c.Estado = "finalizado";

                                if (c.FechaFin == null)
                                    c.FechaFin = DateOnly.FromDateTime(DateTime.UtcNow);
                            }

                            unidadContratos.Estado = "disponible";

                            await _context.SaveChangesAsync();
                        }
                    }
                }

                // =====================================================
                // 4. IMAGEN (SIN CAMBIOS)
                // =====================================================
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var existingImages = _context.ImagenUnidads
                        .Where(i => i.IdUnidad == id)
                        .ToList();

                    var uploads = Path.Combine(_env.WebRootPath, "uploads", "unidades");
                    if (!Directory.Exists(uploads))
                        Directory.CreateDirectory(uploads);

                    var fileName = $"unidad_{id}_{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    _context.ImagenUnidads.Add(new ImagenUnidad
                    {
                        Url = $"/uploads/unidades/{fileName}",
                        Descripcion = "Imagen principal",
                        IdUnidad = id
                    });

                    foreach (var old in existingImages)
                        _context.ImagenUnidads.Remove(old);

                    await _context.SaveChangesAsync();
                }

                // =====================================================
                // 5. RESPUESTA AJAX
                // =====================================================
                if (isAjax)
                {
                    var updated = await _context.UnidadHabitacionals
                        .Include(u => u.ImagenUnidads)
                        .Include(u => u.ContratoAlquilers)
                            .ThenInclude(c => c.IdUsuarioNavigation)
                        .FirstOrDefaultAsync(u => u.IdUnidad == id);

                    var imgUrl = updated?.ImagenUnidads?.FirstOrDefault()?.Url
                                 ?? Url.Content("~/img/placeholder-unidad.jpg");

                    var inquilino = updated?.ContratoAlquilers?
                        .FirstOrDefault(c => c.Estado == "activo")?
                        .IdUsuarioNavigation?.Nombre;

                    return Json(new
                    {
                        success = true,
                        id = updated?.IdUnidad,
                        numero = updated?.Numero,
                        tipo = updated?.Tipo,
                        estado = updated?.Estado,
                        descripcion = updated?.Descripcion,
                        imgUrl,
                        inquilino,

                        // NUEVOS CAMPOS PARA UI
                        cantidadHabitaciones = updated?.CantidadHabitaciones,
                        cantidadBanos = updated?.CantidadBanos,
                        metrosCuadrados = updated?.MetrosCuadrados
                    });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing unidad");

                if (isAjax)
                    return Json(new { success = false, message = "Error interno" });

                throw;
            }
        }

        // GET: UnidadHabitacionals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unidadHabitacional = await _context.UnidadHabitacionals
                .Include(u => u.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdUnidad == id);
            if (unidadHabitacional == null)
            {
                return NotFound();
            }

            return View(unidadHabitacional);
        }

        // POST: UnidadHabitacionals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unidadHabitacional = await _context.UnidadHabitacionals.FindAsync(id);
            if (unidadHabitacional != null)
            {
                _context.UnidadHabitacionals.Remove(unidadHabitacional);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UnidadHabitacionalExists(int id)
        {
            return _context.UnidadHabitacionals.Any(e => e.IdUnidad == id);
        }


        public async Task<IActionResult> Duplicate(int id)
        {
            var unidad = await _context.UnidadHabitacionals
                .Include(u => u.ImagenUnidads)
                .FirstOrDefaultAsync(u => u.IdUnidad == id);

            if (unidad == null)
                return NotFound();

            var vm = new UnidadDuplicarVM
            {
                IdOriginal = unidad.IdUnidad,
                Tipo = unidad.Tipo,
                Estado = "Disponible",
                Descripcion = unidad.Descripcion,
                IdCondominio = unidad.IdCondominio,
                CantidadHabitaciones = unidad.CantidadHabitaciones,
                CantidadBanos = unidad.CantidadBanos,
                MetrosCuadrados = unidad.MetrosCuadrados
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(UnidadDuplicarVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var original = await _context.UnidadHabitacionals
                .FirstOrDefaultAsync(u => u.IdUnidad == vm.IdOriginal);

            if (original == null)
                return NotFound();

            var nuevaUnidad = new UnidadHabitacional
            {
                Numero = vm.Numero,
                Tipo = original.Tipo,
                Estado = "Disponible",
                Descripcion = original.Descripcion,
                IdCondominio = original.IdCondominio,
                CantidadHabitaciones = original.CantidadHabitaciones,
                CantidadBanos = original.CantidadBanos,
                MetrosCuadrados = original.MetrosCuadrados
            };

            _context.UnidadHabitacionals.Add(nuevaUnidad);
            await _context.SaveChangesAsync();

            // IMAGEN
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads", "unidades");
                Directory.CreateDirectory(uploads);

                var fileName = $"unidad_{nuevaUnidad.IdUnidad}_{Guid.NewGuid()}{Path.GetExtension(vm.ImageFile.FileName)}";
                var path = Path.Combine(uploads, fileName);

                using (var stream = System.IO.File.Create(path))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }

                _context.ImagenUnidads.Add(new ImagenUnidad
                {
                    Url = $"/uploads/unidades/{fileName}",
                    IdUnidad = nuevaUnidad.IdUnidad,
                    Descripcion = "Imagen duplicada"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> HistorialAlquileres(int id)
        {
            var unidad = await _context.UnidadHabitacionals
        .Include(u => u.IdCondominioNavigation)
        .Include(u => u.ContratoAlquilers)
            .ThenInclude(c => c.IdUsuarioNavigation)
        .FirstOrDefaultAsync(u => u.IdUnidad == id);

            if (unidad == null)
            {
                return NotFound();
            }

            return View(unidad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarContrato(int id)
        {
            var contrato = await _context.ContratoAlquilers
                .FirstOrDefaultAsync(c =>
                    c.IdUnidad == id &&
                    c.Estado != null &&
                    c.Estado.ToLower() == "activo");

            if (contrato == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No existe un contrato activo para esta unidad."
                });
            }


            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Cerrar contrato
                    contrato.Estado = "finalizado";
                    contrato.FechaFin = DateOnly.FromDateTime(DateTime.Now);

                    _context.ContratoAlquilers.Update(contrato);


                    // Liberar unidad
                    var unidad = await _context.UnidadHabitacionals
                        .FirstOrDefaultAsync(u => u.IdUnidad == id);

                    if (unidad != null)
                    {
                        unidad.Estado = "Disponible";
                        _context.UnidadHabitacionals.Update(unidad);
                    }


                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();


                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();

                    _logger.LogError(ex,
                        "Error finalizando contrato de unidad {IdUnidad}",
                        id);

                    return Json(new
                    {
                        success = false,
                        message = "Error al finalizar contrato."
                    });
                }
            }
        }
    }
}
