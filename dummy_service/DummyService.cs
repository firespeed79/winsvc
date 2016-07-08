﻿using System.ServiceProcess;
using System.Threading;

namespace dummy_service
{
    public class DummyService : ServiceBase
    {
        private readonly AutoResetEvent _event = new AutoResetEvent(false);

        public static string Name = "Dummy Service";
        private Thread _thread;

        public DummyService()
        {
            ServiceName = Name;
            CanShutdown = true;
            CanPauseAndContinue = false;
            CanStop = true;
        }

        protected override void OnStart(string[] args)
        {
            _thread = new Thread(() =>
            {
                _event.WaitOne();

            });
            _thread.Start();
        }

        protected override void OnStop()
        {
            _event.Set();
            _thread.Join();
        }
    }
}