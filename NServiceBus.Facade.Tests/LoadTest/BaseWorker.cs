using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using NServiceBus.Facade.Web.Configuration;

namespace NServiceBus.Facade.Tests.LoadTest
{
    /// <summary>
    /// A worker performs a specific task and reports one or more time durations
    /// </summary>
    public abstract class BaseWorker
    {
        //you probably want to load test this on a true IIS server and not the local visual studio development server to get more accurate numbers
        public static string PostQueryUrl { get { return GetAppSetting("PostQueryUrl", "http://localhost:25708/api/v1/queries"); } }
        public static string PollQueryUrl { get { return GetAppSetting("PollQueryUrl", "http://localhost:25708/api/v1/queries"); } }

        /// <summary>
        /// Callback for worker completed
        /// </summary>
        public EventHandler<EventArgs> OnComplete { get; set; }

        /// <summary>
        /// Used to throttle the max concurrent workers
        /// </summary>
        public Semaphore Semaphore { get; set; }

        /// <summary>
        /// Start delay time.
        /// </summary>
        public double StartDelayTime { get; private set; }

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Logs the specified input.
        /// </summary>
        protected void Log(string input)
        {
            Debug.WriteLine("Task #" + Id + " worker says: " + input);
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        protected abstract void Run();

        /// <summary>
        /// Contains any error a given worker may have encountered.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets a value indicating whether the worker was successful or not on its last run.
        /// </summary>
        public bool IsSuccessful { get { return Exception == null; } }

        /// <summary>
        /// Queues a worker. This is the wrapper for the worker's Run() method
        /// </summary>
        public void ThreadPoolCallback(Object o)
        {
            Semaphore.WaitOne();
            //stagger the start
            Log("I am delayed out by " + StartDelayTime + " seconds.");
            Thread.Sleep((int)(StartDelayTime * 1000));
            try
            {
                Run();
            }
            catch (Exception exp)
            {
                Log("Aww crap: " + exp);
                Exception = exp;
            }
            if (OnComplete != null)
                OnComplete(this, EventArgs.Empty);
            Semaphore.Release();
        }

        protected BaseWorker(double startDelayTime, int id)
        {
            StartDelayTime = startDelayTime;
            Id = id;
        }

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