using System;

namespace INFT3970Backend.Models.Errors
{
    public class InvalidModelException : Exception
    {
        public string Msg { get; set; }
        public int Code { get; set; }

        public InvalidModelException(string msg, int code)
            : base()
        {
            Msg = msg;
            Code = code;
        }
    }
}
