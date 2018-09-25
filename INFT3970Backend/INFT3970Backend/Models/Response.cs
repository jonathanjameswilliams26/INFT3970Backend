using System;

namespace INFT3970Backend.Models
{

    public class Response
    {
        public string Type { get; set; }            //The type of response SUCCESS or ERROR
        public string ErrorMessage { get; set; }    //The error message which outlines why the request failed
        public int ErrorCode { get; set; }          //The error code. 1 = Success, Anything else = error

        public Response()
        {
            Type = "SUCCESS";
            ErrorMessage = "";
            ErrorCode = 1;
        }

        public Response(string errorMessage, int errorCode)
        {
            SetType(errorCode);
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        public Response(string errorMessage, string errorCode)
        {
            int code = int.Parse(errorCode);
            SetType(code);
            ErrorMessage = errorMessage;
            ErrorCode = code;
        }

        public Response(int type, string errorMessage, int errorCode)
        {
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            SetType(type);
        }


        /// <summary>
        /// Sets the type of the response SUCCESS or ERROR depending on the int passed in
        /// </summary>
        /// <param name="type">The type of response 1 or 0</param>
        private void SetType(int type)
        {
            if (type == 1)
                Type = "SUCCESS";
            else
                Type = "ERROR";
        }

        public bool IsSuccessful()
        {
            if (Type == "SUCCESS")
                return true;
            else
                return false;
        }
    }


    public class Response<T> : Response
    {
        //The data / object being returned in the response
        public T Data { get; set; }

        //CONSTRUCTOR
        public Response(T data)
            : base()
        {
            Data = data;
        }

        //CONSTRUCTOR
        public Response(string errorMessage, int errorCode)
            : base(errorMessage, errorCode)
        {
            Data = default(T);
        }

        //CONSTRUCTOR
        public Response(T data, string errorMessage, int errorCode)
            : base(errorMessage, errorCode)
        {
            Data = data;
        }

        //CONSTRUCTOR
        public Response(T data, string type, string errorMessage, int errorCode)
            : base(errorMessage, errorCode)
        {
            Data = data;
        }

        //CONSTRUCTOR
        public Response(T data, int type, string errorMessage, int errorCode)
            : base(type, errorMessage, errorCode)
        {
            Data = data;
        }
    }
}
