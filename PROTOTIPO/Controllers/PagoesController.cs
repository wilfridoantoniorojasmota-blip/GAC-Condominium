using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models.ViewModel;
using PROTOTIPO.Models;

namespace PROTOTIPO.Controllers
{
    public class PagoesController : Controller
    {
        private readonly AppDbContext _context;

        public PagoesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Pagoes
        public async Task<IActionResult> Index(
           int? inquilinoId,
           int? mes,
           int? anio,
           int page = 1)
        {
            const int pageSize = 6;

            var query = _context.Pagos

                .Include(p => p.IdContratoNavigation)
                    .ThenInclude(c => c.IdUsuarioNavigation)

                .Include(p => p.IdContratoNavigation)
                    .ThenInclude(c => c.IdUnidadNavigation)

                .Include(p => p.IdCuotaNavigation)
                    .ThenInclude(c => c.IdUnidadNavigation)
                        .ThenInclude(u => u.ContratoAlquilers)
                            .ThenInclude(c => c.IdUsuarioNavigation)

                .AsQueryable();



            // =====================================================
            // FILTRO POR RESIDENTE
            // ALQUILER + MANTENIMIENTO
            // =====================================================

            if (inquilinoId.HasValue)
            {

                query = query.Where(p =>

                    // Pago alquiler
                    (
                        p.IdContratoNavigation != null &&
                        p.IdContratoNavigation.IdUsuario == inquilinoId
                    )


                    ||

                    // Pago mantenimiento
                    (
                        p.IdCuotaNavigation != null &&

                        p.IdCuotaNavigation
                         .IdUnidadNavigation
                         .ContratoAlquilers

                         .Any(c =>
                             c.IdUsuario == inquilinoId
                         )
                    )

                );

            }




            // =====================================================
            // FILTRO MES
            // =====================================================

            if (mes.HasValue)
            {
                query = query.Where(p =>
                    p.Fecha.HasValue &&
                    p.Fecha.Value.Month == mes
                );
            }



            // =====================================================
            // FILTRO AÑO
            // =====================================================

            if (anio.HasValue)
            {
                query = query.Where(p =>
                    p.Fecha.HasValue &&
                    p.Fecha.Value.Year == anio
                );
            }



            var pagos = await query

                .OrderByDescending(p => p.Fecha)

                .ToListAsync();

            var totalCount = pagos.Count;

            var result = pagos.Select(p => new PagoDTO
            {

                IdPago = p.IdPago,


                IdUsuario =
                    p.IdContratoNavigation?.IdUsuario,



                Residente =

                    // ALQUILER
                    (
                        (
                        p.IdContratoNavigation?
                        .IdUsuarioNavigation?
                        .Nombre
                        ?? ""
                        )
                        +

                        " "

                        +

                        (
                        p.IdContratoNavigation?
                        .IdUsuarioNavigation?
                        .Apellido
                        ?? ""
                        )

                    ).Trim()


                    != ""


                    ?

                    (

                        p.IdContratoNavigation
                        .IdUsuarioNavigation
                        .Nombre

                        +

                        " "

                        +

                        p.IdContratoNavigation
                        .IdUsuarioNavigation
                        .Apellido

                    )


                    :


                    // MANTENIMIENTO
                    p.IdCuotaNavigation?
                    .IdUnidadNavigation?
                    .ContratoAlquilers

                    .OrderByDescending(c =>
                        c.IdContrato)

                    .Select(c =>

                        c.IdUsuarioNavigation.Nombre
                        +
                        " "
                        +
                        c.IdUsuarioNavigation.Apellido

                    )

                    .FirstOrDefault()



                    ??

                    "Sin residente",



                Unidad =

                    p.IdContratoNavigation?
                    .IdUnidadNavigation?
                    .Numero

                    ??

                    p.IdCuotaNavigation?
                    .IdUnidadNavigation?
                    .Numero

                    ??

                    "N/A",



                TipoPago = p.TipoPago,

                Monto = p.Monto,

                Fecha = p.Fecha,

                Estado = p.EstadoPago



            }).ToList();

            // 🔹 APLICAR PAGINACIÓN
            var pageNumber = Math.Max(page, 1);
            var paginatedResult = result
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paginatedList = new PaginatedList<PagoDTO>(paginatedResult, totalCount, pageSize, pageNumber);



            // =====================================================
            // CARGAR INQUILINOS PARA FILTRO
            // =====================================================

            ViewData["Inquilinos"] = await _context.Usuarios

                .OrderBy(u => u.Nombre)

                .Select(u => new
                {
                    u.IdUsuario,

                    Nombre =
                        u.Nombre
                        +
                        " "
                        +
                        u.Apellido
                })

                .ToListAsync();




            ViewData["InquilinoId"] = inquilinoId;

            ViewData["Mes"] = mes;

            ViewData["Anio"] = anio;

            // =====================================================
            // CUENTAS POR COBRAR (ALQUILER + MANTENIMIENTO)
            // =====================================================

            var cuentasCobrar = new List<object>();


            for (int i = 5; i >= 0; i--)
            {
                var fecha = DateTime.Now.AddMonths(-i);


                var periodoActual = fecha.ToString("yyyy-MM");



                // ============================
                // ALQUILER PENDIENTE
                // ============================

                var alquilerPendiente = await _context.Pagos

                    .Where(p =>
                        p.TipoPago == "Alquiler"
                        &&
                        p.EstadoPago != null
                        &&
                        p.EstadoPago.ToLower() == "pendiente"
                        &&
                        p.Fecha.HasValue
                        &&
                        p.Fecha.Value.Month == fecha.Month
                        &&
                        p.Fecha.Value.Year == fecha.Year
                    )

                    .SumAsync(p => p.Monto ?? 0);





                // ============================
                // MANTENIMIENTO PENDIENTE
                // ============================

                var mantenimientoPendiente = await _context.CuotaMantenimientos

                    .Where(c =>
                        c.Estado != null
                        &&
                        c.Estado.ToLower() == "pendiente"
                        &&
                        c.Periodo == periodoActual
                    )

                    .SumAsync(c => c.Monto ?? 0);





                // ============================
                // TOTAL CUENTAS POR COBRAR
                // ============================

                cuentasCobrar.Add(new
                {
                    Mes = fecha.ToString("MMMM"),
                    Monto = alquilerPendiente + mantenimientoPendiente,

                    Alquiler = alquilerPendiente,

                    Mantenimiento = mantenimientoPendiente
                });

            }



            ViewBag.CuentasCobrar = cuentasCobrar;



            return View(paginatedList);

        }
        // GET: Pagoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var pago = await _context.Pagos
                .Include(p => p.IdContratoNavigation)
                .Include(p => p.IdCuotaNavigation)
                .FirstOrDefaultAsync(m => m.IdPago == id);

