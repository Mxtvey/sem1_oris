using MyORMLibrary.Entity;

namespace MyORMLibrary.Repositories;

public static class ReservationRepository
{
    public static List<ReservationEntity> GetAll()
    {
        return Database.ORM.ReadAll<ReservationEntity>("reservation");
    }

    public static bool IsHotelFree(int hotelId, DateTime start, DateTime end)
    {
        return !GetAll().Any(r =>
            r.HotelId == hotelId &&
            r.StartDate < end &&
            r.EndDate > start
        );
    }

}
