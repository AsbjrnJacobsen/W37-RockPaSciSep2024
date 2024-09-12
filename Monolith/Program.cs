// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Monolith;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;

public class Program
{
    public static readonly string ServiceName = "RockPaperScissor";
    public static TracerProvider TracerProvider;
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);
    
    public static ILogger Logger1 => Serilog.Log.Logger;
    
    public static void Main()
    {
        TracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddConsoleExporter()
            .AddZipkinExporter()
            .AddSource(ActivitySource.Name)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName))
            .SetSampler(new AlwaysOnSampler())
            
            .Build();
        
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.Seq("http://localhost:5341")
            .Enrich.WithSpan()
            
            .CreateLogger();
        
        var game = new Game();
        
        using var tracerActivity = Program.ActivitySource.StartActivity("Main Tracer.");
        
        for (int i = 1; i < 1001; i++)
        {   
            using var gameStartActivitytest = Program.ActivitySource.StartActivity("Game trace service.");
            
            using var gameStartAct = Program.ActivitySource.StartActivity("Starting game nr. " + i);
            
            Logger1.Verbose("Game Nr. {0}!", i);
            game.Start();
        }
        
        Console.WriteLine("Finished");
        Log.CloseAndFlush();
        TracerProvider.ForceFlush();

    }
}