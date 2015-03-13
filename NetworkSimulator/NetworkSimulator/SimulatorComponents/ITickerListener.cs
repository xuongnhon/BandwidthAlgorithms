using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSimulator.SimulatorComponents
{
    public interface ITickerListener
    {
        void OnTickerTick(long elapsedTime);
    }
}
