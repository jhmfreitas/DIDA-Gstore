using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using PuppetMaster.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PuppetMaster.Controllers.PCSControllers
{
    class ApplySystemConfigurationController
    {
        public static async Task Execute(ConnectionManager connectionManager, List<string> serverLines, string servers, string partitions)
        {
            List<AsyncUnaryCall<Empty>> asyncUnaryCalls = new List<AsyncUnaryCall<Empty>>();
            foreach (string line in serverLines)
            {
                string[] serverLineSplit = line.Split(" ");

                PCS pcs = connectionManager.GetPCS(connectionManager.GetPCSUrlFromAnUrl(serverLineSplit[1]));
                ServerRequest serverRequest = new ServerRequest()
                {
                    ServerId = serverLineSplit[0],
                    Url = serverLineSplit[1],
                    MinDelay = int.Parse(serverLineSplit[2]),
                    MaxDelay = int.Parse(serverLineSplit[3]),
                    NetworkConfiguration = servers + partitions
                };

                asyncUnaryCalls.Add(pcs.Stub.ServerAsync(serverRequest));
            }

            List<Empty> statusReplies = new List<Empty>();
            foreach (AsyncUnaryCall<Empty> request in asyncUnaryCalls)
            {
                statusReplies.Add(await request.ResponseAsync);
            }
        }
    }
}
