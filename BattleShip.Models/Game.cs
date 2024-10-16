namespace BattleShip.Models
{
    public class Game
    {
        private int id;
        private int size;
        private int difficulty;
        private Dictionary<char, int> battleShips;
        private Grid playerGrid;
        private Grid botGrid;
        private List<(int, int)> botAttacks;
        private List<Move> history;
        private ProbabilityMap botProbabilityMap;
        private PerimeterAttack perimeterAttack;
        private Dictionary<string, Dictionary<char, int>> destructionCounts;
        private Dictionary<string, int> scoreBoard;


        public Game(int difficulty = 2, int size = 10, Dictionary<char, int> battleShips = null, Dictionary<char, List<List<int>>>? playerBoatPositions = null, int playerScore = 0, int botScore = 0)
        {
            this.size = size;
            this.difficulty = difficulty;
            this.playerGrid = new Grid(size, size);
            this.botGrid = new Grid(size, size);
            this.botAttacks = new List<(int, int)>();
            this.history = new List<Move>();
            this.perimeterAttack = new PerimeterAttack(size);

            if (battleShips != null)
            {
                this.battleShips = battleShips;
            }
            else
            {
                this.battleShips = new Dictionary<char, int>() { { 'A', 1 }, { 'B', 2 }, { 'C', 2 }, { 'D', 3 }, { 'E', 3 }, { 'F', 4 } };
            }

            destructionCounts = new Dictionary<string, Dictionary<char, int>>
            {
                { "Player", new Dictionary<char, int>() },
                { "Bot", new Dictionary<char, int>() }
            };

            this.scoreBoard = new Dictionary<string, int>
            {
                { "Player", playerScore },
                { "Bot", botScore }
            };

            foreach (var ship in this.battleShips.Keys)
            {
                destructionCounts["Player"][ship] = 0;
                destructionCounts["Bot"][ship] = 0;
            }

            if (playerBoatPositions != null)
            {
                deployPlayerBattleShipsWithPositions(this.playerGrid, playerBoatPositions);
            }
            else
            {
                deployBattleShips(this.playerGrid);
            }

            deployBattleShips(this.botGrid);

            this.botProbabilityMap = new ProbabilityMap(size, this.battleShips);

        }

        private void deployPlayerBattleShipsWithPositions(Grid grid, Dictionary<char, List<List<int>>> playerBoatPositions)
        {
            foreach (var boat in playerBoatPositions)
            {
                char boatType = boat.Key;
                List<List<int>> positions = boat.Value;

                // Vérifier que le bateau existe dans la liste des bateaux
                if (!battleShips.ContainsKey(boatType))
                {
                    throw new ArgumentException($"Invalid boat type: {boatType}");
                }

                // Vérifier que le nombre de positions correspond à la taille du bateau
                if (positions.Count != battleShips[boatType])
                {
                    throw new ArgumentException($"Incorrect number of positions for boat {boatType}. Expected {battleShips[boatType]} positions, got {positions.Count}");
                }

                // Placer le bateau sur la grille
                foreach (var pos in positions)
                {
                    int row = pos[0];
                    int col = pos[1];

                    if (isCellEmpty(row, col))
                    {
                        grid.UpdateCell(row, col, boatType);
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid position for boat {boatType} at ({row}, {col}). The cell is already occupied.");
                    }
                }
            }
        }


        public int getDifficulty(){
            return this.difficulty;
        }

        public int getScore(string player){
            return this.scoreBoard[player];
        }
        
        public Dictionary<string, int> getScoreBoard(){
            return this.scoreBoard;
        }

        public int getSize(){
            return this.size;
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
        
        public Dictionary<char, List<List<int>>> getPlayerBoatLocation()
        {
            // Initialiser un dictionnaire pour stocker les positions des bateaux
            Dictionary<char, List<List<int>>> boatLocations = new Dictionary<char, List<List<int>>>();

            // Obtenir la grille du joueur
            int[,] grid = this.playerGrid.GetGrid();

            // Parcourir la grille
            for (int row = 0; row < this.size; row++)
            {
                for (int col = 0; col < this.size; col++)
                {
                    // Vérifier si la case contient un bateau
                    if (!isCellEmpty(row, col))
                    {
                        char boatChar = (char)grid[row, col]; // Récupérer la lettre du bateau

                        // Ajouter les coordonnées du bateau dans le dictionnaire
                        if (!boatLocations.ContainsKey(boatChar))
                        {
                            boatLocations[boatChar] = new List<List<int>>();
                        }

                        boatLocations[boatChar].Add(new List<int> { row, col });
                    }
                }
            }

            return boatLocations;
        }


        public int getId(){
            return this.id;
        }

        public void setId(int id){
            this.id = id;
        }

        // Attaque de l'IA sur la grille du joueur
        public (int row, int column) EasyBotAttack()
        {
            int row, column;

            // Générer des coordonnées jusqu'à trouver une position non attaquée
            do
            {
                row = Random.Shared.Next(size);
                column = Random.Shared.Next(size);
            }
            while (botAttacks.Contains((row, column))); // Vérifier si le coup a déjà été joué

            // Ajouter les coordonnées à la liste des attaques effectuées
            botAttacks.Add((row, column));

            return (row, column);
        }

        // Attaque du bot et enregistrement dans l'historique
        public int EasyAttackBot(int row, int column)
        {
            bool hit = false;
            char shipType = '\0';

            if (playerGrid.GetCell(row, column) == 'X' || playerGrid.GetCell(row, column) == 'O')
            {
                return 0;
            }
            else
            {
                if (playerGrid.GetCell(row, column) != '\0')
                {
                    shipType = (char)playerGrid.GetCell(row, column);
                    playerGrid.UpdateCell(row, column, 'X');
                    hit = true;
                }
                else
                {
                    playerGrid.UpdateCell(row, column, 'O');
                }

                // Enregistrer le coup du bot dans l'historique
                history.Add(new Move("Bot", row, column, hit));

                if (hit && IsShipSunk(playerGrid, shipType))
                {
                    destructionCounts["Bot"][shipType]++;
                }

                return hit ? 1 : 0;
            }
        }

        // Méthode pour vérifier si un navire est complètement détruit
        private bool IsShipSunk(Grid grid, char shipType)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (grid.GetCell(i, j) == shipType)
                    {
                        return false; // Le navire n'est pas encore complètement détruit
                    }
                }
            }
            return true; // Le navire est complètement détruit
        }

        // Attaque de l'IA basée sur l'attaque par périmètre
        public (int row, int column) MediumBotAttack()
        {
            // Utiliser l'attaque par périmètre pour déterminer la prochaine attaque
            var (row, column) = this.perimeterAttack.GetNextAttack();
            
            // Ajouter les coordonnées à la liste des attaques effectuées
            botAttacks.Add((row, column));

            return (row, column);
        }

        // Mise à jour de la carte des tirs après chaque attaque du bot
        public int MediumAttackBot(int row, int column)
        {
            bool hit = false;
            char shipType = '\0';

            if (playerGrid.GetCell(row, column) == 'X' || playerGrid.GetCell(row, column) == 'O')
            {
                return 0;
            }
            else
            {
                if (playerGrid.GetCell(row, column) != '\0')
                {
                    shipType = (char)playerGrid.GetCell(row, column);
                    playerGrid.UpdateCell(row, column, 'X');
                    hit = true;
                }
                else
                {
                    playerGrid.UpdateCell(row, column, 'O');
                }

                // Enregistrer le coup du bot dans l'historique
                history.Add(new Move("Bot", row, column, hit));

                if (hit && IsShipSunk(playerGrid, shipType))
                {
                    destructionCounts["Bot"][shipType]++;
                }

                // Mise à jour de la carte des tirs dans l'attaque par périmètre
                this.perimeterAttack.UpdateShotMap(row, column, hit);

                return hit ? 1 : 0;
            }
        }

        // Attaque de l'IA basée sur la carte des probabilités
        public (int row, int column) HardBotAttack()
        {
            // Utiliser la carte des probabilités pour trouver la prochaine attaque intelligente
            var (row, column) = this.botProbabilityMap.GetNextAttack();
            
            // Ajouter les coordonnées à la liste des attaques effectuées
            botAttacks.Add((row, column));

            return (row, column);
        }

        // Mise à jour de la carte des probabilités après chaque attaque du joueur
        public int HardAttackBot(int row, int column)
        {
            bool hit = false;
            char shipType = '\0';

            if (playerGrid.GetCell(row, column) == 'X' || playerGrid.GetCell(row, column) == 'O')
            {
                Console.WriteLine("BOT tape sur une case ou il a deja tape ==>BUG");
                return 0;
            }
            else
            {
                if (playerGrid.GetCell(row, column) != '\0')
                {
                    shipType = (char)playerGrid.GetCell(row, column);
                    playerGrid.UpdateCell(row, column, 'X');
                    hit = true;
                }
                else
                {
                    playerGrid.UpdateCell(row, column, 'O');
                }

                // Enregistrer le coup du bot dans l'historique
                history.Add(new Move("Bot", row, column, hit));

                if (hit && IsShipSunk(playerGrid, shipType))
                {
                    destructionCounts["Bot"][shipType]++;
                }

                // Mise à jour de la carte des probabilités
                this.botProbabilityMap.UpdateShotMap(row, column, hit);

                Console.WriteLine("==========================================================================================");
                this.botProbabilityMap.displayProbMap();
                this.botProbabilityMap.displayShotMap();

                return hit ? 1 : 0;
            }
        }

        // Attaque du joueur et enregistrement dans l'historique
        public int AttackPlayer(int row, int column)
        {
            bool hit = false;
            char shipType = '\0';

            if (botGrid.GetCell(row, column) == 'X' || botGrid.GetCell(row, column) == 'O')
            {
                return 0;
            }
            else
            { 
                if (botGrid.GetCell(row, column) != '\0')
                {
                    shipType = (char)botGrid.GetCell(row, column);
                    botGrid.UpdateCell(row, column, 'X');
                    hit = true;
                }
                else
                {
                    botGrid.UpdateCell(row, column, 'O');
                }

                // Enregistrer le coup dans l'historique
                history.Add(new Move("Player", row, column, hit));

                if (hit && IsShipSunk(botGrid, shipType))
                {
                    destructionCounts["Player"][shipType]++;
                }

                return hit ? 1 : 0;
            }
        }

        // Obtenir le tableau de bord
        public Dictionary<string, Dictionary<char, int>> GetDestructionCounts()
        {
            return destructionCounts;
        }

        // Afficher l'historique des coups joués
        public List<Move> GetHistory()
        {
            return history;
        }

        // Revenir en arrière d'un nombre de coups
        public void Undo(int moves)
        {
            if (moves <= 0 || moves > history.Count)
            {
                Console.WriteLine("Invalid number of moves to undo.");
                return;
            }

            for (int i = 0; i < moves; i++)
            {
                // Supprimer le dernier coup de l'historique
                Move lastMove = history.Last();
                history.RemoveAt(history.Count - 1);

                // Restaurer l'état des grilles en fonction de l'historique supprimé
                if (lastMove.Player == "Player")
                {
                    // Réinitialiser la case
                    botGrid.UpdateCell(lastMove.Row, lastMove.Column, '\0'); // Réinitialiser la case dans la grille du bot
                    this.botProbabilityMap.RestoreShotMap(lastMove.Row, lastMove.Column, lastMove.Hit); // Restaurer la carte des probabilités
                }
                else if (lastMove.Player == "Bot")
                {
                    // Réinitialiser la case
                    playerGrid.UpdateCell(lastMove.Row, lastMove.Column, '\0'); // Réinitialiser la case dans la grille du joueur
                    this.botProbabilityMap.RestoreShotMap(lastMove.Row, lastMove.Column, lastMove.Hit); // Restaurer la carte des probabilités
                }

                Console.WriteLine($"Undo {lastMove.Player}'s move at ({lastMove.Row}, {lastMove.Column})");
            }
        }

        // Vérifier s'il y a un gagnant
        public int? CheckForWinner()
        {
            if (IsAllShipsSunk(botGrid))
            {
                this.scoreBoard["Player"]++;
                return 1;
            }
            if (IsAllShipsSunk(playerGrid))
            {                
                this.scoreBoard["Bot"]++;
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
                    if (grid.GetCell(i, j) != '\0' && grid.GetCell(i, j) != 'O' && grid.GetCell(i, j) != 'X')
                    {
                        return false; // Il reste encore des bateaux non coulés
                    }
                }
            }
            return true; // Tous les bateaux sont coulés
        }

    }
}
