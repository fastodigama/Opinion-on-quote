using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Opinion_on_Quotes.Interfaces;
using Opinion_on_Quotes.Models;
using Opinion_on_Quotes.Services;

namespace Opinion_on_Quotes.Controllers
{
    public class OpinionPageController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IQuoteServices _quoteService; 
        private readonly UserManager<IdentityUser> _userManager;

        /// <summary>
        /// Initializes the controller with required services.
        /// </summary>
        public OpinionPageController(ICommentService commentService, IQuoteServices quoteService, UserManager<IdentityUser> userManager)
        {
            _commentService = commentService; // Inject comment service
            _quoteService = quoteService;     // Inject quote service
            _userManager = userManager;       // Inject user manager
        }

        /// <summary>
        /// Displays all quotes along with their associated comments.
        /// </summary>
        /// <returns>View with quotes and comments.</returns>
        public async Task<IActionResult> Index()
        {
            var quotes = await _quoteService.ListQuotes(); // Fetch all quotes

            var quoteComments = new Dictionary<int, List<CommentDto>>(); // Map quote ID to comments
            foreach (var quote in quotes) {
                var comments = await _commentService.ListCommentsByQuote(quote.quote_id); // Fetch comments for quote
                quoteComments[quote.quote_id] = comments?.ToList() ?? new List<CommentDto>(); // Store comments or empty list
            }

            ViewBag.QuoteComments = quoteComments; // Pass comments to view
            return View(quotes); // Show quotes
        }

        /// <summary>
        /// Displays the form to create a comment for a specific quote.
        /// </summary>
        /// <param name="id">Quote ID.</param>
        /// <returns>View with comment form.</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var quote = await _quoteService.FindQuote(id); // Find quote by ID
            ViewBag.QuoteText = quote.content; // Show quote content in view

            var commentFormData = new CreateCommentDto
            {
                quote_id = id // Set quote ID in form
            };

            return View(commentFormData); // Show comment form
        }

        /// <summary>
        /// Handles submission of a new comment.
        /// </summary>
        /// <param name="formData">Comment data.</param>
        /// <returns>Redirects to index or shows error.</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(CreateCommentDto formData)
        {
            try
            {
                var userId = _userManager.GetUserId(User); // Get current user ID
                var response = await _commentService.AddComment(formData, userId); // Add comment

                if (response.Status == ServiceResponse.ServiceStatus.Created)
                {
                    return RedirectToAction("Index"); // Success: go to index
                }

                return View("Error", new ErrorViewModel { Errors = response.Messages }); // Failure: show error
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel
                {
                    Errors = new List<string> { ex.Message, ex.StackTrace } // Show exception details
                });
            }
        }

        /// <summary>
        /// Displays the form to edit a comment.
        /// Only accessible by Admins.
        /// </summary>
        /// <param name="id">Comment ID.</param>
        /// <returns>View with comment data or error.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _commentService.GetCommentById(id); // Fetch comment

            if (response.Status == ServiceResponse.ServiceStatus.NotFound || response.Data == null)
            {
                return NotFound(); // Comment not found
            }

            var commentDto = response.Data as CommentDto;
            if (commentDto == null)
            {
                return BadRequest(); // Invalid data
            }

            return View(commentDto); // Show edit form
        }

        /// <summary>
        /// Handles submission of updated comment data.
        /// Only accessible by Admins.
        /// </summary>
        /// <param name="commentDto">Updated comment data.</param>
        /// <returns>Redirects to index or shows error.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Update(CommentDto commentDto)
        {
            var userId = _userManager.GetUserId(User); // Get current user ID
            commentDto.UserId = userId; // Set user ID in DTO

            var response = await _commentService.UpdateComment(commentDto); // Update comment

            if (response.Status == ServiceResponse.ServiceStatus.Updated)
            {
                return RedirectToAction("Index"); // Success: go to index
            }
            else
            {
                return View("Error", new ErrorViewModel() { Errors = response.Messages }); // Failure: show error
            }
        }

        /// <summary>
        /// Displays confirmation view before deleting a comment.
        /// </summary>
        /// <param name="id">Comment ID.</param>
        /// <returns>View with comment data or error.</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DeleteConfirmation(int id)
        {
            var response = await _commentService.GetCommentById(id); // Fetch comment
            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(); // Comment not found
            }

            var commentDto = response.Data as CommentDto;
            return View(commentDto); // Show confirmation view
        }

        /// <summary>
        /// Deletes a comment if the user is owner or Admin.
        /// </summary>
        /// <param name="id">Comment ID.</param>
        /// <returns>Redirects to index or returns error.</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User); // Get current user ID
            var response = await _commentService.GetCommentById(id); // Fetch comment

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(); // Comment not found
            }

            var commentDto = response.Data as CommentDto;
            var isOwner = commentDto.UserId == userId; // Check ownership
            var isAdmin = User.IsInRole("Admin"); // Check admin role

            if (!isOwner && !isAdmin)
            {
                return Forbid(); // Not authorized
            }

            var deleteResponse = await _commentService.DeleteComment(id, userId); // Delete comment

            if (deleteResponse.Status != ServiceResponse.ServiceStatus.Deleted)
            {
                return BadRequest(deleteResponse.Messages); // Deletion failed
            }

            return RedirectToAction("Index"); // Success: go to index
        }
    }
}
