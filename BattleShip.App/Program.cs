
using FluentValidation;
using BattleShip.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BattleShip.App
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5205") });

            builder.Services.AddSingleton<GameState>();

            // Ajouter les services FluentValidation
            builder.Services.AddTransient<IValidator<AttackRequest>, AttackRequestValidator>();
            builder.Services.AddTransient<IValidator<UndoRequest>, UndoRequestValidator>();
            builder.Services.AddTransient<IValidator<RestartGameRequest>, RestartGameRequestValidator>();

            await builder.Build().RunAsync();
        }
    }
}