namespace MyServer.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public abstract class HttpMethodAttribute : Attribute
{
    public string Method { get; }
    public string Route { get; }

    protected HttpMethodAttribute(string method, string route)
    {
        Method = method;
        Route = route.Trim().Trim('/');
    }
}
