using System.Text.Json;

namespace MyServer.Framework.Configuration;

public class AppSettings
{
    public string Scheme { get; set; } = "http";
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 8888;
    public string StaticDirectory { get; set; } = "static";

    public string Prefix => $"{Scheme}://{Host}:{Port}/";

    public static AppSettings Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "settings.json");
        if (!File.Exists(path))
            path = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");

        if (!File.Exists(path))
            return new AppSettings();

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new AppSettings();
    }
}
