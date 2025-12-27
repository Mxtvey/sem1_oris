using System.Collections.Generic;
using System.Text.Json;
using MyORMLibrary.Atributes;

namespace MyORMLibrary.Entity;

public class UsefulItem
{
    public string Distance { get; set; }
    public string Label   { get; set; }
}

public class HotelEntity
{
    [PrimaryKey]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("stars")]
    public int Stars { get; set; }

    [Column("rating")]
    public decimal Rating { get; set; }

    [Column("reviews_count")]
    public int ReviewsCount { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; }
    
    [Column("image_url1")]
    public string ImageUrl1 { get; set; }
    
    [Column("image_url2")]
    public string ImageUrl2 { get; set; }
    
    [Column("image_url3")]
    public string ImageUrl3 { get; set; }


    [Column("location_id")]
    public int LocationId { get; set; }

  
    [NotMapped]
    public LocationEntity Location { get; set; }

    [Column("features_json")]
    public string FeaturesJson { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("nights")]
    public int Nights { get; set; }

    [Column("adults")]
    public int Adults { get; set; }

    [Column("meal_type")]
    public string MealType { get; set; }

 
    [NotMapped]
    public string RatingText
    {
        get
        {
            if (Rating >= 9.5m)
                return "Excellent";
            else if (Rating >= 9.0m)
                return "Very good";
            else
                return "Good";
        }
    }

    [Column("has_wifi")]
    public bool HasWifi { get; set; }

    [Column("has_parking")]
    public bool HasParking { get; set; }

    [Column("has_pool")]
    public bool HasPool { get; set; }

    [Column("is_pet_friendly")]
    public bool IsPetFriendly { get; set; }

    [Column("is_kid_friendly")]
    public bool IsKidFriendly { get; set; }
    
    [Column("has_wellness")]
    public bool HasWellness { get; set; }

    [Column("has_air_conditioning")]
    public bool HasAirConditioning { get; set; }

    [Column("has_half_board")]
    public bool HasHalfBoard { get; set; }
    
    [Column("description_html")]
    public string DescriptionHtml { get; set; }


 
    [NotMapped] // <--- ВАЖНО
    public string UsefulInfoJson { get; set; }



    [NotMapped]
    public List<UsefulItem> UsefulInfo =>
        JsonSerializer.Deserialize<List<UsefulItem>>(UsefulInfoJson ?? "[]");

 
    [Column("checkin_from")]
    public string CheckInFrom { get; set; }
    
    [Column("description")]
    public string Description { get; set; }

    [Column("checkin_to")]
    public string CheckInTo { get; set; }

    [Column("checkout_until")]
    public string CheckOutUntil { get; set; }

    [Column("spoken_languages")]
    public string SpokenLanguages { get; set; }

    [Column("payment_policy")]
    public string PaymentPolicy { get; set; }

    [Column("accepted_currency")]
    public string AcceptedCurrency { get; set; }

    [Column("local_tax")]
    public string LocalTax { get; set; }

    [Column("reception_hours")]
    public string ReceptionHours { get; set; }

    [Column("wellness_hours")]
    public string WellnessHours { get; set; }

  
    [NotMapped]
    public string AcceptedCardsJson { get; set; }


    [NotMapped]
    public List<string> AcceptedCards =>
        JsonSerializer.Deserialize<List<string>>(AcceptedCardsJson ?? "[]");
}
