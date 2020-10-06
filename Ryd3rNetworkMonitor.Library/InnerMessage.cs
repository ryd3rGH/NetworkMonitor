using System;
using System.Collections.Generic;
using System.Text;

namespace Ryd3rNetworkMonitor.Library
{
    [Serializable]
    public class InnerMessage
    {
        public InnerMessageTypes Type { get; private set; }
        public string TextMessage { get; private set; }
        public byte[] ImageBytes { get; private set; }
        public int? Clicks { get; private set; }
        public int? KeyPresses { get; private set; }

        public InnerMessage(InnerMessageTypes type, string text, byte[] image = null, int? clicks = null, int? keys = null)
        {
            Type = type;
            TextMessage = text;
            ImageBytes = image;
            Clicks = clicks;
            KeyPresses = keys;
        }
    }
}
