using Grpc.Core;
using PuppetMaster.Controllers.ServerDebugControllers;
using PuppetMaster.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace PuppetMaster.Commands
{
    class CrashServerCommand : Command
    {
        private TextBox txtBoxOutput;

        private ConnectionManager ConnectionManager;
        public CrashServerCommand(TextBox output, ConnectionManager connectionManager) : base(true)
        {
            txtBoxOutput = output;
            ConnectionManager = connectionManager;
        }

        public static int EXPECTED_ARGUMENTS = 1;
        public override async Task ExecuteAsync(List<string> arguments)
        {
            if (arguments.Count != EXPECTED_ARGUMENTS)
            {
                txtBoxOutput.AppendText(Environment.NewLine + "Expected " + EXPECTED_ARGUMENTS + " arguments but found " + arguments.Count + ".");
                return;
            }

            string serverId = arguments.ElementAt(0);

            try
            {
                txtBoxOutput.AppendText(Environment.NewLine + $"Crash... {serverId}");
                await CrashServerController.Execute(ConnectionManager, serverId);
                txtBoxOutput.AppendText(Environment.NewLine + "Crash DONE.");
            }
            catch (RpcException e) when(e.StatusCode == StatusCode.Internal)
            {
                txtBoxOutput.AppendText(Environment.NewLine + serverId + " is not respondig. It will be removed from the system configuration.");
                ConnectionManager.RemoveServerFromConfiguration(serverId);
            }
        }
    }
}
