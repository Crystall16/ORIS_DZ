using System.Net;

public class HttpServer
{
    private HttpListener _listener;
    private bool _isRunning;
    private int _port = 8888;

    public async Task Start()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://127.0.0.1:" + _port.ToString() + "/");
        _listener.Start();
        _isRunning = true;
        Console.WriteLine($"Сервер запущен");

        _ = Task.Run(async () =>
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = ProcessRequestAsync(context);
                }
                catch (HttpListenerException)
                {
                    break; 
                }
            }
        });

        while (_isRunning)
        {
            string command = Console.ReadLine()!;
            if (command?.ToLower() == "stop")
                Stop();
        }
    }
    private void Stop()
    {
        if (_isRunning)
            {
                _isRunning = false;
                _listener?.Stop();
                _listener?.Close();
                Console.WriteLine("Сервер остановлен.");
            }
    }
    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var response = context.Response;
        var request = context.Request;
        Console.WriteLine("Пришел запрос");
            
        string path = request.Url!.LocalPath;
        
        if (path.EndsWith("/"))
            path += "index.html";

        string filePath = Directory.GetCurrentDirectory() + $"/static{path}";
        FileInfo fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            response.StatusCode = 404; 
            filePath = Directory.GetCurrentDirectory() + $"/static/404.html";
        }

        switch (fileInfo.Extension)
        {
            case ".html":
                response.ContentType = "text/html; charset=utf-8";
                break;
            case ".css":
                response.ContentType = "text/css; charset=utf-8";
                break;
            case ".js":
                response.ContentType = "text/javascript; charset=utf-8";
                break;
            case ".png":
                response.ContentType = "image/png";
                break;
            case ".ico":
                response.ContentType = "image/x-icon";
                break;
            case ".svg":
                response.ContentType = "image/svg+xml";
                break;
            case ".jpg":
                response.ContentType = "image/jpeg";
                break;
        }
        byte[] buffer = await File.ReadAllBytesAsync(filePath);
        response.ContentLength64 = buffer.Length;
        using Stream output = response.OutputStream;
        await output.WriteAsync(buffer);
        await output.FlushAsync();
        Console.WriteLine("Запрос обработан");
    }
}