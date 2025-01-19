using Castle.DynamicProxy;
using Serilog;

namespace HanneBogaerts.GlueBox.Interceptors;

public class StickOnFailureInterceptor(int maxRetries) : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        var retries = 0;
        Exception lastException = null;

        while (retries < maxRetries)
        {
            try
            {
                Log.Debug("Attempting {MethodName}, attempt {AttemptNumber}", invocation.Method.Name, retries + 1);
                invocation.Proceed();

                Log.Information("Successfully executed {MethodName} after {RetryCount} retries", invocation.Method.Name,
                    retries);
                return;
            }
            catch (Exception ex)
            {
                retries++;
                lastException = ex;

                Log.Error(ex, "Failed to execute {MethodName} on attempt {AttemptNumber}", invocation.Method.Name,
                    retries);
            }
        }

        if (lastException == null) return;
        Log.Fatal(lastException, "Failed to execute {MethodName} after {MaxRetries} attempts", invocation.Method.Name,
            maxRetries);
        throw lastException;
    }
}