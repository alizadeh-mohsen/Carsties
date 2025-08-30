using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionUpdatedConsumer(IMapper _mapper) : IConsumer<AuctionUpdated>
    {
        public async Task Consume(ConsumeContext<AuctionUpdated> context)
        {
            var updateItem = _mapper.Map<Item>(context.Message);
          
           var result= await DB.Update<Item>()
                 .Match(f => f.Eq(i => i.ID, updateItem.ID))
                 .Modify(i => i.Make, updateItem.Make)
                 .Modify(i => i.Model, updateItem.Model)
                 .Modify(i => i.Year, updateItem.Year)
                 .Modify(i => i.Color, updateItem.Color)
                 .Modify(i => i.Mileage, updateItem.Mileage)
                 .ExecuteAsync();

            if(!result.IsAcknowledged)
                Console.WriteLine($" >>>>>>> AuctionUpdatedConsumer: Failed to update Item with ID: {updateItem.ID}");
            else
                Console.WriteLine($" >>>>>>> AuctionUpdatedConsumer: Updated Item with ID: {updateItem.ID} - {updateItem.Make} {updateItem.Model}");
        }
    }
}