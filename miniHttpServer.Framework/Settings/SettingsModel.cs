namespace HttpServer.Shared;

public class SettingsModel
{

    public string StaticDirectoryPath { get; set; } = "static";


    public string Domain { get; set; } = "localhost";

 
    public int Port { get; set; } = 1235;


    public string Prefix { get; set; } = "http://localhost:1235/tour";


    public string ConnectionString { get; set; } = 
        "Host=localhost;Port=5432;Database=exmpl;Username=postgres;Password=1234";

    // MIME-types
    public Dictionary<string, string> MimeType { get; set; } = new()
    {
        ["html"] = "text/html",
        ["txt"]  = "text/plain",
        ["json"] = "application/json",
        ["css"]  = "text/css",
        ["js"]   = "application/javascript",
        ["mjs"]  = "application/javascript",
        ["png"]  = "image/png",
        ["jpg"]  = "image/jpeg",
        ["jpeg"] = "image/jpeg",
        ["svg"]  = "image/svg+xml",
        ["webp"] = "image/webp",
        ["ico"]  = "image/x-icon"
    };
}