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
            // Réinitialiser la carte des probabilités
            Array.Clear(probMap, 0, probMap.Length);

            // Pour chaque taille de bateau
            foreach (var ship in shipSizes.Values)
            {
                int useSize = ship - 1; // Taille effective du bateau pour les vérifications
                // Parcourir toute la grille
                for (int row = 0; row < size; row++)
                {
                    for (int col = 0; col < size; col++)
                    {
                        if (shotMap[row, col] != 1) // Vérifier que cet endroit n'a pas encore été touché
                        {
                            // Calculer les positions où un bateau pourrait encore se trouver
                            AddShipEndpoints(row, col, useSize);
                        }
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
            shotMap[row, col] = hit ? 1 : -1;
            UpdateProbabilityMap();
        }

        public void RestoreShotMap(int row, int col, bool hit)
        {
            shotMap[row, col] = hit ? 1 : 0;
            UpdateProbabilityMap(); // Recalculer la carte des probabilités après restauration
        }
    }
}
