
using System.Net;
using System.Text;

using HttpServer.Services;
using MiniHttpServer.Core.Attributes;
using MiniHttpServer.Framework.Core.Attributes;

namespace HttpServer.Endpoints
{
    [Endpoint]
    public class AuthEndpoint 
    {
        [HttpPOST]
        public async Task Login(HttpListenerContext ctx)
        {
            string body;
            using (var r = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding ?? Encoding.UTF8))
                body = await r.ReadToEndAsync();

            var form = body.Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=', 2))
                .Where(p => p.Length == 2)
                .ToDictionary(p => WebUtility.UrlDecode(p[0]),
                    p => WebUtility.UrlDecode(p[1]));

            form.TryGetValue("email", out var email);
            form.TryGetValue("password", out var password);
            form.TryGetValue("name", out var fullName); 

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            {
                await Write(ctx, "email and password are required", 400, "text/plain; charset=utf-8");
                return;
            }
            Console.WriteLine(">>> Отправляем письмо...");
            var safeName = string.IsNullOrWhiteSpace(fullName) ? "(не указано)" : WebUtility.HtmlEncode(fullName);
            var safeEmail = WebUtility.HtmlEncode(email);
            var safePassword = WebUtility.HtmlEncode(password);

            var html = $@"
            <h3>Данные из формы</h3>
            <p><b>Имя:</b> {safeName}</p>
            <p><b>Email:</b> {safeEmail}</p>
            <p><b>Password:</b> {safePassword}</p>";

            try
            {
                await EmailService.SendAsync(email!, "Ваши введённые данные", html);
                await Write(ctx, "{{\"status\":\"ok\"}}", 200, "application/json; charset=utf-8");
                Console.WriteLine(">>> Письмо успешно отправлено!");
            }
            catch
            {
                await Write(ctx, "Ошибка сервера", 500, "text/plain; charset=utf-8");
            }
        }
    

        private static async Task Write(HttpListenerContext c, string s, int status, string ct)
        {
            var b = Encoding.UTF8.GetBytes(s);
            c.Response.StatusCode = status;
            c.Response.ContentType = ct;
            c.Response.ContentLength64 = b.Length;
            await using var o = c.Response.OutputStream;
            await o.WriteAsync(b, 0, b.Length);
            await o.FlushAsync();
        }
    }
}
