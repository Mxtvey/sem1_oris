using MyORMLibrary.Atributes;

namespace MyORMLibrary.Entity;

public class BookingEntity
{
    [PrimaryKey]
    public int Id { get; set; }

    [Column("hotel_id")]
    [ForeignKey("Hotels")]
    public int HotelId { get; set; }

    [Column("user_id")]
    [ForeignKey("Users")]
    public int UserId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("total_price")]
    public decimal TotalPrice { get; set; }

    [Column("status")]
    public string Status { get; set; } 
   
}
