using System;
using System.Collections.Generic;
using System.Text;

namespace Ryd3rNetworkMonitor.Library
{
    public interface IServer
    {
        bool Start();
        void StartListening();
        bool Stop();
    }
}
