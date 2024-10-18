namespace BattleShip.Models
{
    public class Move
    {
        public string Player { get; }
        public int Row { get; }
        public int Column { get; }
        public bool Hit { get; }

        public Move(string player, int row, int column, bool hit)
        {
            Player = player;
            Row = row;
            Column = column;
            Hit = hit;
        }

        public override string ToString()
        {
            return $"{Player} attacked ({Row}, {Column}) - {(Hit ? "Hit" : "Miss")}";
        }
    }
}
