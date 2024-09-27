using System.Text.Json;
using BattleShip.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

Dictionary<int, Game> games = new Dictionary<int, Game>();


app.MapGet("/game/new", () =>
{
    // Créer un nouvel objet Game
    Game game = new Game();
    
    // Attribuer un ID au jeu (par exemple, le nombre de jeux existants)
    int gameId = games.Count;
    game.setId(gameId);
    
    // Ajouter le jeu à la collection des jeux
    games.Add(gameId, game);

    // Utiliser un record pour retourner la réponse
    var response = new GameResponse(gameId, game.getPlayerBoatLocation());

    return response;
})
.WithName("GetNewGame")
.WithOpenApi();

app.MapPost("/game/attack", async (HttpContext httpContext) =>
{
    // Lire le corps de la requête
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<AttackRequest>(requestBody);

    // Vérifier si le modèle est valide
    if (request == null)
    {
        return Results.BadRequest("Invalid request body");
    }

    int gameId = request.GameId;
    int row = request.Row;
    int column = request.Column;

    // Vérifier si le jeu existe
    if (!games.ContainsKey(gameId))
    {
        return Results.NotFound($"Game with ID {gameId} not found");
    }

    // Récupérer le jeu
    Game game = games[gameId];

    // Effectuer l'attaque du joueur
    int playerAttack = game.AttackPlayer(row, column);

    // Vérifier si le joueur a gagné
    int? winner = game.CheckForWinner();
    if (winner != null)
    {
        return Results.Ok(new AttackResponse(winner, playerAttack, 0, (-1, -1)));
    }

    // Faire attaquer l'IA
    (int botRow, int botColumn) = game.BotAttack();
    int botAttack = game.AttackBot(botRow, botColumn);

    // Vérifier si l'IA a gagné
    winner = game.CheckForWinner();

    // Retourner l'état de l'attaque et du jeu
    return Results.Ok(new AttackResponse(winner, playerAttack, botAttack, (botRow, botColumn)));
})
.WithName("MakeAttack")
.WithOpenApi();


app.Run();

record GameResponse(int GameId, List<List<List<int>>> BoatLocations);

record AttackResponse(
    int? Winner,          // L'identité du gagnant (null si personne n'a encore gagné)
    int PlayerAttack,      // Résultat de l'attaque du joueur ("Touché" ou "Raté")
    int BotAttack,         // Résultat de l'attaque de l'IA ("Touché" ou "Raté")
    (int Row, int Column) BotAttackCoordinates  // Coordonnées de l'attaque de l'IA
);
record AttackRequest(int GameId, int Row, int Column);

// {
//   "gameId": 1,
//   "row": 3,
//   "column": 4
// }