using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using Sabio.Data;
using Sabio.Web.Models.Requests.Addresses;
using Sabio.Web.Domain;
using Sabio.Web.Domain.Address;
using Sabio.Web.Models.Requests;
using Sabio.Web.Domain.Listings;

namespace Sabio.Web.Services.Addresses
{
    public class AddressService : BaseService, IAddressService
    {
        public int Insert(AddressAddRequest model, string userId)
        {
            int uid = 0;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Address_Insert_V3"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@UserId", userId);
                   paramCollection.AddWithValue("@Line1", model.Line1);
                   paramCollection.AddWithValue("@Line2", model.Line2);
                   paramCollection.AddWithValue("@City", model.City);
                   paramCollection.AddWithValue("@State", model.State);
                   paramCollection.AddWithValue("@ZipCode", model.ZipCode);
                   paramCollection.AddWithValue("@Latitude", model.Latitude);
                   paramCollection.AddWithValue("@Longitude", model.Longitude);
                   SqlParameter amenityIds = new SqlParameter("@AmenityIds", SqlDbType.Structured);
                   if (model.Amenities != null && model.Amenities.Any())
                   {
                       amenityIds.Value = new IntIdTable(model.Amenities.Select(a => a.Id));

                   }
                   paramCollection.Add(amenityIds);

                   SqlParameter p = new SqlParameter("@Id", SqlDbType.Int);
                   p.Direction = ParameterDirection.Output;

