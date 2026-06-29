using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace BagguWeb.Models
{
    // ============================================================
    // 1. USUARIO
    // ============================================================
    [Table("usuarios")]
    public class Usuario : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("nombre_completo")]
        public string NombreCompleto { get; set; } = "";

        [Column("email")]
        public string Email { get; set; } = "";

        [Column("password_hash")]
        public string PasswordHash { get; set; } = "";

        [Column("rol")]
        public string Rol { get; set; } = "Cliente";

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; }
    }

    // ============================================================
    // 2. RESERVA
    // ============================================================
    [Table("reservas")]
    public class Reserva : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = "";

        [Column("email")]
        public string Email { get; set; } = "";

        [Column("telefono")]
        public string Telefono { get; set; } = "";

        [Column("comensales")]
        public int Comensales { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("hora")]
        public string Hora { get; set; } = "";

        [Column("notas")]
        public string Notas { get; set; } = "";

        [Column("estado")]
        public string Estado { get; set; } = "Pendiente";

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; }

        // ✅ NUEVOS CAMPOS (SOLO UNA VEZ CADA UNO)
        [Column("mesa_asignada")]
        public long? MesaAsignada { get; set; }

        [Column("turno")]
        public string Turno { get; set; } = "Primero";

        [Column("duracion_estimada")]
        public int DuracionEstimada { get; set; } = 120;

        [Column("fecha_confirmacion")]
        public DateTime? FechaConfirmacion { get; set; }

        [Column("fecha_llegada")]
        public DateTime? FechaLlegada { get; set; }

        [Column("fecha_liberacion")]
        public DateTime? FechaLiberacion { get; set; }

        [Column("origen")]
        public string Origen { get; set; } = "Web";

        [Column("confirmado")]
        public bool Confirmado { get; set; } = false;

        [Column("tiempo_espera")]
        public int TiempoEspera { get; set; } = 2; // ⏰ 2 MINUTOS
    }

    // ============================================================
    // 3. PRODUCTO
    // ============================================================
    [Table("productos")]
    public class Producto : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = "";

        [Column("descripcion")]
        public string Descripcion { get; set; } = "";

        [Column("precio")]
        public decimal Precio { get; set; }

        // ✅ CAMBIADO a decimal? (nullable) para aceptar NULL
        [Column("precio_original")]
        public decimal? PrecioOriginal { get; set; }  // ← AHORA ACEPTA NULL

        [Column("categoria")]
        public string Categoria { get; set; } = "";

        [Column("stock")]
        public int Stock { get; set; }

        [Column("icono")]
        public string Icono { get; set; } = "🍽️";

        [Column("tiempo_preparacion")]
        public int TiempoPreparacion { get; set; }

        [Column("region")]
        public string Region { get; set; } = "";
    }

    // ============================================================
    // 4. MESA
    // ============================================================
    [Table("mesas")]
    public class Mesa : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("capacidad")]
        public int Capacidad { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "disponible";

        [Column("cliente_ficha")]
        public long? ClienteFicha { get; set; }

        [Column("categoria")]
        public string Categoria { get; set; } = "-";

        // ✅ IGNORADO por Supabase y JsonSerializer
        [JsonIgnore]
        public string Cliente => ClienteFicha.HasValue ? $"F{ClienteFicha.Value:D3}" : "-";
    }
    // ============================================================
    // 5. FICHA
    // ============================================================
    [Table("fichas")]
    public class Ficha : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = "";

        [Column("nombre")]
        public string Nombre { get; set; } = "";

        [Column("categoria")]
        public string Categoria { get; set; } = "NORMAL";

        [Column("personas")]
        public int Personas { get; set; }

        [Column("mesa")]
        public string Mesa { get; set; } = "";

        [Column("posicion")]
        public int Posicion { get; set; }

        [Column("hora_registro")]
        public DateTime HoraRegistro { get; set; }
    }

    // ============================================================
    // 6. CLIENTE FIDELIDAD (Sistema de puntos)
    // ============================================================
    [Table("clientes_fidelidad")]
    public class ClienteFidelidad : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = "";

        [Column("email")]
        public string Email { get; set; } = "";

        [Column("telefono")]
        public string Telefono { get; set; } = "";

        [Column("puntos")]
        public int Puntos { get; set; }

        [Column("nivel")]
        public string Nivel { get; set; } = "Bronce"; // Bronce, Plata, Oro

        [Column("visitas")]
        public int Visitas { get; set; }

        [Column("total_gastado")]
        public decimal TotalGastado { get; set; }

        [Column("ultima_visita")]
        public DateTime UltimaVisita { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; }

        [Column("notas")]
        public string Notas { get; set; } = ""; // Alergias, preferencias, etc.
    }

    // ============================================================
    // 7. ITEM PEDIDO (para el carrito de delivery)
    // ============================================================
    public class ItemPedido
    {
        public long ProductoId { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal => Precio * Cantidad;
    }
    // ============================================================
    // 8. REGISTRO VIEW MODEL
    // ============================================================
    public class RegistroViewModel
    {
        public string NombreCompleto { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }
    // ============================================================
    // 9. PEDIDO DELIVERY
    // ============================================================
    [Table("pedidos_delivery")]
    public class PedidoDelivery : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("cliente_nombre")]
        public string ClienteNombre { get; set; } = "";

        [Column("cliente_email")]
        public string ClienteEmail { get; set; } = "";

        [Column("cliente_telefono")]
        public string ClienteTelefono { get; set; } = "";

        [Column("direccion")]
        public string Direccion { get; set; } = "";

        [Column("total")]
        public decimal Total { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "Pendiente";

        [Column("items")]
        public string Items { get; set; } = ""; // JSON con los productos

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; }
    }
}