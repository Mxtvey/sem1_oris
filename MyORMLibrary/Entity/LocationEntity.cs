using MyORMLibrary.Atributes;

namespace MyORMLibrary.Entity;

public class LocationEntity
{
    [PrimaryKey]
    public int Id { get; set; }

    [Column("country")]
    public string Country { get; set; }

    [Column("city")]
    public string City { get; set; }

    [Column("address")]
    public string Address { get; set; }
}
