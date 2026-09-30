using PROTOTIPO.Models;
using PROTOTIPO.Models.ViewModel;
using PROTOTIPO.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PROTOTIPO.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAvisosAutomaticosService _avisosAutomaticosService;


        public AccountController(
            AppDbContext context,
            IAvisosAutomaticosService avisosAutomaticosService)
        {
            _context = context;
            _avisosAutomaticosService = avisosAutomaticosService;
        }



        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {

            var user = _context.Usuarios
                .FirstOrDefault(x =>
                    x.Email == vm.Email &&
                    x.Password == vm.Password);



            if (user == null)
            {
                ViewBag.Error = "Credenciales incorrectas";
                return View("Index");
            }



            // =========================
            // CONVERTIR ROL
            // =========================

            string rol = user.IdRol switch
            {
                1 => "Admin",
                2 => "Inquilino",
                _ => "Usuario"
            };



            // =========================
            // CREAR CLAIMS
            // =========================

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.Name,
                    user.Nombre
                ),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.IdUsuario.ToString()
                ),

                new Claim(
                    ClaimTypes.Role,
                    rol
                ),

                new Claim(
                    "UserId",
                    user.IdUsuario.ToString()
                )
            };



            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );



            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );



            // =========================
            // GENERAR AVISOS AUTOMÁTICOS
            // =========================

            await _avisosAutomaticosService.GenerarAvisos(user.IdUsuario);



            // =========================
            // REDIRECCIÓN POR ROL
            // =========================

            if (rol == "Admin")
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin"
                );
            }



            return RedirectToAction(
                "Index",
                "Inquilino"
            );

        }





        [HttpPost]
        public async Task<IActionResult> Logout()
        {

            await HttpContext.SignOutAsync();

            return RedirectToAction("Login");

        }





        [HttpGet]
        public IActionResult Login()
        {

            return View("Index");

        }

    }
}