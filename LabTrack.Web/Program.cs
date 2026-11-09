using LabTrack.Web.Data;
using LabTrack.Web.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// En MonsterASP.NET la cadena de conexión se configura con la variable de
// entorno ConnectionStrings__LabTrackConnection (o en appsettings.Production.json).
var connectionString = builder.Configuration.GetConnectionString("LabTrackConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    StartupDiagnostics.ConfigurationError = new InvalidOperationException(
        "Falta la cadena de conexión 'LabTrackConnection'. " +
        "Configure la variable de entorno ConnectionStrings__LabTrackConnection " +
        "con la cadena de acceso LOCAL de su base de datos MSSQL de MonsterASP.NET.");

    // Cadena ficticia: permite construir el contenedor de DI y mostrar la
    // página de diagnóstico en lugar de tumbar el proceso con un 500.30.
    connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=LabTrack_NotConfigured;Trusted_Connection=True;";
}

builder.Services.AddDbContext<LabTrackContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// MonsterASP.NET publica detrás de IIS/proxy: respetar el esquema y la IP originales.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Aplicar migraciones y datos iniciales al arrancar (primer despliegue incluido).
// Si falla la base, NO tumbamos el proceso: guardamos el error y mostramos
// /diagnostico para que sea fácil corregir la configuración en MonsterASP.
if (StartupDiagnostics.ConfigurationError is null)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<LabTrackContext>();
        await DbInitializer.InitializeAsync(context, logger);
    }
    catch (Exception ex)
    {
        StartupDiagnostics.DatabaseInitializationError = ex;
        logger.LogError(ex, "No se pudo inicializar la base de datos. Revise la cadena de conexión y los permisos.");
    }
}
else
{
    app.Logger.LogError(StartupDiagnostics.ConfigurationError, "Configuración incompleta de la aplicación.");
}

app.UseForwardedHeaders();

// Mientras haya un error de configuración/base, cualquier ruta muestra una
// página de diagnóstico en lugar del 500.30 genérico de IIS.
app.Use(async (context, next) =>
{
    if (!StartupDiagnostics.IsHealthy &&
        !context.Request.Path.StartsWithSegments("/diagnostico"))
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(
            DiagnosticPage.Build(app.Environment, app.Configuration, StartupDiagnostics.Error));
        return;
    }

    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
