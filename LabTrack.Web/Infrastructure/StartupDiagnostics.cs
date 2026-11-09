namespace LabTrack.Web.Infrastructure
{
    /// <summary>
    /// Guarda el estado del arranque para poder mostrar una página de
    /// diagnóstico en lugar de un 500.30 genérico de IIS.
    /// </summary>
    public static class StartupDiagnostics
    {
        public static Exception? ConfigurationError { get; set; }
        public static Exception? DatabaseInitializationError { get; set; }

        public static Exception? Error => ConfigurationError ?? DatabaseInitializationError;

        public static bool IsHealthy => Error is null;
    }
}
