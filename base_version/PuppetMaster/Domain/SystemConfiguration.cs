using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace PuppetMaster.Domain
{
    class SystemConfiguration
    {
        private static readonly object Instancelock = new object();
        private static SystemConfiguration Instance = null;

        private static List<string> partitionLines;
        private static List<string> serverLines;
        private ConnectionManager connectionManager;

        private SystemConfiguration()
        {
            partitionLines = new List<string>();
            serverLines = new List<string>();
            connectionManager = new ConnectionManager();
        }

        public static SystemConfiguration GetInstance()
        {
            if (Instance != null) return Instance;
            // lazy implementation
            lock (Instancelock) { Instance = new SystemConfiguration(); }
            return Instance;
        }

        public void AddPartitionConfig(string partitionConfigLine)
        {
            partitionLines.Add(partitionConfigLine);
        }

        public void AddServerConfig(string serverConfigLine)
        {
            serverLines.Add(serverConfigLine);
        }

        public string GetPartitionsArgument()
        {
            return " -p " + string.Join(" -p ", partitionLines);
        }

        public string GetServersArgument()
        {
            StringBuilder aux = new StringBuilder();
            foreach (string serverLine in serverLines)
            {
                string[] serverLineSplit = serverLine.Split(" ");
                aux.Append(" " + serverLineSplit.ElementAt(0) + "," + serverLineSplit.ElementAt(1));
            }
            return " -l " + serverLines.Count + aux.ToString(); 
        }

        public List<string> GetSystemConfiguration()
        {
            List<string> configurationLines = new List<string>();
            string extraArguments = GetServersArgument() + GetPartitionsArgument();
            foreach (string serverLine in serverLines)
            {
                configurationLines.Add(serverLine + extraArguments);
            }
            return configurationLines;
        }

        public List<string> GetServerLines()
        {
            return serverLines;
        }
    }
}
