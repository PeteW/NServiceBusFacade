using System.Configuration;

namespace NServiceBus.Facade.Web.Configuration
{
    public class SettingsManager
    {
        public static string RavenDbUrl { get { return GetAppSetting("RavenDbUrl", "http://localhost:8080/"); } }
        public static string RavenDbDatabaseName { get { return GetAppSetting("RavenDbDatabaseName", "NServiceBusBridge"); } }
        public static int RavenDbRetentionTimeInSeconds { get { return int.Parse( GetAppSetting("RavenDbRetentionTimeInSeconds", "120")); } }
//        public static int RavenDbRetentionTimeInSeconds { get { return int.Parse( GetAppSetting("RavenDbRetentionTimeInSeconds", "86400")); } }

        protected static string GetAppSetting(string setting, string defaultValue)
        {
            try
            {
                return GetAppSetting(setting) ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
        protected static string GetAppSetting(string setting)
        {
            var test = ConfigurationManager.AppSettings[setting];
            return test;
        }

    }
}