using System.Net;

namespace MiniHttpServer.HttpResponce;

public interface IResponseResult
{
    void Execute(HttpListenerContext context);
}