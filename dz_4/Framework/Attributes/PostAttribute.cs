namespace MyServer.Framework.Attributes;

public class PostAttribute : HttpMethodAttribute
{
    public PostAttribute(string route) : base("POST", route)
    {
    }
}
