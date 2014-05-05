using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NServiceBus.Facade.Web.Configuration;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Json.Linq;

namespace NServiceBus.Facade.Web
{
    public interface IServiceBusMessageRepository  
    {
        void Put<T>(Guid id, T val) where T:class;
        T Get<T>(Guid id) where T:class;
    }

    public class InMemoryServiceBusMessageRepository : IServiceBusMessageRepository
    {
        private Dictionary<Guid,object> _db;
        public InMemoryServiceBusMessageRepository()
        {
            _db = new Dictionary<Guid, object>();
        }

        public void Put<T>(Guid id, T val) where T : class
        {
            _db[id] = val;
        }

        public T Get<T>(Guid id) where T : class
        {
            if (!_db.ContainsKey(id))
                return null;
            return _db[id] as T;
        }
    }

    public class RavenMessageRepository:IServiceBusMessageRepository
    {
        private DocumentStore _documentStore = null;

        public RavenMessageRepository()
        {
            _documentStore = new DocumentStore()
                {
                    Url = SettingsManager.RavenDbUrl
                };
            _documentStore.Initialize();
            _documentStore.ActivateBundle("Expiration", Assembly.GetExecutingAssembly().GetName().Name);
            _documentStore.ActivateBundle("DocumentExpiration", Assembly.GetExecutingAssembly().GetName().Name);
            _documentStore.DefaultDatabase = Assembly.GetExecutingAssembly().GetName().Name;
        }

        public void Put<T>(Guid id, T val) where T : class
        {
            var expiration = DateTime.UtcNow.AddSeconds(SettingsManager.RavenDbRetentionTimeInSeconds);
            using (var session = _documentStore.OpenSession())
            {
                session.Store(val, id.ToString());
                session.Advanced.GetMetadataFor(val)["Raven-Expiration-Date"] = new RavenJValue(expiration);
                session.SaveChanges();
            }
        }

        public T Get<T>(Guid id) where T : class
        {
            using (var session = _documentStore.OpenSession())
            {
                return session.Load<T>(id.ToString());
            }
        }
    }
    static class RavenDbExtensions
    {
        /// <summary>
        /// Ensure a bundle is activated
        /// </summary>
        /// <param name="documentStore"></param>
        /// <param name="bundleName"></param>
        /// <param name="databaseName"></param>
        public static void ActivateBundle(this IDocumentStore documentStore, string bundleName, string databaseName)
        {
            using (var session = documentStore.OpenSession())
            {
                var databaseDocument = session.Load<DatabaseDocument>("Raven/Databases/" + databaseName);
                var settings = databaseDocument.Settings;
                var activeBundles = settings.ContainsKey(Constants.ActiveBundles) ? settings[Constants.ActiveBundles] : null;
                if (string.IsNullOrEmpty(activeBundles))
                    settings[Constants.ActiveBundles] = bundleName;
                else if (!activeBundles.Split(new char[]{';'}).Contains(bundleName, StringComparer.OrdinalIgnoreCase))
                    settings[Constants.ActiveBundles] = activeBundles + ";" + bundleName;
                session.SaveChanges();
            }
        }
    }
}