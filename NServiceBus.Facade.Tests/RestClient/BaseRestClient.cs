namespace NServiceBus.Facade.Tests.RestClient
{
    public class BaseRestClient
    {
        protected RestSharp.RestClient _client;

        public BaseRestClient()
        {
            _client = new RestSharp.RestClient();
        } 
    }
}