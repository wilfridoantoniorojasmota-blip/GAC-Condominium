using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;

namespace PROTOTIPO.Services
{
    public class AvisosAutomaticosService : IAvisosAutomaticosService
    {
        private readonly AppDbContext _context;

        public AvisosAutomaticosService(AppDbContext context)
        {
            _context = context;
        }

        public async Task GenerarAvisos(int idUsuario)
        {
            await AvisosContratos(idUsuario);
            await AvisosMantenimiento(idUsuario);
        }

        // =====================================
        // AVISOS DE CONTRATO
        // =====================================

        private async Task AvisosContratos(int idUsuario)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var contratos = await _context.ContratoAlquilers
                .Include(c => c.IdUnidadNavigation)
                .Include(c => c.IdUsuarioNavigation)
                .Where(c =>
                    c.IdUsuario == idUsuario &&
                    c.Estado != null &&
                    c.Estado.ToLower() == "activo" &&
                    c.FechaFin.HasValue)
                .ToListAsync();

            foreach (var contrato in contratos)
            {
                var diasRestantes =
                    contrato.FechaFin.Value.DayNumber -
                    hoy.DayNumber;

                // SOLO CUANDO FALTEN EXACTAMENTE 2 DÍAS
                if (diasRestantes == 2)
                {
                    bool existe = await _context.Avisos.AnyAsync(a =>
                        a.IdUsuario == contrato.IdUsuario &&
                        a.TipoAviso == "Contrato" &&
                        a.IdReferencia == contrato.IdContrato);

                    if (!existe)
                    {
                        _context.Avisos.Add(new Aviso
                        {
                            Titulo = "Contrato próximo a vencer",

                            Contenido =
                                $"Su contrato de alquiler vence el {contrato.FechaFin.Value:dd/MM/yyyy}. " +
                                "Por favor comuníquese con administración.",

                            FechaPublicacion = DateTime.Now,

                            IdUsuario = contrato.IdUsuario,

                            IdCondominio = contrato.IdUnidadNavigation?.IdCondominio,

                            TipoAviso = "Contrato",

                            IdReferencia = contrato.IdContrato,

                            Leido = false
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        // =====================================
        // AVISOS DE MANTENIMIENTO
        // =====================================

        private async Task AvisosMantenimiento(int idUsuario)
        {
            var cuotas = await _context.CuotaMantenimientos

                .Include(c => c.IdUnidadNavigation)
                    .ThenInclude(u => u.ContratoAlquilers)

                .Where(c =>
                    c.Estado != null &&
                    c.Estado.ToLower() == "pendiente" &&

                    c.IdUnidadNavigation.ContratoAlquilers.Any(x =>
                        x.IdUsuario == idUsuario &&
                        x.Estado != null &&
                        x.Estado.ToLower() == "activo"))

                .ToListAsync();

            foreach (var cuota in cuotas)
            {
                var contrato =
                    cuota.IdUnidadNavigation
                    .ContratoAlquilers
                    .FirstOrDefault(c =>
                        c.IdUsuario == idUsuario &&
                        c.Estado != null &&
                        c.Estado.ToLower() == "activo");

                if (contrato == null)
                    continue;

                bool existe = await _context.Avisos.AnyAsync(a =>
                    a.IdUsuario == idUsuario &&
                    a.TipoAviso == "Mantenimiento" &&
                    a.IdReferencia == cuota.IdCuota);

                if (!existe)
                {
                    _context.Avisos.Add(new Aviso
                    {
                        Titulo = "Cuota de mantenimiento pendiente",

                        Contenido =
                            $"Tiene pendiente la cuota de mantenimiento del período {cuota.Periodo} por RD${cuota.Monto:N2}.",

                        FechaPublicacion = DateTime.Now,

                        IdUsuario = idUsuario,

                        IdCondominio = cuota.IdUnidadNavigation.IdCondominio,

                        TipoAviso = "Mantenimiento",

                        IdReferencia = cuota.IdCuota,

                        Leido = false
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}