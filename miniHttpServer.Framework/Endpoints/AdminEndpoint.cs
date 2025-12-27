using System.Net;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using MiniHttpServer.Core.Attributes;
using MiniHttpServer.Framework.Core.Attributes;
using MyORMLibrary;
using MyORMLibrary.Entity;

[Endpoint("/admin/login")]
public class AdminLoginEndpoint
{

    [HttpGET]
    public async Task Get(HttpListenerContext ctx)
    {
        var path = "static/admin/login.html";
        var html = File.ReadAllText(path);

        ctx.Response.ContentType = "text/html; charset=utf-8";
        var buf = Encoding.UTF8.GetBytes(html);
        ctx.Response.ContentLength64 = buf.Length;
        await ctx.Response.OutputStream.WriteAsync(buf, 0, buf.Length);
        ctx.Response.OutputStream.Close();
    }


    [HttpPOST]
    public async Task Post(HttpListenerContext ctx)
    {
        using var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding);
        var body = await reader.ReadToEndAsync();

        var form = ParseForm(body);
        var email = form.GetValueOrDefault("email") ?? "";
        var password = form.GetValueOrDefault("password") ?? "";

        var allUsers = Database.ORM.ReadAll<UserEntity>("users");
        var user = allUsers.FirstOrDefault(u => u.Email == email);
       
       

        if (user == null)
        {
            await WriteSimpleError(ctx, "User not found");
            return;
        }

        if (user.Role != "admin")
        {
            await WriteSimpleError(ctx, "You are not admin");
            return;
        }

        var hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        
        if (hash != user.PasswordHash)
        {
            await WriteSimpleError(ctx, "Wrong password");
            return;
        }
        

      
        var cookie = new Cookie("userId", user.Id.ToString())
        {
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(3)
        };
       
        ctx.Response.Cookies.Add(cookie);

        ctx.Response.StatusCode = 302;
        ctx.Response.Headers["Location"] = "/admin";
        ctx.Response.OutputStream.Close();
    }

    private Dictionary<string,string> ParseForm(string body)
    {
        var dict = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(body)) return dict;

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

    private async Task WriteSimpleError(HttpListenerContext ctx, string message)
    {
        var html = $"<!DOCTYPE html><html><head><meta charset='utf-8'><title>Login error</title></head><body>" +
                   $"<h1>Login error</h1><p>{WebUtility.HtmlEncode(message)}</p>" +
                   "<a href=\"/admin/login\">Back</a></body></html>";
        var buf = Encoding.UTF8.GetBytes(html);
        ctx.Response.ContentType = "text/html; charset=utf-8";
        ctx.Response.ContentLength64 = buf.Length;
        await ctx.Response.OutputStream.WriteAsync(buf, 0, buf.Length);
        ctx.Response.OutputStream.Close();
    }
}
[Endpoint("/admin")]
public class AdminPanelEndpoint
{
    [HttpGET]
    public async Task Get(HttpListenerContext ctx)
    {
       
        var currentUser = GetCurrentUser(ctx);
        if (currentUser == null || currentUser.Role != "admin")
        {
            ctx.Response.StatusCode = 302;
            ctx.Response.Headers["Location"] = "/admin/login";
            ctx.Response.OutputStream.Close();
            return;
        }

     
        var hotels = Database.ORM.ReadAll<HotelEntity>("hotels");

     
        var sb = new StringBuilder();
        sb.Append("""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <title>Admin panel</title>
  <style>
    body { font-family: system-ui, sans-serif; margin: 20px; }
    h1 { margin-bottom: 16px; }
    table { border-collapse: collapse; width: 100%; max-width: 900px; }
    th, td { border: 1px solid #ddd; padding: 6px 8px; font-size: 13px; }
    th { background: #f3f3f3; text-align: left; }
    a.btn { display:inline-block; padding:4px 8px;
            border-radius:4px; border:1px solid #0071c2;
            text-decoration:none; font-size:12px; color:#0071c2; }
    a.btn:hover { background:#0071c2; color:#fff; }
    .topbar { margin-bottom: 12px; font-size: 14px; }
  </style>
</head>
<body>
<div class="topbar">
  <a href="/admin/logout">Logout</a>
</div>

""");

        sb.Append($"<div class=\"topbar\">Logged in as <b>{System.Net.WebUtility.HtmlEncode(currentUser.Username)}</b> ({currentUser.Email})</div>");
        sb.Append("<h1>Hotels (admin)</h1>");
        sb.Append("<p><a class=\"btn\" href=\"/admin/hotel/add\">+ Add hotel</a></p>");

        sb.Append("<table>");
        sb.Append("<tr><th>ID</th><th>Name</th><th>Stars</th><th>Rating</th><th>Actions</th></tr>");

        foreach (var h in hotels)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{h.Id}</td>");
            sb.Append($"<td>{System.Net.WebUtility.HtmlEncode(h.Name)}</td>");
            sb.Append($"<td>{h.Stars}</td>");
            sb.Append($"<td>{h.Rating}</td>");
            sb.Append($"<td><a class=\"btn\" href=\"/admin/hotel/edit?id={h.Id}\">Edit</a></td>");
            sb.Append("</tr>");
            sb.Append($"<td>" +
                      $"<a class=\"btn\" href=\"/admin/hotel/edit?id={h.Id}\">Edit</a> " +
                      $"<a class=\"btn\" href=\"/admin/hotel/delete?id={h.Id}\" " +
                      $"onclick=\"return confirm('Delete this hotel?');\">Delete</a>" +
                      $"</td>");

        }

