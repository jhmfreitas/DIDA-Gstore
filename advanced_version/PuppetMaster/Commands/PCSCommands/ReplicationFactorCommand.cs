using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace PuppetMaster.Commands
{
    class ReplicationFactorCommand : Command
    {
        private TextBox txtBoxOutput;
        public ReplicationFactorCommand(TextBox output) : base(false)
        {
            txtBoxOutput = output;
        }

        public override Task ExecuteAsync(List<string> arguments)
        {
            txtBoxOutput.AppendText(Environment.NewLine + "Unused command.");
            return Task.CompletedTask;
        }
    }
}
