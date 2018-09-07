using System;
using System.Collections.Generic;
using INFT3970Backend.Models;
using INFT3970Backend.Data_Access_Layer;
using System.IO;
using SixLabors.ImageSharp;
using Microsoft.AspNetCore.SignalR;
using INFT3970Backend.Hubs;

namespace INFT3970Backend.Business_Logic_Layer
{
    public class PhotoBL
    {
        public Response<List<Photo>> GetPhotoLocation(int photoID)
        {
            PhotoDAL photoDAL = new PhotoDAL();
            return photoDAL.GetPhotoLocation(photoID);
        }





        /// <summary>
        /// Saves a photo to GameImages with the name of the current time in milliseconds. Returns the path to the file or an empty string if error.
        /// </summary>
        /// <param name="imgUrl">The DataURL of the image, a base64 string containing all the data for the photo.</param>
        /// <returns>The path to the file or empty string if an error occurred</returns>
        public Response<Photo> SavePhoto(string imgUrl, int takenByID, int photoOfID, IHubContext<ApplicationHub> hubContext)
        {
            try
            {
                //Confirm the dataURL is actually a base64 string.
                var base64Data = imgUrl.Replace("data:image/jpeg;base64,", "");
                var binData = Convert.FromBase64String(base64Data);

                //Save the photo to the directory specified
                var filePath = Directory.GetCurrentDirectory() + "\\GameImages\\" + DateTime.Now.Ticks + ".jpg";
                using (var stream = new MemoryStream(binData))
                {
                    using (var image = Image.Load(stream))
                    {
                        image.Save(filePath);
                    }
                }

                //Save the DataURL to the database
                PhotoDAL photoDAL = new PhotoDAL();
                Response<Photo> response = photoDAL.SavePhoto(imgUrl, takenByID, photoOfID);

                //If the response is successful we want to send live updates to clients and
                //email or text message notifications to not connected players
                if(response.Type == "SUCCESS")
                {
                    HubInterface hubInterface = new HubInterface(hubContext);
                    hubInterface.UpdatePhotoUploaded(response.Data);
                }

                return response;
            }
            catch
            {
                return null;
            }
        }
    }
}
