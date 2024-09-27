using System.Data.Common;
using System.Reflection.Metadata.Ecma335;

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
    }
}
