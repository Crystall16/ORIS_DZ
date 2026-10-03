class Program
{
    static async Task Main(string[] args)
    {
        HttpServer server = new HttpServer();
        await server.Start();
    }
}

public class ServerConfig
{
    public string Scheme { get; set; } = "http";
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 8888;
    public string Path { get; set; } = "connection";
}