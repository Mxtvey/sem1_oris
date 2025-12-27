using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MiniHttpServer.Core.Attributes; 
using MiniHttpServer.Framework.Core.Attributes;
using MyORMLibrary;
using MyORMLibrary.Entity;

[Endpoint("/tour/register")]
public class RegisterEndpoint
{
 
    [HttpGET]
    public async Task Get(HttpListenerContext ctx)
    {
    
        var path = "static/auth/registr.html"; 
        if (!File.Exists(path))
        {
            ctx.Response.StatusCode = 404;
            var nf = Encoding.UTF8.GetBytes("Register page not found");
            await ctx.Response.OutputStream.WriteAsync(nf, 0, nf.Length);
            ctx.Response.OutputStream.Close();
            return;
        }

        var html = File.ReadAllText(path);
        ctx.Response.ContentType = "text/html; charset=utf-8";
        var buffer = Encoding.UTF8.GetBytes(html);
        ctx.Response.ContentLength64 = buffer.Length;
        await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        ctx.Response.OutputStream.Close();
    }

 
    [HttpPOST]
    public async Task Post(HttpListenerContext ctx)
    {
        try
        {
          
            using var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding);
            var body = await reader.ReadToEndAsync();

      
            var form = ParseForm(body);

            string email    = form.GetValueOrDefault("email") ?? "";
            string userName = form.GetValueOrDefault("userName") ?? "";
            string phone    = form.GetValueOrDefault("phone") ?? "";
            string password = form.GetValueOrDefault("password") ?? "";
            string password2= form.GetValueOrDefault("password2") ?? "";

          
            var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            var phoneRegex = new Regex(@"^\+?\d{7,15}$");

            var errors = new List<string>();

            if (!emailRegex.IsMatch(email))
                errors.Add("Invalid email");

            if (!string.IsNullOrWhiteSpace(phone) && !phoneRegex.IsMatch(phone))
                errors.Add("Invalid phone");

            if (string.IsNullOrWhiteSpace(userName) || userName.Length < 3)
                errors.Add("Name too short");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                errors.Add("Password too short");

            if (password != password2)
                errors.Add("Passwords do not match");

      
            var allUsers = Database.ORM.ReadAll<UserEntity>("users");
            if (allUsers.Any(u => u.Email == email))
                errors.Add("Email already used");

            if (errors.Count > 0)
            {
                
                var html = new StringBuilder();
                html.Append("<!DOCTYPE html><html><head><meta charset='utf-8'><title>Register error</title></head><body>");
                html.Append("<h1>Registration error</h1><ul>");
                foreach (var e in errors)
                    html.Append("<li>" + WebUtility.HtmlEncode(e) + "</li>");
                html.Append("</ul>");
                html.Append("<a href=\"/auth/register\">Back</a>");
                html.Append("</body></html>");

                var buf = Encoding.UTF8.GetBytes(html.ToString());
                ctx.Response.ContentType = "text/html; charset=utf-8";
                ctx.Response.ContentLength64 = buf.Length;
                await ctx.Response.OutputStream.WriteAsync(buf, 0, buf.Length);
                ctx.Response.OutputStream.Close();
                return;
            }

         
            var hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));

            var user = new UserEntity
            {
                Email = email,
                Username = userName,
                Phone = phone,
                PasswordHash = hash,
                Role = "user"
            };

            Database.ORM.Create(user, "users");

         
            ctx.Response.StatusCode = 302;
            ctx.Response.Headers["Location"] = "/tour";
            ctx.Response.OutputStream.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Register ERROR] " + ex);

            var msg = Encoding.UTF8.GetBytes("Server error");
            ctx.Response.StatusCode = 500;
            await ctx.Response.OutputStream.WriteAsync(msg, 0, msg.Length);
            ctx.Response.OutputStream.Close();
        }
    }

    private Dictionary<string,string> ParseForm(string body)
    {
        var dict = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(body))
            return dict;

        var parts = body.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var kv = part.Split('=', 2);
            var key = HttpUtility.UrlDecode(kv[0] ?? "");
            var val = kv.Length > 1 ? HttpUtility.UrlDecode(kv[1] ?? "") : "";
            dict[key] = val;
        }
        return dict;
    }
}
