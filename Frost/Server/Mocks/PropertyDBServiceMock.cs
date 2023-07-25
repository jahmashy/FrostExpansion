using Frost.Server.Models;
using Frost.Server.Services;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using Frost.Shared.Models.Forms;

namespace Frost.Server.Mocks
{
    public class PropertyDBServiceMock : IPropertyService
    {
        private IPropertyImageService _imageService;
        private IUserDbService _userDbService;
        private static int lastId = 3;
        private static string description = "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est eopksio laborum. Sed ut perspiciatis unde omnis istpoe natus error sit voluptatem accusantium doloremque eopsloi laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunot explicabo. Nemo ernim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sedopk quia consequuntur magni dolores eos qui rationesopl voluptatem sequi nesciunt. Neque porro quisquameo est, qui dolorem ipsum quia dolor sit amet, eopsmiep consectetur, adipisci velit, seisud quia non numquam eius modi tempora incidunt ut labore et dolore wopeir magnam aliquam quaerat voluptatem eoplmuriquisqu";
        
        public PropertyDBServiceMock(IPropertyImageService imageService, IUserDbService userDbService)
        {
            _imageService = imageService;
            _userDbService = userDbService;
        }
        
        private static List<PropertyDetailsDTO> properties = new List<PropertyDetailsDTO>()
        {
            new PropertyDetailsDTO()
            {
                OfferId = 1,
                Title = "Mieszkanie na woli",
                Description = description,
                Price = "2600",
                Surface = "42.5",
                PropertyType = PropertyType.Flat,
                RoomsNumber = "4",
                ConstructionYear = "2010",
                Floor = "4",
                Location = new Location()
                {
                    cityName = "Warszawa",
                    cityPlaceId = "cityPlaceId",
                    administrativeAreaName = "Mazowieckie",
                    administrativeAreaPlaceId = "adId",
                    districtName = "Wola",
                    districtPlaceId = "slPlaceId"
                },
                offerType = OfferType.Rent,
                marketType = MarketType.Primary,
                RoommatesAllowed = true,
                User = new UserDto() {Name = "maksym", Email = "user1@gmail.com", TelNumber="123123123"},
                propertyImagesUrls = getImagesForStaticProperties(1),
                RoommatesList = UserDbServiceMock.GetSampleUsers()
            },
                        new PropertyDetailsDTO()
            {
                OfferId = 2,
                Title = "Dom w centurm Kielc",
                Description = description,
                Price = "1000000",
                Surface = "56",
                PropertyType = PropertyType.House,
                RoomsNumber = "7",
                ConstructionYear = "2010",
                Floor = "4",
                Location = new Location()
                {
                    cityName = "Kielce",
                    cityPlaceId = "cityPlaceId",
                    administrativeAreaName = "Świętokrzyskie",
                    administrativeAreaPlaceId = "adId",
                    districtName = "Bocianek",
                    districtPlaceId = "slPlaceId"

                },
                offerType = OfferType.Rent,
                marketType = MarketType.Primary,
                RoommatesAllowed = false,
                User = new UserDto() {Name = "maksym", Email = "maksym@gmail.com", TelNumber="123123123"},
                propertyImagesUrls = getImagesForStaticProperties(2)
            },
                        new PropertyDetailsDTO()
            {
                OfferId = 3,
                Title = "Kawalerka przy lotnisku",
                Description = description,
                Price = "2000",
                Surface = "27.2",
                PropertyType = PropertyType.Flat,
                RoomsNumber = "2",
                ConstructionYear = "2010",
                Floor = "4",
                Location = new Location()
                {
                    cityName = "Warszawa",
                    cityPlaceId = "cityPlaceId",
                    administrativeAreaName = "Mazowieckie",
                    administrativeAreaPlaceId = "adId",
                    districtName = "Bemowo",
                    districtPlaceId = "slPlaceId"
                },
                offerType = OfferType.Rent,
                marketType = MarketType.Primary,
                RoommatesAllowed = true,
                User = new UserDto() {Name = "maksym", Email = "maksym@gmail.com", TelNumber="123123123"},
                propertyImagesUrls = getImagesForStaticProperties(3)
            }
        };

        public async Task<(bool,int)> CreateProperty(PropertyFormModel property, string email)
        {
            await Task.Delay(10);
            int newId = lastId + 1;
            

            User user = await _userDbService.GetUserAsync(email);
            properties.Add(new PropertyDetailsDTO
            {
                OfferId = newId,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price.ToString(),
                Surface = property.Surface.ToString(),
                PropertyType = property.PropertyType,
                RoomsNumber = property.RoomsNumber.ToString(),
                ConstructionYear = property.ConstructionYear.ToString(),
                Floor = property.Floor.ToString(),
                Location = property.Location,
                offerType = property.offerType,
                marketType = property.marketType,
                RoommatesAllowed = property.RoommatesAllowed,
                User = new UserDto
                {
                    Email = user.email,
                    Name = user.name,
                    TelNumber = user.telNumber.ToString(),
                    ProfileImgUrl = ""
                },
                propertyImagesUrls = null
            });
            lastId++;
            return (true,newId);
        }

        public async Task<PropertyDetailsDTO> GetPropertyDetailsAsync(int offerId)
        {
            await Task.Delay(10);
            return properties.Where(p => p.OfferId == offerId).FirstOrDefault();
        }

        public async Task<IEnumerable<PropertyDetailsDTO>> GetUserPropertiesAsync(string email)
        {
            await Task.Delay(10);
            var userProperties = properties.Where(p => p.User.Email == email).ToList();
            foreach (var p in userProperties)
            {
                (FileStatusCode status, List<string> urls) = _imageService.GetPropertyImagesUrls(p.OfferId);
                if(status == FileStatusCode.Success)
                {
                    p.propertyImagesUrls = urls;
                }
            }
            return userProperties;
        }

        async Task<IEnumerable<PropertyDetailsDTO>> IPropertyService.GetPromotedPropertiesAsync()
        {
            await Task.Delay(10);
            return properties;
        }

        private static List<string> getImagesForStaticProperties(int offerId)
        {
            List<string> propertyImagesUrl = new();
            string dirPath = $"Resources/PropertyImages/{offerId}";
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                propertyImagesUrl.Add($"https://localhost:44350/api/propertyimages/{offerId}/{file.Name}");
            }
            return propertyImagesUrl;
        }
    }
}
