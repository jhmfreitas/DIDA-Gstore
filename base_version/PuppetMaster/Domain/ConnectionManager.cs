using Grpc.Net.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PuppetMaster.Domain
{
    class ConnectionManager
    {
        private static readonly int PCSPORT = 10000;
        private readonly ConcurrentDictionary<string, Server> serverSet;
        private readonly ConcurrentDictionary<string, Client> clientSet;
        private readonly ConcurrentDictionary<string, PCS> pcsSet;

        public ConnectionManager()
        {
            serverSet = new ConcurrentDictionary<string, Server>();
            clientSet = new ConcurrentDictionary<string, Client>();
            pcsSet = new ConcurrentDictionary<string, PCS>();
        }

        // Set NEW Connections

        public void SetNewServerConnection(string serverId, string url)
        {
            GrpcChannel channel = GrpcChannel.ForAddress(url);
            PuppetMasterServerService.PuppetMasterServerServiceClient client =
                   new PuppetMasterServerService.PuppetMasterServerServiceClient(channel);
            serverSet.TryAdd(serverId, new Server(serverId, client));

            SetNewPCSConnection(url);
        }

        public void SetNewClientConnection(string clientUsername, string url)
        {
            GrpcChannel channel = GrpcChannel.ForAddress(url);
            PuppetMasterClientService.PuppetMasterClientServiceClient client =
                new PuppetMasterClientService.PuppetMasterClientServiceClient(channel);
            clientSet.TryAdd(clientUsername, new Client(clientUsername, client));

            SetNewPCSConnection(url);
        }

        public void SetNewPCSConnection(string url)
        {
            string newUrl = GetPCSUrlFromAnUrl(url);

            GrpcChannel channel = GrpcChannel.ForAddress(newUrl);
            PuppetMasterPCSService.PuppetMasterPCSServiceClient client =
                new PuppetMasterPCSService.PuppetMasterPCSServiceClient(channel);
            pcsSet.TryAdd(newUrl, new PCS(client));
        }

        // Get Connections

        public List<Server> GetAllServerStubs()
        {
            return serverSet.Values.ToList();
        }

        public List<Client> GetAllClientStubs()
        {
            return clientSet.Values.ToList();
        }

        public List<PCS> GetAllPCSStubs()
        {
            return pcsSet.Values.ToList();
        }

        public Server GetServer(string serverId)
        {
            bool foundServer = serverSet.TryGetValue(serverId, out Server server);
            if (!foundServer)
            {
                throw new NodeBindException("Server '" + serverId + "' not found.");

            }
            return server;
        }

        public Client GetClient(string username)
        {
            bool foundClient = clientSet.TryGetValue(username, out Client client);
            if (!foundClient)
            {
                throw new NodeBindException("Client '" + username + "' not found.");

            }
            return client;
        }

        public PCS GetPCS(string pcsURL)
        {
            bool foundPCS = pcsSet.TryGetValue(pcsURL, out PCS pcs);
            if (!foundPCS)
            {
                throw new NodeBindException("PCS '" + pcsURL + "' not found.");

            }
            return pcs;
        }

        public string GetPCSUrlFromAnUrl(string url)
        {
            return url.Split(":").ElementAt(0) + ":" + url.Split(":").ElementAt(1) + ":" + PCSPORT;
        }

        public bool RemoveServerFromConfiguration(string serverId)
        {
            return serverSet.TryRemove(serverId, out _);
        }

        public bool RemoveClientFromConfiguration(string username)
        {
            return clientSet.TryRemove(username, out _);
        }
    }

    [Serializable]
    public class NodeBindException : Exception
    {
        public NodeBindException()
        { }

        public NodeBindException(string message)
            : base(message)
        { }

        public NodeBindException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
