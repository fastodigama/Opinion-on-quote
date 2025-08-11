using System.Threading.Tasks;
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

    }
}
    

