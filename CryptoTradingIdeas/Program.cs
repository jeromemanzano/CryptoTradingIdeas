using Avalonia;
using System;
using System.IO;
using ReactiveUI.Avalonia.Splat;
using CryptoTradingIdeas.Core.Injection; // Reference used for source generated RegisterApplicationServices
using Serilog;

namespace CryptoTradingIdeas;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Get the application executable directory
            var executablePath = AppDomain.CurrentDomain.BaseDirectory;
            
            // Create logs directory if it doesn't exist
#if DEBUG
            var logsPath = Path.Combine(executablePath, "debug-logs");
#else
            var logsPath = Path.Combine(executablePath, "logs");
#endif
            Directory.CreateDirectory(logsPath);

            // Configure Serilog with absolute path
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(
                    Path.Combine(logsPath, "log-.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Application starting up");
            
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            // If logging fails, write to console as fallback
            Console.WriteLine($"Failed to initialize application: {ex}");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUIWithMicrosoftDependencyResolver(services =>
            {
                // Register services
                services.RegisterApplicationServices();
            })
            .WithInterFont()
            .LogToTrace();
}