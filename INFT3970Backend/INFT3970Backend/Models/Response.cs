using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class Response<T>
    {
        public T Data { get; set; }
        public ResponseType Type { get; set; }
        public string ErrorMessage { get; set; }


        public Response(T data, ResponseType type, String errorMessage)
        {
            Data = data;
            Type = type;
            ErrorMessage = errorMessage;
        }

        public Response(T data, int type, string errorMessage)
        {
            Data = data;
            SetType(type);
            ErrorMessage = errorMessage;
        }

        private void SetType(int type)
        {
            switch (type)
            { 
                case 1:
                    Type = ResponseType.SUCCESS;
                    break;

                case 0:
                    Type = ResponseType.ERROR;
                    break;

                default:
                    Type = ResponseType.ERROR;
                    break;
            }
        }
    }

    public enum ResponseType
    {
        ERROR,
        SUCCESS
    }


}
