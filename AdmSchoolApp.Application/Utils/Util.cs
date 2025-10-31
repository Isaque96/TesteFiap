namespace AdmSchoolApp.Application.Utils;

public static class Util
{
    public static IEnumerable<Exception> GetAllExceptions(this Exception exception)
    {
        for (var e = exception; e != null; e = e.InnerException)
            yield return e;
    }
    
    public const string SerilogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {SourceContext} " +
    "{CorrelationId} {RequestId} {TraceId} {SpanId} " +
    "- {Message:lj}{NewLine}{Exception}";
}