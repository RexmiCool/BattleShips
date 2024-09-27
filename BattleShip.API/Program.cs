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

app.Run();

record GameResponse(int GameId, List<List<List<int>>> BoatLocations);

