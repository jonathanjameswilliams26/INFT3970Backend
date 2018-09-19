using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models.Requests
{
    public class CreateGameRequest
    {
        public string nickname { get; set; }
        public string contact { get; set; }
        public string imgUrl { get; set; }
    }
}
