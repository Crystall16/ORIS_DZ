using MyServer.Framework.Configuration;
using MyServer.Framework.Handlers;
using MyServer.Framework.Http;

class Program
{
    static async Task Main(string[] args)
    {
        var settings = AppSettings.Load();

        Handler staticFiles = new StaticFileHandler(settings.StaticDirectory);
        Handler controllers = new ControllerHandler();
        Handler notFound = new NotFoundHandler();

        staticFiles.Successor = controllers;
        controllers.Successor = notFound;

        var server = new HttpServer(settings, staticFiles);
        await server.Start();
    }
}
