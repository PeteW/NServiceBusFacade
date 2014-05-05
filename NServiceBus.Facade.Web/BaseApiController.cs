using System;
using System.Net;
using System.Web.Http;

namespace NServiceBus.Facade.Web
{
    public abstract class BaseApiController : ApiController
    {
        /// <summary>
        ///     Returns an HttpResponseException that has a status code of 500 Internal Server error
        ///     and sets the Response Reason Phrase to the exception's Message property, after
        ///     replacing each new line characters with a space.
        /// </summary>
        /// <param name="ex">The exception to extract data from</param>
        /// <returns>HttpResponseException that can be thrown and will communicate what went wrong</returns>
        protected HttpResponseException CreateHttpResponseException(Exception ex)
        {
            var msg = ex.Message.Replace(Environment.NewLine, " ");
            return CreateHttpResponseException(HttpStatusCode.InternalServerError, msg);
        }

        /// <summary>
        ///     Returns an HttpResponseException that will have a status code of whatever
        ///     was passed in and a Response Reason Phrase of whatever the errorMessage was.
        /// </summary>
        /// <param name="statusCode">HttpStatusCode for the exception</param>
        /// <param name="errorMessage">Information to return in the exception</param>
        /// <returns>HttpResponseException that can be thrown and will communicate what went wrong</returns>
        protected HttpResponseException CreateHttpResponseException(HttpStatusCode statusCode, string errorMessage)
        {
            var httpResponseException = new HttpResponseException(statusCode);
            httpResponseException.Data.Add("Error", errorMessage);
            httpResponseException.Response.ReasonPhrase = errorMessage;
            return httpResponseException;
        }
    }
}