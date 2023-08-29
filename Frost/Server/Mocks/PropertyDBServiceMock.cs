using Frost.Server.Models;
using Frost.Server.Repositories;
using Frost.Server.Services.ImageServices;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using Frost.Shared.Models.Forms;
using System.ComponentModel;
using System.Drawing;

namespace Frost.Server.Mocks
{
    public class PropertyDBServiceMock : IPropertyRepository
    {
        private IPropertyImageService _imageService;
        private IUserImageService _userImageService;
        private IUserRepository _userDbService;
        private static int lastId = 3;
        private static string description = "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est eopksio laborum. Sed ut perspiciatis unde omnis istpoe natus error sit voluptatem accusantium doloremque eopsloi laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunot explicabo. Nemo ernim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sedopk quia consequuntur magni dolores eos qui rationesopl voluptatem sequi nesciunt. Neque porro quisquameo est, qui dolorem ipsum quia dolor sit amet, eopsmiep consectetur, adipisci velit, seisud quia non numquam eius modi tempora incidunt ut labore et dolore wopeir magnam aliquam quaerat voluptatem eoplmuriquisqu";
        
        public PropertyDBServiceMock(IPropertyImageService imageService, IUserRepository userDbService, IUserImageService userImageService)
        {
            _imageService = imageService;
            _userDbService = userDbService;
            _userImageService = userImageService;
        }

        private static List<PropertyDetailsDTO> properties = new List<PropertyDetailsDTO>()
        {
            new PropertyDetailsDTO()
            {
                OfferId = 1,
                Title = "Mieszkanie na woli",
                Description = description,
                Price = "2600",
                Surface = "42,5",
                propertyType = PropertyTypeEnum.Flat,
                RoomsNumber = "4",
                ConstructionYear = "2010",
                Floor = "4",
                location = new Location()
                {
                    cityName = "Warszawa",
                    cityPlaceId = "cityPlaceId",
                    administrativeAreaName = "Mazowieckie",
                    administrativeAreaPlaceId = "adId",
                    districtName = "Wola",
                    districtPlaceId = "slPlaceId"
                },
                offerType = OfferTypeEnum.Rent,
                marketType = MarketTypeEnum.Primary,
                RoommatesAllowed = true,
                user = new UserDTO() {userId = 1,name = "maksym", email = "user1@gmail.com", telNumber="123123123"},
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
                propertyType = PropertyTypeEnum.House,
                RoomsNumber = "7",
                ConstructionYear = "2010",
                Floor = "4",
                location = new Location()
                {
                    cityName = "Kielce",
                    cityPlaceId = "cityPlaceId",
                    administrativeAreaName = "Świętokrzyskie",
                    administrativeAreaPlaceId = "adId",
                    districtName = "Bocianek",
                    districtPlaceId = "slPlaceId"

                },
                offerType = OfferTypeEnum.Rent,
                marketType = MarketTypeEnum.Primary,
                RoommatesAllowed = false,
                user = new UserDTO() {userId = 1, name = "maksym", email = "maksym@gmail.com", telNumber="123123123" },
                propertyImagesUrls = getImagesForStaticProperties(2)
            },
                        new PropertyDetailsDTO()
            {
                OfferId = 3,
                Title = "Kawalerka przy lotnisku",
                Description = description,
                Price = "2000",
                Surface = "27,2",
                propertyType = PropertyTypeEnum.Flat,
                RoomsNumber = "2",
                ConstructionYear = "2010",
                Floor = "4",
                location = new Location()
                {
                    cityName = "Warszawa",
                    cityPlaceId = "cityPlaceId",
                    administrativeAreaName = "Mazowieckie",
                    administrativeAreaPlaceId = "adId",
                    districtName = "Bemowo",
                    districtPlaceId = "slPlaceId"
                },
                offerType = OfferTypeEnum.Rent,
                marketType = MarketTypeEnum.Primary,
                RoommatesAllowed = true,
                user = new UserDTO() {userId = 1, name = "maksym", email = "maksym@gmail.com", telNumber="123123123"},
                propertyImagesUrls = getImagesForStaticProperties(3),
                RoommatesList = new List<UserDTO>()
            }
        };

