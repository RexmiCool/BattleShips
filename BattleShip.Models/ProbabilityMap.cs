using System;
using System.Linq;

namespace BattleShip.Models
{
    public class ProbabilityMap
    {
        private int[,] shotMap;
        private int[,] probMap;
        private int size;
        private Dictionary<char, int> shipSizes;

        public ProbabilityMap(int gridSize, Dictionary<char, int> ships)
        {
            this.size = gridSize;
            this.shipSizes = ships;
            this.shotMap = new int[gridSize, gridSize];
            this.probMap = new int[gridSize, gridSize];
            UpdateProbabilityMap();
        }

        // Génère ou met à jour la carte des probabilités en fonction des coups précédents
        public void UpdateProbabilityMap()
        {
            // Réinitialiser les cellules qui n'ont pas été touchées ou manquées
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (shotMap[row, col] == 0)  // Si la cellule n'a pas été touchée ou manquée
                    {
                        probMap[row, col] = 0;  // Réinitialiser la probabilité
                    }
                    // Ne réinitialise pas les cases avec un bateau touché
                }
            }

            // Calculer les probabilités pour les bateaux restants
            foreach (var ship in shipSizes.Values)
            {
                int useSize = ship - 1;  // Taille effective du bateau pour les vérifications
                for (int row = 0; row < size; row++)
                {
                    for (int col = 0; col < size; col++)
                    {
                        if (shotMap[row, col] == 0) // Ne considérer que les cases non touchées
                        {
                            AddShipEndpoints(row, col, useSize);
                        }
                        // Ne réinitialise pas les cases où un bateau a été touché (shotMap[row, col] == 1)
                    }
                }
            }
        }


        // Fonction auxiliaire pour ajouter les "endpoints" du bateau
        private void AddShipEndpoints(int row, int col, int shipSize)
        {
            // Vérifier si le bateau peut tenir horizontalement et verticalement à partir de la case (row, col)
            if (row - shipSize >= 0 && CanPlaceShip(row, col, -1, 0, shipSize))
                IncrementProbabilities(row - shipSize, col, row + 1, col + 1);
            if (row + shipSize < size && CanPlaceShip(row, col, 1, 0, shipSize))
                IncrementProbabilities(row, col, row + shipSize + 1, col + 1);
            if (col - shipSize >= 0 && CanPlaceShip(row, col, 0, -1, shipSize))
                IncrementProbabilities(row, col - shipSize, row + 1, col + 1);
            if (col + shipSize < size && CanPlaceShip(row, col, 0, 1, shipSize))
                IncrementProbabilities(row, col, row + 1, col + shipSize + 1);
        }

        // Vérifie si un bateau peut être placé dans une direction donnée
        private bool CanPlaceShip(int row, int col, int rowStep, int colStep, int shipSize)
        {
            for (int i = 0; i <= shipSize; i++)
            {
                int newRow = row + i * rowStep;
                int newCol = col + i * colStep;
                if (shotMap[newRow, newCol] != 0) // Si une case a déjà été attaquée ou est bloquée
                    return false;
            }
            return true;
        }

        // Incrémenter la probabilité des cellules dans une zone donnée
        private void IncrementProbabilities(int startRow, int startCol, int endRow, int endCol)
        {
            for (int i = startRow; i < endRow; i++)
            {
                for (int j = startCol; j < endCol; j++)
                {
                    probMap[i, j]++;
                }
            }
        }

        private void IncrementAdjacentProbabilities(int row, int col)
        {
            // Vérifier les 4 directions adjacentes (haut, bas, gauche, droite)
            if (row - 1 >= 0 && shotMap[row - 1, col] == 0)
                probMap[row - 1, col] += 5;  // Augmenter les probabilités de manière significative
            if (row + 1 < size && shotMap[row + 1, col] == 0)
                probMap[row + 1, col] += 5;
            if (col - 1 >= 0 && shotMap[row, col - 1] == 0)
                probMap[row, col - 1] += 5;
            if (col + 1 < size && shotMap[row, col + 1] == 0)
                probMap[row, col + 1] += 5;
        }


        // Récupérer la prochaine attaque intelligente basée sur la carte des probabilités
        public (int row, int col) GetNextAttack()
        {
            int maxProbability = probMap.Cast<int>().Max();
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (probMap[row, col] == maxProbability)
                    {
                        // Marquer cet endroit comme attaqué dans le shotMap
                        shotMap[row, col] = 1;
                        return (row, col);
                    }
                }
            }
            // Fallback, ne devrait jamais arriver
            return (0, 0);
        }

        // Met à jour la carte des tirs en fonction des coups réussis/ratés
        public void UpdateShotMap(int row, int col, bool hit)
        {
            if (hit)
            {
                // Marquer la cellule comme touchée
                shotMap[row, col] = 1;

                // Ne pas effacer la probabilité des coups réussis
                // mais plutôt augmenter celle des cases adjacentes
                IncrementAdjacentProbabilities(row, col);
            }
            else
            {
                // Marquer la cellule comme manquée
                shotMap[row, col] = -1;
            }

            // Mettre à jour la carte des probabilités, mais garder en mémoire les coups réussis
            UpdateProbabilityMap();
        }


        public void RestoreShotMap(int row, int col, bool hit)
        {
            shotMap[row, col] = hit ? 1 : 0;
            UpdateProbabilityMap(); // Recalculer la carte des probabilités après restauration
        }

        public void displayProbMap(){
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("ProbMap");
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Console.Write(this.probMap[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("-----------------------------------------");
        }

        public void displayShotMap(){
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("ShotMap");
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Console.Write(this.shotMap[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("-----------------------------------------");
        }
    }
}
