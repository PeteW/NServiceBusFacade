using System;
using System.Threading;
using System.Transactions;
using NServiceBus.Facade.Messages.Commands;
using NServiceBus.Facade.Messages.Responses;

namespace NServiceBus.Facade.Endpoint
{
    public class QueryCommandHandler:IHandleMessages<QueryCommand>
    {
        public IBus Bus { get; set; }

        public void Handle(QueryCommand message)
        {
            //1. log/authenticate/authorize
            Console.WriteLine("I got a message!");

            using (var tx = new TransactionScope())
            {
                //2. do some database work
                tx.Complete();
            }

            //3. respond to client
            Bus.Reply(new QueryResponse(){Status = "OK"});
        }
    }
}