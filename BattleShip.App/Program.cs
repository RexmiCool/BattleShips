using BattleShip.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http;
using System.Threading.Tasks;

namespace BattleShip.App
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            // Configurez l'adresse de base de l'API
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5205") });

            builder.Services.AddSingleton<GameState>();

            await builder.Build().RunAsync();
        }
    }
}