using System.Collections.Generic;
using INFT3970Backend.Models;
using Microsoft.AspNetCore.Mvc;
using INFT3970Backend.Models.Errors;
using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Models.Responses;

namespace INFT3970Backend.Controllers
{
    [ApiController]
    public class Map : ControllerBase
    {
        [HttpGet]
        [Route("api/map")]
        public ActionResult<Response<MapResponse>> GetMap(int playerID)
        {
            try
            {
                var player = new Player(playerID);

                //Get the player from the database
                var playerResponse = new PlayerDAL().GetPlayerByID(playerID);
                if (!playerResponse.IsSuccessful())
                    return new Response<MapResponse>(playerResponse.ErrorMessage, playerResponse.ErrorCode);

                //Call the data access layer to get the last known locations
                var getLastPhotoLocationsResponse = new PhotoDAL().GetLastKnownLocations(player);
                if(!getLastPhotoLocationsResponse.IsSuccessful())
                    return new Response<MapResponse>(getLastPhotoLocationsResponse.ErrorMessage, getLastPhotoLocationsResponse.ErrorCode);

                //If the response was successful compress the photo to remove any unneccessary data needed for the map
                foreach (var photo in getLastPhotoLocationsResponse.Data)
                    photo.CompressForMapRequest();

                MapResponse map;

                //if the player is not a BR player return the list of photos
                if (!playerResponse.Data.IsBRPlayer())
                {
                    map = new MapResponse()
                    {
                        Photos = getLastPhotoLocationsResponse.Data,
                        IsBR = false,
                        Latitude = 0,
                        Longitude = 0,
                        Radius = 0
                    };
                }

                //otherwise, calculate the currenct radius
                else
                {
                    map = new MapResponse()
                    {
                        Photos = getLastPhotoLocationsResponse.Data,
                        IsBR = true,
                        Latitude = playerResponse.Data.Game.Latitude,
                        Longitude = playerResponse.Data.Game.Longitude,
                        Radius = playerResponse.Data.Game.CalculateRadius()
                    };
                }

                return new Response<MapResponse>(map);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<MapResponse>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }
    }   
}

