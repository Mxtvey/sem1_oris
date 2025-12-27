using MyORMLibrary.Atributes;

namespace MyORMLibrary.Entity;

public class ReservationEntity
{
    [PrimaryKey]
    public int Id { get; set; }

    [Column("hotel_id")]
    public int HotelId { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }
}
