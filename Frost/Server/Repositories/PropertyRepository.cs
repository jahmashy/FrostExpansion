using Frost.Server.EntityModels;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using Frost.Shared.Models.Forms;
using Frost.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Collections.Generic;
using System;
using Frost.Server.Services.MailServices;
using Frost.Server.Services.ImageServices;

namespace Frost.Server.Repositories
{
    public interface IPropertyRepository
    {
        public Task<IEnumerable<PropertyDetailsDTO>> GetPromotedPropertiesAsync();
        public Task<PropertyDetailsDTO> GetPropertyDetailsAsync(int offerId);
        public Task<IEnumerable<PropertyDetailsDTO>> GetUserPropertiesAsync(string email);
        public Task<(bool, int)> CreatePropertyAsync(PropertyFormModel property, string email);
        public Task<bool> DeletePropertyAsync(int offerId, int userId);
        public Task<PropertyFormModel>? GetPropertyToEditAsync(int offerId, int userId);
        public Task<(bool, PropertyDetailsDTO)> UpdatePropertyAsync(PropertyFormModel property, string email, int offerId);
        public Task<PropertyStatusCode> FollowOfferAsync(int offerId, int userId);
        public Task<PropertyStatusCode> UnfollowOfferAsync(int offerId, int userId);
        public Task<(PropertyStatusCode, IEnumerable<PropertyDetailsDTO>)> GetFollowedOffersAsync(int userId);
        public Task<bool> isOfferFollowedByUserAsync(int offerId, int userId);
        public Task<bool> isUserPropertyAsync(int offerId, int userId);
        public Task<PropertyStatusCode> AddUserToRoommates(int offerId, int userId);
        public Task<PropertyStatusCode> RemoveUserFromRoommates(int offerId, int userId);
        public List<(string userMail, string offerTitle)> GetOffersAboutToExpire();
        public Task<(IEnumerable<PropertyDetailsDTO>, int)> SearchForOffersAsync(
            string propertyType,
            string offerType,
            string? marketType,
            string? cityId,
            int? minRoomsNumber,
            int? maxRoomsNumber,
            int? minFloor,
            int? maxFloor,
            double? minPrice,
            double? maxPrice,
            double? minMeterPrice,
            double? maxMeterPrice,
            float? minSurface,
            float? maxSurface,
            int? minConstructionYear,
            int? maxConstructionYear,
            bool? allowRoommates,
            string? constructionType,
            int recordsToSkip = 0,
            string sortBy = "DateDesc"
            );
        public Task<(PropertyStatusCode, List<PropertyDetailsDTO>)> GetSavedTemplatesAsync(int userId);
        public Task<(PropertyStatusCode, PropertyDetailsDTO)> SaveTemplateAsync(int offerId, int userId);
        public Task<PropertyStatusCode> DeleteTemplateAsync(int offerId, int userId);
        public Task<(PropertyStatusCode, PropertyDetailsDTO)> PublishPropertyFromTemplateAsync(int offerId, int userId);
        public Task AddViewToPropertyAsync(int offerId, int userId);
        public Task<int> GetPropertyViewsCountAsync(int offerId);
        public Task<int> GetPropertyFollowCountAsync(int offerId);

    }
    public class PropertyRepository : IPropertyRepository
    {
        private FrostDbContext _dbContext;
        private IPropertyImageService _propertyImageService;
        private IUserImageService _userImageService;
        private IMailService _mailService;
        public PropertyRepository(FrostDbContext frostDbContext, IPropertyImageService propertyImageService, IUserImageService userImageService, IMailService mailService)
        {
            _dbContext = frostDbContext;
            _propertyImageService = propertyImageService;
            _userImageService = userImageService;
            _mailService = mailService;
        }

        public async Task<PropertyStatusCode> AddUserToRoommates(int offerId, int userId)
        {
            Property property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == offerId);
            if (property == null)
                return PropertyStatusCode.PropertyDoesNotExists;
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return PropertyStatusCode.UserDoesNotExists;
            if (await _dbContext.PropertyRoommates.AnyAsync(fo => fo.UserId == userId && fo.PropertyId == offerId))
                return PropertyStatusCode.UserIsAlreadyRoommate;
            if (!property.RoommatesAllowed)
                return PropertyStatusCode.PropertyDoesNotAllowRoommates;
            _dbContext.PropertyRoommates.Add(new PropertyRoommate { PropertyId = offerId, UserId = userId });
            await _dbContext.SaveChangesAsync();
            return PropertyStatusCode.Success;

        }