            if (pago == null)
                return NotFound();

            return View(pago);
        }

        // =====================================================
        // GET: Pagoes/Create
        // =====================================================

        public IActionResult Create()
        {

            // =====================================
            // CONTRATOS DE ALQUILER
            // =====================================

            ViewData["IdContrato"] = new SelectList(

                _context.ContratoAlquilers

                    .Include(c => c.IdUsuarioNavigation)

                    .Include(c => c.IdUnidadNavigation)

                    .Where(c => c.Estado == "activo")

                    .Select(c => new
                    {
                        c.IdContrato,

                        Nombre =
                            c.IdUsuarioNavigation.Nombre
                            +
                            " "
                            +
                            c.IdUsuarioNavigation.Apellido
                            +
                            " - Unidad "
                            +
                            c.IdUnidadNavigation.Numero
                            +
                            " - Contrato #"
                            +
                            c.IdContrato
                    }),

                "IdContrato",
                "Nombre"
            );





            // =====================================
            // CUOTAS DE MANTENIMIENTO PENDIENTES
            // =====================================

            ViewData["IdCuota"] = new SelectList(

                _context.CuotaMantenimientos

                    .Include(c => c.IdUnidadNavigation)

                        .ThenInclude(u =>
                            u.ContratoAlquilers)

                        .ThenInclude(c =>
                            c.IdUsuarioNavigation)


                    .Where(c =>
                        c.Estado == "Pendiente"
                    )


                    .Select(c => new
                    {

                        c.IdCuota,


                        Nombre =

                            "Cuota #"
                            +
                            c.IdCuota

                            +

                            " - Unidad "
                            +
                            c.IdUnidadNavigation.Numero


                            +

                            " - "

                            +

                            c.IdUnidadNavigation
                            .ContratoAlquilers

                            .OrderByDescending(x =>
                                x.IdContrato)

                            .Select(x =>
                                x.IdUsuarioNavigation.Nombre
                                +
                                " "
                                +
                                x.IdUsuarioNavigation.Apellido
                            )

                            .FirstOrDefault()


                            +

                            " - Periodo "
                            +
                            c.Periodo


                            +

                            " - RD$"
                            +
                            c.Monto

                    }),


                "IdCuota",
                "Nombre"

            );



            return View();
        }







