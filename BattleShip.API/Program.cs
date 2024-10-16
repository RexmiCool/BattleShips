using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using BattleShip.Models;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IValidator<RestartGameRequest>, RestartGameRequestValidator>();
builder.Services.AddScoped<IValidator<AttackRequest>, AttackRequestValidator>();
builder.Services.AddScoped<IValidator<UndoRequest>, UndoRequestValidator>();

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
app.MapPost("/game/new", async (HttpContext httpContext, IValidator <RestartGameRequest> validator) =>
{
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<RestartGameRequest>(requestBody);

    if (request == null)
    {
        return Results.BadRequest("Invalid request body");
    }

    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    int difficulty = request.difficulty;
    int gridSize = request.gridSize;
    Dictionary<char, List<List<int>>> playerBoatPositions = request.playerBoatPositions;

    // Vérifier la validité de la difficulté
    if (difficulty < 1 || difficulty > 3)
    {
        return Results.BadRequest("Invalid difficulty level. Must be 1 (easy), 2 (medium), or 3 (hard).");
    }

    // Vérifier la validité de la taille de grille
    if (gridSize != 8 && gridSize != 10 && gridSize != 12)
    {
        return Results.BadRequest("Invalid size for the grid. Must be 8 (easy), 10 (medium), or 12 (hard).");
    }
    
    if (playerBoatPositions != null)
    {
        if (!ValidateBoatPositions(playerBoatPositions, gridSize))
        {
            return Results.BadRequest("Invalid boat positions provided.");
        }
    }
    else
    {
        playerBoatPositions = null;
    }

    // Créer un nouvel objet Game
    Game game = new Game(difficulty, gridSize, null, playerBoatPositions);
     
    // Attribuer un ID au jeu (par exemple, le nombre de jeux existants)
    int gameId = games.Count;
    
    game.setId(gameId);
    games.Add(gameId, game);

    // Utiliser un record pour retourner la réponse
    var response = new GameResponse(gameId, game.getSize(), game.getPlayerBoatLocation());

    return Results.Ok(response);
})
.WithName("CreateNewGame")
.WithOpenApi();

// Endpoint pour redémarrer une partie
app.MapPost("/game/restart", async (HttpContext httpContext, IValidator<RestartGameRequest> validator) =>
{
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<RestartGameRequest>(requestBody);

    if (request == null)
    {
        return Results.BadRequest("Invalid request body");
    }

    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    int gameId = request.gameId;
    int difficulty = request.difficulty;
    int gridSize = request.gridSize;
    Dictionary<char, List<List<int>>> playerBoatPositions = request.playerBoatPositions;

    // Vérifier la validité de la difficulté
    if (difficulty < 1 || difficulty > 3)
    {
        return Results.BadRequest("Invalid difficulty level. Must be 1 (easy), 2 (medium), or 3 (hard).");
    }
    
    // Vérifier la validité de la taille de grille
    if (gridSize != 8 && gridSize != 10 && gridSize != 12)
    {
        return Results.BadRequest("Invalid size for the grid. Must be 8 (easy), 10 (medium), or 12 (hard).");
    }

    if (!games.ContainsKey(gameId))
    {
        return Results.NotFound($"Game with ID {gameId} not found");
    }

    if (playerBoatPositions != null)
    {
        if (!ValidateBoatPositions(playerBoatPositions, gridSize))
        {
            return Results.BadRequest("Invalid boat positions provided.");
        }
    }
    else
    {
        playerBoatPositions = null;
    }
    
    // Créer un nouvel objet Game
    Game game = new Game(difficulty, gridSize, null, playerBoatPositions);

    // Attribuer un ID au jeu
    game.setId(gameId);
    
    // Remplacer l'ancien jeu par le nouveau dans la collection
    games[gameId] = game;

    // Utiliser un record pour retourner la réponse
    var response = new GameResponse(gameId, game.getSize(), game.getPlayerBoatLocation());

    return Results.Ok(response);
})
.WithName("RestartGame")
.WithOpenApi();


app.MapPost("/game/attack", async (HttpContext httpContext, IValidator<AttackRequest> validator) =>
{
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<AttackRequest>(requestBody);

    if (request == null)
    {
        return Results.BadRequest("Invalid request body");
    }

    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    int gameId = request.gameId;
    int row = request.row;
    int column = request.column;

    if (!games.ContainsKey(gameId))
    {
        return Results.NotFound($"Game with ID {gameId} not found");
    }

    Game game = games[gameId];
    int playerAttack = game.AttackPlayer(row, column);
    
    int? winner = game.CheckForWinner();
    if (winner != null)
    {
        return Results.Ok(new AttackResponse(winner, playerAttack, 0, new BotAttackCoordinates(-1, -1), game.GetDestructionCounts()));
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
    
    winner = game.CheckForWinner();

    // Retourner l'état de l'attaque et du jeu
    return Results.Ok(new AttackResponse(winner, playerAttack, botAttack, new BotAttackCoordinates(botRow, botColumn), game.GetDestructionCounts()));
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

app.MapPost("/game/undo", async (HttpContext httpContext, IValidator<UndoRequest> validator) =>
{
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<UndoRequest>(requestBody);

    if (request == null || request.moves <= 0)
    {
        return Results.BadRequest("Invalid request body");
    }

    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    if (!games.ContainsKey(request.gameId))
    {
        return Results.NotFound($"Game with ID {request.gameId} not found");
    }

    Game game = games[request.gameId];
    game.Undo(request.moves);

    return Results.Ok("Moves undone successfully.");
})
.WithName("UndoMoves")
.WithOpenApi();


app.Run();

bool ValidateBoatPositions(Dictionary<char, List<List<int>>> playerBoatPositions, int gridSize)
{
    // Liste des tailles de bateaux correspondantes
    Dictionary<char, int> boatSizes = new Dictionary<char, int>()
    {
        { 'A', 1 }, { 'B', 2 }, { 'C', 2 }, { 'D', 3 }, { 'E', 3 }, { 'F', 4 }
    };

    foreach (var boat in playerBoatPositions)
    {
        char boatType = boat.Key;
        List<List<int>> positions = boat.Value;

        // Vérifier si le type de bateau existe
        if (!boatSizes.ContainsKey(boatType))
        {
            return false; // Type de bateau invalide
        }

        // Vérifier que le nombre de positions correspond à la taille du bateau
        if (positions.Count != boatSizes[boatType])
        {
            return false; // Taille de bateau incorrecte
        }

        // Vérifier que toutes les positions sont valides (dans les limites de la grille)
        foreach (var pos in positions)
        {
            int row = pos[0];
            int col = pos[1];

            if (row < 0 || row >= gridSize || col < 0 || col >= gridSize)
            {
                return false; // Position en dehors des limites de la grille
            }
        }
    }

    return true;
}