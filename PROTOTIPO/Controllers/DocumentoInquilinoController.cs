using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;
using System.Security.Claims;

namespace PROTOTIPO.Controllers
{
    [Authorize(Roles = "Inquilino")]
    public class DocumentoInquilinoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;


        public DocumentoInquilinoController(
            AppDbContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }



        // ============================
        // LISTADO
        // ============================

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));


            // Obtener contrato activo del inquilino
            var contrato = await _context.ContratoAlquilers
                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == userId &&
                    c.Estado != null &&
                    c.Estado.ToLower() == "activo");


            if (contrato == null)
            {
                return View(new List<Documento>());
            }


            var unidadId = contrato.IdUnidad;


            var condominioId = await _context.UnidadHabitacionals
                .Where(u => u.IdUnidad == unidadId)
                .Select(u => u.IdCondominio)
                .FirstOrDefaultAsync();



            var documentos = await _context.Documentos

                .Include(d => d.IdUnidadNavigation)

                .Include(d => d.IdCondominioNavigation)

                .Include(d => d.IdContratoNavigation)
                    .ThenInclude(c => c.IdUsuarioNavigation)

                .Where(d =>
                    d.IdUnidad == unidadId ||
                    d.IdCondominio == condominioId ||
                    d.IdContrato == contrato.IdContrato)

                .OrderByDescending(d => d.FechaSubida)

                .ToListAsync();



            return View(documentos);
        }





        // ============================
        // CREATE GET
        // ============================

        public IActionResult Create()
        {
            return View();
        }






        // ============================
        // CREATE POST
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Documento documento,
            IFormFile archivoSubido)
        {


            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));



            var contrato = await _context.ContratoAlquilers
    .FirstOrDefaultAsync(c =>
        c.IdUsuario == userId &&
        c.Estado == "activo");

            if (contrato == null)
            {
                ModelState.AddModelError(
                    "",
                    "No tienes un contrato activo.");

                return View(documento);
            }



            if (archivoSubido == null)
            {
                ModelState.AddModelError(
                    "",
                    "Debe seleccionar un archivo.");

                return View(documento);
            }




            // VALIDAR EXTENSIÓN

            var extensionesPermitidas = new[]
            {
                ".pdf",
                ".doc",
                ".docx",
                ".jpg",
                ".jpeg",
                ".png"
            };


            var extension =
                Path.GetExtension(
                    archivoSubido.FileName)
                    .ToLower();



            if (!extensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError(
                    "",
                    "Tipo de archivo no permitido.");

                return View(documento);
            }





            if (ModelState.IsValid)
            {

                var unidadId = contrato.IdUnidad;



                var condominioId = await _context.UnidadHabitacionals
    .Where(u => u.IdUnidad == unidadId)
    .Select(u => u.IdCondominio)
    .FirstOrDefaultAsync();




                string carpeta =
                    Path.Combine(
                        _env.WebRootPath,
                        "documentos");



                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }




                string nombreArchivo =
                    Guid.NewGuid()
                    + extension;




                string ruta =
                    Path.Combine(
                        carpeta,
                        nombreArchivo);




                using (var stream =
                    new FileStream(
                        ruta,
                        FileMode.Create))
                {
                    await archivoSubido.CopyToAsync(stream);
                }




                documento.Archivo =
                    "/documentos/" + nombreArchivo;



                documento.FechaSubida =
                    DateTime.Now;



                // ASIGNACIÓN AUTOMÁTICA

                documento.IdUnidad =
                    unidadId;


                documento.IdCondominio =
                    condominioId;



                _context.Documentos.Add(documento);


                await _context.SaveChangesAsync();



                return RedirectToAction(nameof(Index));

            }


            return View(documento);
        }






        // ============================
        // DETAILS
        // ============================

        public async Task<IActionResult> Details(int id)
        {

            var documento = await _context.Documentos

                .Include(d => d.IdUnidadNavigation)

                .Include(d => d.IdCondominioNavigation)

                .Include(d => d.IdContratoNavigation)

                .FirstOrDefaultAsync(
                    d => d.IdDocumento == id);



            if (documento == null)
                return NotFound();



            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));



            var contrato = await _context.ContratoAlquilers

                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == userId &&
                    c.Estado != null &&
                    c.Estado.ToLower() == "activo");



            if (contrato == null)
                return Forbid();



            if (documento.IdUnidad != contrato.IdUnidad &&
                documento.IdContrato != contrato.IdContrato)
            {
                return Forbid();
            }



            return View(documento);
        }







        // ============================
        // DESCARGAR
        // ============================

        public async Task<IActionResult> Descargar(int id)
        {

            var documento =
                await _context.Documentos
                .FirstOrDefaultAsync(
                    d => d.IdDocumento == id);



            if (documento == null)
                return NotFound();



            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));



            var contrato = await _context.ContratoAlquilers

                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == userId &&
                    c.Estado != null &&
                    c.Estado.ToLower() == "activo");



            if (contrato == null)
                return Forbid();



            if (documento.IdUnidad != contrato.IdUnidad &&
                documento.IdContrato != contrato.IdContrato)
            {
                return Forbid();
            }




            if (string.IsNullOrEmpty(documento.Archivo))
                return NotFound();



            string ruta =
                Path.Combine(
                    _env.WebRootPath,
                    documento.Archivo.TrimStart('/'));



            if (!System.IO.File.Exists(ruta))
                return NotFound();




            byte[] archivo =
                await System.IO.File.ReadAllBytesAsync(ruta);



            return File(
                archivo,
                "application/octet-stream",
                Path.GetFileName(ruta));

        }







        // ============================
        // DELETE
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {

            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));



            var documento =
                await _context.Documentos
                .FirstOrDefaultAsync(
                    d => d.IdDocumento == id);



            if (documento == null)
                return NotFound();




            var unidadId = await _context.ContratoAlquilers
       .Where(c => c.IdUsuario == userId &&
                   c.Estado == "activo")
       .Select(c => c.IdUnidad)
       .FirstOrDefaultAsync();




            if (documento.IdUnidad != unidadId)
            {
                return Forbid();
            }






            if (!string.IsNullOrEmpty(documento.Archivo))
            {

                string ruta =
                    Path.Combine(
                        _env.WebRootPath,
                        documento.Archivo.TrimStart('/'));



                if (System.IO.File.Exists(ruta))
                {
                    System.IO.File.Delete(ruta);
                }

            }





            _context.Documentos.Remove(documento);


            await _context.SaveChangesAsync();



            return RedirectToAction(nameof(Index));

        }

        // ============================
        // EDIT GET
        // ============================

        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));


            var documento = await _context.Documentos
                .FirstOrDefaultAsync(d => d.IdDocumento == id);


            if (documento == null)
                return NotFound();



            var unidadId = await _context.ContratoAlquilers
      .Where(c => c.IdUsuario == userId &&
                  c.Estado == "activo")
      .Select(c => c.IdUnidad)
      .FirstOrDefaultAsync();



            if (documento.IdUnidad != unidadId)
                return Forbid();



            return View(documento);
        }

        // ============================
        // EDIT POST
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Documento documento,
            IFormFile? archivoSubido)
        {

            if (id != documento.IdDocumento)
                return NotFound();



            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));



            var documentoBD = await _context.Documentos
                .FirstOrDefaultAsync(d => d.IdDocumento == id);



            if (documentoBD == null)
                return NotFound();


            var unidadId = await _context.ContratoAlquilers
        .Where(c => c.IdUsuario == userId &&
                    c.Estado == "activo")
        .Select(c => c.IdUnidad)
        .FirstOrDefaultAsync();



            if (documentoBD.IdUnidad != unidadId)
                return Forbid();




            if (ModelState.IsValid)
            {

                documentoBD.Nombre = documento.Nombre;
                documentoBD.Tipo = documento.Tipo;



                if (archivoSubido != null)
                {

                    if (!string.IsNullOrEmpty(documentoBD.Archivo))
                    {

                        string anterior =
                            Path.Combine(
                                _env.WebRootPath,
                                documentoBD.Archivo.TrimStart('/'));


                        if (System.IO.File.Exists(anterior))
                        {
                            System.IO.File.Delete(anterior);
                        }

                    }




                    string carpeta =
                        Path.Combine(
                            _env.WebRootPath,
                            "documentos");



                    if (!Directory.Exists(carpeta))
                        Directory.CreateDirectory(carpeta);



                    string nombre =
                        Guid.NewGuid()
                        + Path.GetExtension(archivoSubido.FileName);



                    string ruta =
                        Path.Combine(
                            carpeta,
                            nombre);



                    using (var stream =
                        new FileStream(
                            ruta,
                            FileMode.Create))
                    {
                        await archivoSubido.CopyToAsync(stream);
                    }



                    documentoBD.Archivo =
                        "/documentos/" + nombre;

                }



                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));

            }


            return View(documento);

        }

    }
}