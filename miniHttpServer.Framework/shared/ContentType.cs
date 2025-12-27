namespace MiniHttpServer.Shared;

public static class ContentType
{   
    public static Dictionary<string, string> fileTypes = new()
    {
        { ".html", "text/html" },
        { ".css", "text/css" },
        { ".js", "application/javascript" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".webp", "image/webp" },
        { ".ico", "image/x-icon" },
        { ".svg", "image/svg+xml" },
        { ".json", "application/json" }
    };
    public static string GetContentType(string path)
    {
        string extension = Path.GetExtension(path);
        if (fileTypes.ContainsKey(extension))
        {
            return fileTypes[extension];
        }
        return "text/html; charset=UTF-8";
    }
    
}