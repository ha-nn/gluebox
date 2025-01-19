namespace HanneBogaerts.GlueBox.Attributes.CrossCuttingConcerns;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class StickOnFailureAttribute : Attribute
{
    public int Times { get; set; }

    public StickOnFailureAttribute(int times)
    {
        if (times < 1)
        {
            throw new ArgumentException("Times must be greater than 0");
        }

        Times = times;
    }
}