        public async Task<(bool,int)> CreatePropertyAsync(PropertyFormModel property, string email)
        {
            await Task.Delay(10);
            int newId = lastId + 1;
            

            UserDTO user = await _userDbService.GetUserAsync(email);
            properties.Add(new PropertyDetailsDTO
            {
                OfferId = newId,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price.ToString(),
                Surface = property.Surface.ToString(),
                propertyType = property.propertyType,
                RoomsNumber = property.RoomsNumber.ToString(),
                ConstructionYear = property.ConstructionYear.ToString(),
                Floor = property.Floor.ToString(),
                location = property.location,
                offerType = property.offerType,
                marketType = property.marketType,
                RoommatesAllowed = property.RoommatesAllowed,
                user = new UserDTO
                {
                    userId = user.userId,
                    email = user.email,
                    name = user.name,
                    telNumber = user.telNumber.ToString(),
                    profileImgUrl = _userImageService.GetUserProfileImageUrl(user.userId)
                },
                propertyImagesUrls = null
            }); ;
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
            var userProperties = properties.Where(p => p.user.email == email).ToList();
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

        async Task<IEnumerable<PropertyDetailsDTO>> IPropertyRepository.GetPromotedPropertiesAsync()
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

        public Task<bool> DeletePropertyAsync(int offerId, int userId)
        {
            var property = properties.Where(p => p.OfferId == offerId && p.user.userId == userId).FirstOrDefault();
            if (property == null)
                return Task.FromResult(false);
            return Task.FromResult(properties.Remove(property));
        }

        public Task<PropertyFormModel>? GetPropertyToEditAsync(int offerId, int userId)
        {

            var property = properties.Where(p => p.OfferId == offerId && p.user.userId == userId).FirstOrDefault();
            if (property == null)
                return Task.FromResult<PropertyFormModel>(null);
            return Task.FromResult(new PropertyFormModel()
            {
                Title = property.Title,
                Description = property.Description,
                Price = double.Parse(property.Price),
                Surface = float.Parse(property.Surface),
                RoomsNumber = int.Parse(property.RoomsNumber),
                ConstructionYear = int.Parse(property.ConstructionYear),
                Floor = int.Parse(property.Floor),
                location = property.location,
                offerType = property.offerType,
                marketType = property.marketType,
                RoommatesAllowed = property.RoommatesAllowed,
            });
        }

        public async Task<bool> UpdatePropertyAsync(PropertyFormModel propertyUpdate, string email, int offerId)
        {
            var property = properties.Where( p=> p.user.email == email && p.OfferId == offerId).FirstOrDefault();
            if (property == null)
                return false;
            property.Title = propertyUpdate.Title;
            property.Description = propertyUpdate.Description;
            property.Price = propertyUpdate.Price.ToString();
            property.Surface = propertyUpdate.Surface.ToString();
            property.propertyType = propertyUpdate.propertyType;
            property.RoomsNumber = propertyUpdate.RoomsNumber.ToString();
            property.ConstructionYear = propertyUpdate.ConstructionYear.ToString();
            property.Floor = propertyUpdate.Floor.ToString();
            property.location = propertyUpdate.location;
            property.offerType = propertyUpdate.offerType;
            property.marketType = propertyUpdate.marketType;
            property.RoommatesAllowed = propertyUpdate.RoommatesAllowed;

            return true;
        }

        public Task<PropertyStatusCode> FollowOfferAsync(int offerId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<PropertyStatusCode> UnfollowOfferAsync(int offerId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<(PropertyStatusCode, IEnumerable<PropertyDetailsDTO>)> GetFollowedOffersAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> isOfferFollowedByUserAsync(int offerId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> isUserPropertyAsync(int offerId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<PropertyStatusCode> AddUserToRoommates(int offerId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<PropertyStatusCode> RemoveUserFromRoommates(int offerId, int userId)
        {
            throw new NotImplementedException();
        }


        public Task<(IEnumerable<PropertyDetailsDTO>, int)> SearchForOffersAsync(string propertyType, string offerType, string? marketType, string? cityId, int? minRoomsNumber, int? maxRoomsNumber, int? minFloor, int? maxFloor, double? minPrice, double? maxPrice, double? minMeterPrice, double? maxMeterPrice, float? minSurface, float? maxSurface, int? minConstructionYear, int? maxConstructionYear, bool? allowRoommates, string? constructionType, int recordsToSkip = 0)
        {
            throw new NotImplementedException();
        }

        public List<(string userMail, string offerTitle)> GetOffersAboutToExpire()
        {
            throw new NotImplementedException();
        }

        public Task<(PropertyStatusCode, List<PropertyDetailsDTO>)> GetSavedTemplatesAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<PropertyStatusCode> DeleteTemplateAsync(int offerId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<PropertyStatusCode> PublishPropertyFromTemplateAsync(int offerId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task AddViewToPropertyAsync(int offerId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetPropertyViewsCountAsync(int offerId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetPropertyFollowCountAsync(int offerId)
        {
            throw new NotImplementedException();
        }

        Task<(PropertyStatusCode, PropertyDetailsDTO)> IPropertyRepository.PublishPropertyFromTemplateAsync(int offerId, int userId)
        {
            throw new NotImplementedException();
        }

        Task<(PropertyStatusCode, PropertyDetailsDTO)> IPropertyRepository.SaveTemplateAsync(int offerId, int userId)
        {
            throw new NotImplementedException();
        }

        Task<(bool, PropertyDetailsDTO)> IPropertyRepository.UpdatePropertyAsync(PropertyFormModel property, string email, int offerId)
        {
            throw new NotImplementedException();
        }

        public Task<(IEnumerable<PropertyDetailsDTO>, int)> SearchForOffersAsync(string propertyType, string offerType, string? marketType, string? cityId, int? minRoomsNumber, int? maxRoomsNumber, int? minFloor, int? maxFloor, double? minPrice, double? maxPrice, double? minMeterPrice, double? maxMeterPrice, float? minSurface, float? maxSurface, int? minConstructionYear, int? maxConstructionYear, bool? allowRoommates, string? constructionType, int recordsToSkip = 0, string sortBy = "DateDesc")
        {
            throw new NotImplementedException();
        }
    }
}
