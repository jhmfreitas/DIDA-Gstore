using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace Client.Commands
{
    class WaitCommand : Command
    {
        public static int EXPECTED_ARGUMENTS = 1;

        public WaitCommand() : base(false) { }

        public override async Task ExecuteAsync(List<string> arguments)
        {
            if (arguments.Count != EXPECTED_ARGUMENTS)
            {
                Console.WriteLine("Expected " + EXPECTED_ARGUMENTS + " arguments but found " + arguments.Count + ".");
                return;
            }

            try
            {
                int sleep = Int32.Parse(arguments.ElementAt(0));
                Console.WriteLine("Sleeping..." + arguments.ElementAt(0));
                await Task.Delay(sleep);
                Console.WriteLine("Waking up...");
            }
            catch (FormatException)
            {
                Console.WriteLine("Argument must be of type int.");
                return;
            }

        }
    }
}
