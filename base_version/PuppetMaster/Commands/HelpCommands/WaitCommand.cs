using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using Utils;

namespace PuppetMaster.Commands
{
    class WaitCommand : Command
    {
        private readonly TextBox txtBoxOutput;
        public WaitCommand(TextBox output) : base(false)
        {
            txtBoxOutput = output;
        }

        public static int EXPECTED_ARGUMENTS = 1;
        public override async Task ExecuteAsync(List<string> arguments)
        {
            if (arguments.Count != EXPECTED_ARGUMENTS)
            {
                txtBoxOutput.AppendText(Environment.NewLine + $"Expected {EXPECTED_ARGUMENTS} arguments but found {arguments.Count}.");
                return;
            }

            try
            {
                int sleep = int.Parse(arguments.ElementAt(0));
                txtBoxOutput.AppendText(Environment.NewLine + "Sleeping...");
                await Task.Delay(sleep);
                txtBoxOutput.AppendText(Environment.NewLine + "Waking up...");
            }
            catch (FormatException)
            {
                txtBoxOutput.AppendText(Environment.NewLine + "Argument must be of type int.");
                return;
            }
        }
    }
}
