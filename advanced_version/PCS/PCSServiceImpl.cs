using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace PCS
{
    class PCSServiceImpl : PuppetMasterPCSService.PuppetMasterPCSServiceBase
    {
        private const string CLIENT_LOCATION = "..\\..\\..\\..\\Client\\bin\\Debug\\netcoreapp3.1\\Client.exe";
        private const string SERVER_LOCATION = "..\\..\\..\\..\\Server\\bin\\Debug\\netcoreapp3.1\\Server.exe";

        public PCSServiceImpl() { }

        public override Task<Empty> Client(ClientRequest request, ServerCallContext context)
        {
            return Task.FromResult(ExecuteClient(request));
        }

        private Empty ExecuteClient(ClientRequest request)
        {
            Console.WriteLine($"Create Client request-> Username: {request.Username} Client_URL: {request.ClientUrl} Script: {request.ScriptFile}");
            try
            {
                var filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), CLIENT_LOCATION);
                ProcessStartInfo clientInfo = new ProcessStartInfo
                {
                    FileName = filepath,
                    UseShellExecute = true,
                    Arguments = $"{request.Username} {request.ClientUrl} {request.ScriptFile} \"{request.NetworkConfiguration}\""
                };
                Process exeClientProcess = Process.Start(clientInfo);

                Console.WriteLine($"Create Client DONE");
                return new Empty();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }

        public override Task<Empty> Server(ServerRequest request, ServerCallContext context)
        {
            return Task.FromResult(ExecuteServer(request));
        }

        private Empty ExecuteServer(ServerRequest request)
        {
            try
            {
                Console.WriteLine($"Create Server request-> Server_ID: {request.ServerId} URL: {request.Url} Min-Delay: {request.MinDelay} Max-Delay: {request.MaxDelay}");
                var filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SERVER_LOCATION);
                ProcessStartInfo serverInfo = new ProcessStartInfo
                {
                    FileName = filepath,
                    UseShellExecute = true,
                    Arguments = $"{request.ServerId} {request.Url} {request.MinDelay} {request.MaxDelay} \"{request.NetworkConfiguration}\""
                };
                Process exeServerProcess = Process.Start(serverInfo);
                Console.WriteLine($"Create Server DONE");
                return new Empty();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }
    }
}
