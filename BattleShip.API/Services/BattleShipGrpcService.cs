using Grpc.Core;

public class BattleShipGRPCService : BattleShipService.BattleShipServiceBase
{
    public override Task<DisplayResponseGRPC> DisplayMessage(DisplayRequestGRPC request, ServerCallContext context)
    {
        // Affiche le message reçu dans la console
        Console.WriteLine($"Message reçu : {request.Todisplay}");

        // Préparer la réponse
        var response = new DisplayResponseGRPC
        {
            Displayed = $"Message affiché : {request.Todisplay}"
        };

        // Retourner la réponse dans un Task
        return Task.FromResult(response);
    }
}