        sb.Append("</table>");
        sb.Append("</body></html>");

        var html = sb.ToString();
        var buf = Encoding.UTF8.GetBytes(html);
        ctx.Response.ContentType = "text/html; charset=utf-8";
        ctx.Response.ContentLength64 = buf.Length;
        await ctx.Response.OutputStream.WriteAsync(buf, 0, buf.Length);
        ctx.Response.OutputStream.Close();
    }

    private UserEntity? GetCurrentUser(HttpListenerContext ctx)
    {
        var cookie = ctx.Request.Cookies["userId"];
        if (cookie == null) return null;

        if (!int.TryParse(cookie.Value, out var userId))
            return null;

        return Database.ORM.ReadById<UserEntity>(userId, "users");
    }
}

[Endpoint("/admin/hotel")]
public class AdminHotelEndpoint
{
  
   [HttpGET("edit")]
public async Task GetEdit(HttpListenerContext ctx, int id)
{
    var currentUser = GetCurrentUser(ctx);
    if (currentUser == null || currentUser.Role != "admin")
    {
        ctx.Response.StatusCode = 302;
        ctx.Response.Headers["Location"] = "/admin/login";
        ctx.Response.OutputStream.Close();
        return;
    }

    var hotel = Database.ORM.ReadById<HotelEntity>(id, "hotels");
    if (hotel == null)
    {
        var nf = Encoding.UTF8.GetBytes("Hotel not found");
        ctx.Response.StatusCode = 404;
        await ctx.Response.OutputStream.WriteAsync(nf, 0, nf.Length);
        ctx.Response.OutputStream.Close();
        return;
    }

    var sb = new StringBuilder();
    sb.Append("""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <title>Edit hotel</title>
  <style>
    body { font-family: system-ui, sans-serif; margin: 20px; }
    label { display:block; margin-top:8px; font-size:13px; }
    input[type="text"], input[type="number"] {
      width: 320px; padding:6px 8px; font-size:13px;
      border-radius:4px; border:1px solid #ccc; box-sizing:border-box;
    }
    textarea {
      width: 100%; max-width: 600px;
      min-height: 100px;
      padding:6px 8px;
      font-size:13px;
      border-radius:4px;
      border:1px solid #ccc;
      box-sizing:border-box;
      resize: vertical;
    }
    .btn { margin-top:12px; padding:8px 14px; border:none;
           background:#0071c2; color:#fff; border-radius:4px; cursor:pointer; }
    .btn:hover { background:#005c9c; }
    a.back { display:inline-block; margin-top:10px; font-size:13px; }
    .checkbox-row { margin-top:6px; font-size:13px; }
    .checkbox-row label { display:inline; margin-right:12px; }
  </style>
</head>
<body>
""");

    sb.Append($"<h1>Edit hotel #{hotel.Id}</h1>");

    sb.Append($"<form method=\"post\" action=\"/admin/hotel/edit?id={hotel.Id}\">");


    sb.Append("<label>Name</label>");
    sb.Append($"<input type=\"text\" name=\"name\" value=\"{System.Net.WebUtility.HtmlEncode(hotel.Name)}\" />");

 
    sb.Append("<label>Image URL</label>");
    sb.Append($"<input type=\"text\" name=\"imageUrl\" value=\"{System.Net.WebUtility.HtmlEncode(hotel.ImageUrl)}\" />");

 
    sb.Append("<label>Stars</label>");
    sb.Append($"<input type=\"number\" name=\"stars\" min=\"1\" max=\"5\" value=\"{hotel.Stars}\" />");


    sb.Append("<label>Rating</label>");
    sb.Append($"<input type=\"number\" name=\"rating\" step=\"0.1\" min=\"0\" max=\"10\" value=\"{hotel.Rating.ToString(System.Globalization.CultureInfo.InvariantCulture)}\" />");


    sb.Append("<label>Price (€)</label>");
    sb.Append($"<input type=\"number\" name=\"price\" step=\"1\" min=\"0\" value=\"{hotel.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)}\" />");


    sb.Append("<label>Nights</label>");
    sb.Append($"<input type=\"number\" name=\"nights\" min=\"1\" max=\"30\" value=\"{hotel.Nights}\" />");


    sb.Append("<label>Adults</label>");
    sb.Append($"<input type=\"number\" name=\"adults\" min=\"1\" max=\"10\" value=\"{hotel.Adults}\" />");


    sb.Append("<label>Meal type</label>");
    sb.Append("<input type=\"text\" name=\"mealType\" value=\"" +
              System.Net.WebUtility.HtmlEncode(hotel.MealType) + "\" />");


    sb.Append("<label>Rating text</label>");
    sb.Append("<input type=\"text\" name=\"ratingText\" value=\"" +
              System.Net.WebUtility.HtmlEncode(hotel.RatingText) + "\" />");


    sb.Append("<div class=\"checkbox-row\">");
    sb.Append($"<label><input type=\"checkbox\" name=\"hasWifi\" {(hotel.HasWifi ? "checked" : "")} /> Wifi</label>");
    sb.Append($"<label><input type=\"checkbox\" name=\"hasParking\" {(hotel.HasParking ? "checked" : "")} /> Parking</label>");
    sb.Append($"<label><input type=\"checkbox\" name=\"hasPool\" {(hotel.HasPool ? "checked" : "")} /> Pool</label>");
    sb.Append($"<label><input type=\"checkbox\" name=\"isPetFriendly\" {(hotel.IsPetFriendly ? "checked" : "")} /> Pet friendly</label>");
    sb.Append($"<label><input type=\"checkbox\" name=\"isKidFriendly\" {(hotel.IsKidFriendly ? "checked" : "")} /> Kid friendly</label>");
    sb.Append("</div>");

   
    sb.Append("<label>Description</label>");
    sb.Append("<textarea id=\"descriptionHtml\" name=\"descriptionHtml\">"
              + System.Net.WebUtility.HtmlEncode(hotel.DescriptionHtml ?? "")
              + "</textarea>");


    sb.Append("<br/><button type=\"submit\" class=\"btn\">Save</button>");
    sb.Append("</form>");
    sb.Append("<a href=\"/admin\" class=\"back\">← Back to admin</a>");
    sb.Append(@"
<script src=""https://cdn.ckeditor.com/4.22.1/standard/ckeditor.js""></script>
<script>
  CKEDITOR.replace('descriptionHtml');
</script>
</body></html>");

    sb.Append("</body></html>");

    var html = sb.ToString();
    var buf = Encoding.UTF8.GetBytes(html);
    ctx.Response.ContentType = "text/html; charset=utf-8";
    ctx.Response.ContentLength64 = buf.Length;
    await ctx.Response.OutputStream.WriteAsync(buf, 0, buf.Length);
    ctx.Response.OutputStream.Close();
}


    
   [HttpPOST("edit")]
public async Task PostEdit(HttpListenerContext ctx, int id)
{
    var currentUser = GetCurrentUser(ctx);
    if (currentUser == null || currentUser.Role != "admin")
    {
        ctx.Response.StatusCode = 302;
        ctx.Response.Headers["Location"] = "/admin/login";
        ctx.Response.OutputStream.Close();
        return;
    }

    var hotel = Database.ORM.ReadById<HotelEntity>(id, "hotels");
    if (hotel == null)
    {
        ctx.Response.StatusCode = 404;
        var nf = Encoding.UTF8.GetBytes("Hotel not found");
        await ctx.Response.OutputStream.WriteAsync(nf, 0, nf.Length);
        ctx.Response.OutputStream.Close();
        return;
    }

    using var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding);
    var body = await reader.ReadToEndAsync();
    var form = ParseForm(body);

  
    var name        = form.GetValueOrDefault("name") ?? hotel.Name;
    var imageUrl    = form.GetValueOrDefault("imageUrl") ?? hotel.ImageUrl;
    var starsStr    = form.GetValueOrDefault("stars");
    var ratingStr   = form.GetValueOrDefault("rating");
    var priceStr    = form.GetValueOrDefault("price");
    var nightsStr   = form.GetValueOrDefault("nights");
    var adultsStr   = form.GetValueOrDefault("adults");
    var mealType    = form.GetValueOrDefault("mealType") ?? hotel.MealType;
    var ratingText  = form.GetValueOrDefault("ratingText") ?? hotel.RatingText;
    var descHtml    = form.GetValueOrDefault("descriptionHtml") ?? hotel.DescriptionHtml;

    var hasWifi      = form.ContainsKey("hasWifi");
    var hasParking   = form.ContainsKey("hasParking");
    var hasPool      = form.ContainsKey("hasPool");
    var isPetFriendly= form.ContainsKey("isPetFriendly");
    var isKidFriendly= form.ContainsKey("isKidFriendly");

  
    if (int.TryParse(starsStr, out var stars))
        hotel.Stars = stars;

    if (decimal.TryParse(ratingStr, System.Globalization.NumberStyles.Any,
        System.Globalization.CultureInfo.InvariantCulture, out var rating))
        hotel.Rating = rating;

    if (decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any,
        System.Globalization.CultureInfo.InvariantCulture, out var price))
        hotel.Price = price;

