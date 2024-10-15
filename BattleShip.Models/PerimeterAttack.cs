using System;
using System.Collections.Generic;

namespace BattleShip.Models
{
    public class PerimeterAttack
    {
        private int[,] shotMap;
        private int size;
        private List<(int row, int col)> hitPositions; // Liste des coups réussis non encore totalement explorés
        private Random random;

        public PerimeterAttack(int gridSize)
        {
            this.size = gridSize;
            this.shotMap = new int[gridSize, gridSize];
            this.hitPositions = new List<(int, int)>();
            this.random = new Random();
        }

        // Méthode principale pour obtenir la prochaine attaque
        public (int row, int col) GetNextAttack()
        {
            if (hitPositions.Count > 0)
            {
                // Si on a déjà touché un navire, attaquer autour
                var (row, col) = hitPositions[0];
                var nextTarget = GetAdjacentPosition(row, col);
                if (nextTarget != (-1, -1))
                {
                    return nextTarget;
                }
                else
                {
                    // Si tous les adjacents ont été attaqués, enlever la position
                    hitPositions.RemoveAt(0);
                }
            }

            // Sinon, attaque aléatoire
            return GetRandomAttack();
        }

        // Récupérer une position adjacente valide pour attaquer
        private (int row, int col) GetAdjacentPosition(int row, int col)
        {
            var directions = new List<(int, int)> { (0, 1), (1, 0), (0, -1), (-1, 0) }; // Droite, bas, gauche, haut
            foreach (var (dx, dy) in directions)
            {
                int newRow = row + dx;
                int newCol = col + dy;
                if (IsValid(newRow, newCol) && shotMap[newRow, newCol] == 0)
                {
                    return (newRow, newCol);
                }
            }
            return (-1, -1); // Pas de position adjacente disponible
        }

        // Générer une attaque aléatoire sur une case non encore attaquée
        private (int row, int col) GetRandomAttack()
        {
            int row, col;
            do
            {
                row = random.Next(size);
                col = random.Next(size);
            }
            while (shotMap[row, col] != 0); // Répéter tant qu'on trouve une case non attaquée

            shotMap[row, col] = 1; // Marquer la case comme attaquée
            return (row, col);
        }

        // Vérifie si la position donnée est valide (dans les limites de la grille et non attaquée)
        private bool IsValid(int row, int col)
        {
            return row >= 0 && row < size && col >= 0 && col < size && shotMap[row, col] == 0;
        }

        // Met à jour la carte des tirs en fonction du résultat du coup (touché ou manqué)
        public void UpdateShotMap(int row, int col, bool hit)
        {
            shotMap[row, col] = hit ? 1 : -1; // 1 pour touché, -1 pour manqué
            if (hit)
            {
                hitPositions.Add((row, col)); // Ajouter aux positions à attaquer si c'est un coup réussi
            }
        }
    }
}
