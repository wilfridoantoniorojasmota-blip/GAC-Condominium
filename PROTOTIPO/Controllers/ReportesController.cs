using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;

namespace PROTOTIPO.Controllers
{
    public class ReportesController : Controller
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }



        //=========================================================
        // DASHBOARD REPORTES
        //=========================================================

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalResidentes =
                await _context.Usuarios.CountAsync();

            ViewBag.TotalUnidades =
                await _context.UnidadHabitacionals.CountAsync();

            ViewBag.UnidadesDisponibles =
                await _context.UnidadHabitacionals
                    .CountAsync(u => u.Estado == "Disponible");

            ViewBag.UnidadesOcupadas =
                await _context.UnidadHabitacionals
                    .CountAsync(u => u.Estado == "Ocupada");

            ViewBag.ContratosActivos =
                await _context.ContratoAlquilers
                    .CountAsync(c => c.Estado == "activo");

            ViewBag.ContratosFinalizados =
                await _context.ContratoAlquilers
                    .CountAsync(c => c.Estado == "finalizado");

            ViewBag.TotalSolicitudes =
                await _context.SolicitudIngresos.CountAsync();

            ViewBag.SolicitudesPendientes =
                await _context.SolicitudIngresos
                    .CountAsync(s => s.Estado == "pendiente");

            ViewBag.TotalIncidencias =
                await _context.IncidenciaMantenimientos.CountAsync();

            ViewBag.IncidenciasPendientes =
                await _context.IncidenciaMantenimientos
                    .CountAsync(i => i.Estado == "Pendiente");

            ViewBag.TotalReservas =
                await _context.ReservaAreaComuns.CountAsync();

            ViewBag.TotalPagos =
                await _context.Pagos.CountAsync();

            ViewBag.IngresosTotales =
                await _context.Pagos
                    .Where(p => p.EstadoPago == "Pagado")
                    .SumAsync(p => (decimal?)p.Monto) ?? 0;

            return View();
        }

        //=========================================================
        // REPORTE FINANCIERO
        //=========================================================

        public async Task<IActionResult> Financiero(int? mes, int? anio)
        {
            var fechaActual = DateTime.Now;

            int mesSeleccionado = mes ?? fechaActual.Month;
            int anioSeleccionado = anio ?? fechaActual.Year;


            var pagos = await _context.Pagos
                .Include(p => p.IdContratoNavigation)
                    .ThenInclude(c => c.IdUsuarioNavigation)
                .Include(p => p.IdCuotaNavigation)
                .Where(p =>
                    p.Fecha.HasValue &&
                    p.Fecha.Value.Month == mesSeleccionado &&
                    p.Fecha.Value.Year == anioSeleccionado
                )
                .ToListAsync();


            var alquileres = pagos
                .Where(p =>
                    p.TipoPago != null &&
                    p.TipoPago.ToLower() == "alquiler")
                .ToList();


            var mantenimientos = pagos
                .Where(p =>
                    p.TipoPago != null &&
                    p.TipoPago.ToLower() == "mantenimiento")
                .ToList();



            ViewBag.Mes = mesSeleccionado;
            ViewBag.Anio = anioSeleccionado;


            ViewBag.TotalIngresos =
                pagos.Sum(p => p.Monto ?? 0);


            ViewBag.IngresosAlquiler =
                alquileres.Sum(p => p.Monto ?? 0);


            ViewBag.IngresosMantenimiento =
                mantenimientos.Sum(p => p.Monto ?? 0);



            ViewBag.CantidadAlquiler =
                alquileres.Count;


            ViewBag.CantidadMantenimiento =
                mantenimientos.Count;


            return View(pagos);
        }

        //=========================================================
        // REPORTE DE CONTRATOS
        //=========================================================

        public async Task<IActionResult> Contratos(int page = 1)
        {
            int pageSize = 6;

            var contratosQuery = _context.ContratoAlquilers
                .Include(c => c.IdUsuarioNavigation)
                .Include(c => c.IdUnidadNavigation)
                .OrderByDescending(c => c.IdContrato)
                .AsQueryable();


            // ==========================
            // PAGINACIÓN
            // ==========================

            var totalContratos = await contratosQuery.CountAsync();

            ViewBag.TotalPaginas = (int)Math.Ceiling(
                totalContratos / (double)pageSize
            );

            ViewBag.PaginaActual = page;


            var contratos = await contratosQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();



            // ==========================
            // RESUMEN GENERAL
            // ==========================

            ViewBag.Total = totalContratos;


            ViewBag.Activos = await _context.ContratoAlquilers.CountAsync(c =>
                c.Estado != null &&
                c.Estado.ToLower() == "activo");


            ViewBag.Finalizados = await _context.ContratoAlquilers.CountAsync(c =>
                c.Estado != null &&
                c.Estado.ToLower() == "finalizado");



            // ==========================
            // INGRESOS MENSUALES
            // ==========================

            ViewBag.IngresosMensuales = await _context.ContratoAlquilers
                .Where(c =>
                    c.Estado != null &&
                    c.Estado.ToLower() == "activo")
                .SumAsync(c => c.MontoRenta ?? 0);



            // ==========================
            // CONTRATOS POR ESTADO
            // ==========================

            ViewBag.ContratosEstado = await _context.ContratoAlquilers
                .Where(c => !string.IsNullOrEmpty(c.Estado))
                .GroupBy(c => c.Estado)
                .Select(g => new
                {
                    Estado = g.Key,
                    Cantidad = g.Count()
                })
                .ToListAsync();



            // ==========================
            // CONTRATOS CREADOS POR MES
            // ==========================

            ViewBag.ContratosMes = await _context.ContratoAlquilers
                .Where(c => c.FechaInicio.HasValue)
                .GroupBy(c => new
                {
                    Mes = c.FechaInicio.Value.Month,
                    Año = c.FechaInicio.Value.Year
                })
                .Select(g => new
                {
                    Mes = g.Key.Mes,
                    Año = g.Key.Año,
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Año)
                .ThenBy(x => x.Mes)
                .ToListAsync();



            // ==========================
            // CONTRATOS PROXIMOS A VENCER
            // ==========================

            ViewBag.ProximosVencer = await _context.ContratoAlquilers
                .Include(c => c.IdUsuarioNavigation)
                .Include(c => c.IdUnidadNavigation)
                .Where(c =>
                    c.FechaFin.HasValue &&
                    c.FechaFin.Value >= DateOnly.FromDateTime(DateTime.Today) &&
                    c.FechaFin.Value <= DateOnly.FromDateTime(DateTime.Today.AddDays(3)))
                .OrderBy(c => c.FechaFin)
                .ToListAsync();



            return View(contratos);
        }
        //=========================================================
        // REPORTE DE RESIDENTES
        //=========================================================

        public async Task<IActionResult> Residentes()
        {
            var residentes = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .Include(u => u.ContratoAlquilers)
                    .ThenInclude(c => c.IdUnidadNavigation)
                .OrderBy(u => u.Nombre)
                .ToListAsync();


            // ==========================
            // RESUMEN
            // ==========================

            ViewBag.Total = residentes.Count;


            ViewBag.ConContrato = residentes.Count(u =>
                u.ContratoAlquilers.Any(c =>
                    c.Estado != null &&
                    c.Estado.ToLower() == "activo"));



            ViewBag.SinContrato = residentes.Count(u =>
                !u.ContratoAlquilers.Any(c =>
                    c.Estado != null &&
                    c.Estado.ToLower() == "activo"));




            // ==========================
            // RESIDENTES POR ROL
            // ==========================

            ViewBag.ResidentesRol = residentes
                .Where(u => u.IdRolNavigation != null)
                .GroupBy(u => u.IdRolNavigation.Nombre)
                .Select(g => new
                {
                    Rol = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();




            // ==========================
            // ULTIMOS REGISTRADOS
            // ==========================

            ViewBag.UltimosResidentes = residentes
                .OrderByDescending(u => u.IdUsuario)
                .Take(5)
                .ToList();



            return View(residentes);
        }


        //=========================================================
        // REPORTE DE INCIDENCIAS
        //=========================================================

        public async Task<IActionResult> Mantenimiento()
        {
            var incidencias = await _context.IncidenciaMantenimientos
                .Include(i => i.IdUsuarioNavigation)
                .Include(i => i.IdUnidadNavigation)
                .OrderByDescending(i => i.FechaSolicitud)
                .ToListAsync();


            // ==============================
            // RESUMEN GENERAL
            // ==============================

            ViewBag.Total = incidencias.Count;


            ViewBag.Pendientes = incidencias.Count(i =>
                i.Estado != null &&
                i.Estado.ToLower() == "pendiente");


            ViewBag.EnProceso = incidencias.Count(i =>
                i.Estado != null &&
                i.Estado.ToLower() == "en proceso");


            ViewBag.Finalizadas = incidencias.Count(i =>
                i.Estado != null &&
                (
                    i.Estado.ToLower() == "resuelta" ||
                    i.Estado.ToLower() == "cerrada"
                ));



            // ==============================
            // GRAFICO POR ESTADO
            // ==============================

            ViewBag.IncidenciasEstado = incidencias
                .Where(i => !string.IsNullOrEmpty(i.Estado))
                .GroupBy(i => i.Estado)
                .Select(g => new
                {
                    Estado = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();



            // ==============================
            // GRAFICO POR MES
            // ==============================

            ViewBag.IncidenciasMes = incidencias
                .Where(i => i.FechaSolicitud.HasValue)
                .GroupBy(i => new
                {
                    Mes = i.FechaSolicitud.Value.Month,
                    Año = i.FechaSolicitud.Value.Year
                })
                .Select(g => new
                {
                    Mes = g.Key.Mes,
                    Año = g.Key.Año,
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Año)
                .ThenBy(x => x.Mes)
                .ToList();



            // ==============================
            // UNIDADES CON MAS INCIDENCIAS
            // ==============================

            ViewBag.UnidadesProblema = incidencias
                .Where(i => i.IdUnidadNavigation != null)
                .GroupBy(i => i.IdUnidadNavigation.Numero)
                .Select(g => new
                {
                    Unidad = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();



            return View(incidencias);
        }

        //=========================================================
        // REPORTE DE RESERVAS
        //=========================================================

        public async Task<IActionResult> Reservas()
        {
            var reservas = await _context.ReservaAreaComuns
                .Include(r => r.IdUsuarioNavigation)
                .Include(r => r.IdAreaNavigation)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();


            // ==========================
            // RESUMEN
            // ==========================

            ViewBag.Total = reservas.Count;


            ViewBag.Aprobadas = reservas.Count(r =>
                r.Estado != null &&
                r.Estado.ToLower() == "aprobada");


            ViewBag.Pendientes = reservas.Count(r =>
                r.Estado != null &&
                r.Estado.ToLower() == "pendiente");


            ViewBag.Rechazadas = reservas.Count(r =>
                r.Estado != null &&
                r.Estado.ToLower() == "rechazada");



            // ==========================
            // RESERVAS POR ÁREA
            // ==========================

            ViewBag.ReservasArea = reservas
                .Where(r => r.IdAreaNavigation != null)
                .GroupBy(r => r.IdAreaNavigation.Nombre)
                .Select(g => new
                {
                    Area = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();



            // ==========================
            // RESERVAS POR MES
            // ==========================

            ViewBag.ReservasMes = reservas
                .Where(r => r.Fecha.HasValue)
                .GroupBy(r => new
                {
                    Mes = r.Fecha.Value.Month,
                    Año = r.Fecha.Value.Year
                })
                .Select(g => new
                {
                    Mes = g.Key.Mes,
                    Año = g.Key.Año,
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Año)
                .ThenBy(x => x.Mes)
                .ToList();



            // ==========================
            // PRÓXIMAS RESERVAS
            // ==========================

            ViewBag.ProximasReservas = reservas
                .Where(r =>
                    r.Fecha.HasValue &&
                    r.Fecha.Value >= DateOnly.FromDateTime(DateTime.Now))
                .OrderBy(r => r.Fecha)
                .Take(5)
                .ToList();



            return View(reservas);
        }



        //=========================================================
        // REPORTE DE SOLICITUDES
        //=========================================================

        public async Task<IActionResult> Solicitudes()
        {
            var solicitudes = await _context.SolicitudIngresos
                .Include(s => s.IdUnidadNavigation)
                .OrderByDescending(s => s.Fecha)
                .ToListAsync();


            // ==========================
            // RESUMEN
            // ==========================

            ViewBag.Total = solicitudes.Count;


            ViewBag.Pendientes = solicitudes.Count(s =>
                s.Estado != null &&
                s.Estado.ToLower() == "pendiente");


            ViewBag.Aprobadas = solicitudes.Count(s =>
                s.Estado != null &&
                s.Estado.ToLower() == "procesada");


            ViewBag.Rechazadas = solicitudes.Count(s =>
                s.Estado != null &&
                s.Estado.ToLower() == "rechazada");




            // ==========================
            // SOLICITUDES POR TIPO
            // ==========================

            ViewBag.SolicitudesTipo = solicitudes
                .Where(s => !string.IsNullOrEmpty(s.TipoSolicitud))
                .GroupBy(s => s.TipoSolicitud)
                .Select(g => new
                {
                    Tipo = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();




            // ==========================
            // SOLICITUDES POR MES
            // ==========================

            ViewBag.SolicitudesMes = solicitudes
                .Where(s => s.Fecha.HasValue)
                .GroupBy(s => new
                {
                    Mes = s.Fecha.Value.Month,
                    Año = s.Fecha.Value.Year
                })
                .Select(g => new
                {
                    Mes = g.Key.Mes,
                    Año = g.Key.Año,
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Año)
                .ThenBy(x => x.Mes)
                .ToList();




            // ==========================
            // ÚLTIMAS PENDIENTES
            // ==========================

            ViewBag.UltimasPendientes = solicitudes
                .Where(s =>
                    s.Estado != null &&
                    s.Estado.ToLower() == "pendiente")
                .OrderByDescending(s => s.Fecha)
                .Take(5)
                .ToList();



            return View(solicitudes);
        }



        //=========================================================
        // DASHBOARD GENERAL
        //=========================================================

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalResidentes =
                await _context.Usuarios.CountAsync();

            ViewBag.TotalUnidades =
                await _context.UnidadHabitacionals.CountAsync();

            ViewBag.TotalContratos =
                await _context.ContratoAlquilers.CountAsync();

            ViewBag.TotalPagos =
                await _context.Pagos.CountAsync();

            ViewBag.TotalSolicitudes =
                await _context.SolicitudIngresos.CountAsync();

            ViewBag.TotalReservas =
                await _context.ReservaAreaComuns.CountAsync();

            ViewBag.TotalIncidencias =
                await _context.IncidenciaMantenimientos.CountAsync();

            ViewBag.Ingresos =
                await _context.Pagos
                    .Where(p => p.EstadoPago == "Completado")
                    .SumAsync(p => (decimal?)p.Monto) ?? 0;

            ViewBag.UnidadesDisponibles =
                await _context.UnidadHabitacionals
                    .CountAsync(u => u.Estado == "Disponible");

            ViewBag.UnidadesOcupadas =
                await _context.UnidadHabitacionals
                    .CountAsync(u => u.Estado == "Ocupada");

            ViewBag.ContratosActivos =
                await _context.ContratoAlquilers
                    .CountAsync(c => c.Estado == "activo");

            ViewBag.IncidenciasPendientes =
                await _context.IncidenciaMantenimientos
                    .CountAsync(i => i.Estado == "Pendiente");

            ViewBag.SolicitudesPendientes =
                await _context.SolicitudIngresos
                    .CountAsync(s => s.Estado == "pendiente");

            ViewBag.ReservasPendientes =
                await _context.ReservaAreaComuns
                    .CountAsync(r => r.Estado == "pendiente");

            return View();
        }
    }
}