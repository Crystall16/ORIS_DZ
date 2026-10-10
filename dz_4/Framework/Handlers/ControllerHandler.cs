using System.Net;
using System.Reflection;
using System.Text;
using MyServer.Framework.Attributes;

namespace MyServer.Framework.Handlers;

public class ControllerHandler : Handler
{
    private readonly List<RouteEntry> _routes = new();

    public ControllerHandler()
    {
        DiscoverControllers();
    }

    public override async Task HandleRequest(HttpListenerContext context)
    {
        string method = context.Request.HttpMethod.ToUpperInvariant();
        string path = (context.Request.Url?.AbsolutePath ?? "/").Trim('/');

        RouteEntry? route = _routes.FirstOrDefault(r =>
            r.Method == method && string.Equals(r.Path, path, StringComparison.OrdinalIgnoreCase));

        if (route == null)
        {
            await PassNext(context);
            return;
        }

        var arguments = await BindArguments(context, route.Action.GetParameters());
        object? controller = Activator.CreateInstance(route.ControllerType);
        object? result = route.Action.Invoke(controller, arguments);

        string body = result?.ToString() ?? string.Empty;
        byte[] buffer = Encoding.UTF8.GetBytes(body);
        var response = context.Response;
        response.StatusCode = 200;
        response.ContentType = body.TrimStart().StartsWith("<")
            ? "text/html; charset=utf-8"
            : "text/plain; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.Close();
    }

    private void DiscoverControllers()
    {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var controllerAttr = type.GetCustomAttribute<HttpControllerAttribute>();
            if (controllerAttr == null)
                continue;

            foreach (MethodInfo action in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var methodAttr = action.GetCustomAttribute<HttpMethodAttribute>();
                if (methodAttr == null)
                    continue;

                string path = string.Join('/', new[] { controllerAttr.Route, methodAttr.Route }
                    .Where(part => !string.IsNullOrEmpty(part)));

                _routes.Add(new RouteEntry(methodAttr.Method, path, type, action));
            }
        }
    }

    private static async Task<object?[]> BindArguments(HttpListenerContext context, ParameterInfo[] parameters)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (string key in context.Request.QueryString.AllKeys)
        {
            if (key == null)
                continue;
            values[key] = context.Request.QueryString[key] ?? string.Empty;
        }

        if (context.Request.HasEntityBody)
        {
            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            string body = await reader.ReadToEndAsync();
            foreach (var pair in ParseForm(body))
                values[pair.Key] = pair.Value;
        }

        var arguments = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            string name = parameters[i].Name ?? string.Empty;
            if (values.TryGetValue(name, out string? value))
                arguments[i] = value;
            else if (name.Equals("login", StringComparison.OrdinalIgnoreCase) && values.TryGetValue("email", out string? email))
                arguments[i] = email;
            else
                arguments[i] = string.Empty;
        }

        return arguments;
    }

    private static Dictionary<string, string> ParseForm(string body)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(body))
            return result;

        foreach (string pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = pair.Split('=', 2);
            if (parts.Length != 2)
                continue;

            string key = Uri.UnescapeDataString(parts[0].Replace('+', ' '));
            string value = Uri.UnescapeDataString(parts[1].Replace('+', ' '));
            result[key] = value;
        }

        return result;
    }

    private sealed record RouteEntry(string Method, string Path, Type ControllerType, MethodInfo Action);
}
