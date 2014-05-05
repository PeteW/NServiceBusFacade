using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NServiceBus.Facade.Messages.Commands;
using NServiceBus.Facade.Messages.Responses;
using NServiceBus.Facade.Web.Configuration;

namespace NServiceBus.Facade.Web
{
    public interface IServiceBusFacade
    {
        IServiceBusMessageRepository ServiceBusMessageRepository { get; }
        Guid SendQueryCommand(QueryCommand queryCommand);
    }

    public class MockServiceBusFacade : IServiceBusFacade
    {
        public MockServiceBusFacade(IServiceBusMessageRepository serviceBusMessageRepository)
        {
            ServiceBusMessageRepository = serviceBusMessageRepository;
        }

        public IServiceBusMessageRepository ServiceBusMessageRepository { get; private set; }

        public Guid SendQueryCommand(QueryCommand queryCommand)
        {
            var messageId = Guid.NewGuid();
            //asynchronously update the pending state with a result to simulate a message handler
            RunAsync(()=>ServiceBusMessageRepository.Put(messageId,new QueryResponse(){Status = "OK"}));
            return messageId;
        }

        private Random _random = new Random();

        private void RunAsync(Action x)
        {
            Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(_random.Next(100, 150));
                    x();
                }).ContinueWith(task => { });
        }
    }
    public class ServiceBusFacade:IServiceBusFacade
    {
        /// <summary>
        /// hack - cant get dependency injection working
        /// </summary>
        public ServiceBusFacade()
        {
            Bus = AppContext.Current.Container.Resolve<IBus>();
            ServiceBusMessageRepository = AppContext.Current.Container.Resolve<IServiceBusMessageRepository>();
        }

        public IBus Bus { get; set; }
        public IServiceBusMessageRepository ServiceBusMessageRepository { get; private set; }
        public Guid SendQueryCommand(QueryCommand queryCommand)
        {
            var messageId = Guid.NewGuid();
            Bus.Send(queryCommand).Register(m =>
                {
                    var response = m.Messages[0] as QueryResponse;
                    ServiceBusMessageRepository.Put(messageId, response);
                });
            return messageId;
        }
    }
}