using PROTOTIPO.Models;
using PROTOTIPO.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PROTOTIPO.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            ViewBag.AdminName = User.Identity?.Name ?? "Administrador";

            var model = new DashboardVM();
            var now = DateTime.Now;

            // =========================
            // MÉTRICAS GENERALES
            // =========================

            model.TotalUnidades = _context.UnidadHabitacionals.Count();

            model.ResidentesActivos = _context.Usuarios
                .Count(u => u.IdRolNavigation.Nombre == "Inquilino");

            // Count pending maintenance quotas. Consider cuotas with Estado='pendiente'
            // and pagos linked to cuotas that are still pending. Use UNION to avoid double-counting.
            var pendingFromPagos = _context.Pagos
                .Where(p => p.IdCuota != null && p.EstadoPago != null && p.EstadoPago.Trim().ToLower() == "pendiente")
                .Select(p => p.IdCuota);

            var pendingCuotas = _context.CuotaMantenimientos
                .Where(q => q.Estado != null && q.Estado.Trim().ToLower() == "pendiente")
                .Select(q => (int?)q.IdCuota);

            model.CuotasPendientes = pendingFromPagos.Union(pendingCuotas).Distinct().Count();

            // =========================
            // INGRESOS DEL MES (CORREGIDO REAL)
            // =========================

            model.IngresosMes = _context.Pagos
                .Where(p =>
                    p.Fecha.HasValue &&
                    p.Fecha.Value.Month == now.Month &&
                    p.Fecha.Value.Year == now.Year
                )
                .Sum(p => p.Monto ?? 0);

            // =========================
            // INGRESOS ÚLTIMOS 6 MESES
            // =========================

            model.IngresosUltimos6Meses = new List<decimal>();
            for (int i = 5; i >= 0; i--)
            {
                var fecha = now.AddMonths(-i);
                var ingresosDelMes = _context.Pagos
                    .Where(p =>
                        p.Fecha.HasValue &&
                        p.Fecha.Value.Month == fecha.Month &&
                        p.Fecha.Value.Year == fecha.Year
                    )
                    .Sum(p => p.Monto ?? 0);
                model.IngresosUltimos6Meses.Add(ingresosDelMes);
            }

            // =========================
            // ESTADO DE UNIDADES
            // =========================

            // Normalize and count by keyword to tolerate different casings/values in DB
            model.EstadoUnidades = new EstadoUnidadesDTO
            {
                Ocupadas = _context.UnidadHabitacionals.Count(u => u.Estado != null && u.Estado.Trim().ToLower().Contains("ocup")),
                Disponibles = _context.UnidadHabitacionals.Count(u => u.Estado != null && u.Estado.Trim().ToLower().Contains("dispon")),
                EnProceso = _context.UnidadHabitacionals.Count(u => u.Estado != null && u.Estado.Trim().ToLower().Contains("proce"))
            };

            // =========================
            // ACTIVIDADES RECIENTES
            // =========================

            var actividades = new List<ActividadDTO>();

            actividades.AddRange(
                _context.IncidenciaMantenimientos
                .Select(i => new ActividadDTO
                {
                    Descripcion = i.Descripcion,
                    Fecha = i.FechaSolicitud ?? DateTime.Now,
                    Tipo = "Mantenimiento"
                })
            );

            actividades.AddRange(
                _context.Pagos
                .Select(p => new ActividadDTO
                {
                    Descripcion = "Pago recibido - " + (p.TipoPago ?? "General"),
                    Fecha = p.Fecha ?? DateTime.Now,
                    Tipo = "Pago"
                })
            );

            model.ActividadesRecientes = actividades
                .OrderByDescending(a => a.Fecha)
                .Take(5)
                .ToList();

            // =========================
            // PAGOS RECIENTES
            // =========================

            var pagos = _context.Pagos
                .OrderByDescending(p => p.Fecha)
                .Take(5)
                .Include(p => p.IdContratoNavigation)
                    .ThenInclude(c => c.IdUnidadNavigation)
                .Include(p => p.IdContratoNavigation)
                    .ThenInclude(c => c.IdUsuarioNavigation)
                .Include(p => p.IdCuotaNavigation)
                    .ThenInclude(q => q.IdUnidadNavigation)
                        .ThenInclude(u => u.ContratoAlquilers)
                            .ThenInclude(ca => ca.IdUsuarioNavigation)
                .ToList();

            model.PagosRecientes = pagos.Select(p =>
            {
                string residente = "No asignado";

                if (p.IdContratoNavigation?.IdUsuarioNavigation != null)
                {
                    residente = p.IdContratoNavigation.IdUsuarioNavigation.Nombre;
                }
                else if (p.IdCuotaNavigation?.IdUnidadNavigation?.ContratoAlquilers != null)
                {
                    residente = p.IdCuotaNavigation.IdUnidadNavigation.ContratoAlquilers
                        .OrderByDescending(c => c.IdContrato)
                        .Select(c => c.IdUsuarioNavigation?.Nombre)
                        .FirstOrDefault() ?? "Sin residente";
                }

                string unidad = p.IdContratoNavigation != null
                    ? p.IdContratoNavigation.IdUnidadNavigation?.Numero
                    : p.IdCuotaNavigation?.IdUnidadNavigation?.Numero;

                string tipo = !string.IsNullOrWhiteSpace(p.TipoPago)
                    ? p.TipoPago.Trim()
                    : (p.IdContratoNavigation != null ? "Contrato"
                        : (p.IdCuotaNavigation != null ? "Cuota" : "General"));

                string estado = !string.IsNullOrWhiteSpace(p.EstadoPago)
                    ? p.EstadoPago.Trim()
                    : "Desconocido";

                return new PagoDTO
                {
                    Residente = residente,
                    Unidad = unidad,
                    TipoPago = tipo,
                    Monto = p.Monto ?? 0,
                    Fecha = p.Fecha,
                    Estado = estado,
                    Apellido = p.IdContratoNavigation?.IdUsuarioNavigation?.Apellido ?? p.IdCuotaNavigation?.IdUnidadNavigation?.ContratoAlquilers
                        .OrderByDescending(c => c.IdContrato)
                        .Select(c => c.IdUsuarioNavigation?.Apellido)
                        .FirstOrDefault()   
                };
            }).ToList();

            return View(model);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}