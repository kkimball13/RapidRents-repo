using System.Collections.Generic;
using Rapd.Web.Domain.Address;
using Rapd.Web.Models.Requests.Addresses;
using System.Data;

namespace Rapd.Web.Services.Addresses
{
    public interface IAddressService
    {
        int Insert(AddressAddRequest model, string userId);
        void Update(AddressUpdateRequest model);
        void DeleteById(int id);
        void AddressToProperty(int propId, int addId);
        List<Address> GetAll();
        List<Address> GetByAmenity(int id);
        List<Address> GetByGeo(decimal lat, decimal lng, int radius);
        List<AddressWithListings> AddressesByLiId(int listingId);
        List<Address> PLW_Search(string search_item);
        List<Address> AWOP_Search(string search_item);
        Address GetById(int id);
        Address GetByAddress(int id);
        Address MapAddress(IDataReader reader, int actualStartingIndex = 0);

        PagedList<Address> GetAllMapListings(int pageNumber, int pageSize);
        
        void UpdateLongLat(AdressUpdateLongLatRequest model);
        T MapAddressListings<T>(IDataReader reader, int actualStartingIndex = 0) where T : ListingsAddress, new();
        List<Address> RentCheck(RentCheckRequest model);
        List<Address> GetAreaRent(RentCheckRequest model);
        RentCheck RentChecker(RentCheckRequest model);
    }
}