    if (int.TryParse(nightsStr, out var nights))
        hotel.Nights = nights;

    if (int.TryParse(adultsStr, out var adults))
        hotel.Adults = adults;


    hotel.Name          = name;
    hotel.ImageUrl      = imageUrl;
    hotel.MealType      = mealType;
    hotel.DescriptionHtml = descHtml;


    hotel.HasWifi       = hasWifi;
    hotel.HasParking    = hasParking;
    hotel.HasPool       = hasPool;
    hotel.IsPetFriendly = isPetFriendly;
    hotel.IsKidFriendly = isKidFriendly;

    try
    {
        Database.ORM.Update(id, hotel, "hotels");
    }
    catch (Exception ex)
    {
        Console.WriteLine("[ADMIN EDIT HOTEL ERROR] " + ex.Message);
        Console.WriteLine(ex.StackTrace);

        var msg = Encoding.UTF8.GetBytes("DB error: " + ex.Message);
        ctx.Response.StatusCode = 500;
        await ctx.Response.OutputStream.WriteAsync(msg, 0, msg.Length);
        ctx.Response.OutputStream.Close();
        return;
    }

    ctx.Response.StatusCode = 302;
    ctx.Response.Headers["Location"] = "/admin";
    ctx.Response.OutputStream.Close();
}



    private UserEntity? GetCurrentUser(HttpListenerContext ctx)
    {
        var cookie = ctx.Request.Cookies["userId"];
        if (cookie == null) return null;
        if (!int.TryParse(cookie.Value, out var userId))
            return null;
        return Database.ORM.ReadById<UserEntity>(userId, "users");
    }

    private Dictionary<string,string> ParseForm(string body)
    {
        var dict = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(body)) return dict;

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
  
    [HttpGET("add")]
    public async Task GetAdd(HttpListenerContext ctx)
    {
        var currentUser = GetCurrentUser(ctx);
        if (currentUser == null || currentUser.Role != "admin")
        {
            ctx.Response.StatusCode = 302;
            ctx.Response.Headers["Location"] = "/admin/login";
            ctx.Response.OutputStream.Close();
            return;
        }

        var sb = new StringBuilder();
        sb.Append("""
                  <!DOCTYPE html>
                  <html lang="en">
                  <head>
                    <meta charset="UTF-8" />
                    <title>Add hotel</title>
                    <style>
                      body { font-family: system-ui, sans-serif; margin: 20px; }
                      label { display:block; margin-top:8px; font-size:13px; }
                      input[type="text"], input[type="number"] {
                        width: 300px; padding:6px 8px; font-size:13px;
                        border-radius:4px; border:1px solid #ccc; box-sizing:border-box;
                      }
                      .btn { margin-top:12px; padding:8px 14px; border:none;
                             background:#0071c2; color:#fff; border-radius:4px; cursor:pointer; }
                      .btn:hover { background:#005c9c; }
                      a.back { display:inline-block; margin-top:10px; font-size:13px; }
                    </style>
                  </head>
                  <body>
                  <h1>Add hotel</h1>
                  <form method="post" action="/admin/hotel/add">
                    <label>Name</label>
                    <input type="text" name="name" />
                  
                    <label>Stars</label>
                    <input type="number" name="stars" min="1" max="5" value="3" />
                  
                    <label>Rating</label>
                    <input type="number" name="rating" step="0.1" min="0" max="10" value="8.0" />
                  
                    <label>Has wifi</label>
                    <input type="checkbox" name="hasWifi" />
                  
                    <br/>
                    <button type="submit" class="btn">Create</button>
                  </form>

                  <a href="/admin" class="back">← Back to admin</a>
                  </body>
                  </html>
                  """);

        var html = sb.ToString();
        var buf = Encoding.UTF8.GetBytes(html);
        ctx.Response.ContentType = "text/html; charset=utf-8";
        ctx.Response.ContentLength64 = buf.Length;
        await ctx.Response.OutputStream.WriteAsync(buf, 0, buf.Length);
        ctx.Response.OutputStream.Close();
    }
  
    [HttpPOST("add")]
    public async Task PostAdd(HttpListenerContext ctx)
    {
        var currentUser = GetCurrentUser(ctx);
        if (currentUser == null || currentUser.Role != "admin")
        {
            ctx.Response.StatusCode = 302;
            ctx.Response.Headers["Location"] = "/admin/login";
            ctx.Response.OutputStream.Close();
            return;
        }

        using var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding);
        var body = await reader.ReadToEndAsync();
        var form = ParseForm(body);

        var name      = form.GetValueOrDefault("name") ?? "";
        var starsStr  = form.GetValueOrDefault("stars") ?? "3";
        var ratingStr = form.GetValueOrDefault("rating") ?? "8.0";
        var hasWifiStr= form.GetValueOrDefault("hasWifi");

        if (!int.TryParse(starsStr, out var stars)) stars = 3;
        if (!decimal.TryParse(ratingStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var rating))
            rating = 8.0m;

        var hotel = new HotelEntity
        {
            Name = name,
            Stars = stars,
            Rating = rating,
            HasWifi = !string.IsNullOrEmpty(hasWifiStr),

            ReviewsCount = 0,
            ImageUrl = "jpg/default-hotel.jpg",

          
            LocationId = 1,             
            Price = 100,                
            Nights = 7,
            Adults = 2,
            MealType = "Breakfast"
        };

        try
        {
            Database.ORM.Create(hotel, "hotels");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ADMIN ADD HOTEL ERROR] " + ex.Message);
            Console.WriteLine(ex.StackTrace);

            var msg = Encoding.UTF8.GetBytes("DB error: " + ex.Message);
            ctx.Response.StatusCode = 500;
            await ctx.Response.OutputStream.WriteAsync(msg, 0, msg.Length);
            ctx.Response.OutputStream.Close();
            return;
        }

        ctx.Response.StatusCode = 302;
        ctx.Response.Headers["Location"] = "/admin";
        ctx.Response.OutputStream.Close();
    }
   
    [HttpGET("delete")]
    public async Task GetDelete(HttpListenerContext ctx, int id)
    {
        var currentUser = GetCurrentUser(ctx);
        if (currentUser == null || currentUser.Role != "admin")
        {
            ctx.Response.StatusCode = 302;
            ctx.Response.Headers["Location"] = "/admin/login";
            ctx.Response.OutputStream.Close();
            return;
        }

     
        var hotel = Database.ORM.ReadById<HotelEntity>(id, "hotels");
        if (hotel == null)
        {
            ctx.Response.StatusCode = 404;
            var nf = Encoding.UTF8.GetBytes("Hotel not found");
            await ctx.Response.OutputStream.WriteAsync(nf, 0, nf.Length);
            ctx.Response.OutputStream.Close();
            return;
        }

        Database.ORM.Delete(id, "hotels");

        ctx.Response.StatusCode = 302;
        ctx.Response.Headers["Location"] = "/admin";
        ctx.Response.OutputStream.Close();
    }
    [Endpoint("/admin/logout")]
    public class AdminLogoutEndpoint
    {
        [HttpGET]
        public async Task Get(HttpListenerContext ctx)
        {
            var cookie = new Cookie("userId", "")
            {
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            ctx.Response.Cookies.Add(cookie);

            ctx.Response.StatusCode = 302;
            ctx.Response.Headers["Location"] = "/tour";
            ctx.Response.OutputStream.Close();
        }
    }




}



