using BagguWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using static Supabase.Postgrest.Constants;

namespace BagguWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly Supabase.Client _supabase;

        public AdminController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        private bool EsAdmin()
            => HttpContext.Session.GetString("UsuarioRol") == "Administrador";

        private async Task<List<PedidoDelivery>> ObtenerPedidosDelivery()
        {
            var response = await _supabase
                .From<PedidoDelivery>()
                .Where(p => p.Estado != "Entregado")
                .Order(p => p.CreadoEn, Ordering.Ascending)
                .Get();
            return response.Models.ToList();
        }

        public async Task<IActionResult> Index(string tab = "mesas")
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Home");

            var mesasResponse = await _supabase.From<Mesa>().Get();
            var colaResponse = await _supabase.From<Ficha>().Order(f => f.Posicion, Ordering.Ascending).Get();
            var platosResponse = await _supabase.From<Producto>().Get();
            var reservas = await ReservasController.ObtenerTodas();

            var mesas = mesasResponse.Models.ToList();
            var cola = colaResponse.Models.ToList();
            var platos = platosResponse.Models.ToList();
            var mesasDisponibles = mesas.Where(m => m.Estado == "disponible").ToList();

            ViewBag.Tab = tab;
            ViewBag.Mesas = mesas;
            ViewBag.Cola = cola;
            ViewBag.Platos = platos;
            ViewBag.Reservas = reservas;
            ViewBag.MesasDisponibles = mesasDisponibles;
            ViewBag.PedidosDelivery = await ObtenerPedidosDelivery();

            ViewBag.Stats = new
            {
                MesasOcupadas = mesas.Count(m => m.Estado == "ocupada"),
                TotalMesas = mesas.Count,
                ColaTotal = cola.Count,
                ColaVIP = cola.Count(f => f.Categoria == "VIP"),
                ColaImportante = cola.Count(f => f.Categoria == "IMPORTANTE"),
                ColaNormal = cola.Count(f => f.Categoria == "NORMAL"),
            };

            return View();
        }

        // ============================================================
        // TIEMPO DE ESPERA (2 MINUTOS)
        // ============================================================

        private async Task VerificarTiempoEspera()
        {
            var response = await _supabase
                .From<Reserva>()
                .Where(r => r.Estado == "Confirmada" && r.Confirmado == true)
                .Get();

            var reservas = response.Models.ToList();
            var ahora = DateTime.Now;

            foreach (var reserva in reservas)
            {
                if (reserva.FechaConfirmacion.HasValue)
                {
                    var tiempoTranscurrido = (ahora - reserva.FechaConfirmacion.Value).TotalMinutes;

                    if (tiempoTranscurrido > reserva.TiempoEspera)
                    {
                        reserva.Estado = "NoShow";
                        reserva.FechaLiberacion = ahora;
                        await _supabase.From<Reserva>().Update(reserva);

                        if (reserva.MesaAsignada.HasValue)
                        {
                            await _supabase
                                .From<Mesa>()
                                .Where(m => m.Id == reserva.MesaAsignada.Value)
                                .Set(m => m.Estado, "disponible")
                                .Update();
                        }
                    }
                }
            }
        }

        // ============================================================
        // RESERVAS - ACCIONES
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> ConfirmarReserva(long id)
        {
            var response = await _supabase.From<Reserva>().Where(r => r.Id == id).Get();
            var reserva = response.Models.FirstOrDefault();
            if (reserva != null)
            {
                reserva.Estado = "Confirmada";
                reserva.FechaConfirmacion = DateTime.Now;
                reserva.Confirmado = true;
                reserva.TiempoEspera = 2;
                await _supabase.From<Reserva>().Update(reserva);

                if (!reserva.MesaAsignada.HasValue)
                {
                    var mesasDisponibles = await _supabase
                        .From<Mesa>()
                        .Where(m => m.Estado == "disponible" && m.Capacidad >= reserva.Comensales)
                        .Order(m => m.Capacidad, Ordering.Ascending)
                        .Get();

                    var mesa = mesasDisponibles.Models.FirstOrDefault();
                    if (mesa != null)
                    {
                        reserva.MesaAsignada = mesa.Id;
                        await _supabase.From<Reserva>().Update(reserva);

                        await _supabase
                            .From<Mesa>()
                            .Where(m => m.Id == mesa.Id)
                            .Set(m => m.Estado, "ocupada")
                            .Update();
                    }
                }
            }
            return RedirectToAction("Index", new { tab = "reservas" });
        }

        [HttpPost]
        public async Task<IActionResult> AsignarMesa(long id, long mesaId)
        {
            if (mesaId <= 0)
            {
                TempData["Error"] = "Seleccione una mesa válida.";
                return RedirectToAction("Index", new { tab = "reservas" });
            }

            var response = await _supabase.From<Reserva>().Where(r => r.Id == id).Get();
            var reserva = response.Models.FirstOrDefault();
            if (reserva != null)
            {
                reserva.MesaAsignada = mesaId;
                reserva.Estado = "Confirmada";
                await _supabase.From<Reserva>().Update(reserva);
            }

            await _supabase
                .From<Mesa>()
                .Where(m => m.Id == mesaId)
                .Set(m => m.Estado, "ocupada")
                .Update();

            TempData["Exito"] = $"✅ Mesa {mesaId} asignada correctamente.";
            return RedirectToAction("Index", new { tab = "reservas" });
        }

        [HttpPost]
        public async Task<IActionResult> MarcarLlegada(long id)
        {
            var response = await _supabase.From<Reserva>().Where(r => r.Id == id).Get();
            var reserva = response.Models.FirstOrDefault();
            if (reserva != null)
            {
                reserva.Estado = "Llegado";
                reserva.FechaLlegada = DateTime.Now;
                await _supabase.From<Reserva>().Update(reserva);
            }
            return RedirectToAction("Index", new { tab = "reservas" });
        }

        [HttpPost]
        public async Task<IActionResult> CancelarReserva(long id)
        {
            var response = await _supabase.From<Reserva>().Where(r => r.Id == id).Get();
            var reserva = response.Models.FirstOrDefault();
            if (reserva != null)
            {
                reserva.Estado = "Cancelada";
                await _supabase.From<Reserva>().Update(reserva);

                if (reserva.MesaAsignada.HasValue)
                {
                    await _supabase
                        .From<Mesa>()
                        .Where(m => m.Id == reserva.MesaAsignada.Value)
                        .Set(m => m.Estado, "disponible")
                        .Update();
                }
            }
            return RedirectToAction("Index", new { tab = "reservas" });
        }

        [HttpPost]
        public async Task<IActionResult> CrearReservaTelefonica(string nombre, string telefono, int comensales, DateTime fecha, string hora, string notas)
        {
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(telefono) || comensales <= 0)
            {
                TempData["Error"] = "Complete todos los campos obligatorios.";
                return RedirectToAction("Index", new { tab = "reservas" });
            }

            var reserva = new Reserva
            {
                Nombre = nombre,
                Telefono = telefono,
                Comensales = comensales,
                Fecha = fecha,
                Hora = hora,
                Notas = notas ?? "",
                Estado = "Confirmada",
                Origen = "Telefono",
                Confirmado = true,
                CreadoEn = DateTime.Now,
                Turno = string.Compare(hora, "20:00") < 0 ? "Primero" : "Segundo"
            };

            await _supabase.From<Reserva>().Insert(reserva);
            TempData["Exito"] = $"✅ Reserva telefónica creada para {nombre}";
            return RedirectToAction("Index", new { tab = "reservas" });
        }

        public async Task<IActionResult> VerificarEstadoReservas()
        {
            await VerificarTiempoEspera();
            return RedirectToAction("Index", new { tab = "reservas" });
        }

        // ============================================================
        // DELIVERY
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> MarcarEnCamino(long id)
        {
            var response = await _supabase.From<PedidoDelivery>().Where(p => p.Id == id).Get();
            var pedido = response.Models.FirstOrDefault();
            if (pedido != null)
            {
                pedido.Estado = "En Camino";
                await _supabase.From<PedidoDelivery>().Update(pedido);
            }
            return RedirectToAction("Index", new { tab = "cola" });
        }

        [HttpPost]
        public async Task<IActionResult> MarcarEntregado(long id)
        {
            var response = await _supabase.From<PedidoDelivery>().Where(p => p.Id == id).Get();
            var pedido = response.Models.FirstOrDefault();
            if (pedido != null)
            {
                pedido.Estado = "Entregado";
                await _supabase.From<PedidoDelivery>().Update(pedido);
            }
            return RedirectToAction("Index", new { tab = "cola" });
        }
    }
}