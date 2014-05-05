using System;
using System.Collections.Generic;
using System.Net;
using NServiceBus.Facade.Messages.Commands;
using NServiceBus.Facade.Tests.LoadTest;
using NServiceBus.Facade.Web.Configuration;
using RestSharp;

namespace NServiceBus.Facade.Tests.RestClient
{
    public class PostQueryRestClient:BaseRestClient
    {
        public PostQueryRestClient()
        {
            _client.BaseUrl = BaseWorker.PostQueryUrl;            
            _client.Authenticator = new NtlmAuthenticator(CredentialCache.DefaultNetworkCredentials);
            _client.Proxy = System.Net.HttpWebRequest.DefaultWebProxy;
            //if you want to use fiddler
//            _client.Proxy = new WebProxy("127.0.0.1", 8888); // IP, Port.
        }

        public Guid Run()
        {
            var queryCommand = new QueryCommand
                {
                    FirstName = "derp", 
                    LastName = "derp"
                };
            var request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(queryCommand);
            var response = _client.Post<QueryCommand>(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(string.Format("Error on post: [{0}] [{1}]", response.StatusCode, response.Content));
            }
            return new Guid(response.Content.Replace("\"",""));
        }
    }
}