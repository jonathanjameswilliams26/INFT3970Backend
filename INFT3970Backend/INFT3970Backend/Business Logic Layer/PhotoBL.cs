using System;
using System.Collections.Generic;
using INFT3970Backend.Models;
using INFT3970Backend.Data_Access_Layer;
using System.IO;
using SixLabors.ImageSharp;

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
        public string SavePhoto(string imgUrl)
        {
            try
            {
                var base64Data = imgUrl.Replace("data:image/jpeg;base64,", "");
                var binData = Convert.FromBase64String(base64Data);

                var filePath = Directory.GetCurrentDirectory() + "\\GameImages\\" + DateTime.Now.Ticks + ".jpg";

                using (var stream = new MemoryStream(binData))
                {
                    using (var image = Image.Load(stream))
                    {
                        image.Save(filePath);
                    }
                }
                return filePath;
            }
            catch
            {
                return String.Empty;
            }
        }
    }
}
