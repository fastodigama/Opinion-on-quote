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

        public OpinionPageController(ICommentService commentService, IQuoteServices quoteService, UserManager<IdentityUser> userManager)
        {
            _commentService = commentService;
            _quoteService = quoteService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
           var quotes = await _quoteService.ListQuotes();

            // Create a dictionary: quote_id → list of comments
            var quoteComments = new Dictionary<int, List<CommentDto>>();
            foreach (var quote in quotes) {
                // For each quote, fetch its comments
                var comments = await _commentService.ListCommentsByQuote(quote.quote_id);
                if (comments != null && comments.Any())
                {
                    // If comments exist, convert them to a list and store in the dictionary
                    quoteComments[quote.quote_id] = comments.ToList();
                }
                else
                {
                    quoteComments[quote.quote_id] = new List<CommentDto>();
                }
            }
            // Pass the quotes and their comments to the view
            ViewBag.QuoteComments = quoteComments;
            return View(quotes);
        }
        [Authorize] // Only authenticated users can access this
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var quote =  await _quoteService.FindQuote(id);
            ViewBag.QuoteText = quote.content;

            // Set up the form with the quote ID from the URL
            var commentFormData = new CreateCommentDto
            {
                quote_id = id // This tells the form which quote the comment is for, it came from the url
            };


            // Show the form to the user
            return View(commentFormData);
        }

        [Authorize] // Only authenticated users can post comments
        [HttpPost]
        public async Task<IActionResult> Add(CreateCommentDto formData)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var response = await _commentService.AddComment(formData, userId);

                if (response.Status == ServiceResponse.ServiceStatus.Created)
                {
                    return RedirectToAction("Index");
                }

                return View("Error", new ErrorViewModel { Errors = response.Messages });
            }
            catch (Exception ex)
            {
                // Log the error if needed
                return View("Error", new ErrorViewModel
                {
                    Errors = new List<string> { ex.Message, ex.StackTrace }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// 

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _commentService.GetCommentById(id);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound || response.Data == null)
            {
                return NotFound();
            }

            var commentDto = response.Data as CommentDto;
            if (commentDto == null)
            {
                // Handle unexpected case
                return BadRequest();
            }

            return View(commentDto);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quoteDto"></param>
        /// <returns></returns>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Update(CommentDto commentDto)
        {
            // Get current logged-in user’s ID
            var userId = _userManager.GetUserId(User);

            // Set the userId in the dto (important for ownership check)
            commentDto.UserId = userId;

            // Call the updated service method with the DTO
            ServiceResponse response = await _commentService.UpdateComment(commentDto);

            if (response.Status == ServiceResponse.ServiceStatus.Updated)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View("Error", new ErrorViewModel() { Errors = response.Messages });
            }
        }




        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DeleteConfirmation(int id)
        {
            var response = await _commentService.GetCommentById(id);
            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound();
            }

            var commentDto = response.Data as CommentDto;
            return View(commentDto);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var response = await _commentService.GetCommentById(id);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound();
            }

            var commentDto = response.Data as CommentDto;
            var isOwner = commentDto.UserId == userId;
            var isAdmin = User.IsInRole("Admin");

            if (!isOwner && !isAdmin)
            {
                return Forbid();
            }

            var deleteResponse = await _commentService.DeleteComment(id, userId);

            if (deleteResponse.Status != ServiceResponse.ServiceStatus.Deleted)
            {
                return BadRequest(deleteResponse.Messages);
            }

            return RedirectToAction("Index");
        }


    }
}
    

