using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Opinion_on_Quotes.Interfaces;
using Opinion_on_Quotes.Models;

namespace Opinion_on_Quotes.Controllers
{
    public class DramaPageController : Controller
    {
        private readonly IDramaServices _dramaServices;

        /// <summary>
        /// Initializes a new instance of the DramaPageController with injected services.
        /// </summary>
        public DramaPageController(IDramaServices dramaServices)
        {
            _dramaServices = dramaServices; // Inject drama service
        }

        /// <summary>
        /// Retrieves and displays a list of all dramas.
        /// </summary>
        /// <returns>View containing the list of dramas.</returns>
        public async Task<IActionResult> List()
        {
            // Get all dramas from service
            IEnumerable<DramaDto> dramaList = await _dramaServices.ListDramas();
            return View(dramaList); // Return view with drama list
        }

        /// <summary>
        /// Displays the form to create a new drama entry.
        /// Only accessible by users with the Admin role.
        /// </summary>
        /// <returns>View for creating a new drama.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(); // Show empty form to add drama
        }

        /// <summary>
        /// Handles the submission of a new drama entry.
        /// Only accessible by users with the Admin role.
        /// </summary>
        /// <param name="dramaDto">Drama data to be added.</param>
        /// <returns>Redirects to drama list on success or shows error view.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(DramaDto dramaDto)
        {
            // Try to add new drama
            ServiceResponse response = await _dramaServices.AddDrama(dramaDto);

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
        /// Displays a confirmation view before deleting a drama.
        /// Only accessible by users with the Admin role.
        /// </summary>
        /// <param name="id">ID of the drama to delete.</param>
        /// <returns>View for delete confirmation or NotFound.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> DeleteConfirmation(int id)
        {
            // Find drama by ID
            var dramaToDelete = await _dramaServices.FindDrama(id);
            if (dramaToDelete == null)
            {
                return NotFound(); // Drama not found
            }
            return View(dramaToDelete); // Show confirmation page
        }

        /// <summary>
        /// Deletes the specified drama.
        /// Only accessible by users with the Admin role.
        /// </summary>
        /// <param name="id">ID of the drama to delete.</param>
        /// <returns>Redirects to drama list or returns error.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Attempt to delete drama
            var response = await _dramaServices.DeleteDrama(id);

            if (response.Status != ServiceResponse.ServiceStatus.Deleted)
            {
                return BadRequest(response.Messages); // Deletion failed
            }

            return RedirectToAction("List"); // Success: go to list
        }

        /// <summary>
        /// Displays the form to edit an existing drama.
        /// Only accessible by users with the Admin role.
        /// </summary>
        /// <param name="id">ID of the drama to edit.</param>
        /// <returns>View for editing or NotFound.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Find drama to edit
            var dramaToEdit = await _dramaServices.FindDrama(id);
            if (dramaToEdit == null)
            {
                return NotFound(); // Drama not found
            }
            return View(dramaToEdit); // Show edit form
        }

        /// <summary>
        /// Handles the submission of updated drama data.
        /// Only accessible by users with the Admin role.
        /// </summary>
        /// <param name="dramaDto">Updated drama data.</param>
        /// <returns>Redirects to drama list or shows error view.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Update(DramaDto dramaDto)
        {
            // Try to update drama
            ServiceResponse response = await _dramaServices.UpdateDrama(dramaDto);
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
        /// Displays the default index view.
        /// </summary>
        /// <returns>Index view.</returns>
        public IActionResult Index()
        {
            return View(); // Load default index page
        }
    }
}
