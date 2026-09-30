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
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(AppDbContext context, ILogger<UsuariosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(int page = 1)
{
    const int pageSize = 8; // Cantidad de residentes por página

    // ============================================================
    // 1. ESTADÍSTICAS GLOBALES
    //    Estas consultas se realizan sobre TODOS los usuarios
    //    y NO sobre la página actual.
    // ============================================================

    var totalResidentes = await _context.Usuarios
        .CountAsync();

    var conUnidad = await _context.Usuarios
        .CountAsync(u => u.ContratoAlquilers.Any());

    var sinUnidad = await _context.Usuarios
        .CountAsync(u => !u.ContratoAlquilers.Any());

    var roleGroups = await _context.Usuarios
        .Include(u => u.IdRolNavigation)
        .GroupBy(u => u.IdRolNavigation != null
            ? u.IdRolNavigation.Nombre
            : "Sin rol")
        .Select(g => new
        {
            Nombre = g.Key,
            Count = g.Count()
        })
        .OrderByDescending(g => g.Count)
        .ToListAsync();


    // ============================================================
    // 2. PAGINACIÓN
    //    La paginación solamente afecta a la tabla de usuarios.
    // ============================================================

    var totalPages = (int)Math.Ceiling(
        totalResidentes / (double)pageSize
    );

    if (page < 1)
        page = 1;

    if (page > totalPages && totalPages > 0)
        page = totalPages;


    // ============================================================
    // 3. CONSULTA PAGINADA DE USUARIOS
    // ============================================================

    var usuarios = await _context.Usuarios
        .Include(u => u.IdRolNavigation)
        .Include(u => u.ContratoAlquilers)
            .ThenInclude(c => c.IdUnidadNavigation)
        .OrderBy(u => u.IdUsuario)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();


    // ============================================================
    // 4. INFORMACIÓN GLOBAL PARA LA VISTA
    // ============================================================

    ViewBag.TotalResidentes = totalResidentes;
    ViewBag.ConUnidad = conUnidad;
    ViewBag.SinUnidad = sinUnidad;
    ViewBag.RoleGroups = roleGroups;

    // ============================================================
    // 5. INFORMACIÓN DE PAGINACIÓN
    // ============================================================

    ViewBag.CurrentPage = page;
    ViewBag.TotalPages = totalPages;
    ViewBag.PageSize = pageSize;


    // ============================================================
    // 6. LISTA DE ROLES PARA FORMULARIOS
    // ============================================================

    ViewBag.IdRol = new SelectList(
        _context.Rols,
        "IdRol",
        "Nombre"
    );


    // ============================================================
    // 7. LISTA DE CONDOMINIOS PARA FORMULARIOS
    // ============================================================

    ViewBag.IdCondominio = new SelectList(
        _context.Condominios,
        "IdCondominio",
        "Nombre"
    );


    // ============================================================
    // 8. RETORNAR VISTA
    // ============================================================

    return View(usuarios);
}
        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id, bool? fromIndex)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.IdCondominioNavigation)
                .Include(u => u.IdRolNavigation)
                .Include(u => u.ContratoAlquilers)
                    .ThenInclude(c => c.IdUnidadNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null)
                return NotFound();

            // =====================================================
            // SI VIENE DESDE INDEX (MODAL FLOW)
            // =====================================================
            if (fromIndex == true)
            {
                var usuarios = await _context.Usuarios
                    .Include(u => u.IdRolNavigation)
                    .Include(u => u.ContratoAlquilers)
                        .ThenInclude(c => c.IdUnidadNavigation)
                    .ToListAsync();

                ViewBag.TotalResidentes = usuarios.Count;
                ViewBag.ShowResidentModal = true;

                return View("Index", usuarios);
            }

            // =====================================================
            // DETALLE NORMAL
            // =====================================================
            var viewModel = new Usuario
            {
                IdUsuario = usuario.IdUsuario,

                // Datos personales
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                TipoCliente = usuario.TipoCliente,

                // Identificación
                Cedula = usuario.Cedula,
                Pasaporte = usuario.Pasaporte,
                Nacionalidad = usuario.Nacionalidad,

                // Datos jurídicos
                RNC = usuario.RNC,
                NombreEmpresa = usuario.NombreEmpresa,
                PersonaAlojada = usuario.PersonaAlojada,

                // Contacto
                Email = usuario.Email,
                Telefono = usuario.Telefono,

                // Acceso
                Password = usuario.Password,

                // Sistema
                IdRol = usuario.IdRol,
                IdCondominio = usuario.IdCondominio,
                IdRolNavigation = usuario.IdRolNavigation,
                IdCondominioNavigation = usuario.IdCondominioNavigation,

                // Contratos
                ContratoAlquilers = usuario.ContratoAlquilers
            };

            return View(viewModel);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(
                _context.Condominios,
                "IdCondominio",
                "IdCondominio"
            );

            ViewData["IdRol"] = new SelectList(
                _context.Rols,
                "IdRol",
                "Nombre"
            );

            return View();
        }


        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("Nombre,Apellido,Cedula,Pasaporte,RNC,TipoCliente,Nacionalidad,Email,Telefono,Password,NombreEmpresa,PersonaAlojada,IdRol,IdCondominio")]
    Usuario usuario,
    int? IdSolicitud)
        {
            if (ModelState.IsValid)
            {
                _context.Usuarios.Add(usuario);

                try
                {
                    await _context.SaveChangesAsync();

                    // Marcar solicitud como procesada
                    if (IdSolicitud.HasValue)
                    {
                        var solicitud = await _context.SolicitudIngresos
                            .FirstOrDefaultAsync(s => s.IdSolicitud == IdSolicitud.Value);

                        if (solicitud != null)
                        {
                            solicitud.Estado = "procesada";
                            await _context.SaveChangesAsync();
                        }
                    }

                    TempData["SuccessMessage"] = "Residente guardado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Error saving Usuario to database");

                    ModelState.AddModelError(
                        string.Empty,
                        "Error al guardar en la base de datos: " +
                        dbEx.GetBaseException().Message
                    );
                }
            }

            ViewData["IdRol"] = new SelectList(
                _context.Rols,
                "IdRol",
                "Nombre",
                usuario?.IdRol
            );

            ViewData["IdCondominio"] = new SelectList(
                _context.Condominios,
                "IdCondominio",
                "Nombre",
                usuario?.IdCondominio
            );

            var errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            foreach (var error in errores)
            {
                Console.WriteLine(error);
            }

            return View(usuario);
        }
        // GET: Usuarios/Edit/5
        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            ViewData["IdCondominio"] = new SelectList(
                _context.Condominios,
                "IdCondominio",
                "Nombre",
                usuario.IdCondominio
            );

            ViewData["IdRol"] = new SelectList(
                _context.Rols,
                "IdRol",
                "Nombre",
                usuario.IdRol
            );

            return View(usuario);
        }
        // POST: Usuarios/Edit/5
        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    [Bind("IdUsuario,Nombre,Apellido,TipoCliente,Cedula,Pasaporte,Nacionalidad,RNC,NombreEmpresa,PersonaAlojada,Email,Telefono,Password,IdRol,IdCondominio")]
    Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["IdCondominio"] = new SelectList(
                    _context.Condominios,
                    "IdCondominio",
                    "Nombre",
                    usuario.IdCondominio
                );

                ViewData["IdRol"] = new SelectList(
                    _context.Rols,
                    "IdRol",
                    "Nombre",
                    usuario.IdRol
                );

                return View(usuario);
            }

            try
            {
                var userDb = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == id);

                if (userDb == null)
                {
                    return NotFound();
                }

                // =====================================================
                // DATOS GENERALES
                // =====================================================

                userDb.Nombre = usuario.Nombre;
                userDb.Apellido = usuario.Apellido;
                userDb.TipoCliente = usuario.TipoCliente;

                // =====================================================
                // IDENTIFICACIÓN SEGÚN TIPO DE CLIENTE
                // =====================================================

                if (usuario.TipoCliente == "Dominicano")
                {
                    // Cédula
                    userDb.Cedula = usuario.Cedula;

                    // Limpiar datos de extranjero
                    userDb.Pasaporte = null;
                    userDb.Nacionalidad = null;

                    // Limpiar datos jurídicos
                    userDb.RNC = null;
                    userDb.NombreEmpresa = null;
                    userDb.PersonaAlojada = null;
                }
                else if (usuario.TipoCliente == "Extranjero")
                {
                    // Limpiar cédula
                    userDb.Cedula = null;

                    // Datos de extranjero
                    userDb.Pasaporte = usuario.Pasaporte;
                    userDb.Nacionalidad = usuario.Nacionalidad;

                    // Limpiar datos jurídicos
                    userDb.RNC = null;
                    userDb.NombreEmpresa = null;
                    userDb.PersonaAlojada = null;
                }
                else if (usuario.TipoCliente == "Juridico")
                {
                    // Limpiar datos personales
                    userDb.Cedula = null;
                    userDb.Pasaporte = null;
                    userDb.Nacionalidad = null;

                    // Datos jurídicos
                    userDb.RNC = usuario.RNC;
                    userDb.NombreEmpresa = usuario.NombreEmpresa;
                    userDb.PersonaAlojada = usuario.PersonaAlojada;
                }
                else
                {
                    // Si no tiene un tipo válido, limpiar todos
                    // los datos específicos de identificación.

                    userDb.Cedula = null;
                    userDb.Pasaporte = null;
                    userDb.Nacionalidad = null;
                    userDb.RNC = null;
                    userDb.NombreEmpresa = null;
                    userDb.PersonaAlojada = null;
                }

                // =====================================================
                // CONTACTO
                // =====================================================

                userDb.Email = usuario.Email;
                userDb.Telefono = usuario.Telefono;

                // =====================================================
                // ACCESO
                // =====================================================

                userDb.Password = usuario.Password;

                // =====================================================
                // SISTEMA
                // =====================================================

                userDb.IdRol = usuario.IdRol;
                userDb.IdCondominio = usuario.IdCondominio;

                // =====================================================
                // GUARDAR CAMBIOS
                // =====================================================

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(usuario.IdUsuario))
                {
                    return NotFound();
                }

                throw;
            }
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdCondominioNavigation)
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return Json(new
                {
                    success = false,
                    message = "El usuario no existe."
                });
            }

            // 1. Validar estrictamente si tiene un contrato marcado como 'activo'
            var tieneContratoActivo = await _context.ContratoAlquilers
                .AnyAsync(c => c.IdUsuario == id &&
                               c.Estado != null &&
                               c.Estado.Trim().ToLower() == "activo");

            if (tieneContratoActivo)
            {
                return Json(new
                {
                    success = false,
                    message = "Este inquilino tiene un contrato activo. Finalice el contrato antes de eliminar."
                });
            }

            // 2. Intentar eliminar y capturar si hay restricción de clave foránea
            try
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Usuario eliminado correctamente."
                });
            }
            catch (DbUpdateException ex)
            {
                // Esto captura los errores de clave foránea en la BD
                return Json(new
                {
                    success = false,
                    message = "No se puede eliminar el usuario porque tiene registros asociados (pagos, historial, etc.) en el sistema."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error inesperado al eliminar el usuario."
                });
            }
        }
        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerSolicitud(int id)
        {
            var solicitud = await _context.SolicitudIngresos
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud == null)
                return NotFound();

            return Json(new
            {
                tipoCliente = solicitud.TipoCliente,
                nombre = solicitud.NombreSolicitante,
                apellido = solicitud.Apellido,
                cedula = solicitud.Cedula,
                pasaporte = solicitud.Pasaporte,
                rnc = solicitud.RNC,
                nacionalidad = solicitud.Nacionalidad,
                email = solicitud.Contacto,
                telefono = solicitud.Telefono,
                nombreEmpresa = solicitud.NombreEmpresa,
                personaAlojada = solicitud.PersonaAlojada,
            });
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerSolicitudesPendientes()
        {
            var solicitudes = await _context.SolicitudIngresos
                .Where(s =>
                    s.Estado == "procesada" &&
                    !_context.Usuarios.Any(u =>
                        (s.TipoCliente == "Dominicano" &&
                         u.Cedula == s.Cedula) ||

                        (s.TipoCliente == "Extranjero" &&
                         u.Pasaporte == s.Pasaporte) ||

                        (s.TipoCliente == "Juridico" &&
                         u.RNC == s.RNC)
                    ))
                .Include(s => s.IdUnidadNavigation)
                .Select(s => new
                {
                    id = s.IdSolicitud,

                    tipoCliente = s.TipoCliente,

                    nombre = s.NombreSolicitante,
                    apellido = s.Apellido,

                    cedula = s.Cedula,
                    pasaporte = s.Pasaporte,
                    rnc = s.RNC,

                    nacionalidad = s.Nacionalidad,

                    // Datos de contacto
                    contacto = s.Contacto,
                    telefono = s.Telefono,

                    // Datos jurídicos
                    nombreEmpresa = s.NombreEmpresa,
                    personaAlojada = s.PersonaAlojada,

                    unidad = s.IdUnidadNavigation != null
                        ? s.IdUnidadNavigation.Numero
                        : "Sin unidad"
                })
                .ToListAsync();

            return Json(solicitudes);
        }

    }
}