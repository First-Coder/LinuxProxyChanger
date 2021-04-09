using System.Collections.Generic;

namespace LinuxProxyChanger.Models
{
    public class Settings
    {
        public bool CallOnNetworkchange { get; set; }
        public string NetworkChangeAdapter { get; set; }
        public bool SetProxyOnStartUp { get; set; }

        public string UniquePrefixLine { get; set; }
        public string UniqueSuffixLine { get; set; }

        public string BashPath { get; set; }
        public string BashCommandEnable { get; set; }
        public string BashCommandDisable { get; set; }

        public string ProxyIp { get; set; }
        public int Timeout { get; set; }

        public List<FileSettings> Files { get; set; }
    }
}
