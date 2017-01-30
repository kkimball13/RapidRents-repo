using Rapd.Web.Domain.Address;
using Rapd.Web.Models.Requests;
using Rapd.Web.Models.Requests.Addresses;
using Rapd.Web.Models.Responses;
using Rapd.Web.Services;
using Rapd.Web.Services.Addresses;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Rapd.Web.Controllers.Api
{
    [RoutePrefix("api/addresses")]
    public class AddressesApiController : ApiController
    {
        private IAddressService _service;
        public IUserService _userService = null;

        public AddressesApiController(IAddressService svc, IUserService UserService)
        {
            _service = svc;
            _userService = UserService;
        }

        [Route, HttpPost]
        public HttpResponseMessage Add(AddressAddRequest model)
        {
            string userId = _userService.GetCurrentUserId();

            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<int> response = new ItemResponse<int>();

            response.Item = _service.Insert(model, userId);

            return Request.CreateResponse(response);
        }

        [Route("{id:int}"), HttpPut]
        public HttpResponseMessage Update(AddressUpdateRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            _service.Update(model);
            SuccessResponse response = new SuccessResponse();

            return Request.CreateResponse(response);
        }

        [Route, HttpGet]
        public HttpResponseMessage Get()
        {
            ItemsResponse<Address> response = new ItemsResponse<Address>();

            response.Items = _service.GetAll();

            return Request.CreateResponse(response);
        }

        [Route("{id:int}"), HttpGet]
        public HttpResponseMessage GetByID(int id)
        {
            ItemResponse<Address> response = new ItemResponse<Address>();

            response.Item = _service.GetById(id);

            return Request.CreateResponse(response);

        }

        [Route("{id:int}"), HttpDelete]
        public HttpResponseMessage DeleteByID(int id)
        {
            _service.DeleteById(id);

            SuccessResponse response = new SuccessResponse();

            return Request.CreateResponse(response);

        }

        [Route("{id:int}/amenity"), HttpGet]
        public HttpResponseMessage GetByAmenity(int id)
        {
            ItemsResponse<Address> response = new ItemsResponse<Address>();

            response.Items = _service.GetByAmenity(id);

            return Request.CreateResponse(response);

        }

        [Route("{id:int}/AmenitiesByAddress"), HttpGet]
        public HttpResponseMessage GetByAddress(int id)
        {
            ItemResponse<Address> response = new ItemResponse<Address>();

            response.Item = _service.GetByAddress(id);

            return Request.CreateResponse(response);
        }

        [Route("radius"), HttpGet]
        public HttpResponseMessage GetByGeo(decimal lat, decimal lng, int radius)
        {
            ItemsResponse<Address> response = new ItemsResponse<Address>();

            response.Items = _service.GetByGeo(lat, lng, radius);

            return Request.CreateResponse(response);
        }

        [Route("findNearId/{listingId:int}"), HttpGet]
        public HttpResponseMessage AddressesNearListingId(int listingId)
        {
            ItemsResponse<AddressWithListings> response = new ItemsResponse<AddressWithListings>();

            response.Items = _service.AddressesByLiId(listingId);

            return Request.CreateResponse(response);
        }

        [Route("Search/{search_item}"), HttpGet]
        public HttpResponseMessage GetSearchAddressList(string search_item)
        {
            ItemsResponse<Address> response = new ItemsResponse<Address>();

            response.Items = _service.PLW_Search(search_item);

            return Request.CreateResponse(response);
        }

        [Route("maplist/{PageIndex:int}/{PageSize:int}"), HttpGet]
        public HttpResponseMessage Get(int PageIndex, int PageSize)
        {
            ItemResponse<PagedList<Address>> response = new ItemResponse<PagedList<Address>>();

            response.Item = _service.GetAllMapListings(PageIndex, PageSize);

            return Request.CreateResponse(response);
        }

        [Route("addPropSearch/{search_item}"), HttpGet]
        public HttpResponseMessage GetAddressSansProperty(string search_item)
        {
            ItemsResponse<Address> response = new ItemsResponse<Address>();

            response.Items = _service.AWOP_Search(search_item);

            return Request.CreateResponse(response);
        }

        [Route("{propId:int}/{addressId:int}"), HttpPost]
        public HttpResponseMessage AddAddressToProperty(int propId, int addressId)
        {
           
             _service.AddressToProperty(propId, addressId);

            SuccessResponse response = new SuccessResponse();

            return Request.CreateResponse(response);
        }
     
        [Route("LongLat/{id:int}"), HttpPut]        
        public HttpResponseMessage Update(AdressUpdateLongLatRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }     
            SuccessResponse response = new SuccessResponse();

            _service.UpdateLongLat(model);

            return Request.CreateResponse(response);
        }

        [Route("RentCheck"),HttpPost]
        public HttpResponseMessage RentCheck(RentCheckRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            ItemsResponse<Address> response = new ItemsResponse<Address>();

            response.Items = _service.RentCheck(model);

            return Request.CreateResponse(response);
        }

        [Route("RentCheckArea"), HttpPost]
        public HttpResponseMessage GetAreaRent(RentCheckRequest model)
        {
            ItemsResponse<Address> response = new ItemsResponse<Address>();

            response.Items = _service.GetAreaRent(model);

            return Request.CreateResponse(response);
        }

        [Route("RentChecker"), HttpPost]
        public HttpResponseMessage GetAllRents(RentCheckRequest model)
        {
            ItemResponse<RentCheck> response = new ItemResponse<RentCheck>();

            response.Item = _service.RentChecker(model);

            return Request.CreateResponse(response);
        }
    }
}
