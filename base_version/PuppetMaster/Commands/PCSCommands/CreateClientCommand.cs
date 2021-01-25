using PuppetMaster.Controllers.PCSControllers;
using PuppetMaster.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace PuppetMaster.Commands
{
    class CreateClientCommand : Command
    {
        private readonly ConnectionManager ConnectionManager;

        private TextBox txtBoxOutput;
        public CreateClientCommand(TextBox output, ConnectionManager connectionManager) : base(true)
        {
            txtBoxOutput = output;
            ConnectionManager = connectionManager;
        }

        public static int EXPECTED_ARGUMENTS = 3;
        public override async Task ExecuteAsync(List<string> arguments)
        {
            if (arguments.Count != EXPECTED_ARGUMENTS)
            {
                txtBoxOutput.AppendText(Environment.NewLine + "Expected a minimum of " + EXPECTED_ARGUMENTS + " arguments but found " + arguments.Count + ".");
                return;
            }

            string username = arguments.ElementAt(0);
            string clientURL = arguments.ElementAt(1);
            string scriptFile = arguments.ElementAt(2);
            string partitions = SystemConfiguration.GetInstance().GetPartitionsArgument();
            string servers = SystemConfiguration.GetInstance().GetServersArgument();

            await CreateClientController.Execute(ConnectionManager, username, clientURL, scriptFile, servers, partitions);
            ConnectionManager.SetNewClientConnection(username, clientURL);
            txtBoxOutput.AppendText(Environment.NewLine + "Client Created.");
        }
    }
}
