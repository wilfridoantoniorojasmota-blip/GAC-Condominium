using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;
using System.Security.Claims;

namespace PROTOTIPO.Controllers
{
    [Authorize(Roles = "Inquilino")]
    public class AvisosInquilinoController : Controller
    {
        private readonly AppDbContext _context;

        public AvisosInquilinoController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // LISTADO
        // =========================
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == userId);

            var avisos = await _context.Avisos
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
                .ToListAsync();

            ViewBag.NoLeidos = avisos.Count(a => !a.Leido);

            return View(avisos);
        }

        // =========================
        // DETALLE
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == userId);

            var aviso = await _context.Avisos
                .FirstOrDefaultAsync(a =>
                    a.IdAviso == id &&
                    (
                        a.IdUsuario == userId ||

                        (
                            a.IdUsuario == null &&
                            a.IdCondominio == usuario.IdCondominio
                        )
                    ));

            if (aviso == null)
                return NotFound();

            return View(aviso);
        }

        // =========================
        // MARCAR COMO LEÍDO
        // =========================
        [HttpPost]
        public async Task<IActionResult> MarcarLeido(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == userId);

            var aviso = await _context.Avisos
                .FirstOrDefaultAsync(a =>
                    a.IdAviso == id &&
                    (
                        a.IdUsuario == userId ||

                        (
                            a.IdUsuario == null &&
                            a.IdCondominio == usuario.IdCondominio
                        )
                    ));

            if (aviso == null)
                return NotFound();

            aviso.Leido = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}