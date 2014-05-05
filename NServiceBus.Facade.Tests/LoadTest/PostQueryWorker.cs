using System;
using System.Diagnostics;
using System.Threading;
using NServiceBus.Facade.Tests.RestClient;

namespace NServiceBus.Facade.Tests.LoadTest
{
    public class PostQueryWorker : BaseWorker
    {
        public double PostTime { get; set; }
        public double AvgPollTime { get; set; }
        public int NumPolls { get; set; }
        public int PollWaitTime { get; private set; }

        public PostQueryWorker(double startDelayTime, int id, int pollWaitTime): base(startDelayTime, id)
        {
            PollWaitTime = pollWaitTime;
        }

        protected override void Run()
        {
            Log("Lets get started! I am POSTING a new req...");
            var stopWatch = new Stopwatch();
            var postClient = new PostQueryRestClient();
            stopWatch.Start();
            var id = postClient.Run();
            stopWatch.Stop();
            PostTime = stopWatch.ElapsedMilliseconds/1000f;
            stopWatch.Reset();

            var pollClient = new PollQueryRestClient(id);
            var isDone = false;
            NumPolls = 0;
            while (!isDone)
            {
                Log(string.Format("Polling attempt #{1} for [{0}]", id, (NumPolls + 1)));
                stopWatch.Start();
                isDone = pollClient.Run();
                stopWatch.Stop();
                AvgPollTime = ((AvgPollTime*NumPolls) + stopWatch.ElapsedMilliseconds/1000f)/(NumPolls + 1);
                NumPolls++;
                stopWatch.Reset();
                Thread.Sleep(PollWaitTime*1000);
            }
        }
    }
}