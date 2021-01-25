using PuppetMaster.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace PuppetMaster.Commands
{
    class PartitionCommand : Command
    {
        private readonly TextBox txtBoxOutput;
        public PartitionCommand(TextBox output) : base(false)
        {
            txtBoxOutput = output;
        }

        public static int EXPECTED_ARGUMENTS = 2;
        public override Task ExecuteAsync(List<string> arguments)
        {
            try
            {
                int serversNumber = Int32.Parse(arguments[0]);
                int MAX_ARGUMENTS = EXPECTED_ARGUMENTS + serversNumber;
                if (arguments.Count != MAX_ARGUMENTS)
                {
                    throw new ApplySystemConfigurationException($"Expected {MAX_ARGUMENTS} arguments but found {arguments.Count}.");
                }

                SystemConfiguration.GetInstance().AddPartitionConfig(string.Join(" ", arguments));
                txtBoxOutput.AppendText(Environment.NewLine + "Partition Configured.");
            }
            catch (ApplySystemConfigurationException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ApplySystemConfigurationException("Create partition command", e);
            }
            return Task.CompletedTask;
        }
    }
}
