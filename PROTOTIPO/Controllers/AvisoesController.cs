using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;
using System.Security.Claims;

namespace PROTOTIPO.Controllers
{
    public class AvisoesController : Controller
    {
        private readonly AppDbContext _context;

        public AvisoesController(AppDbContext context)
        {
            _context = context;
        }


        // =========================
        // LISTADO
        // =========================
        public async Task<IActionResult> Index()
        {
            var avisos = await _context.Avisos
                .Include(a => a.IdCondominioNavigation)
                .Include(a => a.IdUsuarioNavigation)
                .Where(a => a.TipoAviso == "Manual")
                .OrderByDescending(a => a.FechaPublicacion)
                .ToListAsync();

            return View(avisos);
        }





        // =========================
        // DETALLE
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();



            var aviso = await _context.Avisos

                .Include(a => a.IdCondominioNavigation)

                .Include(a => a.IdUsuarioNavigation)

                .FirstOrDefaultAsync(a =>
                    a.IdAviso == id);



            if (aviso == null)
                return NotFound();



            return View(aviso);
        }






        // =========================
        // CREATE GET
        // =========================
        public IActionResult Create()
        {
            ViewData["IdCondominio"] =
                new SelectList(
                    _context.Condominios,
                    "IdCondominio",
                    "Nombre"
                );


            return View();
        }







        // =========================
        // CREATE POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Aviso aviso)
        {

            if (ModelState.IsValid)
            {

                aviso.FechaPublicacion = DateTime.Now;


                // Avisos creados manualmente
                aviso.TipoAviso = "Manual";


                // Todo aviso nuevo inicia como no leído
                aviso.Leido = false;



                _context.Avisos.Add(aviso);


                await _context.SaveChangesAsync();



                return RedirectToAction(nameof(Index));
            }




            ViewData["IdCondominio"] =
                new SelectList(
                    _context.Condominios,
                    "IdCondominio",
                    "Nombre",
                    aviso.IdCondominio
                );



            return View(aviso);
        }








        // =========================
        // EDIT GET
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
                return NotFound();



            var aviso = await _context.Avisos
                .FindAsync(id);



            if (aviso == null)
                return NotFound();




            // Evitar modificar avisos automáticos
            if (aviso.TipoAviso != "Manual")
            {
                TempData["Error"] =
                    "Los avisos automáticos no pueden ser editados.";

                return RedirectToAction(nameof(Index));
            }





            ViewData["IdCondominio"] =
                new SelectList(
                    _context.Condominios,
                    "IdCondominio",
                    "Nombre",
                    aviso.IdCondominio
                );



            return View(aviso);
        }








        // =========================
        // EDIT POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Aviso aviso)
        {

            if (id != aviso.IdAviso)
                return NotFound();




            var avisoExistente =
                await _context.Avisos
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.IdAviso == id);



            if (avisoExistente == null)
                return NotFound();




            if (avisoExistente.TipoAviso != "Manual")
            {
                TempData["Error"] =
                    "Los avisos automáticos no pueden ser editados.";

                return RedirectToAction(nameof(Index));
            }






            if (ModelState.IsValid)
            {

                aviso.TipoAviso = "Manual";


                _context.Update(aviso);


                await _context.SaveChangesAsync();



                return RedirectToAction(nameof(Index));
            }





            ViewData["IdCondominio"] =
                new SelectList(
                    _context.Condominios,
                    "IdCondominio",
                    "Nombre",
                    aviso.IdCondominio
                );



            return View(aviso);
        }










        // =========================
        // DELETE GET
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
                return NotFound();




            var aviso = await _context.Avisos

                .Include(a => a.IdCondominioNavigation)

                .Include(a => a.IdUsuarioNavigation)

                .FirstOrDefaultAsync(a =>
                    a.IdAviso == id);




            if (aviso == null)
                return NotFound();





            if (aviso.TipoAviso != "Manual")
            {
                TempData["Error"] =
                    "Los avisos automáticos no pueden ser eliminados.";

                return RedirectToAction(nameof(Index));
            }




            return View(aviso);
        }









        // =========================
        // DELETE POST
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var aviso =
                await _context.Avisos
                .FirstOrDefaultAsync(a =>
                    a.IdAviso == id);




            if (aviso != null)
            {

                if (aviso.TipoAviso != "Manual")
                {
                    TempData["Error"] =
                        "Los avisos automáticos no pueden ser eliminados.";

                    return RedirectToAction(nameof(Index));
                }



                _context.Avisos.Remove(aviso);
            }





            await _context.SaveChangesAsync();



            return RedirectToAction(nameof(Index));
        }







        private bool AvisoExists(int id)
        {
            return _context.Avisos.Any(e =>
                e.IdAviso == id);
        }

    }
}