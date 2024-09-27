namespace BattleShip.Models
{
    public class Game
    {
        private int id;
        private int size;
        private Dictionary<char, int> battleShips;
        private Grid playerGrid;
        private Grid botGrid;

        public Game()
        {
            this.size = 10;
            this.battleShips = new Dictionary<char, int>() { { 'A', 1 }, { 'B', 2 }, { 'C', 2 }, { 'D', 3 }, { 'E', 3 }, { 'F', 4 } };
            this.playerGrid = new Grid(size, size);
            this.botGrid = new Grid(size, size);

            this.playerGrid.DisplayGrid();
            Console.WriteLine("-------------------");
            deployBattleShips(this.playerGrid);
            this.playerGrid.DisplayGrid();
        }

        public Game(int size, Dictionary<char, int> battleShips)
        {
            this.size = size;
            this.battleShips = battleShips;
            this.playerGrid = new Grid(size, size);
            this.botGrid= new Grid(size, size);

            this.playerGrid.DisplayGrid();
            Console.WriteLine("-------------------");
            deployBattleShips(this.playerGrid);
            this.playerGrid.DisplayGrid();
        }

        public void deployBattleShips(Grid grid)
        {
            foreach (var battleShip in battleShips)
            {
                int battleshipRow = 0, battleshipCol = 0, direction = 0;
                bool isFirstPositionValid = false, isBattleshipDeployable = false, isHorizontal = false;

                while (!isBattleshipDeployable)
                {
                    while (!isFirstPositionValid)
                    {
                        battleshipRow = Random.Shared.Next(this.size);
                        battleshipCol = Random.Shared.Next(this.size);

                        isFirstPositionValid = isCellEmpty(battleshipRow, battleshipCol);
                    }

                    isHorizontal = Random.Shared.Next(2) == 1;
                    direction = Random.Shared.Next(2) == 0 ? 1 : -1;

                    isBattleshipDeployable = isBattleshipPositionValid(battleShip.Value, battleshipRow, battleshipCol, isHorizontal, direction);                    
                }

                for (int i = 0; i < battleShip.Value; i++)
                {
                    if (isHorizontal)
                    {
                        battleshipCol += direction;
                    }
                    else
                    {
                        battleshipRow += direction;
                    }
                    grid.UpdateCell(battleshipRow, battleshipCol, battleShip.Key);
                }
            }
        }

        private bool isCellEmpty(int row, int column)
        {
            if (row >= 0 && row < this.size && column >= 0 && column < this.size)
            {
                return this.playerGrid.GetCell(row, column) == '\0';
            }
            return false;
        }

        private bool isBattleshipPositionValid(int battleshipSize, int battleshipRow, int battleshipCol, bool isHorizontal, int direction)
        {
            for (int i = 0; i < battleshipSize; i++)
            {
                if (isHorizontal)
                {
                    battleshipCol += direction;
                } else
                {
                    battleshipRow += direction;
                }

                if (!isCellEmpty(battleshipRow, battleshipCol))
                {
                    return false;
                }
            }
            return true;
        }

        public int[,] getPlayerGrid()
        {
            return this.playerGrid.GetGrid();
        }
        /*public List<List<int>> getPlayerGrid()
        {
            int[,] gridArray = this.playerGrid.GetGrid();
            List<List<int>> gridList = new List<List<int>>();
            
            for (int i = 0; i < gridArray.GetLength(0); i++)
            {
                List<int> row = new List<int>();
                for (int j = 0; j < gridArray.GetLength(1); j++)
                {
                    row.Add(gridArray[i, j]);
                }
                gridList.Add(row);
            }
            
            return gridList;
        }*/
        
        public List<List<List<int>>> getPlayerBoatLocation()
        {
            List<List<List<int>>> BoatLocations = new List<List<List<int>>>();
            
            // Initialisation des sous-listes pour chaque bateau
            for (int k = 0; k < this.battleShips.Count; k++)
            {
                BoatLocations.Add(new List<List<int>>());
            }
            
            int[,] grid = this.playerGrid.GetGrid();

            for (int row = 0; row < this.size; row++)
            {
                for (int col = 0; col < this.size; col++)
                {
                    if (!isCellEmpty(row, col))
                    {
                        int index = grid[row, col] - 'A'; // Convertir en index ('A'-'A' = 0, 'B'-'A' = 1, etc.)
                        
                        if (index >= 0 && index < BoatLocations.Count)
                        {
                            BoatLocations[index].Add(new List<int> { row, col });
                        }
                    }
                }
            }
            return BoatLocations;
        }

        public int getId(){
            return this.id;
        }

        public void setId(int id){
            this.id = id;
        }

        // Attaque du joueur sur la grille de l'IA
        public int AttackPlayer(int row, int column)
        {
            if (botGrid.GetCell(row, column) != '\0') // Si un bateau est présent
            {
                botGrid.UpdateCell(row, column, 'X'); // Marquer comme touché
                return 1;
            }
            else
            {
                botGrid.UpdateCell(row, column, 'O'); // Marquer comme raté
                return 0;
            }
        }

        // Attaque de l'IA sur la grille du joueur
        public (int row, int column) BotAttack()
        {
            int row = Random.Shared.Next(size);
            int column = Random.Shared.Next(size);
            return (row, column);
        }

        // Traiter l'attaque de l'IA
        public int AttackBot(int row, int column)
        {
            if (playerGrid.GetCell(row, column) != '\0') // Si un bateau est présent
            {
                playerGrid.UpdateCell(row, column, 'X'); // Marquer comme touché
                return 1;
            }
            else
            {
                playerGrid.UpdateCell(row, column, 'O'); // Marquer comme raté
                return 0;
            }
        }

        // Vérifier s'il y a un gagnant
        public int? CheckForWinner()
        {
            if (IsAllShipsSunk(botGrid))
            {
                return 1;
            }
            if (IsAllShipsSunk(playerGrid))
            {
                return 2;
            }
            return null;
        }

        // Méthode pour vérifier si tous les bateaux sont coulés sur une grille
        private bool IsAllShipsSunk(Grid grid)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (grid.GetCell(i, j) != '\0' && grid.GetCell(i, j) != 'X')
                    {
                        return false; // Il reste encore des bateaux non coulés
                    }
                }
            }
            return true; // Tous les bateaux sont coulés
        }

    }
}
