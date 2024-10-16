namespace BattleShip.Models {
    public record GameResponse(int gameId, int gridSize, Dictionary<char, List<List<int>>> boatLocations);

    public record BotAttackCoordinates(int row, int column);

    public record AttackResponse(
        int? winner,          // (null si personne n'a encore gagné)
        int playerAttack,      // Résultat de l'attaque du joueur ("Touché" ou "Raté")
        int botAttack,         // Résultat de l'attaque de l'IA ("Touché" ou "Raté")
        BotAttackCoordinates botAttackCoordinates,
        Dictionary<string, Dictionary<char, int>> destroyedBoatsCount
    );
}