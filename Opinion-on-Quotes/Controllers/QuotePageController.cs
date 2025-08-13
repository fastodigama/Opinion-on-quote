using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Opinion_on_Quotes.Interfaces;
using Opinion_on_Quotes.Models;

namespace Opinion_on_Quotes.Controllers
{
    public class QuotePageController : Controller
    {
        private readonly IQuoteServices _quoteServices;
        private readonly IDramaServices _dramaServices;

        /// <summary>
        /// Initializes the controller with quote and drama services.
        /// </summary>
        public QuotePageController(IQuoteServices quoteServices, IDramaServices dramaServices)
        {
            _quoteServices = quoteServices;   // Inject quote service
            _dramaServices = dramaServices;   // Inject drama service
        }

        /// <summary>
        /// Displays a list of all quotes.
        /// </summary>
        /// <returns>View with list of quotes.</returns>
        public async Task<IActionResult> List()
        {
            IEnumerable<QuoteDto> quoteList = await _quoteServices.ListQuotes(); // Fetch all quotes
            return View(quoteList); // Show quotes in view
        }

        /// <summary>
        /// Displays details of a specific quote.
        /// </summary>
        /// <param name="id">Quote ID.</param>
        /// <returns>View with quote details or NotFound.</returns>
        public async Task<IActionResult> Details(int id)
        {
            QuoteDto quoteToView = await _quoteServices.FindQuote(id); // Find quote by ID
            if (quoteToView == null)
            {
                return NotFound(); // Quote not found
            }
            return View(quoteToView); // Show quote details
        }

        /// <summary>
        /// Displays the form to edit a quote.
        /// Only accessible by Admins.
        /// </summary>
        /// <param name="id">Quote ID.</param>
        /// <returns>View with quote data or NotFound.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dramas = await _dramaServices.ListDramas(); // Get list of dramas
            ViewBag.Dramas = dramas; // Pass dramas to view

            var quoteToEdit = await _quoteServices.FindQuote(id); // Find quote to edit
            if (quoteToEdit == null)
            {
                return NotFound(); // Quote not found
            }
            return View(quoteToEdit); // Show edit form
        }

        /// <summary>
        /// Handles submission of updated quote data.
        /// Only accessible by Admins.
        /// </summary>
        /// <param name="quoteDto">Updated quote data.</param>
        /// <returns>Redirects to list or shows error.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Update(QuoteDto quoteDto)
        {
            ServiceResponse response = await _quoteServices.UpdateQuote(quoteDto); // Update quote

            if (response.Status == ServiceResponse.ServiceStatus.Updated)
            {
                return RedirectToAction("List"); // Success: go to list
            }
            else
            {
                return View("Error", new ErrorViewModel() { Errors = response.Messages }); // Failure: show error
            }
        }

        /// <summary>
        /// Displays the form to create a new quote.
        /// Only accessible by Admins.
        /// </summary>
        /// <returns>View with quote creation form.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var dramas = await _dramaServices.ListDramas(); // Get list of dramas
            ViewBag.Dramas = dramas; // Pass dramas to view
            return View(); // Show create form
        }

        /// <summary>
        /// Handles submission of a new quote.
        /// </summary>
        /// <param name="quoteDto">New quote data.</param>
        /// <returns>Redirects to list or shows error.</returns>
        [HttpPost]
        public async Task<IActionResult> Add(QuoteDto quoteDto)
        {
            ServiceResponse response = await _quoteServices.AddQuote(quoteDto); // Add new quote

            if (response.Status == ServiceResponse.ServiceStatus.Created)
            {
                return RedirectToAction("List"); // Success: go to list
            }
            else
            {
                return View("Error", new ErrorViewModel() { Errors = response.Messages }); // Failure: show error
            }
        }

        /// <summary>
        /// Displays confirmation view before deleting a quote.
        /// Only accessible by Admins.
        /// </summary>
        /// <param name="id">Quote ID.</param>
        /// <returns>View with quote data or NotFound.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> DeleteConfirmation(int id)
        {
            var quoteToDelete = await _quoteServices.FindQuote(id); // Find quote to delete
            if (quoteToDelete == null)
            {
                return NotFound(); // Quote not found
            }
            return View(quoteToDelete); // Show confirmation view
        }

        /// <summary>
        /// Deletes a quote.
        /// Only accessible by Admins.
        /// </summary>
        /// <param name="id">Quote ID.</param>
        /// <returns>Redirects to list or returns error.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _quoteServices.DeleteQuote(id); // Delete quote

            if (response.Status != ServiceResponse.ServiceStatus.Deleted)
            {
                return BadRequest(response.Messages); // Deletion failed
            }

            return RedirectToAction("List"); // Success: go to list
        }

        /// <summary>
        /// Displays the default index view.
        /// </summary>
        /// <returns>Index view.</returns>
        public IActionResult Index()
        {
            return View(); // Load default index page
        }
    }
}
