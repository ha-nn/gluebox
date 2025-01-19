using Castle.DynamicProxy;
using HanneBogaerts.GlueBox.Attributes.CrossCuttingConcerns;
using Serilog;

namespace HanneBogaerts.GlueBox.Interceptors;

public class GlueTimerInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        var hasMethodAttribute = invocation.Method
            .GetCustomAttributes(false)
            .Any(a => a.GetType() == typeof(GlueTimerAttribute));

        var hasClassAttribute = invocation.Method.DeclaringType?
            .GetCustomAttributes(false)
            .Any(a => a.GetType() == typeof(GlueTimerAttribute)) ?? false;

        if (hasMethodAttribute || hasClassAttribute)
        {
            var duration = DateTime.Now.Ticks;
            invocation.Proceed();
            duration = (DateTime.Now.Ticks - duration) / 10000; // Convert to milliseconds
            Log.Information("GlueTimer: {MethodName} took {Duration}ms", invocation.Method.Name, duration);
        }
        else
        {
            invocation.Proceed();
        }
    }
}