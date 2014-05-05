using System;

namespace NServiceBus.Facade.Messages.Responses
{
    public class QueryResponse:IMessage
    {
        public string Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}