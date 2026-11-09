using LabTrack.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace LabTrack.Web.Controllers
{
    public class DiagnosticoController : Controller
    {
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public DiagnosticoController(IHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            string html = DiagnosticPage.Build(
                _environment,
                _configuration,
                StartupDiagnostics.Error);

            return Content(html, "text/html; charset=utf-8");
        }
    }
}
