using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Helpers;
using PROTOTIPO.Models;
using PROTOTIPO.Services;
using Microsoft.Extensions.Configuration;

public class SolicitudesIngresoController : Controller
{
    private readonly AppDbContext _context;
    private readonly EmailServices _emailService;
    private readonly IConfiguration _config;

    public SolicitudesIngresoController(AppDbContext context, EmailServices emailService, IConfiguration config)
    {
        _context = context;
        _emailService = emailService;
        _config = config;
    }

    // =========================
    // LANDING FORM (PUBLICO)
    // =========================
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Unidades = _context.UnidadHabitacionals
            .Where(u => u.Estado == "Disponible")
            .ToList();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SolicitudIngreso model)
    {
        



        // =========================
        // DATOS DEL FORMULARIO
        // =========================

        var form = HttpContext.Request.Form;

        // =========================
        // VALIDAR reCAPTCHA
        // =========================

        var recaptchaToken =
            form["g-recaptcha-response"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(recaptchaToken))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = false,
                    message = "Debes confirmar que no eres un robot."
                });
            }

            ModelState.AddModelError(
                "",
                "Debes confirmar que no eres un robot."
            );

            ViewBag.Unidades = _context.UnidadHabitacionals
                .Where(u => u.Estado == "Disponible")
                .ToList();

            return View(model);
        }

        var secretKey = _config["Recaptcha:SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            return StatusCode(
                500,
                "No se ha configurado la clave secreta de reCAPTCHA."
            );
        }

        using var httpClient = new HttpClient();

        var response = await httpClient.PostAsync(
            "https://www.google.com/recaptcha/api/siteverify",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["secret"] = secretKey,
                    ["response"] = recaptchaToken
                }
            )
        );

        var recaptchaResult =
            await response.Content.ReadFromJsonAsync<RecaptchaResponse>();

        if (recaptchaResult == null || !recaptchaResult.Success)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = false,
                    message = "La verificación de seguridad no fue válida. Inténtalo nuevamente."
                });
            }

            ModelState.AddModelError(
                "",
                "La verificación de seguridad no fue válida. Inténtalo nuevamente."
            );

            ViewBag.Unidades = _context.UnidadHabitacionals
                .Where(u => u.Estado == "Disponible")
                .ToList();

            return View(model);
        }

        var telefonoForm = form["Telefono"].FirstOrDefault();

        model.Telefono = telefonoForm;


        // =========================
        // NORMALIZAR CONTACTO
        // =========================

        if (string.IsNullOrWhiteSpace(model.Contacto))
        {
            var contacto = form["Contacto"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(contacto))
            {
                var email = form["Email"].FirstOrDefault();

                contacto = !string.IsNullOrWhiteSpace(email)
                    ? email
                    : telefonoForm;
            }

            model.Contacto = contacto;
        }


        // =========================
        // VALIDAR TIPO DE CLIENTE
        // =========================

        if (string.IsNullOrWhiteSpace(model.TipoCliente))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = false,
                    message = "Seleccione el tipo de cliente."
                });
            }

            ModelState.AddModelError(
                "TipoCliente",
                "Seleccione el tipo de cliente."
            );

            ViewBag.Unidades = _context.UnidadHabitacionals
                .Where(u => u.Estado == "Disponible")
                .ToList();

            return View(model);
        }


        // =========================
        // VALIDAR DOMINICANO
        // =========================

        if (model.TipoCliente == "Dominicano")
        {
            if (string.IsNullOrWhiteSpace(model.NombreSolicitante) ||
                string.IsNullOrWhiteSpace(model.Apellido) ||
                string.IsNullOrWhiteSpace(model.Cedula) ||
                string.IsNullOrWhiteSpace(model.Contacto))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Complete los campos obligatorios: nombre, apellido, cédula y contacto."
                    });
                }

                ModelState.AddModelError(
                    "",
                    "Complete los campos obligatorios: nombre, apellido, cédula y contacto."
                );

                ViewBag.Unidades = _context.UnidadHabitacionals
                    .Where(u => u.Estado == "Disponible")
                    .ToList();

                return View(model);
            }
        }


        // =========================
        // VALIDAR EXTRANJERO
        // =========================

        if (model.TipoCliente == "Extranjero")
        {
            if (string.IsNullOrWhiteSpace(model.NombreSolicitante) ||
                string.IsNullOrWhiteSpace(model.Apellido) ||
                string.IsNullOrWhiteSpace(model.Pasaporte) ||
                string.IsNullOrWhiteSpace(model.Contacto))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Complete los campos obligatorios: nombre, apellido, pasaporte y contacto."
                    });
                }

                ModelState.AddModelError(
                    "",
                    "Complete los campos obligatorios: nombre, apellido, pasaporte y contacto."
                );

                ViewBag.Unidades = _context.UnidadHabitacionals
                    .Where(u => u.Estado == "Disponible")
                    .ToList();

                return View(model);
            }
        }


        // =========================
        // VALIDAR JURÍDICO
        // =========================

        if (model.TipoCliente == "Juridico")
        {
            if (string.IsNullOrWhiteSpace(model.NombreEmpresa) ||
                string.IsNullOrWhiteSpace(model.RNC) ||
                string.IsNullOrWhiteSpace(model.PersonaAlojada) ||
                string.IsNullOrWhiteSpace(model.Contacto))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Complete los campos obligatorios: empresa, RNC, persona alojada y contacto."
                    });
                }

                ModelState.AddModelError(
                    "",
                    "Complete los campos obligatorios: empresa, RNC, persona alojada y contacto."
                );

                ViewBag.Unidades = _context.UnidadHabitacionals
                    .Where(u => u.Estado == "Disponible")
                    .ToList();

                return View(model);
            }
        }


        // =========================
        // VALIDAR CÉDULA
        // =========================

        if (model.TipoCliente == "Dominicano")
        {
            if (!CedulaValidator.EsValida(model.Cedula))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "La cédula introducida no es válida. Verifique el número ingresado."
                    });
                }

                ModelState.AddModelError(
                    "Cedula",
                    "La cédula introducida no es válida. Verifique el número ingresado."
                );

                ViewBag.Unidades = _context.UnidadHabitacionals
                    .Where(u => u.Estado == "Disponible")
                    .ToList();

                return View(model);
            }

            // Formatear cédula
            model.Cedula = CedulaValidator.Formatear(model.Cedula);
        }


        // =========================
        // NOTA:
        // NO SE VALIDA RNC
        // =========================

        // El RNC solamente se verifica arriba
        // para comprobar que no esté vacío.
        //
        // Ejemplo aceptado:
        // 1-01-01063-2


        // =========================
        // DATOS DE LA SOLICITUD
        // =========================

        model.Fecha = DateTime.Now;

        model.Estado = "pendiente";


        // =========================
        // GUARDAR SOLICITUD
        // =========================

        _context.SolicitudIngresos.Add(model);

        await _context.SaveChangesAsync();


        // =========================
        // REFERENCIA LABORAL
        // =========================

        var trabaja =
            form["Trabaja"].FirstOrDefault() == "true";

        if (trabaja)
        {
            var referenciaLaboral = new ReferenciaLaboral
            {
                IdSolicitud = model.IdSolicitud,

                Trabaja = true,

                TipoTrabajo =
                    form["TipoTrabajo"].FirstOrDefault(),

                Empresa =
                    form["Empresa"].FirstOrDefault(),

                Cargo =
                    form["Cargo"].FirstOrDefault(),

                TiempoLaborando =
                    form["TiempoLaborando"].FirstOrDefault(),

                IngresoMensual =
                    decimal.TryParse(
                        form["IngresoMensual"],
                        out var ingreso
                    )
                        ? ingreso
                        : null,

                TelefonoEmpresa =
                    form["TelefonoEmpresa"].FirstOrDefault(),

                DireccionEmpresa =
                    form["DireccionEmpresa"].FirstOrDefault()
            };

            _context.ReferenciaLaborals.Add(
                referenciaLaboral
            );

            await _context.SaveChangesAsync();
        }


        // =========================
        // CORREO DE CONFIRMACIÓN
        // =========================

        try
        {
            var nombreCompleto =
                $"{model.NombreSolicitante} {model.Apellido}".Trim();

            // Para jurídico, como no hay NombreSolicitante/Apellido,
            // usamos el nombre de la empresa.
            if (model.TipoCliente == "Juridico")
            {
                nombreCompleto = model.NombreEmpresa ?? "Cliente";
            }

            var html =
                EmailTemplates.SolicitudRecibida(
                    nombreCompleto
                );

            await _emailService.SendEmailAsync(
                model.Contacto,
                "Solicitud recibida - Condominio Los Almendros",
                html
            );
        }
        catch (Exception ex)
        {
            // La solicitud ya fue guardada.
            // Si el correo falla, no se pierde la solicitud.

            Console.WriteLine(
                $"Error al enviar correo de recepción: {ex.Message}"
            );
        }


        // =========================
        // RESPUESTA AJAX
        // =========================

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new
            {
                success = true
            });
        }


        // =========================
        // REDIRECCIÓN
        // =========================

        return RedirectToAction("Gracias");
    }

    public IActionResult Gracias()
    {
        return View();
    }

    // =========================
    // PANEL ADMIN
    // =========================

    public async Task<IActionResult> Index(string estado = null, int page = 1)
    {
        const int pageSize = 8;

        // ============================================================
        // CONSULTA BASE
        // ============================================================

        var query = _context.SolicitudIngresos
            .Include(s => s.IdUnidadNavigation)
            .AsQueryable();

        // ============================================================
        // FILTRO
        // ============================================================

        if (!string.IsNullOrEmpty(estado))
        {
            query = query.Where(s => s.Estado == estado);
        }

        // ============================================================
        // TOTAL DE SOLICITUDES DEL FILTRO
        // ============================================================

        var totalCount = await query.CountAsync();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize
        );

        if (page < 1)
            page = 1;

        if (page > totalPages && totalPages > 0)
            page = totalPages;

        // ============================================================
        // SOLICITUDES DE LA PÁGINA ACTUAL
        // ============================================================

        var lista = await query
            .OrderByDescending(s => s.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ============================================================
        // ESTADÍSTICAS GLOBALES
        // NO DEPENDEN DE LA PAGINACIÓN
        // ============================================================

        var totalSolicitudes = await _context.SolicitudIngresos
            .CountAsync();

        var pendientes = await _context.SolicitudIngresos
            .CountAsync(s =>
                s.Estado != null &&
                s.Estado.ToLower() == "pendiente"
            );

        var aprobadas = await _context.SolicitudIngresos
            .CountAsync(s =>
                s.Estado != null &&
                s.Estado.ToLower() == "procesada"
            );

        var rechazadas = await _context.SolicitudIngresos
            .CountAsync(s =>
                s.Estado != null &&
                s.Estado.ToLower() == "rechazada"
            );

        // ============================================================
        // INFORMACIÓN PARA LA VISTA
        // ============================================================

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.EstadoActual = estado;

        // Estadísticas globales
        ViewBag.TotalSolicitudes = totalSolicitudes;
        ViewBag.Pendientes = pendientes;
        ViewBag.Aprobadas = aprobadas;
        ViewBag.Rechazadas = rechazadas;

        return View(lista);
    }

    // =========================
    // DETALLE
    // =========================

    public async Task<IActionResult> Details(int id)
    {
        var solicitud = await _context.SolicitudIngresos
            .Include(s => s.IdUnidadNavigation)
                .ThenInclude(u => u.ImagenUnidads)
            .Include(s => s.ReferenciaLaboral)
            .FirstOrDefaultAsync(s => s.IdSolicitud == id);

        if (solicitud == null)
            return NotFound();

        // Número visible de la solicitud según su posición
        // entre las solicitudes existentes.
        var numeroSolicitud = await _context.SolicitudIngresos
            .CountAsync(s => s.IdSolicitud <= solicitud.IdSolicitud);

        ViewBag.NumeroSolicitud = numeroSolicitud;

        return View(solicitud);
    }
 

    // =========================
    // APROBAR
    // =========================
    [HttpPost]
    public async Task<IActionResult> Aprobar(int id)
    {
        var solicitud = await _context.SolicitudIngresos.FindAsync(id);

        if (solicitud == null)
            return NotFound();

        solicitud.Estado = "procesada";
        await _context.SaveChangesAsync();

        try
        {
            var html = EmailTemplates.Aprobada(solicitud.NombreSolicitante);

            await _emailService.SendEmailAsync(
                solicitud.Contacto,
                "Solicitud aprobada - Los Almendros",
                html
            );

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Correo enviado correctamente.", estado = solicitud.Estado });
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Error al enviar correo." });
            }

            return Content(ex.ToString());
        }
    }

    // =========================
    // RECHAZAR
    // =========================
    [HttpPost]
    public async Task<IActionResult> Rechazar(int id)
    {
        var solicitud = await _context.SolicitudIngresos.FindAsync(id);

        if (solicitud == null)
            return NotFound();

        solicitud.Estado = "rechazada";

        await _context.SaveChangesAsync();

        try
        {
            var html = EmailTemplates.Rechazada(solicitud.NombreSolicitante);

            await _emailService.SendEmailAsync(
                solicitud.Contacto,
                "Solicitud rechazada - Los Almendros",
                html
            );

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Correo enviado correctamente.", estado = solicitud.Estado });
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Error al enviar correo." });
            }

            return Content(ex.ToString());
        }
    }

    // =========================
    // ELIMINAR
    // =========================
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var solicitud = await _context.SolicitudIngresos.FindAsync(id);

        if (solicitud == null)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = "Solicitud no encontrada." });

            return NotFound();
        }

        _context.SolicitudIngresos.Remove(solicitud);
        await _context.SaveChangesAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, message = "Solicitud eliminada." });
        }

        return RedirectToAction(nameof(Index));
    }
}
