using PuppetMaster.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace PuppetMaster.Commands
{
    class CreateServerCommand : Command
    {
        private readonly TextBox txtBoxOutput;
        private readonly ConnectionManager ConnectionManager;
        public CreateServerCommand(TextBox output, ConnectionManager connectionManager) : base(false)
        {
            txtBoxOutput = output;
            ConnectionManager = connectionManager;
        }

        public static int EXPECTED_ARGUMENTS = 4;
        public override Task ExecuteAsync(List<string> arguments)
        {
            if (arguments.Count != EXPECTED_ARGUMENTS)
            {
                throw new ApplySystemConfigurationException($"Expected {EXPECTED_ARGUMENTS} arguments but found {arguments.Count}.");
            }

            try
            {
                SystemConfiguration.GetInstance().AddServerConfig(string.Join(" ", arguments));
                ConnectionManager.SetNewServerConnection(arguments[0], arguments[1]);
                txtBoxOutput.AppendText(Environment.NewLine + "Server Configured.");
            }
            catch (Exception e)
            {
                throw new ApplySystemConfigurationException("Create server command", e);
            }
            return Task.CompletedTask;
        }
    }
}
