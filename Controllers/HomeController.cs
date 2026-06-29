using Microsoft.AspNetCore.Mvc;
using BagguWeb.Models;
using Supabase;

namespace BagguWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly Supabase.Client _supabase;

        // Usuarios hardcodeados (para pruebas sin BD)
        private static readonly List<Usuario> _usuarios = new()
        {
            new Usuario { Id = 1, NombreCompleto = "Administrador", Email = "admin@baggu.com", PasswordHash = "admin123", Rol = "Administrador" },
            new Usuario { Id = 2, NombreCompleto = "Cliente Demo",  Email = "cliente@baggu.com", PasswordHash = "cliente123", Rol = "Cliente" }
        };

        public HomeController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Buscar usuario en Supabase
            var response = await _supabase
                .From<Usuario>()
                .Where(u => u.Email == email && u.PasswordHash == password)
                .Get();

            var usuario = response.Models.FirstOrDefault();

            if (usuario == null)
            {
                TempData["LoginError"] = "Email o contraseña incorrectos.";
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetString("UsuarioNombre", usuario.NombreCompleto);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
            HttpContext.Session.SetString("UsuarioEmail", usuario.Email);

            if (usuario.Rol == "Administrador")
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult Error()
        {
            return View();
        }
        // GET: Página de registro
        public IActionResult Registro()
        {
            return View();
        }

        // POST: Registrar nuevo usuario
        [HttpPost]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Complete todos los campos correctamente.";
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                TempData["Error"] = "Las contraseñas no coinciden.";
                return View(model);
            }

            // Verificar si el email ya existe
            var existing = await _supabase
                .From<Usuario>()
                .Where(u => u.Email == model.Email)
                .Get();

            if (existing.Models.Any())
            {
                TempData["Error"] = "Este email ya está registrado.";
                return View(model);
            }

            // Crear nuevo usuario
            var nuevoUsuario = new Usuario
            {
                NombreCompleto = model.NombreCompleto,
                Email = model.Email,
                PasswordHash = model.Password, // En producción usar BCrypt
                Rol = "Cliente"
            };

            await _supabase.From<Usuario>().Insert(nuevoUsuario);

            TempData["Exito"] = "✅ Registro exitoso. Ahora puedes iniciar sesión.";
            return RedirectToAction("Index");
        }
    }
}