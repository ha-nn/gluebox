namespace HanneBogaerts.GlueBox.Attributes.UserImput;

[AttributeUsage(AttributeTargets.Method)]
public class GlueInputAttribute(string command) : Attribute
{
    public string Command { get; } = command;
}