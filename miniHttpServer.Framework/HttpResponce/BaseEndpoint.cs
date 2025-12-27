using System.Net;

namespace MiniHttpServer.HttpResponce;


public abstract class BaseEndpoint
{
    protected HttpListenerContext Context { get; private set; }
    
    public void SetContext(HttpListenerContext context)
    {
        this.Context = context;
    }
    
    protected IResponseResult Page(string templatePath, object model)
    {
        return new PageResult(templatePath, model);
    }
}