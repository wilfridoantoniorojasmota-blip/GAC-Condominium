using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROTOTIPO.Models;

namespace PROTOTIPO.Controllers
{
    public class DocumentosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;


        public DocumentosController(
            AppDbContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }



        // GET: Documentos
        // GET: Documentos
        
 
public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 8; // Cantidad de documentos por página

            // ============================================
            // 1. ESTADÍSTICAS GLOBALES
            // ============================================

            var totalDocumentos = await _context.Documentos.CountAsync();

            var documentosDeUnidades = await _context.Documentos
                .CountAsync(d => d.IdUnidad != null);

            var documentosGenerales = await _context.Documentos
                .CountAsync(d => d.IdUnidad == null);


            // ============================================
            // 2. PAGINACIÓN
            // ============================================

            var totalPages = (int)Math.Ceiling(
                totalDocumentos / (double)pageSize
            );

            if (page < 1)
                page = 1;

            if (page > totalPages && totalPages > 0)
                page = totalPages;


            // ============================================
            // 3. CONSULTA PAGINADA
            // ============================================

            var documentos = await _context.Documentos
                .Include(d => d.IdCondominioNavigation)
                .Include(d => d.IdUnidadNavigation)
                .Include(d => d.IdContratoNavigation)
                .OrderByDescending(d => d.FechaSubida)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // ============================================
            // 4. DATOS PARA LA VISTA
            // ============================================

            ViewBag.TotalDocumentos = totalDocumentos;
            ViewBag.DocumentosDeUnidades = documentosDeUnidades;
            ViewBag.DocumentosGenerales = documentosGenerales;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;


