using System;

namespace INFT3970Backend.Models
{
    public enum ResponseType
    {
        ERROR,  //0 = Error
        SUCCESS //1 = Success
    }

    public class Response<T>
    {
        public T Data { get; set; }                 //The data / object being returned in the response if applicable
        public string Type { get; set; }      //The type of response SUCCESS or ERROR
        public string ErrorMessage { get; set; }    //The error message which outlines why the request failed
        public int ErrorCode { get; set; }          //The error code. 1 = Success, Anything else = error




        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">The data being returned in the response</param>
        /// <param name="type">The type of response being returned</param>
        /// <param name="errorMessage">The error message if the respose is an error</param>
        public Response(T data, string type, String errorMessage, int errorCode)
        {
            Data = data;
            Type = type;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }




        /// <summary>
        /// Overloaded contructor
        /// </summary>
        /// <param name="data">The data being returned in the response</param>
        /// <param name="type">An integer, 0 = ERROR, 1 = SUCCESS</param>
        /// <param name="errorMessage">The error message if the type is success</param>
        public Response(T data, int type, string errorMessage, int errorCode)
        {
            Data = data;
            SetType(type);
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }




        /// <summary>
        /// Sets the type of the response SUCCESS or ERROR depending on the int passed in
        /// </summary>
        /// <param name="type">The type of response 1 or 0</param>
        private void SetType(int type)
        {
            switch (type)
            { 
                case 1:
                    Type = "SUCCESS";
                    break;

                case 0:
                    Type = "ERROR";
                    break;

                default:
                    Type = "ERROR";
                    break;
            }
        }
    }
}
