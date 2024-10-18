namespace BattleShip.Models {
    public record GameResponse(int gameId, int gridSize, Dictionary<char, List<List<int>>> boatLocations);

    public record BotAttackCoordinates(int row, int column);

    public record AttackResponse(
        int? winner,          // (null si personne n'a encore gagn�)
        int playerAttack,      // R�sultat de l'attaque du joueur ("Touch�" ou "Rat�")
        int botAttack,         // R�sultat de l'attaque de l'IA ("Touch�" ou "Rat�")
        BotAttackCoordinates botAttackCoordinates,
        Dictionary<string, Dictionary<char, int>> destroyedBoatsCount, 
        Dictionary<string, int> scoreBoard
    );

    public record NewUserResponse(
        int userId,
        string username
    );

    
}