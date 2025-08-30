using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionCreatedConsumer(IMapper _mapper) : IConsumer<AuctionCreated>
    {
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            var newItem =  _mapper.Map<Item>(context.Message);
            await DB.SaveAsync(newItem);
            Console.WriteLine($" >>>>>>> AuctionCreatedConsumer: {newItem.ID} - {newItem.Make} {newItem.Model}");
        }
    }
}
