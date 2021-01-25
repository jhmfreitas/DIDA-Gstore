using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using PuppetMaster.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster.Controllers.PCSControllers
{
    class StatusController
    {
        public static async Task Execute(TextBox output, ConnectionManager connectionManager)
        {
            IDictionary<string, AsyncUnaryCall<Empty>> asyncUnaryCalls = new Dictionary<string, AsyncUnaryCall<Empty>>();
            foreach (Domain.Server server in connectionManager.GetAllServerStubs())
            {
                asyncUnaryCalls.Add(server.Id, server.Stub.StatusAsync(new Empty()));
            }

            foreach (Client client in connectionManager.GetAllClientStubs())
            {
                asyncUnaryCalls.Add(client.Username, client.Stub.StatusAsync(new Empty()));
            }

            // this is a dummy implementation (in the future it will receive information)
            List<Empty> statusReplies = new List<Empty>();
            foreach (KeyValuePair<string, AsyncUnaryCall<Empty>> request in asyncUnaryCalls)
            {
                try
                {
                    statusReplies.Add(await request.Value.ResponseAsync);
                }
                catch (RpcException e) when (e.StatusCode == StatusCode.Internal)
                {
                    output.AppendText(Environment.NewLine + request.Key + " is not respondig. It will be removed from the system configuration.");
                    if (!connectionManager.RemoveServerFromConfiguration(request.Key))
                    { // if didn't successfully removed from servers is because it was a client
                        connectionManager.RemoveClientFromConfiguration(request.Key);
                    }
                }
            }
        }
    }
}
