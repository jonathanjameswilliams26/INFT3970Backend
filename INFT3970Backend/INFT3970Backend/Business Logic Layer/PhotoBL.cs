using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Backend.Models;
using INFT3970Backend.Data_Access_Layer;

namespace INFT3970Backend.Business_Logic_Layer
{
    public class PhotoBL
    {
        public Response<List<Photo>> GetPhotoLocation(int photoID)
        {
            PhotoDAL photoDAL = new PhotoDAL();
            return photoDAL.GetPhotoLocation(photoID);
        }

    }
}
