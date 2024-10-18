namespace BattleShip.App.Services
{
    using Grpc.Net.Client;
    using System.Threading.Tasks;

    public class BattleShipServiceClient
    {
        private readonly BattleShipService.BattleShipServiceClient _client;

        public BattleShipServiceClient(GrpcChannel channel)
        {
            _client = new BattleShipService.BattleShipServiceClient(channel);
        }

        // Méthode pour appeler le service DisplayMessage
        public async Task<string> DisplayMessageAsync(string message)
        {
            // Créer une requête
            var request = new DisplayRequestGRPC { Todisplay = message };

            // Appeler le service gRPC
            var response = await _client.DisplayMessageAsync(request);

            // Retourner le message affiché
            return response.Displayed;
        }
    }

}
