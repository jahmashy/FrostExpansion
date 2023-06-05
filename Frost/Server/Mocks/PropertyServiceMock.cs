using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Frost.Shared.Models.Enums;

namespace Frost.Server.Mocks
{
    public class PropertyServiceMock : IPropertyService
    {

        async Task<IEnumerable<PropertyOffer>> IPropertyService.GetPromotedPropertiesAsync()
        {
            List<string> propertyImagesUrl1 = new();
            List<string> propertyImagesUrl2 = new();
            List<string> propertyImagesUrl3 = new();
            for(int i = 1; i <= 3; i++)
            {
                string dirPath = $"Resources/PropertyImages/{i}";
                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                if (!dirInfo.Exists)
                {
                    Console.WriteLine("Dir does not exists");
                }
                FileInfo[] files = dirInfo.GetFiles();
                foreach (FileInfo file in files)
                {
                    switch (i)
                    {
                        case 1:
                            propertyImagesUrl1.Add($"https://localhost:44350/api/propertyimages/{i}/{file.Name}");
                            break;
                        case 2:
                            propertyImagesUrl2.Add($"https://localhost:44350/api/propertyimages/{i}/{file.Name}");
                            break;
                        case 3:
                            propertyImagesUrl3.Add($"https://localhost:44350/api/propertyimages/{i}/{file.Name}");
                            break;
                    }
                    
                }
            }        

            Location l1 = new Location("Warszawa","cityPlaceId","Mazowieckie","adId","Wola","slPlaceId");
            Location l2 = new Location("Kielce", "cityPlaceId", "Świętokrzyskie", "adId", "Bocianek", "slPlaceId");
            Location l3 = new Location("Warszawa", "cityPlaceId", "Mazowieckie", "adId", "Bemowo", "slPlaceId");

            Property p1 = new Property(42.5, PropertyType.Flat, 4, 2010, 6, l1,propertyImagesUrl1);
            Property p2 = new Property(56, PropertyType.House, 7, 2010, 1, l2, propertyImagesUrl2);
            Property p3 = new Property(27.2, PropertyType.Flat, 2, 2010, 6, l3, propertyImagesUrl3);

            string description = "Culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptartem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi ropeior architecto beatae vitae dicta sunt.";

            PropertyOffer offer1 = new PropertyOffer(1,"Mieszkanie na woli",2600,description,p1,OfferType.Rent,MarketType.Secondary,true);
            PropertyOffer offer2 = new PropertyOffer(2, "Dom w centurm Kielc", 1000000, description, p2, OfferType.Buy, MarketType.Primary, false);
            PropertyOffer offer3 = new PropertyOffer(3, "Kawalerka przy lotnisku", 2000, description, p3, OfferType.Rent, MarketType.Secondary, true);

            List<PropertyOffer> offerList = new List<PropertyOffer>();
            offerList.Add(offer1);
            offerList.Add(offer2);
            offerList.Add(offer3);

            await Task.Delay(1000);

            return offerList;
        }
    }
}