        public async Task<(bool, int)> CreatePropertyAsync(PropertyFormModel property, string email)
        {
            User user = _dbContext.Users.FirstOrDefault(x => x.Email == email);
            if (user == null)
                return (false, 0);

            if (!await _dbContext.Cities.AnyAsync(c => c.GoogleId == property.location.cityPlaceId))
            {
                await _dbContext.Cities.AddAsync(new City { Name = property.location.cityName, GoogleId = property.location.cityPlaceId });
                _dbContext.SaveChanges();
            }

            if (!string.IsNullOrEmpty(property.location.districtName))
            {
                if (!await _dbContext.Sublocalities.AnyAsync(s => s.GoogleId == property.location.districtPlaceId))
                {
                    await _dbContext.Sublocalities.AddAsync(new Sublocality { Name = property.location.districtName, GoogleId = property.location.districtPlaceId });
                    _dbContext.SaveChanges();
                }
            }

            Property createdProperty = new Property();
            createdProperty.Title = property.Title;
            createdProperty.Description = property.Description;
            createdProperty.Price = (double)property.Price;
            createdProperty.Surface = (float)property.Surface;
            createdProperty.PropertyType = await _dbContext.PropertyTypes.FirstAsync(pt => pt.Type == property.propertyType.ToString());
            createdProperty.RoomsNumber = (int)property.RoomsNumber;
            createdProperty.ConstructionYear = (int)property.ConstructionYear;
            createdProperty.Floor = (int)property.Floor;
            createdProperty.City = await _dbContext.Cities.FirstAsync(c => c.GoogleId == property.location.cityPlaceId);
            createdProperty.Sublocality = !string.IsNullOrEmpty(property.location.districtName) ? await _dbContext.Sublocalities.FirstAsync(s => s.GoogleId == property.location.districtPlaceId) : null;
            createdProperty.CreationDate = DateTime.UtcNow;
            createdProperty.ExpirationDate = DateTime.UtcNow.AddDays(7);
            createdProperty.User = user;
            createdProperty.OfferType = await _dbContext.OfferTypes.FirstAsync(ot => ot.Type == property.offerType.ToString());
            createdProperty.MarketType = await _dbContext.MarketTypes.FirstAsync(mt => mt.Type == property.marketType.ToString());
            createdProperty.PropertyType = await _dbContext.PropertyTypes.FirstAsync(mt => mt.Type == property.propertyType.ToString());
            createdProperty.RoommatesAllowed = property.RoommatesAllowed;
            await _dbContext.AddAsync(createdProperty);
            _dbContext.SaveChanges();
            List<User> usersToNotify = _dbContext.Users.Include(u => u.SavedFilters).Where(u => u.NotifyAboutNewOffers == true && u.SavedFiltersId != null).ToList();
            foreach (User userToNotify in usersToNotify)
            {
                SavedFilter filter = userToNotify.SavedFilters;
                if (
                    createdProperty.PropertyTypeId == filter.PropertyTypeId &&
                    createdProperty.OfferTypeId == filter.OfferTypeId &&
                    createdProperty.MarketTypeId == (filter.MarketTypeId is null ? createdProperty.MarketTypeId : filter.MarketTypeId) &&
                    createdProperty.CityId == (filter.CityId is null ? createdProperty.CityId : filter.CityId) &&
                    createdProperty.RoomsNumber >= (filter.MinRooms ?? 0) &&
                    createdProperty.RoomsNumber <= (filter.MaxRooms ?? int.MaxValue) &&
                    createdProperty.Floor >= (filter.MinFloor ?? 0) &&
                    createdProperty.Floor <= (filter.MaxFloor ?? int.MaxValue) &&
                    createdProperty.Price >= (filter.MinPrice ?? 0) &&
                    createdProperty.Price <= (filter.MaxPrice ?? double.MaxValue) &&
                    createdProperty.Price / Convert.ToDouble(createdProperty.Surface) > (filter.MinMeterPrice ?? 0) &&
                    createdProperty.Price / Convert.ToDouble(createdProperty.Surface) < (filter.MaxMeterPrice ?? double.MaxValue) &&
                    createdProperty.Surface >= (filter.MinSurface ?? 0) &&
                    createdProperty.Surface <= (filter.MaxSurface ?? float.MaxValue) &&
                    createdProperty.ConstructionYear > (filter.MinConstructionYear ?? 0) &&
                    createdProperty.ConstructionYear < (filter.MaxConstructionYear ?? int.MaxValue) &&
                    createdProperty.RoommatesAllowed == (filter.Roommates ?? createdProperty.RoommatesAllowed) &&
                    createdProperty.UserId != userToNotify.Id)
                {
                    _mailService.NotifyUserAboutNewOfferAsync(userToNotify.Email, $"https://frosthousing.ddns.net/propertydetails/{createdProperty.Id}");
                }
            }
            return (true, createdProperty.Id);
        }

