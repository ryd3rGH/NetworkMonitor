using Ryd3rNetworkMonitor.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    internal class HostControlTemplate
    {
        public Host Host { get; set; }
        public List<HostMessage> Messages { get; set; }
        public List<byte[]> Screenshots { get; set; }
        public byte[] CurrentImage { get; set; }
    }
}
