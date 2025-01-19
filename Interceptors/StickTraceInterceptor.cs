using Castle.DynamicProxy;
using HanneBogaerts.GlueBox.Attributes.CrossCuttingConcerns;
using Serilog;

namespace HanneBogaerts.GlueBox.Interceptors;

public class StickTraceInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        if (invocation.Method.DeclaringType?
                .GetCustomAttributes(false)
                .Any(a => a.GetType() == typeof(StickTraceAttribute)) ?? false)
        {
            Log.Information("StickTrace: Start {MethodName}", invocation.Method.Name);
            invocation.Proceed();
            Log.Information("StickTrace: {MethodName} ended", invocation.Method.Name);
        }
        else
        {
            invocation.Proceed();
        }
    }
}