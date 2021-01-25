using Client.Controllers;
using Client.Domain;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace Client.Commands
{
    class WriteCommand : Command
    {
        public static int EXPECTED_ARGUMENTS = 3;

        private ConnectionManager ConnectionManager;

        public WriteCommand(ConnectionManager connectionManager) : base(false)
        {
            ConnectionManager = connectionManager ?? throw new ArgumentNullException("ConnectionManager cannot be null.");
        }

        public override async Task ExecuteAsync(List<string> arguments)
        {
            if (arguments.Count != EXPECTED_ARGUMENTS)
            {
                Console.WriteLine("Expected " + EXPECTED_ARGUMENTS + " arguments but found " + arguments.Count + ".");
                return;
            }

            string partitionId = arguments.ElementAt(0);
            string objectId = arguments.ElementAt(1);
            string value = arguments.ElementAt(2);

            try
            {
                Console.WriteLine($"Write... {partitionId} {objectId} {value}");
                await WriteController.Execute(ConnectionManager, partitionId, objectId, value);
                Console.WriteLine($"Success...");
            }
            catch (ServerBindException e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
            }
            catch (RpcException ex) when(ex.StatusCode == StatusCode.Internal)
            {
                Console.WriteLine($"Could not establish connection with server.");
            }}
    }
}
