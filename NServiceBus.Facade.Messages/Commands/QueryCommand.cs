
namespace NServiceBus.Facade.Messages.Commands
{
    public class QueryCommand:ICommand
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}