using System;
using System.Collections.Generic;
using System.Net;
using NServiceBus.Facade.Messages.Responses;
using NServiceBus.Facade.Tests.LoadTest;
using NServiceBus.Facade.Web.Configuration;
using RestSharp;

namespace NServiceBus.Facade.Tests.RestClient
{
    public class PollQueryRestClient:BaseRestClient
    {
        public PollQueryRestClient(Guid id)
        {
            _client.BaseUrl = BaseWorker.PollQueryUrl+"/"+id;
            _client.Authenticator = new NtlmAuthenticator(CredentialCache.DefaultNetworkCredentials);
        }

        public bool Run()
        {
            var request = new RestRequest(Method.GET);
            request.RequestFormat = DataFormat.Json;
            var response = _client.Get<QueryResponse>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(string.Format("Error on poll: [{0}] [{1}]", response.StatusCode, response.Content));
            }
            return response.Data.Status=="OK";
        }
    }
}