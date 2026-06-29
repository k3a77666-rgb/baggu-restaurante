using BagguWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using static Supabase.Postgrest.Constants;

namespace BagguWeb.Controllers
{
    public class ReservasController : Controller
    {
        private static Supabase.Client _supabase;

        public ReservasController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Reserva reserva)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(reserva.Nombre) || string.IsNullOrEmpty(reserva.Email))
            {
                TempData["Error"] = "Complete todos los campos obligatorios.";
                return RedirectToAction("Index");
            }

            reserva.Estado = "Pendiente";
            reserva.CreadoEn = DateTime.Now;

            var response = await _supabase
                .From<Reserva>()
                .Insert(reserva);

            if (response.Models.Any())
            {
                TempData["Exito"] = $"✅ Reserva confirmada para {reserva.Nombre} el {reserva.Fecha:dd/MM/yyyy} a las {reserva.Hora}.";
            }
            else
            {
                TempData["Error"] = "❌ Error al guardar la reserva.";
            }

            return RedirectToAction("Index");
        }

        // ✅ MÉTODO ESTÁTICO PARA QUE ADMIN PUEDA ACCEDER
        public static async Task<List<Reserva>> ObtenerTodas()
        {
            if (_supabase == null) return new List<Reserva>();

            // ✅ CORREGIDO: Order con dirección
            var response = await _supabase
                .From<Reserva>()
                .Order(r => r.Fecha, Ordering.Ascending)
                .Get();

            return response.Models.ToList();
        }
    }
}