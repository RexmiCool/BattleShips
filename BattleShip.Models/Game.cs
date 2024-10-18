namespace BattleShip.Models
{
    public class Game
    {
        private int id;
        private bool multi;
        private int size;
        private int difficulty;
        private string turn;
        private User playerOne;
        private User playerTwo;
        private Dictionary<char, int> battleShips;
        private Grid playerOneGrid;
        private Grid botPlayerTwoGrid;
        private List<Move> history;
        private ProbabilityMap botProbabilityMap;
        private PerimeterAttack perimeterAttack;
        private Dictionary<string, Dictionary<char, int>> destructionCounts;
        private Dictionary<string, int> scoreBoard;


        public Game(bool multi,
                    User playerOne,
                    User playerTwo = null,
                    int difficulty = 2,
                    int size = 10,
                    Dictionary<char, int> battleShips = null,
                    Dictionary<char, List<List<int>>>? playerOneBoatPositions = null,
                    Dictionary<char, List<List<int>>>? playerTwoBoatPositions = null,
                    int playerOneScore = 0,
                    int botPlayerTwoScore = 0)
        {
            this.multi = multi;
            this.size = size;
            this.playerOne = playerOne;
            this.playerTwo = playerTwo;
            this.playerOneGrid = new Grid(size, size);
            this.botPlayerTwoGrid = new Grid(size, size);
            this.history = new List<Move>();
            this.turn = playerOne.getUsername();
            if (battleShips != null)
            {
                this.battleShips = battleShips;
            }
            else
            {
                this.battleShips = new Dictionary<char, int>() { { 'A', 1 }, { 'B', 2 }, { 'C', 2 }, { 'D', 3 }, { 'E', 3 }, { 'F', 4 } };
            }
            if (playerOneBoatPositions != null)
            {
                deployPlayerBattleShipsWithPositions(this.playerOneGrid, playerOneBoatPositions);
            }
            else
            {
                deployBattleShips(this.playerOneGrid);
            }
            if (playerTwoBoatPositions != null)
            {
                deployPlayerBattleShipsWithPositions(this.botPlayerTwoGrid, playerTwoBoatPositions);
            }
            else
            {
                deployBattleShips(this.botPlayerTwoGrid);
            }

            if(multi){
                destructionCounts = new Dictionary<string, Dictionary<char, int>>
                {
                    { playerOne.getUsername(), new Dictionary<char, int>() },
                    { playerTwo.getUsername(), new Dictionary<char, int>() }
                };
                this.scoreBoard = new Dictionary<string, int>
                {
                    { playerOne.getUsername(), playerOneScore },
                    { playerTwo.getUsername(), botPlayerTwoScore }
                };
                foreach (var ship in this.battleShips.Keys)
                {
                    destructionCounts[playerOne.getUsername()][ship] = 0;
                    destructionCounts[playerTwo.getUsername()][ship] = 0;
                }

            }
            else
            {
                this.difficulty = difficulty;
                this.perimeterAttack = new PerimeterAttack(size);

                destructionCounts = new Dictionary<string, Dictionary<char, int>>
                {
                    { playerOne.getUsername(), new Dictionary<char, int>() },
                    { "Bot", new Dictionary<char, int>() }
                };

                this.scoreBoard = new Dictionary<string, int>
                {
                    { playerOne.getUsername(), playerOneScore },
                    { "Bot", botPlayerTwoScore }
                };

                foreach (var ship in this.battleShips.Keys)
                {
                    destructionCounts[playerOne.getUsername()][ship] = 0;
                    destructionCounts["Bot"][ship] = 0;
                }

                this.botProbabilityMap = new ProbabilityMap(size, this.battleShips);
            }
            

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

        public bool getMulti(){
            return this.multi;
        }

        public User getPlayerOne(){
            return this.playerOne;
        }

        public User getPlayerTwo(){
            return this.playerTwo;
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
            foreach (var battleShip in this.battleShips)
            {
                int battleshipRow = 0, battleshipCol = 0;
                bool isBattleshipDeployable = false;
                bool isHorizontal;

                isHorizontal = Random.Shared.Next(2) == 1;

                while (!isBattleshipDeployable)
                {
                    // Choisir une position aléatoire pour le bateau
                    battleshipRow = Random.Shared.Next(this.size);
                    battleshipCol = Random.Shared.Next(this.size);
                    isHorizontal = Random.Shared.Next(2) == 1; // True pour horizontal, False pour vertical

                    // Vérifier si la position est valide pour le déploiement
                    isBattleshipDeployable = IsBattleshipPositionValid(battleShip.Value, battleshipRow, battleshipCol, isHorizontal, grid);
                }

                // Déployer le bateau
                for (int i = 0; i < battleShip.Value; i++)
                {
                    if (isHorizontal)
                    {
                        grid.UpdateCell(battleshipRow, battleshipCol + i, battleShip.Key); // Placer le bateau horizontalement
                    }
                    else
                    {
                        grid.UpdateCell(battleshipRow + i, battleshipCol, battleShip.Key); // Placer le bateau verticalement
                    }
                }
            }
        }

        // Vérifie si la position du bateau est valide (ne chevauche pas d'autres bateaux et reste dans les limites de la grille)
        private bool IsBattleshipPositionValid(int shipSize, int startRow, int startCol, bool isHorizontal, Grid grid)
        {
            for (int i = 0; i < shipSize; i++)
            {
                int row = isHorizontal ? startRow : startRow + i;
                int col = isHorizontal ? startCol + i : startCol;

                // Vérifier les limites de la grille
                if (row < 0 || row >= size || col < 0 || col >= size || !(grid.GetCell(row, col)=='\0'))
                {
                    return false; // Position non valide si hors limites ou si la cellule n'est pas vide
                }
            }
            return true; // La position est valide
        }


        private bool isCellEmpty(int row, int column)
        {
            if (row >= 0 && row < this.size && column >= 0 && column < this.size)
            {
                return this.playerOneGrid.GetCell(row, column) == '\0';
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

        public int[,] getPlayerOneGrid()
        {
            return this.playerOneGrid.GetGrid();
        }
        
        public Dictionary<char, List<List<int>>> getPlayerBoatLocation()
        {
            // Initialiser un dictionnaire pour stocker les positions des bateaux
            Dictionary<char, List<List<int>>> boatLocations = new Dictionary<char, List<List<int>>>();

            // Obtenir la grille du joueur
            int[,] grid = this.playerOneGrid.GetGrid();

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

            row = Random.Shared.Next(size);
            column = Random.Shared.Next(size);

            return (row, column);
        }

        // Attaque du bot et enregistrement dans l'historique
        public int EasyAttackBot(int row, int column)
        {
            bool hit = false;
            char shipType = '\0';

            while (playerOneGrid.GetCell(row, column) == 'X' || playerOneGrid.GetCell(row, column) == 'O')
            {
                (row, column) = EasyBotAttack();
            }

            if (playerOneGrid.GetCell(row, column) == 'X' || playerOneGrid.GetCell(row, column) == 'O')
            {
                return 0;
            }
            else
            {
                if (playerOneGrid.GetCell(row, column) != '\0')
                {
                    shipType = (char)playerOneGrid.GetCell(row, column);
                    playerOneGrid.UpdateCell(row, column, 'X');
                    hit = true;
                }
                else
                {
                    playerOneGrid.UpdateCell(row, column, 'O');
                }

                // Enregistrer le coup du bot dans l'historique
                history.Add(new Move("Bot", row, column, hit));

                if (hit && IsShipSunk(playerOneGrid, shipType))
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

            return (row, column);
        }

        // Mise à jour de la carte des tirs après chaque attaque du bot
        public int MediumAttackBot(int row, int column)
        {
            bool hit = false;
            char shipType = '\0';

            if (playerOneGrid.GetCell(row, column) == 'X' || playerOneGrid.GetCell(row, column) == 'O')
            {
                return 0;
            }
            else
            {
                if (playerOneGrid.GetCell(row, column) != '\0')
                {
                    shipType = (char)playerOneGrid.GetCell(row, column);
                    playerOneGrid.UpdateCell(row, column, 'X');
                    hit = true;
                }
                else
                {
                    playerOneGrid.UpdateCell(row, column, 'O');
                }

                // Enregistrer le coup du bot dans l'historique
                history.Add(new Move("Bot", row, column, hit));

                if (hit && IsShipSunk(playerOneGrid, shipType))
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
            
            return (row, column);
        }

        // Mise à jour de la carte des probabilités après chaque attaque du joueur
        public int HardAttackBot(int row, int column)
        {
            bool hit = false;
            char shipType = '\0';

            if (playerOneGrid.GetCell(row, column) == 'X' || playerOneGrid.GetCell(row, column) == 'O')
            {
                Console.WriteLine("BOT tape sur une case ou il a deja tape ==>BUG");
                return 0;
            }
            else
            {
                if (playerOneGrid.GetCell(row, column) != '\0')
                {
                    shipType = (char)playerOneGrid.GetCell(row, column);
                    playerOneGrid.UpdateCell(row, column, 'X');
                    hit = true;
                }
                else
                {
                    playerOneGrid.UpdateCell(row, column, 'O');
                }

                // Enregistrer le coup du bot dans l'historique
                history.Add(new Move("Bot", row, column, hit));

                if (hit && IsShipSunk(playerOneGrid, shipType))
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

            if (botPlayerTwoGrid.GetCell(row, column) == 'X' || botPlayerTwoGrid.GetCell(row, column) == 'O')
            {
                return 0;
            }
            else
            { 
                if (botPlayerTwoGrid.GetCell(row, column) != '\0')
                {
                    shipType = (char)botPlayerTwoGrid.GetCell(row, column);
                    botPlayerTwoGrid.UpdateCell(row, column, 'X');
                    hit = true;
                }
                else
                {
                    botPlayerTwoGrid.UpdateCell(row, column, 'O');
                }

                // Enregistrer le coup dans l'historique
                history.Add(new Move("Player", row, column, hit));

                if (hit && IsShipSunk(botPlayerTwoGrid, shipType))
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
            moves = moves * 2;
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
                    botPlayerTwoGrid.UpdateCell(lastMove.Row, lastMove.Column, '\0'); // Réinitialiser la case dans la grille du bot
                    this.botProbabilityMap.RestoreShotMap(lastMove.Row, lastMove.Column, lastMove.Hit); // Restaurer la carte des probabilités
                }
                else if (lastMove.Player == "Bot")
                {
                    // Réinitialiser la case
                    playerOneGrid.UpdateCell(lastMove.Row, lastMove.Column, '\0'); // Réinitialiser la case dans la grille du joueur
                    this.botProbabilityMap.RestoreShotMap(lastMove.Row, lastMove.Column, lastMove.Hit); // Restaurer la carte des probabilités
                }

                Console.WriteLine($"Undo {lastMove.Player}'s move at ({lastMove.Row}, {lastMove.Column})");
            }
        }

        // Vérifier s'il y a un gagnant
        public int? CheckForWinner()
        {
            if (IsAllShipsSunk(botPlayerTwoGrid))
            {
                this.scoreBoard["Player"]++;
                return 1;
            }
            if (IsAllShipsSunk(playerOneGrid))
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
