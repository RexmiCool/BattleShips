namespace BattleShip.Models
{
    public class GameState
    {
        private static GameState? _instance;

        // Grille du joueur (contient les lettres des bateaux)
        public char[,] PlayerGrid { get; private set; }

        // Grille de l'adversaire (null = jamais tiré, true = touché, false = raté)
        public bool?[,] OpponentGrid { get; private set; }

        public string Message { get; set; }

        public int GameId { get; set; }

        public GameState()
        {
            int size = 10;  // taille par défaut de la grille, 10x10
            PlayerGrid = new char[size, size];
            OpponentGrid = new bool?[size, size];
            Message = "";
            GameId = 0;
        }

        // Méthode statique pour obtenir l'instance unique du GameState
        public static GameState Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameState();
                }
                return _instance;
            }
        }

        // Méthode pour réinitialiser l'état du jeu (utile pour démarrer une nouvelle partie)
        public void ResetGame()
        {
            int size = PlayerGrid.GetLength(0);
            PlayerGrid = new char[size, size];
            OpponentGrid = new bool?[size, size];
            Message = "";
            GameId = 0;
        }

        // Méthode pour afficher les grilles (pour le debug)
        public void DisplayGrids()
        {
            Console.WriteLine("Grille du joueur :");
            DisplayGrid(PlayerGrid);
            Console.WriteLine("Grille de l'adversaire :");
            DisplayGrid(OpponentGrid);
        }

        private void DisplayGrid(char[,] grid)
        {
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    Console.Write(grid[row, col] + " ");
                }
                Console.WriteLine();
            }
        }

        private void DisplayGrid(bool?[,] grid)
        {
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    var cell = grid[row, col];
                    if (cell == null) Console.Write(". ");
                    else if (cell == true) Console.Write("X ");
                    else Console.Write("O ");
                }
                Console.WriteLine();
            }
        }
    }
}
