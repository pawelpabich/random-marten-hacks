using Serilog;

namespace MartenPlayground
{
    class ConfigureLogging
    {
        public static void Run()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .CreateLogger();
        }
    }
}