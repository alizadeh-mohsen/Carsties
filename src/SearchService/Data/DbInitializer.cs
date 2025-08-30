using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;
using System.Text.Json;

namespace SearchService.Data
{
    public class DbInitializer
    {
        public static async Task InitDbAsync(WebApplication app)
        {
            try
            {
                await DB.InitAsync("SearchDb", MongoClientSettings
                    .FromConnectionString(app.Configuration.GetConnectionString("DefaultConnection")));

                await DB.Index<Item>()
                    .Key(x => x.Make, KeyType.Text)
                    .Key(x => x.Model, KeyType.Text)
                    .Key(x => x.Color, KeyType.Text)
                    .CreateAsync();

                var scope = app.Services.CreateScope();
                var auctionHttpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();
                var items = await auctionHttpClient.GetAuctionsAsync();
                Console.WriteLine($" >>>>>> {items.Count} returned from the auction service");
                
                if (items.Count > 0)
                    await DB.SaveAsync(items);

                Console.WriteLine($" >>>>>>> SUCCESS MongoDB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" >>>>>>> ERROR MongoDB: {ex.Message}");
            }
        }
    }
}