        public async Task<bool> DeletePropertyAsync(int offerId, int userId)
        {
            Property property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == offerId);
            if (property == null)
                return false;
            User user = await _dbContext.Users.Include(u => u.Role).FirstOrDefaultAsync(p => p.Id == userId);
            if (property.UserId != userId && user.Role.RoleName != "Admin")
                return false;
            _dbContext.ViewCounts.RemoveRange(_dbContext.ViewCounts.Where(x => x.PropertyId == property.Id));
            _dbContext.PropertyRoommates.RemoveRange(_dbContext.PropertyRoommates.Where(x => x.PropertyId == property.Id));
            _dbContext.FollowedOffers.RemoveRange(_dbContext.FollowedOffers.Where(x => x.PropertyId == property.Id));
            _dbContext.Properties.Remove(property);
            _dbContext.SaveChanges();
            return true;
        }

        public async Task<PropertyStatusCode> FollowOfferAsync(int offerId, int userId)
        {
            Property property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == offerId);
            if (property == null)
                return PropertyStatusCode.PropertyDoesNotExists;
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return PropertyStatusCode.UserDoesNotExists;
            if (await _dbContext.FollowedOffers.AnyAsync(fo => fo.UserId == userId && fo.PropertyId == offerId))
                return PropertyStatusCode.PropertyIsAlreadyFollowed;
            _dbContext.FollowedOffers.Add(new FollowedOffer
            {
                Property = property,
                User = user,
            });
            await _dbContext.SaveChangesAsync();
            return PropertyStatusCode.Success;
        }

        public async Task<(PropertyStatusCode, IEnumerable<PropertyDetailsDTO>)> GetFollowedOffersAsync(int userId)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return (PropertyStatusCode.UserDoesNotExists, null);
            List<Property> followedOffers = _dbContext.Properties
                .Include(p => p.MarketType)
                .Include(p => p.PropertyType)
                .Include(p => p.City)
                .Include(p => p.AdministrativeArea)
                .Include(p => p.Sublocality)
                .Include(p => p.OfferType)
                .Include(p => p.User)
                .Where(p => DateTime.Compare(p.ExpirationDate, DateTime.UtcNow) > 0 && p.IsTemplate == false)
                .Join(_dbContext.FollowedOffers.Where(fo => fo.UserId == user.Id), property => property.Id, followedOffer => followedOffer.PropertyId, (property, followedOffer) => property).ToList();
            List<PropertyDetailsDTO> followedOffersDTO = new List<PropertyDetailsDTO>();
            foreach (Property property in followedOffers)
            {
                followedOffersDTO.Add(ConvertToDTO(property));
            }
            return (PropertyStatusCode.Success, followedOffersDTO);

        }
        public async Task<(PropertyStatusCode, List<PropertyDetailsDTO>)> GetSavedTemplatesAsync(int userId)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return (PropertyStatusCode.UserDoesNotExists, null);
            List<Property> savedTemplates = _dbContext.Properties
                .Where(p => p.UserId == userId && p.IsTemplate == true)
                .Include(p => p.OfferType)
                .Include(p => p.MarketType)
                .Include(p => p.PropertyType)
                .Include(p => p.City)
                .Include(p => p.AdministrativeArea)
                .Include(p => p.Sublocality)
                .Include(p => p.OfferType)
                .ToList();
            List<PropertyDetailsDTO> savedTemplatesDTO = new List<PropertyDetailsDTO>();
            foreach (Property property in savedTemplates)
            {
                savedTemplatesDTO.Add(ConvertToDTO(property));
            }
            return (PropertyStatusCode.Success, savedTemplatesDTO);
        }
        public async Task<(PropertyStatusCode, PropertyDetailsDTO)> SaveTemplateAsync(int offerId, int userId)
        {
            Property property = await _dbContext.Properties
                .Include(p => p.OfferType)
                .Include(p => p.MarketType)
                .Include(p => p.PropertyType)
                .Include(p => p.City)
                .Include(p => p.AdministrativeArea)
                .Include(p => p.Sublocality)
                .Include(p => p.OfferType)
                .FirstOrDefaultAsync(p => p.Id == offerId);
            if (property == null)
                return (PropertyStatusCode.PropertyDoesNotExists, null);
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return (PropertyStatusCode.UserDoesNotExists, null);
            if (property.UserId != userId)
                return (PropertyStatusCode.UserIsNotOwner, null);
            Property createdProperty = new Property();
            createdProperty.Title = property.Title;
            createdProperty.Description = property.Description;
            createdProperty.Price = property.Price;
            createdProperty.Surface = property.Surface;
            createdProperty.PropertyType = property.PropertyType;
            createdProperty.RoomsNumber = property.RoomsNumber;
            createdProperty.ConstructionYear = property.ConstructionYear;
            createdProperty.Floor = property.Floor;
            createdProperty.City = property.City;
            createdProperty.Sublocality = property.Sublocality;
            createdProperty.CreationDate = DateTime.UtcNow;
            createdProperty.ExpirationDate = DateTime.UtcNow.AddDays(7);
            createdProperty.User = user;
            createdProperty.OfferType = property.OfferType;
            createdProperty.MarketType = property.MarketType;
            createdProperty.RoommatesAllowed = property.RoommatesAllowed;
            createdProperty.IsTemplate = true;
            _dbContext.Properties.Add(createdProperty);
            await _dbContext.SaveChangesAsync();
            return (PropertyStatusCode.Success, ConvertToDTO(createdProperty));
        }
        public async Task<PropertyStatusCode> DeleteTemplateAsync(int offerId, int userId)
        {
            Property property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == offerId);
            if (property == null)
                return PropertyStatusCode.TemplateDoesNotExist;
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return PropertyStatusCode.UserDoesNotExists;
            if (property.User.Id != userId)
                return PropertyStatusCode.UserIsNotOwner;
            _dbContext.ViewCounts.RemoveRange(_dbContext.ViewCounts.Where(x => x.PropertyId == property.Id));
            _dbContext.PropertyRoommates.RemoveRange(_dbContext.PropertyRoommates.Where(x => x.PropertyId == property.Id));
            _dbContext.FollowedOffers.RemoveRange(_dbContext.FollowedOffers.Where(x => x.PropertyId == property.Id));
            _dbContext.Properties.Remove(property);
            await _dbContext.SaveChangesAsync();
            return PropertyStatusCode.Success;
        }
        public async Task<(PropertyStatusCode, PropertyDetailsDTO)> PublishPropertyFromTemplateAsync(int offerId, int userId)
        {
            Property property = await _dbContext.Properties
                .Include(p => p.OfferType)
                .Include(p => p.MarketType)
                .Include(p => p.PropertyType)
                .Include(p => p.City)
                .Include(p => p.AdministrativeArea)
                .Include(p => p.Sublocality)
                .Include(p => p.OfferType)
                .FirstOrDefaultAsync(p => p.Id == offerId);
            if (property == null)
                return (PropertyStatusCode.PropertyDoesNotExists, null);
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return (PropertyStatusCode.UserDoesNotExists, null);
            if (property.User.Id != userId)
                return (PropertyStatusCode.UserIsNotOwner, null);
            Property createdProperty = new Property();
            createdProperty.Title = property.Title;
            createdProperty.Description = property.Description;
            createdProperty.Price = property.Price;
            createdProperty.Surface = property.Surface;
            createdProperty.PropertyType = property.PropertyType;
            createdProperty.RoomsNumber = property.RoomsNumber;
            createdProperty.ConstructionYear = property.ConstructionYear;
            createdProperty.Floor = property.Floor;
            createdProperty.City = property.City;
            createdProperty.Sublocality = property.Sublocality;
            createdProperty.CreationDate = DateTime.UtcNow;
            createdProperty.ExpirationDate = DateTime.UtcNow.AddDays(7);
            createdProperty.User = user;
            createdProperty.OfferType = property.OfferType;
            createdProperty.MarketType = property.MarketType;
            createdProperty.RoommatesAllowed = property.RoommatesAllowed;
            createdProperty.IsTemplate = false;
            _dbContext.Properties.Add(createdProperty);
            await _dbContext.SaveChangesAsync();
            return (PropertyStatusCode.Success, ConvertToDTO(createdProperty));
        }

        public List<(string userMail, string offerTitle)> GetOffersAboutToExpire()
        {
            List<(string userMail, string offerTitle)> data = new List<(string userMail, string offerTitle)>();
            List<Property> offers = _dbContext.Properties.Include(p => p.User).Where(p => DateTime.Compare(p.ExpirationDate, DateTime.UtcNow) > 0 && DateTime.Compare(p.ExpirationDate, DateTime.UtcNow.AddDays(3)) < 0 && p.User.NotifyAboutExpiringOffers == true && p.IsTemplate == false).ToList();
            foreach (Property property in offers)
            {
                data.Add((property.User.Email, property.Title));
            }
            return data;
        }

        public async Task<IEnumerable<PropertyDetailsDTO>> GetPromotedPropertiesAsync()
        {
            List<PropertyDetailsDTO> propertiesDTO = new List<PropertyDetailsDTO>();
            List<Property> properties = await _dbContext.Properties
                .Include(p => p.User)
                .Include(p => p.OfferType)
                .Include(p => p.MarketType)
                .Include(p => p.PropertyType)
                .Include(p => p.City)
                .Include(p => p.AdministrativeArea)
                .Include(p => p.Sublocality)
                .Where(p => p.IsTemplate == false && DateTime.Compare(p.ExpirationDate, DateTime.UtcNow) > 0)
                .Take(5).ToListAsync();
            foreach (Property property in properties)
            {
                propertiesDTO.Add(ConvertToDTO(property));
            }
            return propertiesDTO;
        }

        public async Task<PropertyDetailsDTO> GetPropertyDetailsAsync(int offerId)
        {
            Property property = await _dbContext.Properties.Where(p => DateTime.Compare(p.ExpirationDate, DateTime.UtcNow) > 0 && p.IsTemplate == false).Include(p => p.User)
                .Include(p => p.OfferType)
                .Include(p => p.MarketType)
                .Include(p => p.PropertyType)
                .Include(p => p.City)
                .Include(p => p.AdministrativeArea)
                .Include(p => p.Sublocality).FirstOrDefaultAsync(p => p.Id == offerId);
            if (property == null)
                return null;

            FileStatusCode status;
            List<string> propertiesUrl;
            (status, propertiesUrl) = _propertyImageService.GetPropertyImagesUrls(offerId);

            return ConvertToDTO(property, true);
        }

        public async Task<PropertyFormModel>? GetPropertyToEditAsync(int offerId, int userId)
        {
            Property property = await _dbContext.Properties.Include(p => p.User)
                .Include(p => p.OfferType)
                .Include(p => p.MarketType)
                .Include(p => p.PropertyType)
                .Include(p => p.City)
                .Include(p => p.AdministrativeArea)
                .Include(p => p.Sublocality).FirstOrDefaultAsync(p => p.Id == offerId && p.User.Id == userId);
            if (property == null)
                return null;
            PropertyFormModel model = new PropertyFormModel();
            model.Title = property.Title;
            model.Description = property.Description;
            model.Price = property.Price;
            model.Surface = property.Surface;
            model.RoommatesAllowed = property.RoommatesAllowed;
            model.RoomsNumber = property.RoomsNumber;
            model.Floor = property.Floor;
            model.offerType = (OfferTypeEnum)Enum.Parse(typeof(OfferTypeEnum), property.OfferType.Type, true);
            model.marketType = (MarketTypeEnum)Enum.Parse(typeof(MarketTypeEnum), property.MarketType.Type, true);
            model.propertyType = (PropertyTypeEnum)Enum.Parse(typeof(PropertyTypeEnum), property.PropertyType.Type, true);
            model.ConstructionYear = property.ConstructionYear;
            model.location = new Location()
            {
                cityName = property.City.Name,
                cityPlaceId = property.City.GoogleId,
                districtName = property.Sublocality?.Name,
                districtPlaceId = property.Sublocality?.GoogleId
            };
            return model;

        }

        public async Task<IEnumerable<PropertyDetailsDTO>> GetUserPropertiesAsync(string email)
        {
            List<Property> userProperties = await _dbContext.Properties.Where(p => p.User.Email == email && DateTime.Compare(p.ExpirationDate, DateTime.UtcNow) > 0 && p.IsTemplate == false)
                .Include(p => p.User)
                .Include(p => p.OfferType)
                .Include(p => p.MarketType)
                .Include(p => p.PropertyType)
                .Include(p => p.City)
                .Include(p => p.AdministrativeArea)
                .Include(p => p.Sublocality).ToListAsync();
            List<PropertyDetailsDTO> userPropertiesDTO = new List<PropertyDetailsDTO>();
            foreach (var property in userProperties)
            {
                userPropertiesDTO.Add(ConvertToDTO(property));
            }
            return userPropertiesDTO;
        }

        public async Task<bool> isOfferFollowedByUserAsync(int offerId, int userId)
        {
            bool isFollowedByUser = await _dbContext.FollowedOffers.AnyAsync(fo => fo.UserId == userId && fo.PropertyId == offerId);
            return isFollowedByUser;
        }

        public async Task<bool> isUserPropertyAsync(int offerId, int userId)
        {
            bool isUserProperty = await _dbContext.Properties.Where(p => p.Id == offerId && p.UserId == userId).AnyAsync();
            return isUserProperty;
        }

        public async Task<PropertyStatusCode> RemoveUserFromRoommates(int offerId, int userId)
        {
            Property property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == offerId);
            if (property == null)
                return PropertyStatusCode.PropertyDoesNotExists;
            bool userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return PropertyStatusCode.UserDoesNotExists;
            if (!await _dbContext.PropertyRoommates.AnyAsync(fo => fo.UserId == userId && fo.PropertyId == offerId))
                return PropertyStatusCode.UserIsNotRoommate;
            if (!property.RoommatesAllowed)
                return PropertyStatusCode.PropertyDoesNotAllowRoommates;
            PropertyRoommate propertyRoommate = await _dbContext.PropertyRoommates.FirstOrDefaultAsync(fo => fo.UserId == userId && fo.PropertyId == offerId);
            _dbContext.PropertyRoommates.Remove(propertyRoommate);
            await _dbContext.SaveChangesAsync();
            return PropertyStatusCode.Success;
        }

        public async Task<(IEnumerable<PropertyDetailsDTO>, int)> SearchForOffersAsync(
            string propertyType,
            string offerType,
            string? marketType,
            string? cityId,
            int? minRoomsNumber,
            int? maxRoomsNumber,
            int? minFloor,
            int? maxFloor,
            double? minPrice,
            double? maxPrice,
            double? minMeterPrice,
            double? maxMeterPrice,
            float? minSurface,
            float? maxSurface,
            int? minConstructionYear,
            int? maxConstructionYear,
            bool? allowRoommates,
            string? constructionType,
            int recordsToSkip = 0,
            string sortBy = "DateDesc")
        {
            var propertiesEntities = _dbContext.Properties
                .Include(p => p.OfferType)
                .Include(p => p.MarketType)
                .Include(p => p.PropertyType)
                .Include(p => p.City)
                .Include(p => p.AdministrativeArea)
                .Include(p => p.Sublocality)
                .Include(p => p.User)
                .Where(p =>
            p.PropertyTypeId == _dbContext.PropertyTypes.FirstOrDefault(pt => pt.Type == propertyType).Id &&
            p.OfferTypeId == _dbContext.OfferTypes.FirstOrDefault(ot => ot.Type == offerType).Id &&
            p.MarketTypeId == (string.IsNullOrEmpty(marketType) ? p.MarketTypeId : _dbContext.MarketTypes.FirstOrDefault(mt => mt.Type == marketType).Id) &&
            p.CityId == (string.IsNullOrEmpty(cityId) ? p.CityId : _dbContext.Cities.FirstOrDefault(c => c.GoogleId == cityId).Id) &&
            p.RoomsNumber >= (minRoomsNumber ?? 0) &&
            p.RoomsNumber <= (maxRoomsNumber ?? int.MaxValue) &&
            p.Floor >= (minFloor ?? 0) &&
            p.Floor <= (maxFloor ?? int.MaxValue) &&
            p.Price >= (minPrice ?? 0) &&
            p.Price <= (maxPrice ?? double.MaxValue) &&
            p.Price / Convert.ToDouble(p.Surface) > (minMeterPrice ?? 0) &&
            p.Price / Convert.ToDouble(p.Surface) < (maxMeterPrice ?? double.MaxValue) &&
            p.Surface >= (minSurface ?? 0) &&
            p.Surface <= (maxSurface ?? float.MaxValue) &&
            p.ConstructionYear > (minConstructionYear ?? 0) &&
            p.ConstructionYear < (maxConstructionYear ?? int.MaxValue) &&
            p.RoommatesAllowed == (allowRoommates ?? p.RoommatesAllowed) &&
            DateTime.Compare(p.ExpirationDate, DateTime.UtcNow) > 0 &&
            p.IsTemplate == false
            ).Skip(recordsToSkip).Take(10);
            if (sortBy == "DateDesc")
                propertiesEntities = propertiesEntities.OrderByDescending(p => p.CreationDate);
            if (sortBy == "DateAsc")
                propertiesEntities = propertiesEntities.OrderBy(p => p.CreationDate);
            if (sortBy == "PriceDesc")
                propertiesEntities = propertiesEntities.OrderByDescending((p) => p.Price);
            if (sortBy == "PriceAsc")
                propertiesEntities = propertiesEntities.OrderBy((p) => p.Price);
            if (sortBy == "SurfaceDesc")
                propertiesEntities = propertiesEntities.OrderByDescending(p => p.Surface);
            if (sortBy == "SurfaceAsc")
                propertiesEntities = propertiesEntities.OrderBy(p => p.Surface);
            List<Property> properties = propertiesEntities.ToList();

            int remainingRecords = _dbContext.Properties
                .Include(p => p.OfferType)
                .Include(p => p.MarketType)
                .Include(p => p.PropertyType)
                .Include(p => p.City)
                .Include(p => p.AdministrativeArea)
                .Include(p => p.Sublocality)
                .Include(p => p.User)
                .Where(p =>
            p.PropertyTypeId == _dbContext.PropertyTypes.FirstOrDefault(pt => pt.Type == propertyType).Id &&
            p.OfferTypeId == _dbContext.OfferTypes.FirstOrDefault(ot => ot.Type == offerType).Id &&
            p.MarketTypeId == (string.IsNullOrEmpty(marketType) ? p.MarketTypeId : _dbContext.MarketTypes.FirstOrDefault(mt => mt.Type == marketType).Id) &&
            p.CityId == (string.IsNullOrEmpty(cityId) ? p.CityId : _dbContext.Cities.FirstOrDefault(c => c.GoogleId == cityId).Id) &&
            p.RoomsNumber >= (minRoomsNumber ?? 0) &&
            p.RoomsNumber <= (maxRoomsNumber ?? int.MaxValue) &&
            p.Floor >= (minFloor ?? 0) &&
            p.Floor <= (maxFloor ?? int.MaxValue) &&
            p.Price >= (minPrice ?? 0) &&
            p.Price <= (maxPrice ?? double.MaxValue) &&
            p.Price / Convert.ToDouble(p.Surface) > (minMeterPrice ?? 0) &&
            p.Price / Convert.ToDouble(p.Surface) < (maxMeterPrice ?? double.MaxValue) &&
            p.Surface >= (minSurface ?? 0) &&
            p.Surface <= (maxSurface ?? float.MaxValue) &&
            p.ConstructionYear > (minConstructionYear ?? 0) &&
            p.ConstructionYear < (maxConstructionYear ?? int.MaxValue) &&
            p.RoommatesAllowed == (allowRoommates ?? p.RoommatesAllowed)
            ).Skip(recordsToSkip + 10).Count();

            List<PropertyDetailsDTO> filteredProperties = new List<PropertyDetailsDTO>();
            foreach (Property property in propertiesEntities)
            {
                filteredProperties.Add(ConvertToDTO(property));
            }
            return (filteredProperties, remainingRecords);
        }

        public async Task<PropertyStatusCode> UnfollowOfferAsync(int offerId, int userId)
        {
            Property property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == offerId);
            if (property == null)
                return PropertyStatusCode.PropertyDoesNotExists;
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return PropertyStatusCode.UserDoesNotExists;
            if (!await _dbContext.FollowedOffers.AnyAsync(fo => fo.UserId == userId && fo.PropertyId == offerId))
                return PropertyStatusCode.PropertyIsNotFollowed;
            FollowedOffer followedOffer = await _dbContext.FollowedOffers.FirstAsync(fo => fo.UserId == userId && property.Id == offerId);
            _dbContext.FollowedOffers.Remove(followedOffer);
            await _dbContext.SaveChangesAsync();
            return PropertyStatusCode.Success;
        }


        public async Task<(bool, PropertyDetailsDTO)> UpdatePropertyAsync(PropertyFormModel propertyToUpdate, string email, int offerId)
        {
            Property userProperty = await _dbContext.Properties.Include(p => p.User)
                .Include(p => p.OfferType)
                .Include(p => p.MarketType)
                .Include(p => p.PropertyType)
                .Include(p => p.City)
                .Include(p => p.AdministrativeArea)
                .Include(p => p.Sublocality)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == offerId && p.User.Email == email);
            if (propertyToUpdate == null)
                return (false, null);

            if (!await _dbContext.Cities.AnyAsync(c => c.GoogleId == propertyToUpdate.location.cityPlaceId))
            {
                await _dbContext.Cities.AddAsync(new City { Name = propertyToUpdate.location.cityName, GoogleId = propertyToUpdate.location.cityPlaceId });
                _dbContext.SaveChanges();
            }

            if (!string.IsNullOrEmpty(propertyToUpdate.location.districtName))
            {
                if (!await _dbContext.Sublocalities.AnyAsync(s => s.GoogleId == propertyToUpdate.location.districtPlaceId))
                {
                    await _dbContext.Sublocalities.AddAsync(new Sublocality { Name = propertyToUpdate.location.districtName, GoogleId = propertyToUpdate.location.districtPlaceId });
                    _dbContext.SaveChanges();
                }
            }

            userProperty.Title = propertyToUpdate.Title;
            userProperty.Description = propertyToUpdate.Description;
            userProperty.Price = (double)propertyToUpdate.Price;
            userProperty.Surface = (float)propertyToUpdate.Surface;
            userProperty.PropertyType = await _dbContext.PropertyTypes.FirstAsync(pt => pt.Type == propertyToUpdate.propertyType.ToString());
            userProperty.RoomsNumber = (int)propertyToUpdate.RoomsNumber;
            userProperty.ConstructionYear = (int)propertyToUpdate.ConstructionYear;
            userProperty.Floor = (int)propertyToUpdate.Floor;
            userProperty.City = await _dbContext.Cities.FirstAsync(c => c.GoogleId == propertyToUpdate.location.cityPlaceId);
            userProperty.Sublocality = !string.IsNullOrEmpty(propertyToUpdate.location.districtName) ? await _dbContext.Sublocalities.FirstAsync(s => s.GoogleId == propertyToUpdate.location.districtPlaceId) : null;
            userProperty.ExpirationDate = DateTime.UtcNow.AddDays(7);
            userProperty.OfferType = await _dbContext.OfferTypes.FirstAsync(ot => ot.Type == propertyToUpdate.offerType.ToString());
            userProperty.MarketType = await _dbContext.MarketTypes.FirstAsync(mt => mt.Type == propertyToUpdate.marketType.ToString());
            userProperty.PropertyType = await _dbContext.PropertyTypes.FirstAsync(mt => mt.Type == propertyToUpdate.propertyType.ToString());
            userProperty.RoommatesAllowed = propertyToUpdate.RoommatesAllowed;
            await _dbContext.SaveChangesAsync();
            return (true, ConvertToDTO(userProperty));

        }
        private PropertyDetailsDTO ConvertToDTO(Property property, bool getAveragePrice = false)
        {

            FileStatusCode status;
            List<string> propertiesUrl;
            (status, propertiesUrl) = _propertyImageService.GetPropertyImagesUrls(property.Id);

            PropertyDetailsDTO propertyDetailsDTO = new PropertyDetailsDTO();
            propertyDetailsDTO.OfferId = property.Id;
            propertyDetailsDTO.Title = property.Title;
            propertyDetailsDTO.Description = property.Description;
            propertyDetailsDTO.Surface = property.Surface.ToString();
            propertyDetailsDTO.Price = property.Price.ToString();
            propertyDetailsDTO.RoommatesAllowed = property.RoommatesAllowed;
            propertyDetailsDTO.RoomsNumber = property.RoomsNumber.ToString();
            propertyDetailsDTO.Floor = property.Floor.ToString();
            propertyDetailsDTO.offerType = (OfferTypeEnum)Enum.Parse(typeof(OfferTypeEnum), property.OfferType.Type, true);
            propertyDetailsDTO.marketType = (MarketTypeEnum)Enum.Parse(typeof(MarketTypeEnum), property.MarketType.Type, true);
            propertyDetailsDTO.propertyType = (PropertyTypeEnum)Enum.Parse(typeof(PropertyTypeEnum), property.PropertyType.Type, true);
            propertyDetailsDTO.ConstructionYear = property.ConstructionYear.ToString();
            propertyDetailsDTO.propertyImagesUrls = status == FileStatusCode.Success ? propertiesUrl : new List<string>();
            propertyDetailsDTO.location = new Location()
            {
                cityName = property.City.Name,
                cityPlaceId = property.City.GoogleId,
                districtName = property.Sublocality?.Name,
                districtPlaceId = property.Sublocality?.GoogleId
            };
            propertyDetailsDTO.user = new UserDTO()
            {
                userId = property.UserId,
                name = property.User.Name,
                email = property.User.Email,
                telNumber = property.User.TelNumber.ToString(),
                description = property.User.Description,
                profileImgUrl = _userImageService.GetUserProfileImageUrl(property.UserId),
            };
            List<UserDTO> roommatesDTO = new List<UserDTO>();
            if (property.RoommatesAllowed)
            {
                List<User> roommates = _dbContext.Users.Join(_dbContext.PropertyRoommates.Where(pr => pr.PropertyId == property.Id), user => user.Id, propertyRoomate => propertyRoomate.UserId, (user, propertyRoomate) => user).ToList();
                foreach (User roommate in roommates)
                {
                    roommatesDTO.Add(new UserDTO
                    {
                        userId = roommate.Id,
                        email = roommate.Email,
                        name = roommate.Name,
                        telNumber = roommate.TelNumber.ToString(),
                        description = roommate.Description,
                        city = roommate.City?.Name,
                        cityPlaceId = roommate.CityId.ToString(),
                        profileImgUrl = _userImageService.GetUserProfileImageUrl(roommate.Id)
                    });
                }
            }
            propertyDetailsDTO.RoommatesList = roommatesDTO;
            propertyDetailsDTO.CreationDate = property.CreationDate;
            propertyDetailsDTO.ExpirationDate = property.ExpirationDate;
            if (getAveragePrice)
            {
                List<double> prices = _dbContext.Properties.Where(
                                p => p.CityId == property.CityId &&
                                p.Surface > property.Surface - property.Surface * 0.1 &&
                                p.Surface < property.Surface + property.Surface * 0.1 &&
                                p.OfferTypeId == property.OfferTypeId &&
                                p.Id != property.Id
                                ).Select(p => p.Price).ToList();
                if (prices.Count > 0)
                {
                    double priceAverage = 0;
                    foreach (double price in prices)
                    {
                        priceAverage += price;
                    }
                    priceAverage /= prices.Count;
                    if (priceAverage > property.Price)
                    {
                        propertyDetailsDTO.PriceLowerThanAverage = true;
                        propertyDetailsDTO.PercentageDiff = (priceAverage - property.Price) / priceAverage * 100;
                    }
                    else if (priceAverage < property.Price)
                    {
                        propertyDetailsDTO.PriceLowerThanAverage = false;
                        propertyDetailsDTO.PercentageDiff = (priceAverage - property.Price) / priceAverage * 100;
                    }
                }
            }
            return propertyDetailsDTO;
        }

        public async Task AddViewToPropertyAsync(int offerId, int userId)
        {
            if (!await _dbContext.Properties.AnyAsync(x => x.Id == offerId))
                return;
            if (!await _dbContext.Users.AnyAsync(x => x.Id == userId))
                return;
            if (await _dbContext.ViewCounts.AnyAsync(x => x.PropertyId == offerId && x.UserId == userId))
                return;
            _dbContext.ViewCounts.Add(new ViewCount { PropertyId = offerId, UserId = userId });
            await _dbContext.SaveChangesAsync();
            return;
        }

        public async Task<int> GetPropertyViewsCountAsync(int offerId)
        {
            return await _dbContext.ViewCounts.Where(x => x.PropertyId == offerId).CountAsync();
        }

        public async Task<int> GetPropertyFollowCountAsync(int offerId)
        {
            return await _dbContext.FollowedOffers.Where(x => x.PropertyId == offerId).CountAsync();
        }
    }
}
