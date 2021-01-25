using PuppetMaster.Domain;
using System.Threading.Tasks;

namespace PuppetMaster.Controllers.PCSControllers
{
    class CreateClientController
    {
        public static async Task Execute(ConnectionManager connectionManager, string username, string clientURL, string scriptFile, string servers, string partitions)
        {
            PCS pcs = connectionManager.GetPCS(connectionManager.GetPCSUrlFromAnUrl(clientURL));
            ClientRequest createClientRequest = new ClientRequest()
            {
                Username = username,
                ClientUrl = clientURL,
                ScriptFile = scriptFile,
                NetworkConfiguration = servers + partitions
            };
            await pcs.Stub.ClientAsync(createClientRequest);
        }
    }
}
