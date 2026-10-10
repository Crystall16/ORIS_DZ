using System.Net;
using MyServer.Framework.Configuration;
using MyServer.Framework.Handlers;

namespace MyServer.Framework.Http;

public class HttpServer
{
    private readonly AppSettings _settings;
    private readonly Handler _chain;
    private HttpListener? _listener;
    private bool _isRunning;

    public HttpServer(AppSettings settings, Handler chain)
    {
        _settings = settings;
        _chain = chain;
    }

    public async Task Start()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(_settings.Prefix);
        _listener.Start();
        _isRunning = true;

        _ = Task.Run(async () =>
        {
            while (_isRunning)
            {
                try
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    _ = ProcessRequestAsync(context);
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        });

        while (_isRunning)
        {
            string? command = Console.ReadLine();
            if (string.Equals(command, "stop", StringComparison.OrdinalIgnoreCase))
                Stop();
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        try
        {
            await _chain.HandleRequest(context);
        }
        catch (Exception)
        {
            try
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
            catch
            {
            }
        }
    }

    private void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _listener?.Stop();
        _listener?.Close();
    }
}
