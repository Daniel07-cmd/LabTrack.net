using System.Net;

namespace LabTrack.Web.Infrastructure
{
    /// <summary>
    /// Página HTML de diagnóstico. No muestra la cadena de conexión ni
    /// credenciales, solo si está configurada y el mensaje del error.
    /// </summary>
    public static class DiagnosticPage
    {
        public static string Build(IHostEnvironment environment, IConfiguration configuration, Exception? error)
        {
            bool hasConnectionString =
                !string.IsNullOrWhiteSpace(configuration.GetConnectionString("LabTrackConnection"));

            string statusColor = error is null ? "#198754" : "#dc3545";
            string statusText = error is null ? "OK" : "ERROR";
            string environmentName = WebUtility.HtmlEncode(environment.EnvironmentName);
            string framework = WebUtility.HtmlEncode(
                System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);

            string errorBlock = error is null
                ? "<p class=\"ok\">La base de datos se inicializó correctamente.</p>"
                : $@"<h2>Detalle del error</h2>
                     <p class=""muted"">Tipo: <code>{WebUtility.HtmlEncode(error.GetType().Name)}</code></p>
                     <pre>{WebUtility.HtmlEncode(error.Message)}</pre>";

            string connectionBlock = hasConnectionString
                ? "<span class=\"ok\">Configurada</span>"
                : "<span class=\"bad\">NO configurada</span>";

            string hint = error is null
                ? "<p>El sistema está listo. Puede <a href=\"/\">ir al inicio</a>.</p>"
                : @"
                   <h2>Qué revisar</h2>
                   <ol>
                     <li>Que el sitio de MonsterASP tenga runtime <strong>.NET 10</strong>.</li>
                     <li>Que <code>web.config</code>, <code>LabTrack.Web.dll</code> y <code>appsettings.json</code>
                         estén directamente en <code>wwwroot</code> (no dentro de una subcarpeta).</li>
                     <li>Que exista la variable de entorno
                         <code>ConnectionStrings__LabTrackConnection</code> con la cadena
                         <strong>Local access</strong> de la base MSSQL de MonsterASP.</li>
                     <li>Que la base de datos exista y que el usuario/contraseña sean correctos.</li>
                     <li>Si prefieres no usar variables de entorno, edita
                         <code>appsettings.Production.json</code> en <code>wwwroot</code> y pega la cadena
                         en <code>ConnectionStrings:LabTrackConnection</code>, luego reinicia el sitio.</li>
                   </ol>
                   <p class=""muted"">
                     La cadena debe verse así:
                     <code>Server=dbXXXX.databaseasp.net; Database=dbXXXX; User Id=dbXXXX; Password=...; Encrypt=False; MultipleActiveResultSets=True;</code>
                   </p>";

            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>LabTrack.Web - Diagnóstico</title>
    <style>
        body {{ font-family: system-ui, -apple-system, Segoe UI, Roboto, sans-serif; background: #f8fafc; color: #0f172a; margin: 0; padding: 2rem; }}
        .card {{ max-width: 860px; margin: 0 auto; background: #fff; border: 1px solid #e2e8f0; border-radius: 16px; padding: 2rem; box-shadow: 0 10px 30px rgba(15,23,42,.08); }}
        h1 {{ margin-top: 0; font-size: 1.6rem; }}
        h2 {{ font-size: 1.05rem; margin-top: 1.6rem; }}
        .badge {{ display: inline-block; padding: .25rem .7rem; border-radius: 999px; color: #fff; background: {statusColor}; font-weight: 700; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 1rem; }}
        th, td {{ text-align: left; padding: .6rem .4rem; border-bottom: 1px solid #e2e8f0; vertical-align: top; }}
        th {{ width: 34%; color: #475569; font-weight: 600; }}
        code {{ background: #f1f5f9; padding: .1rem .35rem; border-radius: 6px; }}
        pre {{ background: #0f172a; color: #e2e8f0; padding: 1rem; border-radius: 10px; overflow-x: auto; white-space: pre-wrap; }}
        .ok {{ color: #15803d; font-weight: 600; }}
        .bad {{ color: #b91c1c; font-weight: 600; }}
        .muted {{ color: #64748b; }}
        ol li {{ margin-bottom: .5rem; }}
        a {{ color: #2563eb; }}
    </style>
</head>
<body>
    <div class=""card"">
        <h1>LabTrack.Web <span class=""badge"">{statusText}</span></h1>
        <p class=""muted"">Página de diagnóstico generada por la aplicación.</p>

        <table>
            <tr><th>Entorno</th><td>{environmentName}</td></tr>
            <tr><th>Framework</th><td>{framework}</td></tr>
            <tr><th>Cadena de conexión</th><td>{connectionBlock}</td></tr>
        </table>

        {errorBlock}
        {hint}
    </div>
</body>
</html>";
        }
    }
}
