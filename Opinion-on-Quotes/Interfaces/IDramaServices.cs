using Opinion_on_Quotes.Models;

namespace Opinion_on_Quotes.Interfaces
{
    public interface IDramaServices
    {
        /// <summary>
        /// Retrieves a list of all dramas.
        /// </summary>
        /// <returns>An enumerable collection of DramaDto objects.</returns>
        Task<IEnumerable<DramaDto>> ListDramas();

        /// <summary>
        /// Finds a specific drama by its ID.
        /// </summary>
        /// <param name="id">The ID of the drama to find.</param>
        /// <returns>The DramaDto if found; otherwise, null.</returns>
        Task<DramaDto?> FindDrama(int id);

        /// <summary>
        /// Updates an existing drama.
        /// </summary>
        /// <param name="dramaDto">DTO containing updated drama data.</param>
        /// <returns>A ServiceResponse indicating success or failure.</returns>
        Task<Models.ServiceResponse> UpdateDrama(DramaDto dramaDto);

        /// <summary>
        /// Adds a new drama.
        /// </summary>
        /// <param name="dramaDto">DTO containing new drama data.</param>
        /// <returns>A ServiceResponse indicating success or failure.</returns>
        Task<Models.ServiceResponse> AddDrama(DramaDto dramaDto);

        /// <summary>
        /// Deletes a drama by its ID.
        /// </summary>
        /// <param name="id">The ID of the drama to delete.</param>
        /// <returns>A ServiceResponse indicating success or failure.</returns>
        Task<Models.ServiceResponse> DeleteDrama(int id);
    }
}
