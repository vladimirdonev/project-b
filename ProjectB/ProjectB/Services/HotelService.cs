using ProjectB.Validators;

namespace ProjectB.Services;

public class HotelService : IHotelService
{
    private IHotelClients _hotelClients;
    private IMapper _mapper;
    private readonly ICacheFilter<HotelOverview> _hotelOverviewCache;
    private readonly IValidator _validator;

    public HotelService(IHotelClients hotelClients, IMapper mapper, 
        ICacheFilter<HotelOverview> hotelOverviewCache, IValidator validator)
    {
        _hotelClients = hotelClients;
        _mapper = mapper;
        _hotelOverviewCache = hotelOverviewCache;
        _validator = validator;
    }

    public async Task<ICollection<HotelsViewModel>> GetDestinationIdAsync(string cityName)
    {
        var destination = await _hotelClients.GetDestination(cityName);


        var destinationId = int.MinValue;
        foreach (var item in destination.Suggestions)
        {
            foreach (var number in item.CityProperties)
            {
                destinationId = int.Parse(number.DestinationId);
                break;
            }
            break;
        }

        if (!_validator.ValidateDestination(destinationId))
        {
            throw new ArgumentException($"Wrong destination please try again");
        }
        

        return await GetHotelsByDestinationIdAsync(destinationId);
    }

    public async Task<ICollection<HotelsViewModel>> GetHotelsByDestinationIdAsync(int id)
    {
        var hotels = await this._hotelClients.GetHotels(id);
        var hotelsViewModel = new List<HotelsViewModel>();

        foreach (var item in hotels.Data.Body.SearchResults.Results)
        {
            var hotel = new HotelsViewModel();
            hotel = _mapper.Map<HotelByCity, HotelsViewModel>(item);
            hotelsViewModel.Add(hotel);
        }

        return hotelsViewModel;
    }

    public async Task<HotelViewModel> GetHotelDetailsById(int id, string checkIn, string checkOut)
    {
        var cacheKey = $"{id}_{checkIn}_{checkOut}";
        var hotelDetails = _hotelOverviewCache.Get(cacheKey);
        var Hotel = new HotelViewModel();

        if (hotelDetails == null)
        {
            hotelDetails = await _hotelClients.GetHotel(id, checkIn, checkOut);
            _hotelOverviewCache.Set(cacheKey, hotelDetails);
        }
        Hotel = _mapper.Map(hotelDetails.HotelDetails.Hotel, Hotel);
        var services = hotelDetails.HotelDetails.Hotel.Amenities
                .SelectMany(x => x.HotelService.Where(x => x.Heading == "Services")).ToArray();
        foreach (var item in services)
        {
            Hotel.HotelService = item.ServiceDescription;
        }
        return Hotel;
    }
}
