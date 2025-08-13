using Opinion_on_Quotes.Models;

namespace Opinion_on_Quotes.Interfaces
{
    public interface IQuoteServices
    {
        /// <summary>
        /// Retrieves a list of all quotes.
        /// </summary>
        /// <returns>An enumerable collection of QuoteDto objects.</returns>
        Task<IEnumerable<QuoteDto>> ListQuotes();

        /// <summary>
        /// Finds a specific quote by its ID.
        /// </summary>
        /// <param name="id">The ID of the quote to find.</param>
        /// <returns>The QuoteDto if found; otherwise, null.</returns>
        Task<QuoteDto?> FindQuote(int id);

        /// <summary>
        /// Updates an existing quote.
        /// </summary>
        /// <param name="QuoteDto">DTO containing updated quote data.</param>
        /// <returns>A ServiceResponse indicating success or failure.</returns>
        Task<Models.ServiceResponse> UpdateQuote(QuoteDto QuoteDto);

        /// <summary>
        /// Adds a new quote.
        /// </summary>
        /// <param name="QuoteDto">DTO containing new quote data.</param>
        /// <returns>A ServiceResponse indicating success or failure.</returns>
        Task<Models.ServiceResponse> AddQuote(QuoteDto QuoteDto);

        /// <summary>
        /// Deletes a quote by its ID.
        /// </summary>
        /// <param name="id">The ID of the quote to delete.</param>
        /// <returns>A ServiceResponse indicating success or failure.</returns>
        Task<Models.ServiceResponse> DeleteQuote(int id);

        /// <summary>
        /// Retrieves all quotes associated with a specific drama.
        /// </summary>
        /// <param name="id">The ID of the drama.</param>
        /// <returns>A ServiceResponse containing the list of quotes or error info.</returns>
        Task<Models.ServiceResponse> ListQuotesForDrama(int id);
    }
}
