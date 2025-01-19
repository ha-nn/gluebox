using HanneBogaerts.GlueBox.Exceptions;
using Serilog;

namespace HanneBogaerts.GlueBox.DI;

public class AdhesionRegistry
{
    private readonly Dictionary<Type, GlueBinding> _registrations = new();

    public Dictionary<Type, GlueBinding> GetRegistrations()
    {
        Log.Debug("Getting all registrations");
        return _registrations;
    }

    public void StickSingleton(Type type)
    {
        Log.Debug("Sticking singleton of type {Type}", type.Name);
        if (_registrations.ContainsKey(type))
        {
            Log.Error("Two implementations of type {Type} added", type.Name);
            throw new ServiceAlreadyStuckException($"Two implementations of type {type} added");
        }

        _registrations[type] = new GlueBinding(type, AdhesionMode.SINGLETON);
    }

    public void StickSingleton<TImplementation>()
    {
        Log.Debug("Sticking singleton of type {TImplementation}", typeof(TImplementation).Name);
        if (_registrations.ContainsKey(typeof(TImplementation)))
        {
            Log.Error("Two implementations of type {TImplementation} added", typeof(TImplementation).Name);
            throw new ServiceAlreadyStuckException($"Two implementations of type {typeof(TImplementation)} added");
        }

        _registrations[typeof(TImplementation)] = new GlueBinding(typeof(TImplementation), AdhesionMode.SINGLETON);
    }

    public void StickSingleton<TService, TImplementation>()
    {
        Log.Debug("Sticking singleton of type {TService} with implementation of type {TImplementation}"
            , typeof(TService).Name
            , typeof(TImplementation).Name);
        if (_registrations.ContainsKey(typeof(TService)))
        {
            Log.Error("Two implementations of type {TService} added", typeof(TService).Name);
            throw new ServiceAlreadyStuckException($"Two implementations of type {typeof(TService)} added");
        }

        _registrations[typeof(TService)] = new GlueBinding(typeof(TImplementation), AdhesionMode.SINGLETON);
    }

    public void StickTransient<TImplementation>()
    {
        Log.Debug("Sticking transient of type {TImplementation}", typeof(TImplementation).Name);
        if (_registrations.ContainsKey(typeof(TImplementation)))
        {
            Log.Error("Two implementations of type {TImplementation} added", typeof(TImplementation).Name);
            throw new ServiceAlreadyStuckException($"Two implementations of type {typeof(TImplementation)} added");
        }

        _registrations[typeof(TImplementation)] = new GlueBinding(typeof(TImplementation), AdhesionMode.TRANSIENT);
    }

    public void StickTransient<TService, TImplementation>()
    {
        Log.Debug("Sticking transient of type {TService} with implementation of type {TImplementation}"
            , typeof(TService).Name
            , typeof(TImplementation).Name);
        if (_registrations.ContainsKey(typeof(TService)))
        {
            Log.Error("Two implementations of type {TService} added", typeof(TService).Name);
            throw new ServiceAlreadyStuckException($"Two implementations of type {typeof(TService)} added");
        }

        _registrations[typeof(TService)] = new GlueBinding(typeof(TImplementation), AdhesionMode.TRANSIENT);
    }
}