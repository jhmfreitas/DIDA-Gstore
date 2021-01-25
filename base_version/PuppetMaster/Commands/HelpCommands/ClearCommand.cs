using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace PuppetMaster.Commands
{
    class ClearCommand : Command
    {
        private readonly TextBox txtBoxOutput;
        public ClearCommand(TextBox output) : base(false)
        {
            txtBoxOutput = output;
        }

        public static int EXPECTED_ARGUMENTS = 0;
        public override Task ExecuteAsync(List<string> arguments)
        {
            if (arguments.Count != EXPECTED_ARGUMENTS)
            {
                txtBoxOutput.AppendText(Environment.NewLine + $"Expected {EXPECTED_ARGUMENTS} arguments but found {arguments.Count}.");
                return Task.CompletedTask;
            }

            txtBoxOutput.Clear();
            return Task.CompletedTask;
        }
    }
}
