using System.Net;
using System.Text;
using System.IO;

namespace MiniHttpServer.HttpResponce;

public class PageResult : IResponseResult
{   
    private readonly object _data;
    private readonly string _pathTemplate;
    private int _statusCode = 200;

    public PageResult(string pathTemplate, object data)
    {
        _pathTemplate = pathTemplate ?? throw new ArgumentNullException(nameof(pathTemplate));
        _data = data;
     
    }
    
    public PageResult WithStatusCode(int statusCode)
    {
        _statusCode = statusCode;
        return this;
    }
    
    public void Execute(HttpListenerContext context)
    {

        
        try
        {
            // Проверяем существование файла
            if (!File.Exists(_pathTemplate))
            {
            
                SendError(context, 404, $"Template not found: {_pathTemplate}");
                return;
            }
            
            string template = File.ReadAllText(_pathTemplate);
      
            
    
            var renderer = new HtmlTemplateRenderer();
            string html = renderer.RenderFromString(template, _data);
            
          
    
            SendHtmlResponse(context, html, _statusCode);
            
 
        }
        catch (Exception ex)
        {
            SendError(context, 500, $"Error: {ex.Message}");
        }
    }
    
    private void SendHtmlResponse(HttpListenerContext context, string html, int statusCode)
    {
        var response = context.Response;
        response.StatusCode = statusCode;
        
        byte[] buffer = Encoding.UTF8.GetBytes(html);
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
        
    }
    
    private void SendError(HttpListenerContext context, int statusCode, string message)
    {
        var response = context.Response;
        response.StatusCode = statusCode;
        
        string errorHtml = $@"
            <!DOCTYPE html>
            <html>
            <head><title>Error {statusCode}</title></head>
            <body>
                <h1>Error {statusCode}</h1>
                <p>{message}</p>
            </body>
            </html>";
        
        byte[] buffer = Encoding.UTF8.GetBytes(errorHtml);
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}