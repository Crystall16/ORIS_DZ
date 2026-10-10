using System.Net;

namespace MyServer.Framework.Handlers;

public class StaticFileHandler : Handler
{
    private readonly string _staticRoot;

    public StaticFileHandler(string staticDirectory)
    {
        _staticRoot = ResolveDirectory(staticDirectory);
    }

    public override async Task HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        if (!string.Equals(request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
        {
            await PassNext(context);
            return;
        }

        string relative = (request.Url?.AbsolutePath ?? "/").TrimStart('/');
        if (string.IsNullOrEmpty(relative) || relative.Contains(".."))
        {
            await PassNext(context);
            return;
        }

        string filePath = Path.GetFullPath(Path.Combine(_staticRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!filePath.StartsWith(_staticRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(filePath))
        {
            await PassNext(context);
            return;
        }

        byte[] buffer = await File.ReadAllBytesAsync(filePath);
        var response = context.Response;
        response.StatusCode = 200;
        response.ContentType = GetContentType(filePath);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.Close();
    }

    private static string ResolveDirectory(string staticDirectory)
    {
        string nextToExe = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, staticDirectory));
        if (Directory.Exists(nextToExe))
            return nextToExe;

        string cwd = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), staticDirectory));
        return Directory.Exists(cwd) ? cwd : nextToExe;
    }

    private static string GetContentType(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "text/javascript; charset=utf-8",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream"
        };
    }
}
