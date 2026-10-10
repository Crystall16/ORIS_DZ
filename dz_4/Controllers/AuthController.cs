using MyServer.Framework.Attributes;

namespace MyServer.Controllers;

[HttpController("auth")]
public class AuthController
{
    [Get("login")]
    public string Login()
    {
        return ReadLoginPage();
    }

    [Post("login")]
    public string Login(string login, string password)
    {
        Console.WriteLine(login);
        Console.WriteLine(password);
        return string.Empty;
    }

    private static string ReadLoginPage()
    {
        string[] candidates =
        {
            Path.Combine(AppContext.BaseDirectory, "static", "LoginForm", "login.html"),
            Path.Combine(Directory.GetCurrentDirectory(), "static", "LoginForm", "login.html")
        };

        string? path = candidates.FirstOrDefault(File.Exists);
        if (path == null)
            return string.Empty;

        return File.ReadAllText(path);
    }
}
