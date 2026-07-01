using Supabase;
using Supabase.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. SERVICIOS MVC
// ============================================================
builder.Services.AddControllersWithViews();
// ✅ SOLO FORZAR EL PUERTO CUANDO ESTAMOS EN RENDER (variable PORT existe)
// En local (Visual Studio) usará el puerto de launchSettings.json normalmente
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
// ============================================================
// 2. SESIONES (para login)
// ============================================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ============================================================
// 3. SUPABASE CLIENT CON VALIDACIÓN
// ============================================================
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:AnonKey"];

// ⚠️ VALIDACIÓN: Si falta algo, lanza un error claro
if (string.IsNullOrEmpty(supabaseUrl))
    throw new Exception("❌ FALTA Supabase:Url en appsettings.json");
if (string.IsNullOrEmpty(supabaseKey))
    throw new Exception("❌ FALTA Supabase:AnonKey en appsettings.json");

Console.WriteLine($"✅ Supabase URL: {supabaseUrl}");
Console.WriteLine($"✅ Supabase Key: {supabaseKey.Substring(0, 10)}...");

builder.Services.AddSingleton<Supabase.Client>(provider =>
{
    var options = new SupabaseOptions
    {
        AutoConnectRealtime = true
    };
    return new Supabase.Client(supabaseUrl, supabaseKey, options);
});

// ============================================================
// 4. APLICACIÓN
// ============================================================
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();