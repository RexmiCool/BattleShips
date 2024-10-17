// public class UndoMoveService
// {
//     private readonly Dictionary<int, Game> _games;

//     public UndoMoveService(Dictionary<int, Game> games)
//     {
//         _games = games;
//     }

//     public string UndoMoves(UndoRequest request)
//     {
//         if (!_games.ContainsKey(request.gameId))
//         {
//             throw new Exception($"Game with ID {request.gameId} not found");
//         }

//         Game game = _games[request.gameId];
//         game.Undo(request.moves);
//         return "Moves undone successfully.";
//     }
// }
