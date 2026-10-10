namespace MyServer.Framework.Attributes;

public class GetAttribute : HttpMethodAttribute
{
    public GetAttribute(string route) : base("GET", route)
    {
    }
}
