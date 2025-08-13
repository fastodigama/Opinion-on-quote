using Opinion_on_Quotes.Models;

namespace Opinion_on_Quotes.Interfaces
{
    public interface ICommentService
    {
        /// <summary>
        /// Adds a new comment to a quote.
        /// </summary>
        /// <param name="createCommentDto">DTO containing comment details.</param>
        /// <param name="userId">ID of the user creating the comment.</param>
        /// <returns>A ServiceResponse indicating success or failure.</returns>
        Task<ServiceResponse> AddComment(CreateCommentDto createCommentDto, string? userId);

        /// <summary>
        /// Lists all comments for a specific quote.
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>A list of CommentDto objects.</returns>
        Task<IEnumerable<CommentDto>> ListCommentsByQuote(int quoteId);

        /// <summary>
        /// Deletes a comment by its ID.
        /// </summary>
        /// <param name="commentId">The ID of the comment to delete.</param>
        /// <param name="userId">ID of the user requesting deletion.</param>
        /// <returns>A ServiceResponse indicating success or failure.</returns>
        Task<ServiceResponse> DeleteComment(int commentId, string userId);

        /// <summary>
        /// Updates an existing comment.
        /// </summary>
        /// <param name="commentDto">DTO containing updated comment data.</param>
        /// <returns>A ServiceResponse indicating success or failure.</returns>
        Task<ServiceResponse> UpdateComment(CommentDto commentDto);

        /// <summary>
        /// Retrieves a comment by its ID.
        /// </summary>
        /// <param name="id">The ID of the comment.</param>
        /// <returns>A ServiceResponse containing the comment data or error info.</returns>
        Task<ServiceResponse> GetCommentById(int id);
    }
}
