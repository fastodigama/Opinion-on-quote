using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Opinion_on_Quotes.Data;
using Opinion_on_Quotes.Interfaces;
using Opinion_on_Quotes.Models;

namespace Opinion_on_Quotes.Services
{
    public class QuoteService : IQuoteServices
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        // Inject UserManager and database context
        public QuoteService(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Retrieve all quotes with their associated drama and comments
        public async Task<IEnumerable<QuoteDto>> ListQuotes()
        {
            List<Quote> quotes = await _context.Quotes
                .Include(q => q.Comments)
                .Include(q => q.Drama)
                .ToListAsync();

            List<QuoteDto> quoteDtos = new List<QuoteDto>();

            foreach (var quote in quotes)
            {
                // Convert each Quote entity to a QuoteDto
                quoteDtos.Add(new QuoteDto
                {
                    quote_id = quote.quote_id,
                    content = quote.content,
                    actor = quote.actor,
                    episode = quote.episode,
                    drama_id = quote.drama_id,
                    drama_title = quote.Drama?.title
                });
            }

            return quoteDtos;
        }

        // Find a specific quote by ID and include its comments
        public async Task<QuoteDto?> FindQuote(int id)
        {
            var quote = await _context.Quotes
                .Include(q => q.Comments)
                .Include(q => q.Drama)
                .FirstOrDefaultAsync(c => c.quote_id == id);

            if (quote == null)
            {
                return null; // Quote not found
            }

            List<CommentDto> commentDtos = new List<CommentDto>();

            // Convert each comment to CommentDto
            foreach (var comment in quote.Comments)
            {
                var user = await _userManager.FindByIdAsync(comment.UserId);
                string username = user?.UserName ?? "Anonymous";

                commentDtos.Add(new CommentDto
                {
                    CommentId = comment.CommentId,
                    CommentText = comment.CommentText,
                    CreatedAt = comment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UserName = username,
                    quote_id = comment.quote_id
                });
            }

            // Convert Quote entity to QuoteDto
            QuoteDto QuoteDtos = new QuoteDto()
            {
                quote_id = quote.quote_id,
                content = quote.content,
                actor = quote.actor,
                episode = quote.episode,
                drama_id = quote.drama_id,
                drama_title = quote.Drama?.title,
                comments = commentDtos
            };

            return QuoteDtos;
        }

        // Update an existing quote
        public async Task<ServiceResponse> UpdateQuote(QuoteDto QuoteDto)
        {
            ServiceResponse serviceResponse = new();

            var drama = await _context.Dramas.FindAsync(QuoteDto.drama_id); // Validate drama exists
            var quoteToUpdate = await _context.Quotes.FindAsync(QuoteDto.quote_id); // Find quote

            if (quoteToUpdate == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add($"Quote with ID {QuoteDto.quote_id} not found.");
                return serviceResponse;
            }

            if (drama == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add($"Drama with ID {QuoteDto.drama_id} not found.");
                return serviceResponse;
            }

            // Update quote fields
            quoteToUpdate.content = QuoteDto.content;
            quoteToUpdate.actor = QuoteDto.actor;
            quoteToUpdate.episode = QuoteDto.episode;
            quoteToUpdate.Drama = drama;

            try
            {
                await _context.SaveChangesAsync(); // Save changes
                serviceResponse.Status = ServiceResponse.ServiceStatus.Updated;
            }
            catch (DbUpdateConcurrencyException)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("An error occurred updating the record");
                return serviceResponse;
            }

            return serviceResponse;
        }

        // Add a new quote to the database
        public async Task<ServiceResponse> AddQuote(QuoteDto QuoteDto)
        {
            ServiceResponse serviceResponse = new();

            var drama = await _context.Dramas.FindAsync(QuoteDto.drama_id); // Validate drama exists

            if (drama == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add($"Drama with ID {QuoteDto.drama_id} not found.");
                return serviceResponse;
            }

            // Create new Quote entity
            Quote Quote = new Quote()
            {
                quote_id = QuoteDto.quote_id,
                content = QuoteDto.content,
                actor = QuoteDto.actor,
                episode = QuoteDto.episode,
                Drama = drama
            };

            try
            {
                _context.Quotes.Add(Quote); // Add to context
                var rows = await _context.SaveChangesAsync(); // Save changes

                if (rows > 0)
                {
                    serviceResponse.Status = ServiceResponse.ServiceStatus.Created;
                    serviceResponse.CreatedId = Quote.quote_id;
                }
                else
                {
                    serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                    serviceResponse.Messages.Add("Drama could not be added.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while saving quote: {ex.Message}");
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("There was an error adding the Quote.");
                if (ex.InnerException != null)
                {
                    serviceResponse.Messages.Add(ex.InnerException.Message);
                }
            }

            return serviceResponse;
        }

        // Delete a quote by ID
        public async Task<ServiceResponse> DeleteQuote(int id)
        {
            ServiceResponse response = new();

            var Quote = await _context.Quotes.FindAsync(id); // Find quote

            if (Quote == null)
            {
                response.Status = ServiceResponse.ServiceStatus.NotFound;
                response.Messages.Add("Quote cannot be deleted because it does not exist.");
                return response;
            }

            try
            {
                _context.Quotes.Remove(Quote); // Remove quote
                await _context.SaveChangesAsync(); // Save changes
            }
            catch (Exception ex)
            {
                response.Status = ServiceResponse.ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting the Quote");
                return response;
            }

            response.Status = ServiceResponse.ServiceStatus.Deleted;
            return response;
        }

        // List all quotes associated with a specific drama
        public async Task<ServiceResponse> ListQuotesForDrama(int id)
        {
            ServiceResponse response = new();

            List<Quote> Quotes = await _context.Quotes
                .Include(q => q.Drama)
                .Where(q => q.drama_id == id)
                .ToListAsync();

            List<QuoteDto> QuoteDtos = new List<QuoteDto>();

            foreach (Quote Quote in Quotes)
            {
                // Convert each Quote entity to QuoteDto
                QuoteDtos.Add(new QuoteDto()
                {
                    quote_id = Quote.quote_id,
                    content = Quote.content,
                    actor = Quote.actor,
                    episode = Quote.episode,
                    drama_id = Quote.drama_id
                });
            }

            if (!QuoteDtos.Any())
            {
                response.Status = ServiceResponse.ServiceStatus.NotFound;
                response.Messages.Add("No quotes found for the specified drama ID.");
                return response;
            }

            response.Status = ServiceResponse.ServiceStatus.Success;
            response.Data = QuoteDtos;

            return response;
        }
    }
}
