

using System.ComponentModel.DataAnnotations.Schema;
using MyORMLibrary.Atributes;

namespace MyORMLibrary.Entity;

[Table("rooms")]
public class RoomEntity
{
    [PrimaryKey]
    public int Id { get; set; }

    [Atributes.Column("hotel_id")]
    public int HotelId { get; set; }

    [Atributes.Column("name")]
    public string Name { get; set; }

    [Atributes.Column("area_m2")]
    public int AreaM2 { get; set; }

    [Atributes.Column("bedrooms")]
    public int Bedrooms { get; set; }

    [Atributes.Column("beds")]
    public int Beds { get; set; }

    [Atributes.Column("has_wifi")]
    public bool HasWifi { get; set; }

    [Atributes.Column("has_ac")]
    public bool HasAc { get; set; }

    [Atributes.Column("balcony")]
    public bool Balcony { get; set; }
    
    [Atributes.Column("image_url")]
    public string ImageUrl { get; set; }

    [Atributes.Column("price_per_night")]
    public decimal PricePerNight { get; set; }
}