        // =====================================================
        // POST: Pagoes/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("IdPago,Fecha,Monto,MetodoPago,EstadoPago,IdCuota,IdContrato,TipoPago")]
    Pago pago)
        {
            // =====================================
            // VALIDACIONES DE NEGOCIO
            // =====================================

            if (pago.TipoPago == "Alquiler")
            {
                pago.IdCuota = null;

                if (pago.IdContrato == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Debe seleccionar un contrato de alquiler."
                    );
                }
            }


            else if (pago.TipoPago == "Mantenimiento")
            {
                pago.IdContrato = null;

                if (pago.IdCuota == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Debe seleccionar una cuota de mantenimiento."
                    );
                }
            }


            if (!ModelState.IsValid)
            {
                // Recargar contratos completos
                ViewData["IdContrato"] = new SelectList(
                    _context.ContratoAlquilers
                        .Include(c => c.IdUsuarioNavigation)
                        .Include(c => c.IdUnidadNavigation)
                        .Where(c => c.Estado == "activo")
                        .Select(c => new
                        {
                            c.IdContrato,

                            Nombre =
                                c.IdUsuarioNavigation.Nombre + " " +
                                c.IdUsuarioNavigation.Apellido +
                                " - Unidad " +
                                c.IdUnidadNavigation.Numero +
                                " - Contrato #" +
                                c.IdContrato
                        }),
                    "IdContrato",
                    "Nombre",
                    pago.IdContrato
                );


                // Recargar cuotas completas
                ViewData["IdCuota"] = new SelectList(
                    _context.CuotaMantenimientos
                        .Include(c => c.IdUnidadNavigation)
                            .ThenInclude(u => u.ContratoAlquilers)
                                .ThenInclude(c => c.IdUsuarioNavigation)
                        .Where(c => c.Estado == "Pendiente")
                        .Select(c => new
                        {
                            c.IdCuota,

                            Nombre =
                                "Cuota #" + c.IdCuota +
                                " - Unidad " +
                                c.IdUnidadNavigation.Numero +
                                " - " +
                                c.IdUnidadNavigation.ContratoAlquilers
                                    .OrderByDescending(x => x.IdContrato)
                                    .Select(x =>
                                        x.IdUsuarioNavigation.Nombre +
                                        " " +
                                        x.IdUsuarioNavigation.Apellido)
                                    .FirstOrDefault()
                                +
                                " - Periodo " +
                                c.Periodo +
                                " - RD$" +
                                c.Monto
                        }),
                    "IdCuota",
                    "Nombre",
                    pago.IdCuota
                );


                return View(pago);
            }



            // =====================================
            // GUARDAR PAGO
            // =====================================

            _context.Pagos.Add(pago);

            await _context.SaveChangesAsync();



            // =====================================
            // ACTUALIZAR CUOTA DE MANTENIMIENTO
            // =====================================

            if (pago.TipoPago == "Mantenimiento"
                && pago.IdCuota.HasValue
                && pago.EstadoPago == "Pagado")
            {

                var cuota = await _context.CuotaMantenimientos
                    .FirstOrDefaultAsync(c =>
                        c.IdCuota == pago.IdCuota.Value);


                if (cuota != null)
                {
                    cuota.Estado = "Pagado";

                    _context.CuotaMantenimientos.Update(cuota);

                    await _context.SaveChangesAsync();
                }
            }



            return RedirectToAction(nameof(Index));
        }
        // GET: Pagoes/Edit/5
        // =====================================================
        // GET: Pagoes/Edit/5
        // =====================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();


            var pago = await _context.Pagos
                .FirstOrDefaultAsync(p => p.IdPago == id);


            if (pago == null)
                return NotFound();



            ViewData["IdContrato"] =
                new SelectList(
                    _context.ContratoAlquilers,
                    "IdContrato",
                    "IdContrato",
                    pago.IdContrato
                );


            ViewData["IdCuota"] =
                new SelectList(
                    _context.CuotaMantenimientos,
                    "IdCuota",
                    "IdCuota",
                    pago.IdCuota
                );


            return View(pago);
        }



        // POST: Pagoes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(
    int id,
    [Bind(
        "IdPago,Fecha,Monto,MetodoPago,EstadoPago,IdCuota,IdContrato,TipoPago"
    )]
    Pago pago)
        {

            if (id != pago.IdPago)
                return NotFound();



            if (pago.TipoPago == "Alquiler")
            {

                pago.IdCuota = null;


                if (pago.IdContrato == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Debe seleccionar un contrato."
                    );
                }

            }



            if (pago.TipoPago == "Mantenimiento")
            {

                pago.IdContrato = null;


                if (pago.IdCuota == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Debe seleccionar una cuota."
                    );
                }


            }





            if (ModelState.IsValid)
            {

                try
                {

                    _context.Update(pago);



                    // si es mantenimiento actualiza cuota

                    if (
                        pago.TipoPago == "Mantenimiento"
                        &&
                        pago.IdCuota.HasValue
                    )
                    {

                        var cuota =
                            await _context.CuotaMantenimientos
                            .FirstOrDefaultAsync(c =>
                                c.IdCuota == pago.IdCuota);



                        if (cuota != null)
                        {

                            cuota.Estado =
                                pago.EstadoPago == "Pagado"
                                ?
                                "Pagado"
                                :
                                "Pendiente";


                            _context.Update(cuota);

                        }

                    }



                    await _context.SaveChangesAsync();


                }

                catch (DbUpdateConcurrencyException)
                {

                    if (!PagoExists(pago.IdPago))
                        return NotFound();

                    throw;

                }


                return RedirectToAction(nameof(Index));

            }



            ViewData["IdContrato"] =
                new SelectList(
                    _context.ContratoAlquilers,
                    "IdContrato",
                    "IdContrato",
                    pago.IdContrato
                );


            ViewData["IdCuota"] =
                new SelectList(
                    _context.CuotaMantenimientos,
                    "IdCuota",
                    "IdCuota",
                    pago.IdCuota
                );


            return View(pago);

        }



        // GET: Pagoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
                return NotFound();



            var pago =
                await _context.Pagos

                .Include(p =>
                    p.IdContratoNavigation)

                .Include(p =>
                    p.IdCuotaNavigation)

                .FirstOrDefaultAsync(p =>
                    p.IdPago == id);



            if (pago == null)
                return NotFound();



            return View(pago);

        }



        // POST: Pagoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {


            var pago =
                await _context.Pagos
                .FirstOrDefaultAsync(p =>
                    p.IdPago == id);



            if (pago != null)
            {

                // si elimina mantenimiento
                // devolvemos cuota a pendiente

                if (
                    pago.TipoPago == "Mantenimiento"
                    &&
                    pago.IdCuota.HasValue
                )
                {

                    var cuota =
                        await _context.CuotaMantenimientos
                        .FirstOrDefaultAsync(c =>
                            c.IdCuota == pago.IdCuota);



                    if (cuota != null)
                    {

                        cuota.Estado =
                            "Pendiente";

                    }


                }



                _context.Pagos.Remove(pago);


                await _context.SaveChangesAsync();

            }



            return RedirectToAction(nameof(Index));

        }


        private bool PagoExists(int id)
        {
            return _context.Pagos.Any(e => e.IdPago == id);
        }
        public async Task<IActionResult> Historial(
    int id,
    int? mes,
    int? anio,
    string? estado)
        {


            var query =
                _context.Pagos


                .Include(p =>
                    p.IdContratoNavigation)

                    .ThenInclude(c =>
                        c.IdUsuarioNavigation)


                .Include(p =>
                    p.IdContratoNavigation)

                    .ThenInclude(c =>
                        c.IdUnidadNavigation)



                .Include(p =>
                    p.IdCuotaNavigation)

                    .ThenInclude(c =>
                        c.IdUnidadNavigation)

                        .ThenInclude(u =>
                            u.ContratoAlquilers)

                            .ThenInclude(c =>
                                c.IdUsuarioNavigation)



                .Where(p =>

                    // alquiler

                    (
                        p.IdContratoNavigation != null
                        &&
                        p.IdContratoNavigation.IdUsuario == id
                    )


                    ||

                    // mantenimiento

                    (
                        p.IdCuotaNavigation != null

                        &&

                        p.IdCuotaNavigation
                        .IdUnidadNavigation
                        .ContratoAlquilers

                        .Any(c =>
                            c.IdUsuario == id
                        )
                    )

                )

                .AsQueryable();





            if (mes.HasValue)
            {
                query =
                    query.Where(p =>
                        p.Fecha.HasValue
                        &&
                        p.Fecha.Value.Month == mes
                    );
            }





            if (anio.HasValue)
            {
                query =
                    query.Where(p =>
                        p.Fecha.HasValue
                        &&
                        p.Fecha.Value.Year == anio
                    );
            }





            if (!string.IsNullOrEmpty(estado))
            {

                query =
                    query.Where(p =>
                        p.EstadoPago == estado
                    );

            }





            var historial =
                await query

                .OrderByDescending(p =>
                    p.Fecha)


                .Select(p => new PagoDTO
                {

                    IdPago =
                        p.IdPago,


                    IdUsuario =
                        p.IdContratoNavigation != null
                        ?
                        p.IdContratoNavigation.IdUsuario
                        :
                        p.IdCuotaNavigation
                        .IdUnidadNavigation
                        .ContratoAlquilers
                        .OrderByDescending(c =>
                            c.IdContrato)
                        .Select(c =>
                            c.IdUsuario)
                        .FirstOrDefault(),



                    Residente =

                        p.IdContratoNavigation != null

                        ?

                        p.IdContratoNavigation
                        .IdUsuarioNavigation
                        .Nombre
                        +
                        " "
                        +
                        p.IdContratoNavigation
                        .IdUsuarioNavigation
                        .Apellido


                        :

                        p.IdCuotaNavigation
                        .IdUnidadNavigation
                        .ContratoAlquilers

                        .OrderByDescending(c =>
                            c.IdContrato)

                        .Select(c =>
                            c.IdUsuarioNavigation.Nombre
                            +
                            " "
                            +
                            c.IdUsuarioNavigation.Apellido)

                        .FirstOrDefault(),





                    Unidad =

                        p.IdContratoNavigation != null

                        ?

                        p.IdContratoNavigation
                        .IdUnidadNavigation
                        .Numero


                        :

                        p.IdCuotaNavigation
                        .IdUnidadNavigation
                        .Numero,





                    TipoPago =
                        p.TipoPago,


                    Monto =
                        p.Monto,


                    Fecha =
                        p.Fecha,


                    Estado =
                        p.EstadoPago



                })

                .ToListAsync();





            ViewBag.Residente =
                historial.FirstOrDefault()?.Residente;


            ViewData["Mes"] = mes;

            ViewData["Anio"] = anio;

            ViewData["Estado"] = estado;



            return View(historial);

        }

        [HttpGet]
        public async Task<IActionResult> ObtenerMontoContrato(int idContrato)
        {
            var contrato = await _context.ContratoAlquilers
                .FirstOrDefaultAsync(c => c.IdContrato == idContrato);

            if (contrato == null)
                return Json(new { monto = 0 });


            return Json(new
            {
                monto = contrato.MontoRenta
            });
        }



        [HttpGet]
        public async Task<IActionResult> ObtenerMontoCuota(int idCuota)
        {
            var cuota = await _context.CuotaMantenimientos
                .FirstOrDefaultAsync(c => c.IdCuota == idCuota);

            if (cuota == null)
                return Json(new { monto = 0 });


            return Json(new
            {
                monto = cuota.Monto
            });
        }
    }
}