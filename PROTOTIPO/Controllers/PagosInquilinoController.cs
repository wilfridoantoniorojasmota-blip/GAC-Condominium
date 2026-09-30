using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;
using System.Security.Claims;

namespace PROTOTIPO.Controllers
{
    [Authorize(Roles = "Inquilino")]
    public class PagosInquilinoController : Controller
    {
        private readonly AppDbContext _context;

        public PagosInquilinoController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var contrato = await _context.ContratoAlquilers
    .Include(c => c.IdUnidadNavigation)
    .FirstOrDefaultAsync(c => c.IdUsuario == userId);

            if (contrato == null)
                return View(new List<Pago>());

            ViewBag.Unidad = contrato.IdUnidadNavigation?.Numero ?? "Sin asignar";

            if (contrato == null)
                return View(new List<Pago>());

            var pagos = await _context.Pagos
                .Include(p => p.IdCuotaNavigation)
                .Where(p => p.IdContrato == contrato.IdContrato
                            || (p.IdCuotaNavigation != null && p.IdCuotaNavigation.IdUnidad == contrato.IdUnidad))
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            // 🔹 DASHBOARD FINANCIERO
            // Normalizar estado y aceptar sinónimos/case-insensitive para evitar valores inesperados en BD
            decimal totalPagado = 0, totalPendiente = 0, totalAtrasado = 0;

            foreach (var p in pagos)
            {
                var s = (p.EstadoPago ?? string.Empty).Trim().ToLowerInvariant();
                if (s == "completado" || s == "pagado" || s == "pagado ")
                {
                    totalPagado += p.Monto ?? 0;
                }
                else if (s == "pendiente")
                {
                    totalPendiente += p.Monto ?? 0;
                }
                else if (s == "atrasado" || s == "vencido")
                {
                    totalAtrasado += p.Monto ?? 0;
                }
            }

            ViewBag.TotalPagado = totalPagado;
            ViewBag.Pendiente = totalPendiente;
            ViewBag.Atrasado = totalAtrasado;

            return View(pagos);
        }
        public async Task<IActionResult> MisDeudas()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var contrato = await _context.ContratoAlquilers
                .FirstOrDefaultAsync(c => c.IdUsuario == userId);

            if (contrato == null)
            {
                ViewBag.Cuotas = new List<Pago>();
                ViewBag.Rentas = new List<Pago>();

                ViewBag.TotalRenta = 0;
                ViewBag.TotalCuotas = 0;

                return View();
            }

            var cuotasPendientes = await _context.Pagos
                .Where(p => p.IdContrato == contrato.IdContrato
                         && p.TipoPago == "Cuota"
                         && (p.EstadoPago == "Pendiente" || p.EstadoPago == "Atrasado"))
                .ToListAsync();

            var rentasPendientes = await _context.Pagos
                .Where(p => p.IdContrato == contrato.IdContrato
                         && p.TipoPago == "Renta"
                         && (p.EstadoPago == "Pendiente" || p.EstadoPago == "Atrasado"))
                .ToListAsync();

            // ✔ AQUÍ VA TU CÁLCULO
            ViewBag.TotalRenta = rentasPendientes.Sum(x => x.Monto ?? 0);
            ViewBag.TotalCuotas = cuotasPendientes.Sum(x => x.Monto ?? 0);

            ViewBag.Cuotas = cuotasPendientes;
            ViewBag.Rentas = rentasPendientes;

            return View();
        }

        public async Task<IActionResult> Historial(string tipoPago, string estadoPago)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var contrato = await _context.ContratoAlquilers
                .FirstOrDefaultAsync(c => c.IdUsuario == userId);

            if (contrato == null)
                return View(new List<Pago>());

            var query = _context.Pagos
                .Include(p => p.IdCuotaNavigation)
                    .ThenInclude(c => c.IdUnidadNavigation)
                .Where(p =>
                    // Pagos de alquiler vinculados por contrato
                    p.IdContrato == contrato.IdContrato
                    ||
                    // Pagos de mantenimiento vinculados por cuota de la unidad
                    (p.IdCuotaNavigation != null && 
                     p.IdCuotaNavigation.IdUnidad == contrato.IdUnidad)
                )
                .AsQueryable();

            // 🔹 FILTRO POR TIPO
            if (!string.IsNullOrEmpty(tipoPago))
            {
                query = query.Where(p => p.TipoPago == tipoPago);
            }

            // 🔹 FILTRO POR ESTADO (normalizado para case-insensitive y sinónimos)
            if (!string.IsNullOrEmpty(estadoPago))
            {
                var estadoNormalizado = estadoPago.Trim().ToLowerInvariant();

                query = query.Where(p =>
                    (estadoNormalizado == "completado" || estadoNormalizado == "pagado") &&
                    (p.EstadoPago.ToLower() == "completado" || 
                     p.EstadoPago.ToLower() == "pagado" || 
                     p.EstadoPago.ToLower().Trim() == "pagado")
                    ||
                    estadoNormalizado == "pendiente" &&
                    p.EstadoPago.ToLower() == "pendiente"
                    ||
                    (estadoNormalizado == "atrasado" || estadoNormalizado == "vencido") &&
                    (p.EstadoPago.ToLower() == "atrasado" || 
                     p.EstadoPago.ToLower() == "vencido")
                );
            }

            var pagos = await query
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            // 🔹 Para mantener selección en la vista
            ViewBag.TipoPago = tipoPago;
            ViewBag.EstadoPago = estadoPago;

            return View(pagos);
        }
    }
}