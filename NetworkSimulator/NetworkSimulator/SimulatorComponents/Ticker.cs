using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MicroLibrary;

namespace NetworkSimulator.SimulatorComponents
{
    public class Ticker
    {
        private static Ticker _SingleTonObject = null;

        private MicroTimer _Timer;

        private long _ElapsedTime;

        public long ElapsedTime
        {
            get { return _ElapsedTime; }
        }

        private long _Interval;
        private List<ITickerListener> _Listeners;

        public static Ticker GetInstance()
        {
            if (_SingleTonObject == null)
                _SingleTonObject = new Ticker(0);
            return _SingleTonObject;
        }

        public Ticker(long interval)
        {
            _Interval = interval;
            Initialize();
        }

        private void Initialize()
        {
            _Listeners = new List<ITickerListener>();

            // Instantiate new MicroTimer and add event handler
            _Timer = new MicroTimer();

            _Timer.MicroTimerElapsed +=
                new MicroLibrary.MicroTimer.MicroTimerElapsedEventHandler(OnTimedEvent);

            _Timer.Interval = _Interval;
        }

        public void Start()
        {
            _Timer.Enabled = true;
        }

        public void Stop()
        {
            _Timer.Enabled = false;
        }

        public void AddListener(ITickerListener listener)
        {
            _Listeners.Add(listener);
        }


        private void OnTimedEvent(object sender, MicroTimerEventArgs timerEventArgs)
        {
            _ElapsedTime++;
           // Console.WriteLine("................................................................... Tick-" + _ElapsedTime);
            foreach (var listener in _Listeners)
            {
                listener.OnTickerTick(_ElapsedTime);
            }
        }
    }
}
