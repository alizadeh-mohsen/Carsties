using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController(AuctionDbContext _context, IMapper _mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAuctions(string? date)
        {
            var query = _context.Auctions.OrderBy(a => a.Item.Make).AsQueryable();

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDateTime))
            {
                query = query.Where(a => a.UpdatedAt > parsedDateTime.ToUniversalTime());
            }
            var auctions = await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
            return Ok(auctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auctions = await _context.Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auctions == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AuctionDto>(auctions));
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);
            auction.Seller = "todo";
            _context.Auctions.Add(auction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, _mapper.Map<AuctionDto>(auction));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
        {
            var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Year = auctionDto.Year;
            auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = auctionDto.Mileage;

            var result = await _context.SaveChangesAsync() > 0;
            if (!result)
            {
                return BadRequest("Failed to update auction");
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuction(Guid id)
        {

            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null)
            {
                return NotFound();
            }

            _context.Auctions.Remove(auction);
            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Failed to delete auction");
            }
            return Ok();
        }
    }
}
