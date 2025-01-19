using Serilog;

namespace HanneBogaerts.GlueBox.DI;

public class AdhesionManager
{
    private readonly Dictionary<Type, List<Type>> _dependencies = new();

    public void AddDependencies(Type implementationType)
    {
        Log.Debug("Adding dependencies for {ImplementationType}", implementationType.Name);
        _dependencies[implementationType] = GetDependencies(implementationType);
    }

    public void AddDependencies<TImplementation>()
    {
        Log.Debug("Adding dependencies for {TImplementation}", typeof(TImplementation).Name);
        _dependencies[typeof(TImplementation)] = GetDependencies(typeof(TImplementation));
    }

    public List<Type> GetDependenciesForService(Type serviceType)
    {
        Log.Debug("Getting dependencies for service of type {ServiceType}", serviceType.Name);
        return _dependencies[serviceType];
    }

    private static List<Type> GetDependencies(Type implementationType)
    {
        var constructors = implementationType.GetConstructors();
        List<Type> dependencies = [];
        foreach (var constructor in constructors)
        {
            dependencies.AddRange(constructor.GetParameters().Select(p => p.ParameterType));
        }

        if (dependencies.Count == 0)
        {
            Log.Debug("No dependencies found for {ImplementationType}", implementationType.Name);
        }
        else
        {
            Log.Debug("Dependencies for {ImplementationType}: {Dependencies}", implementationType.Name,
                string.Join(", ", dependencies.Select(d => d.Name)));
        }

        return dependencies;
    }
}