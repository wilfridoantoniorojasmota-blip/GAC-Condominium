using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;
using PROTOTIPO.Models.ViewModel;
using PROTOTIPO.Models.ViewModels;
using System.Security.Claims;

namespace PROTOTIPO.Controllers
{
    public class InquilinoController : Controller
    {
        private readonly AppDbContext _context;

        public InquilinoController(AppDbContext context)
        {
            _context = context;
        }

       public IActionResult Index()
{
    // 🔐 Usuario logueado
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (userIdClaim == null)
        return RedirectToAction("Login", "Account");

    int userId = int.Parse(userIdClaim);

    // =========================
    // 🔎 USUARIO + RELACIONES
    // =========================
    var usuario = _context.Usuarios
        .Include(u => u.ContratoAlquilers)
            .ThenInclude(c => c.Pagos)
        .Include(u => u.IncidenciaMantenimientos)
        .Include(u => u.ReservaAreaComuns)
        .FirstOrDefault(u => u.IdUsuario == userId);

    if (usuario == null)
        return NotFound();

    var contrato = usuario.ContratoAlquilers.FirstOrDefault();

    var unidad = contrato != null
        ? _context.UnidadHabitacionals.FirstOrDefault(x => x.IdUnidad == contrato.IdUnidad)
        : null;

            // =========================
            // 📢 AVISOS
            // =========================
            var avisos = _context.Avisos
                .Where(a =>

                    // Avisos personales
                    a.IdUsuario == userId

                    ||

                    // Avisos generales del condominio
                    (
                        a.IdUsuario == null &&
                        a.IdCondominio == usuario.IdCondominio
                    )
                )
                .OrderByDescending(a => a.FechaPublicacion)
                .Take(5)
                .ToList();

            // =========================
            // 🔧 INCIDENCIAS
            // =========================
            var incidencias = usuario.IncidenciaMantenimientos.ToList();

    int incidenciasActivas = incidencias.Count(x =>
        x.Estado != "Resuelta" && x.Estado != "Cerrada");

            // 🔥 NUEVO: incidencias recientes para el dashboard
            var incidenciasRecientes = incidencias
            .Where(i => i.Estado == "Pendiente")
            .OrderByDescending(i => i.FechaSolicitud)
            .Take(3)
            .ToList();

            // =========================
            // 💰 BALANCE REAL
            // =========================
            decimal balance = 0;

    if (contrato != null)
    {
        var totalPagado = contrato.Pagos.Sum(p => p.Monto ?? 0);
        var renta = contrato.MontoRenta ?? 0;

        balance = renta - totalPagado;
    }

    // =========================
    // ⚠️ ESTADO DE DEUDA
    // =========================
    bool tieneDeuda = balance > 0;

            // =========================
            // 📅 PRÓXIMO PAGO (simple)
            // =========================
            DateOnly? proximoPago = contrato?.FechaFin;

            // =========================
            // 📊 VIEWMODEL
            // =========================
            var vm = new DashboardTenantVM
    {
        NombreInquilino = usuario.Nombre,
        Unidad = unidad?.Numero ?? "Sin asignar",

        CuotaMensual = contrato?.MontoRenta ?? 0,
        Balance = balance,

        IncidenciasActivas = incidenciasActivas,
        IncidenciasRecientes = incidenciasRecientes,

        AvisosRecientes = avisos,

        TieneDeuda = tieneDeuda,
        ProximoPago = proximoPago
    };

    return View(vm);
}

        public async Task<IActionResult> MiUnidad()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);

            var usuario = await _context.Usuarios
                .Include(u => u.ContratoAlquilers)
                .FirstOrDefaultAsync(u => u.IdUsuario == userId);

            if (usuario == null)
                return NotFound();

            var contrato = usuario.ContratoAlquilers.FirstOrDefault();

            if (contrato == null)
                return View(new MiUnidadVM());

            var unidad = await _context.UnidadHabitacionals
                .Include(u => u.ImagenUnidads)
                .FirstOrDefaultAsync(u => u.IdUnidad == contrato.IdUnidad);

            var vm = new MiUnidadVM
            {
                NombreInquilino = usuario.Nombre,
                Email = usuario.Email,
                Telefono = usuario.Telefono,

                NumeroUnidad = unidad?.Numero,
                TipoUnidad = unidad?.Tipo,
                EstadoUnidad = unidad?.Estado,
                DescripcionUnidad = unidad?.Descripcion,

                MontoRenta = contrato.MontoRenta ?? 0,
                FechaInicio = contrato.FechaInicio,
                FechaFin = contrato.FechaFin,

                ImagenPrincipal = unidad?.ImagenUnidads
                    .FirstOrDefault()?.Url
            };

            var cuota = _context.CuotaMantenimientos
                .Where(x => x.IdUnidad == unidad.IdUnidad)
                 .OrderByDescending(x => x.Periodo)
                .FirstOrDefault();

            vm.CuotaMensual = cuota?.Monto ?? 0;
            return View(vm);
        }
    }
}