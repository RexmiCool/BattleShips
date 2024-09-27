using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip.Models
{
    internal class Game
    {
        private int size;
        private Dictionary<char, int> battleShips;
        private Grid playerGrid;
        private Grid botGrid;

        public Game()
        {
            this.size = 10;
            this.battleShips = new Dictionary<char, int>() { { 'A', 1 }, { 'B', 2 }, { 'C', 2 }, { 'D', 3 }, { 'E', 3 }, { 'F', 4 } };
            playerGrid = new Grid(size, size);
            botGrid = new Grid(size, size);

            deployBattleShips();
        }

        public Game(int size, Dictionary<char, int> battleShips)
        {
            this.size = size;
            this.battleShips = battleShips;
            playerGrid = new Grid(size, size);
            botGrid= new Grid(size, size);

            deployBattleShips();
        }

        public void deployBattleShips()
        {
            foreach (var battleShip in battleShips)
            {
                int column = 0;
                int row = 0;
                bool isFree = false;

                while (!isFree)
                {
                    column = Random.Shared.Next(this.size);
                    row = Random.Shared.Next(this.size);

                    isFree = isCellEmpty(row, column);
                }

                int orientation = Random.Shared.Next(4);

                //switch (orientation)
                //{
                //    case 0: // Droite
                //        for (int i = 0; i < battleShip.Value; i++)
                //        {
                //            column++;
                //            isFreisCellEmpty(row, column);
                //        }


                //}
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
