using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NServiceBus.Facade.Tests.LoadTest
{
    [TestClass]
    public class LoadTest
    {
         [TestMethod]
         public void Run()
         {
             //how many tasks to perform
             var taskCount = 50;
             //how many workers performing the tasks
             var workerCount = 50;
             //how much time (in seconds) between the first and last worker waking up?
             var workerStaggerTime = 0;
             //how much time (in seconds) does each worker wait before polling again?
             var pollWaitTime = 3;
             //a container for any errors
             var exceptions = new List<Exception>();

             var threadSignal = new ManualResetEvent(false);
             var workers = new List<PostQueryWorker>();
             var semaphone = new Semaphore(workerCount, workerCount);
             var random = new Random();
             var stopWatch = new Stopwatch();
             stopWatch.Start();
             for (var i = 0; i < taskCount; i++)
             {
                 var worker = new PostQueryWorker(random.NextDouble() * workerStaggerTime, i+1, pollWaitTime);
                 worker.Semaphore = semaphone;
                 worker.OnComplete = (sender, eventargs) =>
                 {
                     var w = (BaseWorker)sender;
                     if (!w.IsSuccessful)
                         exceptions.Add(w.Exception);
                     Interlocked.Decrement(ref taskCount);
                     Debug.WriteLine("----------Tasks remaining: " + taskCount);
                     if (taskCount == 0)
                         threadSignal.Set();
                 };
                 workers.Add(worker);
                 ThreadPool.QueueUserWorkItem(worker.ThreadPoolCallback);
             }
             threadSignal.WaitOne();
             stopWatch.Stop();
             var overallTime = stopWatch.ElapsedMilliseconds/1000f;
             foreach (var b in workers)
             {
                 Console.WriteLine("Worker: [{0}] Initial post: [{1}] Avg poll: [{2}] Total polls: [{3}] ", b.Id, b.PostTime, b.AvgPollTime, b.NumPolls);
             }
             Console.WriteLine("===============================================");
             Console.WriteLine("Averages:");
             Console.WriteLine("Avg post wait time: [{0}] Avg poll wait time: [{1}] Avg num polls per task: [{2}] Overall time: [{3}] Errors: [{4}]",
                 workers.Average(x => x.PostTime),
                 workers.Average(x => x.AvgPollTime),
                 workers.Average(x => x.NumPolls),
                 overallTime, 
                 exceptions.Count);
         }
    }
}