            return View(documentos);
        }
 
 




        // GET: Documentos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();


            var documento = await _context.Documentos
                .Include(d => d.IdCondominioNavigation)
                .Include(d => d.IdUnidadNavigation)
                .FirstOrDefaultAsync(d =>
                    d.IdDocumento == id);


            if (documento == null)
                return NotFound();


            return View(documento);
        }







        // GET: Documentos/Create
        public IActionResult Create()
        {
            CargarCombos();

            return View();
        }







        // POST: Documentos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Documento documento,
            IFormFile archivoSubido)
        {


            if (archivoSubido == null)
            {
                ModelState.AddModelError(
                    "",
                    "Debe seleccionar un archivo.");
            }
            else
            {
                ValidarArchivo(archivoSubido);
            }



            if (ModelState.IsValid)
            {

                documento.Archivo =
                    await GuardarArchivo(archivoSubido);


                documento.FechaSubida =
                    DateTime.Now;



                _context.Documentos.Add(documento);

                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));
            }



            CargarCombos(documento);

            return View(documento);
        }








        // GET: Documentos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();


            var documento =
                await _context.Documentos.FindAsync(id);



            if (documento == null)
                return NotFound();



            CargarCombos(documento);


            return View(documento);
        }








        // POST: Documentos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Documento documento,
            IFormFile? archivoSubido)
        {

            if (id != documento.IdDocumento)
                return NotFound();



            var documentoBD =
                await _context.Documentos
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.IdDocumento == id);



            if (documentoBD == null)
                return NotFound();




            if (archivoSubido != null)
            {
                ValidarArchivo(archivoSubido);
            }




            if (ModelState.IsValid)
            {


                if (archivoSubido != null)
                {

                    EliminarArchivo(documentoBD.Archivo);


                    documento.Archivo =
                        await GuardarArchivo(archivoSubido);

                }
                else
                {
                    documento.Archivo =
                        documentoBD.Archivo;
                }



                documento.FechaSubida =
                    documentoBD.FechaSubida;



                _context.Documentos.Update(documento);


                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));

            }



            CargarCombos(documento);

            return View(documento);
        }









        // Descargar documento
        public async Task<IActionResult> Descargar(int id)
        {

            var documento =
                await _context.Documentos
                .FirstOrDefaultAsync(d =>
                    d.IdDocumento == id);



            if (documento == null ||
                string.IsNullOrEmpty(documento.Archivo))
            {
                return NotFound();
            }



            var ruta =
                Path.Combine(
                    _env.WebRootPath,
                    documento.Archivo.TrimStart('/')
                );



            if (!System.IO.File.Exists(ruta))
                return NotFound();



            var archivo =
                await System.IO.File.ReadAllBytesAsync(ruta);



            return File(
                archivo,
                "application/octet-stream",
                documento.Nombre
            );
        }









        // GET Delete
        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
                return NotFound();



            var documento =
                await _context.Documentos
                .Include(d => d.IdUnidadNavigation)
                .Include(d => d.IdCondominioNavigation)
                .FirstOrDefaultAsync(d =>
                    d.IdDocumento == id);



            if (documento == null)
                return NotFound();



            return View(documento);
        }








        // POST Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var documento =
                await _context.Documentos
                .FindAsync(id);



            if (documento != null)
            {

                EliminarArchivo(documento.Archivo);


                _context.Documentos.Remove(documento);


                await _context.SaveChangesAsync();
            }



            return RedirectToAction(nameof(Index));
        }









        private void CargarCombos(
            Documento? documento = null)
        {

            ViewData["IdCondominio"] =
                new SelectList(
                    _context.Condominios,
                    "IdCondominio",
                    "Nombre",
                    documento?.IdCondominio);



            ViewData["IdUnidad"] =
                new SelectList(
                    _context.UnidadHabitacionals,
                    "IdUnidad",
                    "Numero",
                    documento?.IdUnidad);

        }









        private void ValidarArchivo(IFormFile archivo)
        {

            var extensiones =
                new[]
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
                    archivo.FileName)
                    .ToLower();



            if (!extensiones.Contains(extension))
            {
                ModelState.AddModelError(
                    "",
                    "Formato de archivo no permitido.");
            }


            if (archivo.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "",
                    "El archivo no puede superar los 10MB.");
            }

        }








        private async Task<string> GuardarArchivo(
            IFormFile archivo)
        {

            string carpeta =
                Path.Combine(
                    _env.WebRootPath,
                    "documentos");


            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);



            string nombre =
                Guid.NewGuid()
                + Path.GetExtension(
                    archivo.FileName);



            string ruta =
                Path.Combine(
                    carpeta,
                    nombre);



            using (var stream =
                new FileStream(
                    ruta,
                    FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }



            return "/documentos/" + nombre;

        }








        private void EliminarArchivo(string? archivo)
        {

            if (string.IsNullOrEmpty(archivo))
                return;



            string ruta =
                Path.Combine(
                    _env.WebRootPath,
                    archivo.TrimStart('/')
                );



            if (System.IO.File.Exists(ruta))
            {
                System.IO.File.Delete(ruta);
            }

        }

        // ===============================
        // CREAR CONTRATO (GET)
        // ===============================
        public IActionResult CreateContrato()
        {
            ViewBag.Inquilinos = _context.Usuarios
                .Where(u => u.IdRolNavigation.Nombre.ToLower() == "inquilino" &&
                           !_context.ContratoAlquilers.Any(c =>
                    c.IdUsuario == u.IdUsuario))
                .OrderBy(u => u.Nombre)
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = u.Nombre + " " + u.Apellido
                })
                .ToList();

            return View();
        }

        // ===============================
        // CREAR CONTRATO (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateContrato(
            ContratoAlquiler contrato,
            IFormFile archivoSubido)
        {

            if (archivoSubido == null)
            {
                ModelState.AddModelError("", "Debe subir el contrato PDF.");
            }


            if (!ModelState.IsValid)
            {
                ViewBag.Inquilinos = new SelectList(
                    _context.Usuarios
                        .Where(u => u.IdRol == 2)
                        .Select(u => new
                        {
                            u.IdUsuario,
                            NombreCompleto = u.Nombre + " " + u.Apellido
                        }),
                    "IdUsuario",
                    "NombreCompleto",
                    contrato.IdUsuario
                );

                return View(contrato);
            }


            contrato.Estado = "Activo";


            _context.ContratoAlquilers.Add(contrato);

            await _context.SaveChangesAsync();



            var rutaArchivo = await GuardarArchivo(archivoSubido);



            var documento = new Documento
            {
                Nombre = "Contrato de alquiler",
                Tipo = "Contrato de alquiler",
                Archivo = rutaArchivo,
                FechaSubida = DateTime.Now,
                IdContrato = contrato.IdContrato,
                IdUnidad = null,
                IdCondominio = null
            };


            _context.Documentos.Add(documento);

            await _context.SaveChangesAsync();



            TempData["Success"] =
                "Contrato creado correctamente.";


            return RedirectToAction(nameof(Index));
        }

    }
}