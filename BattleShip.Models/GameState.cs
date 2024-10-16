namespace BattleShip.Models
{
    public class GameState
    {
        private static readonly Lazy<GameState> instance = new Lazy<GameState>(() => new GameState());

        public static GameState Instance => instance.Value;

        public char[,] PlayerGrid { get; private set; }
        public bool?[,] OpponentGrid { get; private set; }

        public string Message { get; set; }

        public GameState()
        {
            // Initialisation des grilles
            PlayerGrid = new char[10, 10];  // Exemple de taille de grille
            OpponentGrid = new bool?[10, 10]; // NULL = jamais tiré, true = touché, false = raté
            Message = "";
        }

        public void ResetGame()
        {
            // Réinitialisation des grilles
            PlayerGrid = new char[10, 10];
            OpponentGrid = new bool?[10, 10];
            Message = "";
        }
    }
}
