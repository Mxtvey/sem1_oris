using HttpServer.Shared;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.HttpResponce;
using MyORMLibrary;
using MyORMLibrary.Entity;
using MyORMLibrary.Repositories;


namespace MiniHttpServer.Framework.Endpoints
{
    [Endpoint("tour")]
    public class TourEndpoint : BaseEndpoint
    {
        private readonly ORMContext _orm;

        public TourEndpoint()
        {
            var settings = SettingsModelSingleton.Instance;
            _orm = new ORMContext(settings.ConnectionString);
        }

        [HttpGET("")]
        public IResponseResult Index()
        {
            
            var hotels = HotelRepository.GetAllHotels();

            string templatePath = "static/tour/index.html";

            var result = Page(templatePath, new { Hotels = hotels });
         

            return result;
        }

        [HttpGET("details")]
        public IResponseResult Details(int id)
        {
            var hotel = _orm.ReadById<HotelEntity>(id, "hotels");
            if (hotel == null)
                return new PageResult("static/errors/404.html", null).WithStatusCode(404);

            hotel.Location = _orm.ReadById<LocationEntity>(hotel.LocationId, "locations");

            var allRooms = _orm.ReadAll<RoomEntity>("rooms");
          

            var rooms = allRooms.Where(r => r.HotelId == hotel.Id).ToList();
         

            return new PageResult("static/tour/tour.html", new { Hotel = hotel, Rooms = rooms });
        }




    }


}



