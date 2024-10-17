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

        public int GridSize { get; private set; }  // Taille de la grille choisie

        public bool? GameFinished { get; set; }

        public GameState()
        {
            Message = "";
            GameId = 0;
            GameFinished = false;
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

        // Méthode pour définir la taille de la grille
        public void Initialize(int gridSize)
        {
            GridSize = gridSize;
            PlayerGrid = new char[GridSize, GridSize];
            OpponentGrid = new bool?[GridSize, GridSize];
            Message = "";
            GameId = 0;
            GameFinished = false;
        }

        // Méthode pour réinitialiser l'état du jeu selon la nouvelle taille
        public void ResetGame(int gridSize)
        {
            Initialize(gridSize);  // Réutiliser la méthode d'initialisation
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