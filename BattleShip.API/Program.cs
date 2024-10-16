using System.Text.Json;
using BattleShip.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

Dictionary<int, Game> games = new Dictionary<int, Game>();

// Endpoint pour créer une partie
app.MapPost("/game/new", async (HttpContext httpContext) =>
{
    // Lire le corps de la requête
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<RestartGameRequest>(requestBody);

    // Vérifier si le modèle est valide
    if (request == null)
    {
        return Results.BadRequest("Invalid request body");
    }

    int difficulty = request.Difficulty;

    // Vérifier la validité de la difficulté
    if (difficulty < 1 || difficulty > 3)
    {
        return Results.BadRequest("Invalid difficulty level. Must be 1 (easy), 2 (medium), or 3 (hard).");
    }
    
    // Créer un nouvel objet Game
    Game game = new Game(difficulty);
     
    // Attribuer un ID au jeu (par exemple, le nombre de jeux existants)
    int gameId = games.Count;
    game.setId(gameId);
    
    // Ajouter le jeu à la collection des jeux
    games.Add(gameId, game);

    // Utiliser un record pour retourner la réponse
    var response = new GameResponse(gameId, game.getPlayerBoatLocation());

    return Results.Ok(response);
})
.WithName("CreateNewGame")
.WithOpenApi();

// Endpoint pour redémarrer une partie
app.MapPost("/game/restart", async (HttpContext httpContext) =>
{
    // Lire le corps de la requête
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<RestartGameRequest>(requestBody);

    // Vérifier si le modèle est valide
    if (request == null)
    {
        return Results.BadRequest("Invalid request body");
    }

    int gameId = request.GameId;
    int difficulty = request.Difficulty;

    // Vérifier la validité de la difficulté
    if (difficulty < 1 || difficulty > 3)
    {
        return Results.BadRequest("Invalid difficulty level. Must be 1 (easy), 2 (medium), or 3 (hard).");
    }

    if (!games.ContainsKey(gameId))
    {
        return Results.NotFound($"Game with ID {gameId} not found");
    }
    
    // Créer un nouvel objet Game
    Game game = new Game(difficulty);
    
    // Attribuer un ID au jeu
    game.setId(gameId);
    
    // Remplacer l'ancien jeu par le nouveau dans la collection
    games[gameId] = game;

    // Utiliser un record pour retourner la réponse
    var response = new GameResponse(gameId, game.getPlayerBoatLocation());

    return Results.Ok(response);
})
.WithName("RestartGame")
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
        return Results.Ok(new AttackResponse(winner, playerAttack, 0, new BotAttackCoordinates(-1, -1)));
    }

    // Faire attaquer l'IA
    int botAttack = 0;
    int botRow, botColumn;
    if (game.getDifficulty() == 1){
        (botRow, botColumn) = game.EasyBotAttack();
        botAttack = game.EasyAttackBot(botRow, botColumn);
    }
    else if (game.getDifficulty() == 3)
    {
        (botRow, botColumn) = game.HardBotAttack();
        botAttack = game.HardAttackBot(botRow, botColumn);
    }
    else
    {
        (botRow, botColumn) = game.MediumBotAttack();
        botAttack = game.MediumAttackBot(botRow, botColumn);
    }
    

    // Vérifier si l'IA a gagné
    winner = game.CheckForWinner();

    // Retourner l'état de l'attaque et du jeu
    return Results.Ok(new AttackResponse(winner, playerAttack, botAttack, new BotAttackCoordinates(botRow, botColumn)));
})
.WithName("MakeAttack")
.WithOpenApi();

app.MapGet("/game/history", (int gameId) =>
{
    if (!games.ContainsKey(gameId))
    {
        return Results.NotFound($"Game with ID {gameId} not found");
    }

    Game game = games[gameId];
    var history = game.GetHistory();
    return Results.Ok(history.Select(move => move.ToString()));
})
.WithName("GetHistory")
.WithOpenApi();

app.MapPost("/game/undo", async (HttpContext httpContext) =>
{
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<UndoRequest>(requestBody);

    if (request == null || request.Moves <= 0)
    {
        return Results.BadRequest("Invalid request body");
    }

    if (!games.ContainsKey(request.GameId))
    {
        return Results.NotFound($"Game with ID {request.GameId} not found");
    }

    Game game = games[request.GameId];
    game.Undo(request.Moves);

    return Results.Ok("Moves undone successfully.");
})
.WithName("UndoMoves")
.WithOpenApi();


app.Run();

record UndoRequest(int GameId, int Moves);

record GameResponse(int GameId, List<List<List<int>>> BoatLocations);

record BotAttackCoordinates(int Row, int Column);

record AttackResponse(
    int? Winner,          // L'identité du gagnant (null si personne n'a encore gagné)
    int PlayerAttack,      // Résultat de l'attaque du joueur ("Touché" ou "Raté")
    int BotAttack,         // Résultat de l'attaque de l'IA ("Touché" ou "Raté")
    BotAttackCoordinates BotAttackCoordinates  // Coordonnées de l'attaque de l'IA
);
record AttackRequest(int GameId, int Row, int Column);

public class RestartGameRequest
{
    public int GameId { get; set; }
    public int Difficulty { get; set; }
}