                   paramCollection.Add(p);

               }, returnParameters: delegate (SqlParameterCollection param)
               {
                   int.TryParse(param["@Id"].Value.ToString(), out uid);
               });
            return uid;
        }

        public void Update(AddressUpdateRequest model)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Address_Update_V2"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Line1", model.Line1);
                   paramCollection.AddWithValue("@Line2", model.Line2);
                   paramCollection.AddWithValue("@City", model.City);
                   paramCollection.AddWithValue("@State", model.State);
                   paramCollection.AddWithValue("@ZipCode", model.ZipCode);
                   paramCollection.AddWithValue("@Latitude", model.Latitude);
                   paramCollection.AddWithValue("@Longitude", model.Longitude);
                   paramCollection.AddWithValue("@Id", model.Id);
                   SqlParameter amenityIds = new SqlParameter("@AmenityIds", SqlDbType.Structured);

                   if (model.Amenities != null && model.Amenities.Any())
                   {
                       amenityIds.Value = new IntIdTable(model.Amenities.Select(a => a.Id));
                   }
                   paramCollection.Add(amenityIds);
               });
        }

        public Amenity MapAmenities(IDataReader reader)
        {
            Amenity p = new Amenity();
            int startingIndex = 0; 

            p.Id = reader.GetSafeInt32(startingIndex++);
            p.UserId = reader.GetSafeString(startingIndex++);
            p.AmenityName = reader.GetSafeString(startingIndex++);
            p.TypeId = reader.GetSafeBool(startingIndex++);
            p.Description = reader.GetSafeString(startingIndex++);
            p.DateAdded = reader.GetSafeDateTime(startingIndex++);
            p.DateModified = reader.GetSafeDateTime(startingIndex++);

            return p;
        }

        public List<Address> GetAll()
        {
            List<Address> list = null;

            Dictionary<int, List<Amenity>> dict = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Address_Select"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {

               }
               , map: delegate (IDataReader reader, short set)
               {
                   switch (set)
                   {
                       case 0:
                           {
                               Address p = MapAddress(reader);

                               if (list == null)
                               {
                                   list = new List<Address>();
                               }

                               list.Add(p);
                               break;
                           }

                       case 1:
                           {
                               Amenity amenity = new Amenity();
                               int startingIndex = 0;

                               amenity.AmenityName = reader.GetSafeString(startingIndex++);
                               amenity.Description = reader.GetSafeString(startingIndex++);
                               amenity.Id = reader.GetSafeInt32(startingIndex++);
                               amenity.AddressId = reader.GetSafeInt32(startingIndex++);

                               if (dict == null)
                               {
                                   dict = new Dictionary<int, List<Amenity>>();
                               }

                               if (!dict.ContainsKey(amenity.AddressId))
                               {
                                   dict[amenity.AddressId] = new List<Amenity>();
                               }

                               dict[amenity.AddressId].Add(amenity);

                               break;
                           }
                   }
               });

            foreach (Address currentAddress in list)
            {
                if (dict.ContainsKey(currentAddress.Id))
                {
                    currentAddress.Amenities = dict[currentAddress.Id];
                }
            }
            return list;
        }

        public Address MapAddress(IDataReader reader, int actualStartingIndex = 0)
        {
            Address p = new Address();
            int startingIndex = actualStartingIndex;

            p.Id = reader.GetSafeInt32(startingIndex++);
            p.UserId = reader.GetSafeString(startingIndex++);
            p.Line1 = reader.GetSafeString(startingIndex++);
            p.Line2 = reader.GetSafeString(startingIndex++);
            p.City = reader.GetSafeString(startingIndex++);
            p.State = reader.GetSafeInt32(startingIndex++);
            p.ZipCode = reader.GetSafeString(startingIndex++);
            p.DateAdded = reader.GetSafeDateTime(startingIndex++);
            p.DateModified = reader.GetSafeDateTime(startingIndex++);
            p.Latitude = reader.GetSafeDecimal(startingIndex++);
            p.Longitude = reader.GetSafeDecimal(startingIndex++);
            p.RentCost = reader.GetSafeInt32(startingIndex++);


            return p;
        }

        public T MapAddressListings<T>(IDataReader reader, int actualStartingIndex = 0) where T : ListingsAddress, new()
        {
            T p = new T();
            int startingIndex = actualStartingIndex;

            p.Id = reader.GetSafeInt32(startingIndex++);
            p.UserId = reader.GetSafeString(startingIndex++);
            p.Line1 = reader.GetSafeString(startingIndex++);
            p.Line2 = reader.GetSafeString(startingIndex++);
            p.City = reader.GetSafeString(startingIndex++);
            p.State = reader.GetSafeInt32(startingIndex++);
            p.ZipCode = reader.GetSafeString(startingIndex++);
            p.DateAdded = reader.GetSafeDateTime(startingIndex++);
            p.DateModified = reader.GetSafeDateTime(startingIndex++);
            p.Latitude = reader.GetSafeDecimal(startingIndex++);
            p.Longitude = reader.GetSafeDecimal(startingIndex++);

            return p;
        }

        public Address GetById(int id)
        {
            Address singleAddress = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Address_Select_By_Id"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Id", id);
               }
               , map: delegate (IDataReader reader, short set)
               {
                   singleAddress = MapAddress(reader);

                   if (singleAddress == null)
                   {
                       singleAddress = new Address();
                   }
               });
            return singleAddress;
        }

        public void DeleteById(int id)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Address_Delete_By_Id_V2"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Id", id);
               });
        }

        public Address GetByAddress(int addressId)
        {
            Address p = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Amenities_By_AddressV2"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@AddressId", addressId);
               }
               , map: delegate (IDataReader reader, short set)
               {
                   switch (set)
                   {
                       case 0:
                           {
                               p = MapAddress(reader);

                               break;
                           }

                       case 1:
                           {
                               Amenity a = MapAmenities(reader);

                               if (p.Amenities == null)
                               {
                                   p.Amenities = new List<Amenity>();
                               }

                               p.Amenities.Add(a);
                               break;
                           }
                   }
               });
            return p;
        }

        public List<Address> GetByAmenity(int amenityId)
        {
            List<Address> list = null;
            Address p = null;
            int i = 0;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Addresses_By_AmenityV2"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@AmenityId", amenityId);
               }
               , map: delegate (IDataReader reader, short set)
               {
                   switch (set)
                   {
                       case 0:
                           {
                               p = MapAddress(reader);

                               if (list == null)
                               {
                                   list = new List<Address>();
                               }

                               list.Add(p);

                               break;
                           }

                       case 1:
                           {
                               Amenity a = new Amenity();
                               int startingIndex = 0;

                               a.AddressId = reader.GetSafeInt32(startingIndex++);
                               a.Id = reader.GetSafeInt32(startingIndex++);
                               a.AmenityName = reader.GetSafeString(startingIndex++);
                               a.TypeId = reader.GetSafeBool(startingIndex++);
                               a.Description = reader.GetSafeString(startingIndex++);

                               if (a.AddressId == list[i].Id)
                               {
                                   if (list[i].Amenities == null)
                                   {
                                       list[i].Amenities = new List<Amenity>();
                                   }

                                   list[i].Amenities.Add(a);
                               }

                               else
                               {
                                   i++;
                                   if (list[i].Amenities == null)
                                   {
                                       list[i].Amenities = new List<Amenity>();
                                   }
                                   list[i].Amenities.Add(a);
                               }
                               break;
                           }
                   }
               });
            return list;
        }

        public List<Address> GetByGeo(decimal lat, decimal lng, int radius)
        {
            List<Address> list = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Address_GetByGeo"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Lat", lat);
                   paramCollection.AddWithValue("@Lng", lng);
                   paramCollection.AddWithValue("@Miles", radius);
               }
               , map: delegate (IDataReader reader, short set)
               {
                   Address p = MapAddress(reader);

                   if (list == null)
                   {
                       list = new List<Address>();
                   }
                   list.Add(p);
               });
            return list;
        }

        public List<AddressWithListings> AddressesByLiId(int listingId)
        {
            List<AddressWithListings> list = null;
            Dictionary<int, ListingsPhotoFiles> listingsDict = null;
            Dictionary<int, List<string>> filesDict = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.AddressesByLiId"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@ListingId", listingId);
               }
               , map: delegate (IDataReader reader, short set)
               {
                   //
                   switch (set)
                   {
                       case 0:
                           {
                               AddressWithListings p = new AddressWithListings();
                               int startingIndex = 0;

                               p.ListingId = reader.GetSafeInt32(startingIndex++);
                               p.Id = reader.GetSafeInt32(startingIndex++);
                               p.Line1 = reader.GetSafeString(startingIndex++);
                               p.Line2 = reader.GetSafeString(startingIndex++);
                               p.City = reader.GetSafeString(startingIndex++);
                               p.State = reader.GetSafeInt32(startingIndex++);
                               p.Distance = reader.GetSafeDouble(startingIndex++);

                               if (list == null)
                               {
                                   list = new List<AddressWithListings>();
                               }
                               list.Add(p);

                               break;
                           }

                       case 1:
                           {
                               ListingsPhotoFiles l = new ListingsPhotoFiles();
                               int startingIndex = 0;

                               l.Id = reader.GetSafeInt32(startingIndex++);
                               l.Headline = reader.GetSafeString(startingIndex++);
                               l.Description = reader.GetSafeString(startingIndex++);
                               l.LeaseTerms = reader.GetSafeInt32(startingIndex++);
                               l.AvailabilityDate = reader.GetSafeDateTime(startingIndex++);
                               l.RentCost = reader.GetSafeInt32(startingIndex++);

                               if (listingsDict == null)
                               {
                                   listingsDict = new Dictionary<int, ListingsPhotoFiles>();
                               }

                               listingsDict[l.Id] = new ListingsPhotoFiles();
                               listingsDict[l.Id] = l;

                               break;
                           }

                       case 2:
                           {
                               int id = reader.GetSafeInt32(0);
                               string imgFilePath = reader.GetSafeString(2);
                               if (filesDict == null)
                               {
                                   filesDict = new Dictionary<int, List<string>>();
                               }

                               if (!filesDict.ContainsKey(id))
                               {
                                   filesDict[id] = new List<string>();
                                   filesDict[id].Add(imgFilePath);
                               }

                               else if (filesDict.ContainsKey(id))
                               {
                                   filesDict[id].Add(imgFilePath);
                               }
                               break;
                           }
                   }
               });

            if(list != null)
            {
                foreach (AddressWithListings addressItem in list)
                {
                    if (listingsDict != null && listingsDict.ContainsKey(addressItem.ListingId))
                    {
                        addressItem.AddressListing = new ListingsPhotoFiles();
                        addressItem.AddressListing = listingsDict[addressItem.ListingId];
                    }

                    if (filesDict != null && filesDict.ContainsKey(addressItem.ListingId))
                    {
                        addressItem.AddressListing.ImgFilePaths = new List<string>();
                        addressItem.AddressListing.ImgFilePaths = filesDict[addressItem.ListingId];
                    }
                }

            }

            return list;
        }

        public List<Address> PLW_Search(string search_item)
        {
            List<Address> list = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Addresses_Search_For_Wizard"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@search_item", search_item);
               }
               , map: delegate (IDataReader reader, short set)
               {
                   Address p = new Address();
                   int startingIndex = 0;

                   p.Id = reader.GetSafeInt32(startingIndex++);
                   p.Line1 = reader.GetSafeString(startingIndex++);
                   p.Line2 = reader.GetSafeString(startingIndex++);
                   p.City = reader.GetSafeString(startingIndex++);
                   p.State = reader.GetSafeInt32(startingIndex++);
                   p.ZipCode = reader.GetSafeString(startingIndex++);

                   if (list == null)
                   {
                       list = new List<Address>();
                   }

                   list.Add(p);
               }
               );
            return list;
        }

        public PagedList<Address> GetAllMapListings(int pageIndex, int pageSize)
        {
            List<Address> list = null;

            PagedList<Address> page = null;
            int totalCount = 0;

            Address p = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.Addresses_By_Page"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@pageindex", pageIndex);
                   paramCollection.AddWithValue("@pagesize", pageSize);

               }, map: delegate (IDataReader reader, short set)
               {
                   p = MapAddress(reader);

                   int startingIndex = 11;

                   if (totalCount == 0)
                   {
                       totalCount = reader.GetSafeInt32(startingIndex++);
                   }

                   if (list == null)
                   {
                       list = new List<Address>();
                       page = new PagedList<Address>(list, pageIndex, pageSize, totalCount);
                   }
                   list.Add(p);
               });
            return page;
        }



        public List<Address> AWOP_Search(string search_item)
        {
            List<Address> list = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Address_WithOut_Property"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@search_item", search_item);
    }
               , map: delegate (IDataReader reader, short set)
               {
                   Address p = new Address();
                   int startingIndex = 0;

                   p.Id = reader.GetSafeInt32(startingIndex++);
                   p.Line1 = reader.GetSafeString(startingIndex++);
                   p.Line2 = reader.GetSafeString(startingIndex++);
                   p.City = reader.GetSafeString(startingIndex++);
                   p.State = reader.GetSafeInt32(startingIndex++);
                   p.ZipCode = reader.GetSafeString(startingIndex++);

                   if (list == null)
                   {
                       list = new List<Address>();
                   }

                   list.Add(p);
}
               );

            return list;
        }



        public void AddressToProperty(int propId, int addId)
        {
           
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.PropertyAddressLookUp_Insert"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@PropertyId", propId);
                   paramCollection.AddWithValue("@AddressesId", addId);

            

               });

        }

        public void UpdateLongLat(AdressUpdateLongLatRequest model)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Addresses_UpdateLongLat"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Latitude", model.Latitude);
                   paramCollection.AddWithValue("@Longitude", model.Longitude);
                   paramCollection.AddWithValue("@Id", model.Id);
               });
        }

        public List<Address> RentCheck(RentCheckRequest model)
        {
            List<Address> list = null;
            Address p = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Addresses_CheckRent"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@Line1", model.Line1);
                  paramCollection.AddWithValue("@City", model.City);
                  paramCollection.AddWithValue("@ZipCode", model.ZipCode);
              }, map: delegate (IDataReader reader, short set)
              {
                  p = MapAddress(reader);

                  if (list == null)
                  {
                      list = new List<Address>();
    }

                  list.Add(p);
              });
            return list;
}

        public List<Address> GetAreaRent(RentCheckRequest model)
        {
            List<Address> list = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Address_GetByGeoRent"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Line1", model.Line1);
                   paramCollection.AddWithValue("@Lat", model.lat);
                   paramCollection.AddWithValue("@Lng", model.lng);
                   paramCollection.AddWithValue("@Miles", model.radius);
               }
               , map: delegate (IDataReader reader, short set)
               {
                   Address p = MapAddress(reader);

                   if (list == null)
                   {
                       list = new List<Address>();
                   }
                   list.Add(p);
               });
            return list;
        }

        public RentCheck RentChecker(RentCheckRequest model)
        {
            RentCheck rents = null;

            List<Address> Match = null;

            List<Address> Area = null;

            Address p = null;

            Address t = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Addresses_RentChecker"
             , inputParamMapper: delegate (SqlParameterCollection paramCollection)
             {
                 paramCollection.AddWithValue("@Line1", model.Line1);
                 paramCollection.AddWithValue("@City", model.City);
                 paramCollection.AddWithValue("@stateId", model.State);
                 paramCollection.AddWithValue("@ZipCode", model.ZipCode);
                 paramCollection.AddWithValue("@Lat", model.lat);
                 paramCollection.AddWithValue("@Lng", model.lng);
                 paramCollection.AddWithValue("@Miles", model.radius);

             }, map: delegate (IDataReader reader, short set)
             {
                 switch (set)
                 {
                     case 0:
                         {
                             p = MapAddress(reader);

                             if (Match == null)
                             {
                                 Match = new List<Address>();
                             }

                             Match.Add(p);

                             break;
                         }

                     case 1:
                         {
                             t = MapAddress(reader);

                             if (Area == null)
                             {
                                 Area = new List<Address>();
                             }
                             Area.Add(t);

                             break;
                         }
                 }

             });

            rents = new RentCheck();

            if (rents.Matches == null)
            {
                rents.Matches = Match;
            }
            if (rents.Area == null)
            {
                rents.Area = Area;
            }
            return rents;
        }
    }
}
