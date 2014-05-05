using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Autofac;
using NServiceBus.Facade.Messages.Commands;
using NServiceBus.Facade.Messages.Responses;
using NServiceBus.Facade.Web.Configuration;

namespace NServiceBus.Facade.Web.Controllers
{
    [RoutePrefix("api/v1/queries")]
    public class QueriesController : BaseApiController
    {
        public QueriesController(IServiceBusMessageRepository serviceBusMessageRepository, IServiceBusFacade serviceBusFacade)
        {
            ServiceBusMessageRepository = serviceBusMessageRepository;
            ServiceBusFacade = serviceBusFacade;
        }

        /// <summary>
        /// hack- i shouldnt have to add a parameterless constructor
        /// </summary>
        public QueriesController()
        {
            ServiceBusMessageRepository = AppContext.Current.Container.Resolve<IServiceBusMessageRepository>();
            ServiceBusFacade = AppContext.Current.Container.Resolve<IServiceBusFacade>();
        }

        public IServiceBusMessageRepository ServiceBusMessageRepository { get; set; }
        public IServiceBusFacade ServiceBusFacade { get; set; }

        /// <summary>
        /// Place a QueryRequest on the bus for fulfillment
        /// </summary>
        /// <param name="command">The QueryRequest
        /// </param>
        /// <returns>
        ///     The request Id which can be used as a polling mechanism
        /// </returns>
        [HttpPost, AcceptVerbs("POST"), Route("")]
        public Guid PostQuery(QueryCommand command)
        {
            //call the bus and get a message id we can use as a request handle
            var messageId = ServiceBusFacade.SendQueryCommand(command);

            //place a pending response on the repository. The bus message handler will update this later
            ServiceBusMessageRepository.Put(messageId, new QueryResponse() {Status = "Pending"});

            return messageId;
        }

        /// <summary>
        /// Get QueryPollingResponse by request id.
        /// </summary>
        /// <param name="id">The request id.</param>
        /// <returns>The QueryPollingResponse
        /// </returns>
        [HttpGet, Route("{id:Guid}")]
        public QueryResponse GetQueryById(Guid id)
        {
            var response = ServiceBusMessageRepository.Get<QueryResponse>(id);
            if (response == null)
                throw CreateHttpResponseException(HttpStatusCode.NotFound, string.Format("No messages found for [{0}]", id));
            return response;
        }
    }
}