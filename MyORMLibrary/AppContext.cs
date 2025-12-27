using MyORMLibrary.Entity;

namespace MyORMLibrary;

public class AppContext : ORMContext
{
    public DbSet<HotelEntity> Hotels { get; }
    public DbSet<LocationEntity> Locations { get; }
    public DbSet<UserEntity> Users { get; }
    public DbSet<BookingEntity> Bookings { get; }

    public AppContext(string connectionString) 
        : base(connectionString)
    {
        Hotels = new DbSet<HotelEntity>(this, "hotels");
        Users  = new DbSet<UserEntity>(this, "users");
        Locations = new DbSet<LocationEntity>(this, "locations");
        Bookings = new DbSet<BookingEntity>(this, "bookings");
    }
}
