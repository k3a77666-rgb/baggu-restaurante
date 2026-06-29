using Microsoft.AspNetCore.Mvc;
using BagguWeb.Models;

namespace BagguWeb.Controllers
{
    public class FichasController : Controller
    {
        private static readonly List<Ficha> _cola = new();
        private static int _contador = 1;

        private static readonly string[] _codigosVIP = { "VIP2024A1", "VIP2024B2", "VIP2024D4" };

        public IActionResult Index()
        {
            ViewBag.Cola = _cola.OrderBy(f => f.Posicion).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult GenerarFicha(string nombre, int personas, string codigoVIP)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                TempData["Error"] = "Ingrese su nombre.";
                return RedirectToAction("Index");
            }

            string categoria = "NORMAL";
            if (!string.IsNullOrEmpty(codigoVIP))
            {
                if (_codigosVIP.Contains(codigoVIP.Trim().ToUpper()))
                    categoria = "VIP";
                else
                {
                    TempData["Error"] = "Código VIP inválido.";
                    return RedirectToAction("Index");
                }
            }

            var ficha = new Ficha
            {
                Posicion = _cola.Count + 1,
                Codigo = $"F{_contador:D3}",
                Nombre = nombre,
                Categoria = categoria,
                Personas = personas,
                Mesa = "-",
                HoraRegistro = DateTime.Now
            };
            _cola.Add(ficha);
            _contador++;

            TempData["FichaGenerada"] = $"Tu ficha es: {ficha.Codigo} | Categoría: {categoria} | Posición: {ficha.Posicion}";
            return RedirectToAction("Index");
        }
    }
}
