namespace BattleShip.Models
{
    public record UndoRequest(int gameId, int moves);

    public record AttackRequest(int gameId, int row, int column);

    public class RestartGameRequest
    {
        public int gameId { get; set; }
        public int difficulty { get; set; }
        public int gridSize { get; set; }
        public Dictionary<char, List<List<int>>> playerBoatPositions { get; set; }
    }
}
