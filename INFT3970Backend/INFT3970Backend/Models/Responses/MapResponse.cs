using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models.Responses
{
    public class MapResponse
    {
        public List<Photo> Photos { get; set; }
        public bool IsBR { get; set; }
        public double Radius { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
