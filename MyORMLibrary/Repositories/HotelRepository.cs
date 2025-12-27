using MyORMLibrary.Entity;

namespace MyORMLibrary.Repositories;


public static class HotelRepository
{
    public static List<HotelEntity> GetAllHotels()
    {
        return Database.ORM.ReadAll<HotelEntity>("hotels");
    }

    public static HotelEntity? GetById(int id)
    {
        return Database.ORM.ReadById<HotelEntity>(id, "hotels");
    }

    public static List<HotelEntity> GetAvailableHotels(string? checkin, string? checkout)
    {
        if (string.IsNullOrWhiteSpace(checkin) || string.IsNullOrWhiteSpace(checkout))
            return GetAllHotels();


        if (!DateTime.TryParse(checkin, out var start))
            return GetAllHotels();

        if (!DateTime.TryParse(checkout, out var end))
            return GetAllHotels();

        var allHotels = GetAllHotels();


        var reservations = Database.ORM.ReadAll<ReservationEntity>("reservation");

 
        return allHotels.Where(h =>
        {

            var hotelRes = reservations.Where(r => r.HotelId == h.Id);

            foreach (var r in hotelRes)
            {
               
                bool overlap =
                    start < r.EndDate &&
                    end > r.StartDate;

                if (overlap)
                    return false;
            }

        
            return true;
        }).ToList();
    }


}

