using Frost.Client.Shared.Components;
using Frost.Server.EntityModels;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Frost.Server.Repositories
{
    public interface IFiltersRepository
    {
        public Task<PropertyFiltersDTO> GetUserFiltersAsync(int userId);
        public Task<bool> CreateUserFiltersAsync(PropertyFiltersDTO filtersDTO, int userId);
        public Task DeleteUserFilters(int userId);
    }
    public class FiltersRepository : IFiltersRepository
    {
        private FrostDbContext _dbContext;
        public FiltersRepository(FrostDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PropertyFiltersDTO> GetUserFiltersAsync(int userId)
        {
            SavedFilter filters = await _dbContext.SavedFilters.
                Include(f => f.MarketType)
                .Include(f => f.OfferType)
                .Include(f => f.PropertyType)
                .Include(f => f.City)
                .FirstOrDefaultAsync(sv => sv.Id == _dbContext.Users.FirstOrDefault(u => u.Id == userId).SavedFiltersId);
            return ConvertToDto(filters);

        }
        public async Task<bool> CreateUserFiltersAsync(PropertyFiltersDTO filtersDTO, int userId)
        {
            bool userExists = _dbContext.Users.Any(u => u.Id == userId);
            if (!userExists)
                return false;
            SavedFilter filters = new SavedFilter();
            filters.MinPrice = filtersDTO.minPrice;
            filters.MaxPrice = filtersDTO.maxPrice;
            filters.MinSurface = filtersDTO.minSurface;
            filters.MaxSurface = filtersDTO.maxSurface;
            filters.MinMeterPrice = filtersDTO.minMeterPrice;
            filters.MaxMeterPrice = filtersDTO.maxMeterPrice;
            filters.MinFloor = filtersDTO.minFloor;
            filters.MaxFloor = filtersDTO.maxFloor;
            filters.MinRooms = filtersDTO.minRoomsNumber;
            filters.MaxRooms = filtersDTO.maxRoomsNumber;
            filters.MinConstructionYear = filtersDTO.minConstructionYear;
            filters.MaxConstructionYear = filtersDTO.maxConstructionYear;
            if (filtersDTO.roommates != RoommatesEnum.Any)
            {
                if (filtersDTO.roommates == RoommatesEnum.Included)
                    filters.Roommates = true;
                if (filtersDTO.roommates == RoommatesEnum.Excluded)
                    filters.Roommates = false;
            }
            string test = filtersDTO.offerType.ToString();
            filters.CityId = _dbContext.Cities.FirstOrDefault(c => c.GoogleId == filtersDTO.cityPlaceId)?.Id;
            if (filtersDTO.marketType != null)
                filters.MarketTypeId = _dbContext.MarketTypes.FirstOrDefault(mt => mt.Type == filtersDTO.marketType.ToString()).Id;
            filters.OfferTypeId = _dbContext.OfferTypes.FirstOrDefault(ot => ot.Type == filtersDTO.offerType.ToString()).Id;
            filters.PropertyTypeId = _dbContext.PropertyTypes.FirstOrDefault(pt => pt.Type == filtersDTO.propertyType.ToString()).Id;
            _dbContext.Users.First(u => u.Id == userId).SavedFilters = filters;
            await _dbContext.SaveChangesAsync();
            return true;

        }
        public async Task DeleteUserFilters(int userId)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return;
            SavedFilter filters = await _dbContext.SavedFilters.FirstOrDefaultAsync(sv => sv.Id == _dbContext.Users.FirstOrDefault(u => u.Id == userId).SavedFiltersId);
            if (filters == null) return;
            user.SavedFiltersId = null;
            _dbContext.SavedFilters.Remove(filters);

            await _dbContext.SaveChangesAsync();
        }
        private PropertyFiltersDTO ConvertToDto(SavedFilter? filter)
        {
            if (filter is null)
                return null;

            PropertyFiltersDTO propertyFiltersDTO = new PropertyFiltersDTO();
            propertyFiltersDTO.minPrice = filter?.MinPrice;
            propertyFiltersDTO.maxPrice = filter?.MaxPrice;
            propertyFiltersDTO.minSurface = filter?.MinSurface;
            propertyFiltersDTO.maxSurface = filter?.MaxSurface;
            propertyFiltersDTO.minMeterPrice = filter?.MinMeterPrice;
            propertyFiltersDTO.maxMeterPrice = filter?.MaxMeterPrice;
            propertyFiltersDTO.minFloor = filter?.MinFloor;
            propertyFiltersDTO.maxFloor = filter?.MaxFloor;
            propertyFiltersDTO.minRoomsNumber = filter?.MinRooms;
            propertyFiltersDTO.maxRoomsNumber = filter?.MaxRooms;
            propertyFiltersDTO.minConstructionYear = filter?.MinConstructionYear;
            propertyFiltersDTO.maxConstructionYear = filter?.MaxConstructionYear;
            propertyFiltersDTO.offerType = filter is null ? OfferTypeEnum.Sell : (OfferTypeEnum)Enum.Parse(typeof(OfferTypeEnum), filter.OfferType.Type, true);
            propertyFiltersDTO.marketType = filter is null || filter.MarketType is null ? null : (MarketTypeEnum)Enum.Parse(typeof(MarketTypeEnum), filter.MarketType.Type, true);
            propertyFiltersDTO.propertyType = filter is null ? PropertyTypeEnum.Flat : (PropertyTypeEnum)Enum.Parse(typeof(PropertyTypeEnum), filter.PropertyType.Type, true);
            if (filter.Roommates == null)
            {
                propertyFiltersDTO.roommates = RoommatesEnum.Any;
            }
            else if (filter.Roommates == true)
            {
                propertyFiltersDTO.roommates = RoommatesEnum.Included;
            }
            else if (filter.Roommates == false)
            {
                propertyFiltersDTO.roommates = RoommatesEnum.Excluded;
            }
            propertyFiltersDTO.cityName = filter?.City?.Name ?? null;
            propertyFiltersDTO.cityPlaceId = filter?.City?.GoogleId ?? null;

            return propertyFiltersDTO;
        }
    }
}
