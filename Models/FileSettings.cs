using System.Collections.Generic;

namespace LinuxProxyChanger.Models
{
    public class FileSettings
    {
        public string Path { get; set; }
        public List<string> Proxy { get; set; }
    }
}
