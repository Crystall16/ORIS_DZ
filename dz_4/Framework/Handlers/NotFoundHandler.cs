using System.Net;
using System.Text;

namespace MyServer.Framework.Handlers;

public class NotFoundHandler : Handler
{
    public override async Task HandleRequest(HttpListenerContext context)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "static", "404.html");
        string html = File.Exists(path)
            ? await File.ReadAllTextAsync(path)
            : "404";

        byte[] buffer = Encoding.UTF8.GetBytes(html);
        var response = context.Response;
        response.StatusCode = 404;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.Close();
    }
}
