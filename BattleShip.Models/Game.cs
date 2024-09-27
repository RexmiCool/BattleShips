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
    }
}
