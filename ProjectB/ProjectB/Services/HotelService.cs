﻿using AutoMapper;
using ProjectB.Clients;
using ProjectB.Clients.Models.HotelDetails;
using ProjectB.Clients.Models.Hotels;
using ProjectB.Infrastructure;
using ProjectB.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ProjectB.Services
{
    public class HotelService : IHotelService
    {
        private IHotelClients hotelClients;
        private IMapper mapper;
        private readonly ICacheFilter<HotelOverview> _hotelOverviewCache;

        public HotelService(IHotelClients hotelClients, IMapper mapper, ICacheFilter<HotelOverview> hotelOverviewCache)
        {
            this.hotelClients = hotelClients;
            this.mapper = mapper;
            _hotelOverviewCache = hotelOverviewCache;
        }

        public async Task<int> GetDestinationIdAsync(string cityName)
        {
            var destination = await this.hotelClients.GetDestination(cityName);


            var destinationId = 0;
            foreach (var item in destination.Suggestions)
            {
                foreach (var number in item.CityProperties)
                {
                    destinationId = int.Parse(number.DestinationId);
                    break;
                }
                break;
            }

            return destinationId;
        }

        public async Task<ICollection<HotelsViewModel>> GetHotelsByDestinationIdAsync(int id)
        {
            var hotels = await this.hotelClients.GetHotels(id);
            var hotelsViewModel = new List<HotelsViewModel>();

            foreach (var item in hotels.Data.Body.SearchResults.Results)
            {
                var hotel = new HotelsViewModel();
                hotel = this.mapper.Map<HotelByCity, HotelsViewModel>(item);
                hotelsViewModel.Add(hotel);
            }

            return hotelsViewModel;
        }

        public async Task<HotelOverview> GetHotelDetailsById(int id, string checkIn, string checkOut)
        {
            var cacheKey = $"{id}_{checkIn}_{checkOut}";
            var hotelDetails = _hotelOverviewCache.Get(cacheKey);

            if (hotelDetails == null)
            {
                hotelDetails = await hotelClients.GetHotel(id, checkIn, checkOut);
                _hotelOverviewCache.Set(cacheKey, hotelDetails);
            }

            return hotelDetails;
        }
    }
}
