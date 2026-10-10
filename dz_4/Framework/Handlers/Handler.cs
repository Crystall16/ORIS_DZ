using System.Net;

namespace MyServer.Framework.Handlers;

public abstract class Handler
{
    public Handler? Successor { get; set; }

    public abstract Task HandleRequest(HttpListenerContext context);

    protected Task PassNext(HttpListenerContext context)
    {
        if (Successor != null)
            return Successor.HandleRequest(context);

        return Task.CompletedTask;
    }
}
