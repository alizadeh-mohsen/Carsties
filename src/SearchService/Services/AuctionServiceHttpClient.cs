using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    public class AuctionServiceHttpClient(HttpClient _httpClient, IConfiguration _configuration)
    {
        public async Task<List<Item>> GetAuctionsAsync()
        {
            var count = await DB.CountAsync<Item>();
            var uri = _configuration["AuctionServiceUrl"] + "/api/auctions";

            if (count > 0)
            {
                var latestAuctionsJson = await DB.Find<Item, string>()
                    .Sort(x => x.Descending(a => a.UpdatedAt))
                    .Project(x => x.UpdatedAt.ToUniversalTime().ToString())
                    .ExecuteAnyAsync();
                uri += "?date=" + latestAuctionsJson;
            }

            var response = await _httpClient.
                GetFromJsonAsync<List<Item>>(uri);

            return response;

        }
    }
}
