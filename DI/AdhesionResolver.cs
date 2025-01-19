using System.Reflection;
using Castle.DynamicProxy;
using HanneBogaerts.GlueBox.Attributes.CrossCuttingConcerns;
using HanneBogaerts.GlueBox.Attributes.UserImput;
using HanneBogaerts.GlueBox.Exceptions;
using HanneBogaerts.GlueBox.Interceptors;
using Serilog;

namespace HanneBogaerts.GlueBox.DI;

public class AdhesionResolver(AdhesionRegistry adhesionRegistry, AdhesionManager adhesionManager)
{
    private readonly Dictionary<Type, object> _singletons = new();
    public Dictionary<string, Action> CommandAdhesionMap { get; } = new();

    public TService Resolve<TService>()
    {
        Log.Debug("Resolving service of type {TService}", typeof(TService).Name);
        var cycleBreaker = new CycleBreaker(adhesionRegistry, adhesionManager);
        cycleBreaker.DetectStickyLoop();
        return (TService)Resolve(typeof(TService));
    }

    private object Resolve(Type serviceType)
    {
        Log.Debug("Resolving service of type {ServiceType}", serviceType.Name);
        if (!adhesionRegistry.GetRegistrations().TryGetValue(serviceType, out var registration))
        {
            var errorMessage =
                $"Unknown service type '{serviceType.FullName}'. Ensure it is registered before resolving.";

            Log.Error(errorMessage);
            throw new ServiceNotStickedException(errorMessage);
        }

        Log.Debug(
            "Service type {ServiceType} is registered with implementation type {ImplementationType} and adhesion mode {AdhesionMode}"
            , serviceType.Name
            , registration.ImplementationType.Name
            , registration.AdhesionMode);

        if (registration.AdhesionMode != AdhesionMode.SINGLETON)
            return CreateNewComponent(registration.ImplementationType);

        if (_singletons.TryGetValue(serviceType, out var singletonInstance))
        {
            Log.Debug("Returning existing singleton of type {ServiceType}", serviceType.Name);
            return singletonInstance;
        }

        Log.Debug("Creating new singleton of type {ServiceType}", serviceType.Name);
        var newSingletonInstance = CreateNewComponent(registration.ImplementationType);
        _singletons[serviceType] = newSingletonInstance;
        return newSingletonInstance;
    }

    private object CreateNewComponent(Type implementationType)
    {
        Log.Debug("Creating new component of type {ImplementationType}", implementationType.Name);
        var constructors = implementationType.GetConstructors();
        ConstructorInfo constructor;

        switch (constructors.Length)
        {
            case 0:
                Log.Error("No constructors found for {ImplementationType}", implementationType.Name);
                throw new ConstructorNotFoundException($"No constructors found for {implementationType.Name}");
            case 1:
                constructor = constructors[0];
                Log.Debug("Using default constructor for {ImplementationType}", implementationType.Name);
                break;
            default:
            {
                var parameterizedConstructors = constructors.Where(c => c.GetParameters().Length > 0).ToList();

                if (parameterizedConstructors.Count != 1)
                {
                    Log.Error("Multiple parameterized constructors found for {ImplementationType}",
                        implementationType.Name);
                    throw new ConstructorAmbiguityException(
                        $"Multiple parameterized constructors found for {implementationType.Name}");
                }

                constructor = parameterizedConstructors[0];
                Log.Debug("Using parameterized constructor for {ImplementationType}", implementationType.Name);
                break;
            }
        }

        var parameters = constructor.GetParameters()
            .Select(paramInfo => paramInfo.ParameterType)
            .Select(Resolve)
            .ToArray();

        Log.Information("Creating new instance of {ImplementationType}", implementationType.Name);
        Log.Debug("Parameters: {Parameters}", string.Join(", ", parameters.Select(p => p?.GetType().Name)));

        var generator = new ProxyGenerator();
        var interceptors = GetInterceptorsFor(implementationType);
        var proxy = generator.CreateClassProxy(implementationType, parameters, interceptors);

        RegisterCommands(proxy);

        return proxy;
    }

    private void RegisterCommands(object proxy)
    {
        var methods = proxy.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var method in methods)
        {
            var glueInputAttribute = method.GetCustomAttribute<GlueInputAttribute>();
            if (glueInputAttribute == null) continue;
            if (CommandAdhesionMap.ContainsKey(glueInputAttribute.Command))
            {
                throw new InvalidOperationException($"Duplicate command '{glueInputAttribute.Command}' detected.");
            }

            if (method.GetParameters().Length > 0)
            {
                throw new InvalidOperationException(
                    $"Command '{glueInputAttribute.Command}' cannot be invoked because it requires parameters.");
            }

            CommandAdhesionMap.Add(glueInputAttribute.Command, () => method.Invoke(proxy, null));
        }
    }


    private static IInterceptor[] GetInterceptorsFor(Type implementationType)
    {
        var interceptors = new List<IInterceptor>
        {
            new GlueTimerInterceptor(),
            new StickTraceInterceptor()
        };

        var stickOnFailureAttribute = implementationType.GetCustomAttribute<StickOnFailureAttribute>();
        if (stickOnFailureAttribute != null)
        {
            interceptors.Add(new StickOnFailureInterceptor(stickOnFailureAttribute.Times));
        }

        return interceptors.ToArray();
    }
}