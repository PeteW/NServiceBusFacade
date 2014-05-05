using Autofac;
using NServiceBus.Installation.Environments;

namespace NServiceBus.Facade.Web.Configuration
{
    public class AppContext
    {
        private static volatile AppContext _appContext = null;
        private AppContext()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<AutofacModule>();
            Container = containerBuilder.Build();
        }

        public static AppContext Current
        {
            get
            {
                if (_appContext == null)
                {
                    lock (typeof(AppContext))
                    {
                        if (_appContext == null)
                        {
                            _appContext = new AppContext();
                        }
                    }
                }
                return _appContext;
            }
        }

        public IContainer Container { get; private set; }
    }

    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            var bus = NServiceBus.Configure.With()
                                 .DefaultBuilder()
                                 .UnicastBus()
                                 .CreateBus()
                                 .Start(() =>
                                     {
                                         NServiceBus.Configure.Instance
                                                    .ForInstallationOn<Windows>()
                                                    .Install();
                                     });
            builder.RegisterInstance(bus).As<IBus>();
            builder.RegisterType<RavenMessageRepository>().As<IServiceBusMessageRepository>().SingleInstance();
            builder.RegisterType<ServiceBusFacade>().As<IServiceBusFacade>();
        }
    }
}