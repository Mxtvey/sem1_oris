using System.Text.Json;

namespace HttpServer.Shared;

public class SettingsModelSingleton
{
    private static readonly Lazy<SettingsModel> _instance =
        new Lazy<SettingsModel>(LoadConfig, LazyThreadSafetyMode.ExecutionAndPublication);

    public static SettingsModel Instance => _instance.Value;

    private static SettingsModel LoadConfig()
    {
        SettingsModel model = new SettingsModel();

       
        try
        {
            string full = Path.GetFullPath("Settings/settings.json");
            Console.WriteLine("Trying to load: " + full);

            if (File.Exists(full))
            {
                string json = File.ReadAllText(full);
                model = JsonSerializer.Deserialize<SettingsModel>(json) ?? new SettingsModel();
            }
            else
            {
                Console.WriteLine("settings.json not found, using defaults");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to load settings.json: " + ex.Message);
        }

     
        ApplyEnvOverrides(model);

        return model;
    }

    private static void ApplyEnvOverrides(SettingsModel model)
    {
       
        string? prefix = Environment.GetEnvironmentVariable("Prefix");
        if (!string.IsNullOrWhiteSpace(prefix))
            model.Prefix = prefix;

     
        string? domain = Environment.GetEnvironmentVariable("Domain");
        if (!string.IsNullOrWhiteSpace(domain))
            model.Domain = domain;

    
        string? port = Environment.GetEnvironmentVariable("Port");
        if (int.TryParse(port, out int parsedPort))
            model.Port = parsedPort;

 
        string? conn = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (!string.IsNullOrWhiteSpace(conn))
            model.ConnectionString = conn;

  
        if (string.IsNullOrWhiteSpace(prefix))
            model.Prefix = $"http://{model.Domain}:{model.Port}/";
    }


}
