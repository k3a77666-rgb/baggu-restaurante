using Microsoft.AspNetCore.Mvc;
using BagguWeb.Models;
using Supabase;
using System.Text.Json;

namespace BagguWeb.Controllers
{
    public class DeliveryController : Controller
    {
        private readonly Supabase.Client _supabase;

        private static readonly List<Producto> _productos = new()
        {
            new Producto { Id=1,  Nombre="Alitas para probar (6)",       Descripcion="6 piezas + papas",          Precio=37, PrecioOriginal=45, Categoria="Alitas",       Icono="🍗" },
            new Producto { Id=2,  Nombre="2 económicos (pierna y muslo)", Descripcion="2 piezas + papas",          Precio=54, PrecioOriginal=63, Categoria="Pollos",       Icono="🍗" },
            new Producto { Id=3,  Nombre="1/4 de pollo broasted",         Descripcion="Con papas incluidas",       Precio=34, PrecioOriginal=36, Categoria="Pollos",       Icono="🍗" },
            new Producto { Id=4,  Nombre="Hamburguesa Clásica",           Descripcion="Carne, queso, lechuga",     Precio=25, PrecioOriginal=30, Categoria="Hamburguesas", Icono="🍔" },
            new Producto { Id=5,  Nombre="Nuggets (12 piezas)",           Descripcion="Con salsa incluida",        Precio=28, PrecioOriginal=35, Categoria="Nuggets",      Icono="🍟" },
            new Producto { Id=6,  Nombre="Papas Supremas",                Descripcion="Con queso y tocino",        Precio=18, PrecioOriginal=22, Categoria="Papas",        Icono="🍟" },
            new Producto { Id=7,  Nombre="Refresco Personal",             Descripcion="350ml",                     Precio=8,  PrecioOriginal=10, Categoria="Bebidas",      Icono="🥤" },
            new Producto { Id=8,  Nombre="Postre Helado",                 Descripcion="Con chocolate",             Precio=12, PrecioOriginal=15, Categoria="Descuentos",   Icono="🍨" },
            new Producto { Id=9,  Nombre="Pollo Entero",                  Descripcion="Con papas y ensalada",      Precio=85, PrecioOriginal=95, Categoria="Pollos",       Icono="🍗" },
            new Producto { Id=10, Nombre="Alitas Picantes",               Descripcion="12 piezas + salsa",         Precio=42, PrecioOriginal=50, Categoria="Alitas",       Icono="🌶️" },
        };

        public DeliveryController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public IActionResult Index(string categoria = "Menu")
        {
            ViewBag.Categoria = categoria;
            ViewBag.Categorias = new[] { "Menu", "Descuentos", "Recomendados", "Pollos", "Alitas", "Nuggets", "Hamburguesas", "Papas", "Bebidas" };
            var productos = categoria == "Menu"
                ? _productos
                : _productos.Where(p => p.Categoria == categoria).ToList();
            return View(productos);
        }

        // ✅ GUARDAR PEDIDO EN SUPABASE
        [HttpPost]
        public async Task<IActionResult> GuardarPedido([FromBody] PedidoDeliveryRequest request)
        {
            try
            {
                // Validar datos
                if (string.IsNullOrEmpty(request.ClienteNombre) || string.IsNullOrEmpty(request.Direccion))
                {
                    return Json(new { success = false, message = "Complete todos los campos obligatorios." });
                }

                // Guardar en Supabase
                var pedido = new PedidoDelivery
                {
                    ClienteNombre = request.ClienteNombre,
                    ClienteEmail = request.ClienteEmail ?? "",
                    ClienteTelefono = request.ClienteTelefono ?? "",
                    Direccion = request.Direccion,
                    Total = request.Total,
                    Items = JsonSerializer.Serialize(request.Items),
                    Estado = "Pendiente",
                    CreadoEn = DateTime.Now
                };

                await _supabase.From<PedidoDelivery>().Insert(pedido);

                return Json(new { success = true, message = "✅ Pedido confirmado. Tiempo estimado: 30-45 min." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ Error: {ex.Message}" });
            }
        }
    }

    // Clase para recibir los datos del pedido
    public class PedidoDeliveryRequest
    {
        public string ClienteNombre { get; set; } = "";
        public string ClienteEmail { get; set; } = "";
        public string ClienteTelefono { get; set; } = "";
        public string Direccion { get; set; } = "";
        public decimal Total { get; set; }
        public List<ItemPedido> Items { get; set; } = new();
    }
}