namespace BattleShip.Models
{
    public class Game
    {
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
            deployBattleShips();
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
            deployBattleShips();
            this.playerGrid.DisplayGrid();
        }

        public void deployBattleShips()
        {
            foreach (var battleShip in battleShips)
            {
                int column = 0;
                int row = 0;
                bool isFree = false;
                bool isBoatPlaceFree = false;
                int orientation_1 = 0;
                int orientation_2 = 0;

                while (!isBoatPlaceFree)
                {
                    while (!isFree)
                    {
                        column = Random.Shared.Next(this.size);
                        row = Random.Shared.Next(this.size);

                        isFree = isCellEmpty(row, column);
                    }

                    orientation_1 = Random.Shared.Next(2);
                    orientation_2 = Random.Shared.Next(2) == 0 ? 1 : -1;
                    int i = 0;
                    isBoatPlaceFree = true;

                    if (orientation_1 == 0)
                    {
                        while (i < battleShip.Value && isBoatPlaceFree)
                        {
                            isBoatPlaceFree = isCellEmpty(row, column + (i * orientation_2));
                            i++;
                        }
                    }
                    else
                    {
                        while (i < battleShip.Value && isBoatPlaceFree)
                        {
                            isBoatPlaceFree = isCellEmpty(row + (i * orientation_2), column);
                            i++;
                        }
                    }
                }

                if (orientation_1 == 0)
                {
                    for (int i = 0; i < battleShip.Value; i++)
                    {
                        this.playerGrid.UpdateCell(row, column + (i * orientation_2), battleShip.Key);
                        }
                }
                else
                {
                    for (int i = 0; i < battleShip.Value; i++)
                    {
                        this.playerGrid.UpdateCell(row + (i * orientation_2), column, battleShip.Key);
                    }
                }
            }
        }

        public bool isCellEmpty(int row, int column)
        {
            if (row >= 0 && row < this.size && column >= 0 && column < this.size)
            {
                return this.playerGrid.GetCell(row, column) == '\0';
            }
            return false;
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
            for (int k = 0; k < 6; k++)
            {
                BoatLocations.Add(new List<List<int>>());
            }
            
            int[,] grid = this.playerGrid.GetGrid();

            for (int i = 0; i < this.size; i++)
            {
                for (int j = 0; j < this.size; j++)
                {
                    if (grid[i, j] != '\0') // Si la cellule contient un bateau
                    {
                        int index = grid[i, j] - 'A'; // Convertir en index (0 pour 'A', 1 pour 'B', etc.)
                        
                        if (index >= 0 && index < BoatLocations.Count)
                        {
                            BoatLocations[index].Add(new List<int> { i, j });
                        }
                    }
                }
            }

            return BoatLocations;
        }

    }
}
