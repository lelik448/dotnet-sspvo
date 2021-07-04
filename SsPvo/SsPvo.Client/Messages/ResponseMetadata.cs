using RestSharp;
using System.Net;

namespace SsPvo.Client.Messages
{
    public class ResponseMetadata
    {
        public string ContentType { get; private set; }
        public long ContentLength { get; private set; }
        public string ContentEncoding { get; private set; }
        public string Content { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public bool IsSuccessful { get; private set; }
        public string StatusDescription { get; private set; }
        public byte[] RawBytes { get; private set; }
        public string ErrorMessage { get; private set; }


        public static ResponseMetadata From(IRestResponse restResponse)
        {
            return new ResponseMetadata
            {
                ContentType = restResponse.ContentType,
                ContentLength = restResponse.ContentLength,
                ContentEncoding = restResponse.ContentEncoding,
                Content = restResponse.Content,
                StatusCode = restResponse.StatusCode,
                IsSuccessful = restResponse.IsSuccessful,
                StatusDescription = restResponse.StatusDescription,
                RawBytes = restResponse.RawBytes,
                ErrorMessage = restResponse.ErrorMessage
            };
        }
    }
}
