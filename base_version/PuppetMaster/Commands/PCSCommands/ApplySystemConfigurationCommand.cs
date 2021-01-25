using PuppetMaster.Controllers.PCSControllers;
using PuppetMaster.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace PuppetMaster.Commands.PCSCommands
{
    class ApplySystemConfigurationCommand : Command
    {
        private readonly ConnectionManager ConnectionManager;

        private readonly TextBox txtBoxOutput;
        public ApplySystemConfigurationCommand(TextBox output, ConnectionManager connectionManager) : base(true)
        {
            txtBoxOutput = output;
            ConnectionManager = connectionManager;
        }

        public override async Task ExecuteAsync(List<string> arguments)
        {
            try
            {
                List<string> serverLines = SystemConfiguration.GetInstance().GetServerLines();
                if (serverLines == null || serverLines.Count == 0)
                {
                    throw new ApplySystemConfigurationException("No configuration provided. Please configure the system first.");
                }

                string servers = SystemConfiguration.GetInstance().GetServersArgument();
                string partitions = SystemConfiguration.GetInstance().GetPartitionsArgument();

                await ApplySystemConfigurationController.Execute(ConnectionManager, serverLines, servers, partitions);

                txtBoxOutput.AppendText(Environment.NewLine + "System Configuration Applied.");
            }
            catch (ApplySystemConfigurationException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ApplySystemConfigurationException("Apply system configuration (internal command)", e);
            }
            
        }
    }
}
