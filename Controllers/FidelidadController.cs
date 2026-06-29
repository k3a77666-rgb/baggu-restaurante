using BagguWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using static Supabase.Postgrest.Constants;

namespace BagguWeb.Controllers
{
    public class FidelidadController : Controller
    {
        private readonly Supabase.Client _supabase;

        public FidelidadController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        private bool EsAdmin()
            => HttpContext.Session.GetString("UsuarioRol") == "Administrador";

        public async Task<IActionResult> Index()
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Home");

            var response = await _supabase
                .From<ClienteFidelidad>()
                .Order(c => c.Puntos, Ordering.Descending)
                .Get();

            ViewBag.Clientes = response.Models.ToList();

            // Estadísticas
            var stats = new
            {
                TotalClientes = response.Models.Count,
                Bronce = response.Models.Count(c => c.Nivel == "Bronce"),
                Plata = response.Models.Count(c => c.Nivel == "Plata"),
                Oro = response.Models.Count(c => c.Nivel == "Oro"),
                PuntosTotales = response.Models.Sum(c => c.Puntos)
            };
            ViewBag.Stats = stats;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarCliente(string nombre, string email, string telefono, string notas)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                TempData["Error"] = "El nombre es obligatorio.";
                return RedirectToAction("Index");
            }

            // Verificar si ya existe
            var existing = await _supabase
                .From<ClienteFidelidad>()
                .Where(c => c.Email == email || c.Telefono == telefono)
                .Get();

            if (existing.Models.Any())
            {
                TempData["Error"] = "Este cliente ya está registrado en el sistema de fidelidad.";
                return RedirectToAction("Index");
            }

            var cliente = new ClienteFidelidad
            {
                Nombre = nombre,
                Email = email ?? "",
                Telefono = telefono ?? "",
                Notas = notas ?? "",
                Nivel = "Bronce",
                Puntos = 0,
                Visitas = 0,
                TotalGastado = 0,
                FechaRegistro = DateTime.Now,
                UltimaVisita = DateTime.Now
            };

            await _supabase.From<ClienteFidelidad>().Insert(cliente);

            TempData["Exito"] = $"✅ Cliente {nombre} registrado en el sistema de fidelidad.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SumarVisita(long id, decimal gasto)
        {
            var response = await _supabase.From<ClienteFidelidad>().Where(c => c.Id == id).Get();
            var cliente = response.Models.FirstOrDefault();

            if (cliente != null)
            {
                cliente.Visitas += 1;
                cliente.TotalGastado += gasto;
                cliente.UltimaVisita = DateTime.Now;

                // Sumar puntos: 1 punto por cada Bs. 10 gastados
                int puntosGanados = (int)(gasto / 10);
                cliente.Puntos += puntosGanados;

                // Actualizar nivel automáticamente
                if (cliente.Puntos >= 100 && cliente.Puntos < 300)
                    cliente.Nivel = "Plata";
                else if (cliente.Puntos >= 300)
                    cliente.Nivel = "Oro";
                else
                    cliente.Nivel = "Bronce";

                await _supabase.From<ClienteFidelidad>().Update(cliente);
                TempData["Exito"] = $"✅ {cliente.Nombre} ahora tiene {cliente.Puntos} puntos (Nivel: {cliente.Nivel}).";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AgregarNota(long id, string nota)
        {
            var response = await _supabase.From<ClienteFidelidad>().Where(c => c.Id == id).Get();
            var cliente = response.Models.FirstOrDefault();

            if (cliente != null)
            {
                cliente.Notas = nota;
                await _supabase.From<ClienteFidelidad>().Update(cliente);
                TempData["Exito"] = "✅ Nota actualizada.";
            }

            return RedirectToAction("Index");
        }
    }
}