namespace HanneBogaerts.GlueBox.DI;

public class GlueBinding(Type implementationType, AdhesionMode adhesionMode)
{
    public Type ImplementationType { get; } = implementationType;
    public AdhesionMode AdhesionMode { get; } = adhesionMode;
}