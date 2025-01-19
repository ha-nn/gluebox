using System.Reflection;
using HanneBogaerts.GlueBox.DI;
using HanneBogaerts.GlueBox.Logging;
using Serilog;

namespace HanneBogaerts.GlueBox;

public class GlueBox
{
    private readonly AdhesionRegistry _adhesionRegistry = new();
    private readonly AdhesionManager _adhesionManager = new();
    private readonly AdhesionResolver _adhesionResolver;

    public GlueBox()
    {
        LoggingManager.InitializeLogger();
        _adhesionResolver = new AdhesionResolver(_adhesionRegistry, _adhesionManager);
    }

    public void StickSingleton<TImplementation>()
    {
        _adhesionRegistry.StickSingleton<TImplementation>();
        _adhesionManager.AddDependencies<TImplementation>();
    }

    public void StickSingleton<TService, TImplementation>()
    {
        _adhesionRegistry.StickSingleton<TService, TImplementation>();
        _adhesionManager.AddDependencies<TImplementation>();
    }

    public void StickTransient<TImplementation>()
    {
        _adhesionRegistry.StickTransient<TImplementation>();
        _adhesionManager.AddDependencies<TImplementation>();
    }

    public void StickTransient<TService, TImplementation>()
    {
        _adhesionRegistry.StickTransient<TService, TImplementation>();
        _adhesionManager.AddDependencies<TImplementation>();
    }

    public TService Resolve<TService>()
    {
        return _adhesionResolver.Resolve<TService>();
    }

    public void CompleteAdhesion()
    {
        Log.Debug("Completing adhesion");
        var assembly = Assembly.GetCallingAssembly();
        var controllerTypes = assembly.GetTypes()
            .Where(type => typeof(IGlueController).IsAssignableFrom(type) && !type.IsAbstract);

        foreach (var type in controllerTypes)
        {
            Log.Debug("Sticking singleton of type {Type}", type.Name);
            _adhesionRegistry.StickSingleton(type);
            _adhesionManager.AddDependencies(type);
        }
    }

    public Dictionary<string, Action> GetCommandAdhesionMap()
    {
        return _adhesionResolver.CommandAdhesionMap;
    }

    public static void SetLogLevel(string level)
    {
        LoggingManager.SetLogLevel(level);
    }

    public static void ChangeLogOutput(string method)
    {
        LoggingManager.ChangeLogOutput(method);
    }

    public static void ChangeLogOutput(string method, string path)
    {
        LoggingManager.ChangeLogOutput(method, path);
    }
}