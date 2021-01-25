using System;
using System.Collections.Generic;
using System.Text;

namespace PuppetMaster
{
    class PuppetMasterDomain
    {

        // possible commands
        internal static string ReplicationFactorCommand(string r)
        {
            //throw new NotImplementedException();
            return "TODO";
        }

        internal static string PartitionCommand(string server_id, string partition_name, List<string> servers_id)
        {
            //throw new NotImplementedException();
            return "TODO";
        }

        internal static string ServerCommand(string server_id, string url, string min_delay, string max_delay)
        {
            //throw new NotImplementedException();
            return "TODO";
        }

        internal static string ClientCommand(string username, string client_url, string script_file)
        {
            //throw new NotImplementedException();
            return "TODO";
        }

        internal static string StatusCommand()
        {
            //throw new NotImplementedException();
            return "TODO";
        }

        // debug commands (replica)

        internal static string CrashCommand(string server_id)
        {
            //throw new NotImplementedException();
            return "TODO";
        }

        internal static string FreezeCommand(string server_id)
        {
            //throw new NotImplementedException();
            return "TODO";
        }

        internal static string UnfreezeCommand(string server_id)
        {
            //throw new NotImplementedException();
            return "TODO";
        }

    }
}
