using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Opinion_on_Quotes.Data;
using Opinion_on_Quotes.Interfaces;
using Opinion_on_Quotes.Models;
using static Opinion_on_Quotes.Models.ServiceResponse;

namespace Opinion_on_Quotes.Services
{
    public class CommentService : ICommentService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        // Inject UserManager and database context
        public CommentService(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Add a new comment to a quote
        public async Task<ServiceResponse> AddComment(CreateCommentDto createCommentDto, string userId)
        {
            var response = new ServiceResponse();

            // Check if the quote exists
            var quote = await _context.Quotes.FindAsync(createCommentDto.quote_id);
            if (quote == null)
            {
                response.Status = ServiceResponse.ServiceStatus.NotFound;
                response.Messages.Add("Quote not found.");
                return response;
            }

            // Create and save the new comment
            var comment = new Comment
            {
                CommentText = createCommentDto.CommentText,
                CreatedAt = DateTime.Now,
                quote_id = createCommentDto.quote_id,
                Quote = quote,
                UserId = userId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            response.Status = ServiceResponse.ServiceStatus.Created;
            response.Messages.Add("Comment added successfully.");

            // Prepare list of comments for the quote
            var commentDtos = new List<CommentDto>();
            var comments = await _context.Comments
                .Where(c => c.quote_id == quote.quote_id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            foreach (var c in comments)
            {
                var user = await _userManager.FindByIdAsync(c.UserId);

                commentDtos.Add(new CommentDto
                {
                    CommentId = c.CommentId,
                    CommentText = c.CommentText,
                    CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    quote_id = c.quote_id,
                    UserId = c.UserId
                });
            }

            // Attach quote and comments to response
            response.QuoteData = new QuoteDto
            {
                quote_id = quote.quote_id,
                content = quote.content,
                actor = quote.actor,
                episode = quote.episode,
                drama_id = quote.drama_id,
                comments = commentDtos
            };

            return response;
        }

        // List all comments for a specific quote
        public async Task<IEnumerable<CommentDto>> ListCommentsByQuote(int quote_id)
        {
            var comments = await _context.Comments
                .Where(c => c.quote_id == quote_id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var commentDtos = new List<CommentDto>();

            foreach (var comment in comments)
            {
                var user = await _userManager.FindByIdAsync(comment.UserId);
                string username = user?.UserName ?? "Anonymous";

                commentDtos.Add(new CommentDto
                {
                    CommentId = comment.CommentId,
                    CommentText = comment.CommentText,
                    CreatedAt = comment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UserName = username,
                    quote_id = comment.quote_id,
                    UserId = comment.UserId
                });
            }

            return commentDtos;
        }

        // Retrieve a comment by its ID
        public async Task<ServiceResponse> GetCommentById(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return new ServiceResponse
                {
                    Status = ServiceResponse.ServiceStatus.NotFound,
                };
            }

            var user = await _userManager.FindByIdAsync(comment.UserId);
            string username = user?.UserName ?? "Anonymous";

            var commentDto = new CommentDto
            {
                CommentId = comment.CommentId,
                CommentText = comment.CommentText,
                UserId = comment.UserId,
                UserName = username
            };

            return new ServiceResponse
            {
                Status = ServiceResponse.ServiceStatus.Success,
                Data = commentDto
            };
        }

        // Delete a comment if user is owner or admin
        public async Task<ServiceResponse> DeleteComment(int commentId, string userId)
        {
            var response = new ServiceResponse();
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                response.Status = ServiceResponse.ServiceStatus.NotFound;
                response.Messages.Add("Comment not found.");
                return response;
            }

            // Check if user is admin
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            // Check ownership or admin rights
            if (comment.UserId != userId && !isAdmin)
            {
                response.Status = ServiceResponse.ServiceStatus.Forbidden;
                response.Messages.Add("You are not authorized to delete this comment.");
                return response;
            }

            // Remove comment from database
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            response.Status = ServiceResponse.ServiceStatus.Deleted;
            response.Messages.Add("Comment deleted successfully.");
            return response;
        }

        // Update a comment if user is owner or admin
        public async Task<ServiceResponse> UpdateComment(CommentDto commentDto)
        {
            var response = new ServiceResponse();

            var comment = await _context.Comments.FindAsync(commentDto.CommentId);
            if (comment == null)
            {
                response.Status = ServiceResponse.ServiceStatus.NotFound;
                response.Messages.Add("Comment not found.");
                return response;
            }

            var user = await _userManager.FindByIdAsync(commentDto.UserId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            // Check ownership or admin rights
            if (comment.UserId != commentDto.UserId && !isAdmin)
            {
                response.Status = ServiceResponse.ServiceStatus.Forbidden;
                response.Messages.Add("You are not authorized to update this comment.");
                return response;
            }

            // Update comment text
            comment.CommentText = commentDto.CommentText;
            await _context.SaveChangesAsync();

            response.Status = ServiceResponse.ServiceStatus.Updated;
            response.Messages.Add("Comment updated successfully.");

            return response;
        }
    }
}
