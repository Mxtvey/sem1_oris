using System.Net;   
using System.Text.Json;
using MiniHttpServer.Framework.Core.Attributes;
using MyORMLibrary.Repositories;

[Endpoint("/api/hotels")]
public class HotelsApiEndpoint
{
    [HttpGET]
    public async Task Get(HttpListenerContext ctx)
    {
        try
        {   
            var q = ctx.Request.QueryString;

            string? checkin  = q["checkin"];
            string? checkout = q["checkout"];

           
            bool hasPool          = q["HasPool"] == "true";
            bool hasWifi          = q["HasWifi"] == "true";
            bool hasParking       = q["HasParking"] == "true";
            bool hasWellness      = q["HasWellness"] == "true";
            bool hasAC            = q["HasAirConditioning"] == "true";
            bool hasHalfBoard     = q["HasHalfBoard"] == "true";
            
        

          
            var hotels = HotelRepository.GetAvailableHotels(checkin, checkout);
        
            if (hasPool)
                hotels = hotels.Where(h => h.HasPool).ToList();

            if (hasWifi)
                hotels = hotels.Where(h => h.HasWifi).ToList();

            if (hasParking)
                hotels = hotels.Where(h => h.HasParking).ToList();

           
            if (hasWellness)
                hotels = hotels.Where(h => h.HasWellness).ToList();

            if (hasAC)
                hotels = hotels.Where(h => h.HasAirConditioning).ToList();

            if (hasHalfBoard)
                hotels = hotels.Where(h => h.HasHalfBoard).ToList();
            string json = JsonSerializer.Serialize(hotels);
            

            ctx.Response.ContentType = "application/json";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);

            await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            ctx.Response.OutputStream.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}