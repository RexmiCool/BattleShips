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

builder.Services.AddGrpc();


var app = builder.Build();

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

Dictionary<int, Game> games = new Dictionary<int, Game>();
Dictionary<string, User> users = new Dictionary<string, User>();
Dictionary<string, int> leaderBoard = new Dictionary<string, int>();


app.MapPost("/user/new", async (HttpContext httpContext) =>
{
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<CreateUserRequest>(requestBody);

    string username = request.username;

    if (users.Keys.Contains(username))
    {
        return Results.Ok(new NewUserResponse(users[username].getId(), username));
    }

    User user = new User(username);
     
    int userId = users.Count;
    
    user.setId(userId);
    users.Add(username, user);

    var response = new NewUserResponse(user.getId(), user.getUsername());

    return Results.Ok(response);
})
.WithName("CreateNewUser")
.WithOpenApi();


// Endpoint pour créer une partie
app.MapPost("/game/new", async (HttpContext httpContext, IValidator <RestartGameRequest> validator) =>
{
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<RestartGameRequest>(requestBody);

    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    bool multi = request.multi;
    string playerOneUsername = request.playerOne;
    string playerTwoUsername = request.playerTwo;
    int difficulty = request.difficulty;
    int gridSize = request.gridSize;
    Dictionary<char, List<List<int>>> playerOneBoatPositions = request.playerOneBoatPositions;
    Dictionary<char, List<List<int>>> playerTwoBoatPositions = request.playerTwoBoatPositions;

    User playerOne = users[playerOneUsername];
    User playerTwo = null;

    if (multi)
    {
        playerTwo = users["playerTwoUsername"];
    }

    
    if (playerOneBoatPositions == null)
    {
        playerOneBoatPositions = null;
    }
    
    if (playerTwoBoatPositions == null)
    {
        playerTwoBoatPositions = null;
    }

    // Créer un nouvel objet Game
    Game game = new Game(multi,
                        playerOne,
                        playerTwo,
                        difficulty,
                        gridSize,
                        null,
                        playerOneBoatPositions,
                        playerTwoBoatPositions);
         
    game.setId(games.Count);
    games.Add(game.getId(), game);

    var response = new GameResponse(game.getId(), game.getSize(), game.getPlayerBoatLocation());

    return Results.Ok(response);
})
.WithName("CreateNewGame")
.WithOpenApi();

// Endpoint pour redémarrer une partie
app.MapPost("/game/restart", async (HttpContext httpContext, IValidator<RestartGameRequest> validator) =>
{
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<RestartGameRequest>(requestBody);

    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    int difficulty = request.difficulty;
    int gameId = request.gameId;
    int gridSize = request.gridSize;
    Dictionary<char, List<List<int>>> playerOneBoatPositions = request.playerOneBoatPositions;
    Dictionary<char, List<List<int>>> playerTwoBoatPositions = request.playerTwoBoatPositions;
    
    if (playerOneBoatPositions == null)
    {
        playerOneBoatPositions = null;
    }

    if (playerTwoBoatPositions != null)
    {
        playerTwoBoatPositions = null;
    }

    int playerOneScore = games[gameId].getScore(games[gameId].getPlayerOne().getUsername());
    int botPlayerTwoScore = 0;
    if (games[gameId].getMulti()){
        botPlayerTwoScore = games[gameId].getScore(games[gameId].getPlayerTwo().getUsername());
    }
    else{
        botPlayerTwoScore = games[gameId].getScore("Bot");
    }
    
    Game game = new Game(games[gameId].getMulti(),
                        games[gameId].getPlayerOne(),
                        games[gameId].getPlayerTwo(),
                        difficulty,
                        gridSize,
                        null,
                        playerOneBoatPositions,
                        playerTwoBoatPositions,
                        playerOneScore,
                        botPlayerTwoScore);

    game.setId(gameId);
    
    games[gameId] = game;

    var response = new GameResponse(gameId, game.getSize(), game.getPlayerBoatLocation());

    return Results.Ok(response);
})
.WithName("RestartGame")
.WithOpenApi();


app.MapPost("/game/attack", async (HttpContext httpContext, IValidator<AttackRequest> validator) =>
{
    var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<AttackRequest>(requestBody);

    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    int gameId = request.gameId;
    int row = request.row;
    int column = request.column;

    Game game = games[gameId];
    int playerAttack = game.AttackPlayer(row, column);
    
    int? winner = game.CheckForWinner();
    if (winner != null)
    {
        increaseLeaderboard(game, winner);
        return Results.Ok(new AttackResponse(winner, playerAttack, 0, new BotAttackCoordinates(-1, -1), game.GetDestructionCounts(), game.getScoreBoard()));
    }

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
    increaseLeaderboard(game, winner);

    return Results.Ok(new AttackResponse(winner, playerAttack, botAttack, new BotAttackCoordinates(botRow, botColumn), game.GetDestructionCounts(), game.getScoreBoard()));
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

    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    Game game = games[request.gameId];
    game.Undo(request.moves);

    return Results.Ok("Moves undone successfully.");
})
.WithName("UndoMoves")
.WithOpenApi();

app.MapGet("/game/leaderboard", () =>
{
    return Results.Ok(leaderBoard);
})
.WithName("GetLeaderboard")
.WithOpenApi();

app.MapGrpcService<BattleShipGRPCService>();

app.Run();

void increaseLeaderboard(Game game, int? winner){
    if(winner != null)
    {
        string playerName;
        if (winner == 1)
        {
            playerName = game.getPlayerOne().getUsername();
        }
        else{
            playerName = game.getPlayerTwo().getUsername();
        }
        if (leaderBoard.ContainsKey(playerName))
        {
            leaderBoard[playerName]++;
        }
        else
        {
            leaderBoard[playerName] = 1;
        }
    }
}

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
