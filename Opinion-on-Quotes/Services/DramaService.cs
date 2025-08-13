using Microsoft.EntityFrameworkCore;
using Opinion_on_Quotes.Data;
using Opinion_on_Quotes.Interfaces;
using Opinion_on_Quotes.Models;
using System;

namespace Opinion_on_Quotes.Services
{
    public class DramaService : IDramaServices
    {
        private readonly ApplicationDbContext _context;

        // Inject the database context
        public DramaService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieve all dramas from the database
        public async Task<IEnumerable<DramaDto>> ListDramas()
        {
            List<Drama> dramas = await _context.Dramas.ToListAsync(); // Fetch all drama records
            List<DramaDto> DramaDtos = new List<DramaDto>(); // Prepare DTO list

            foreach (Drama Drama in dramas)
            {
                // Convert each Drama entity to a DramaDto
                DramaDtos.Add(new DramaDto()
                {
                    drama_id = Drama.drama_id,
                    title = Drama.title,
                    release_year = Drama.release_year,
                    genre = Drama.genre,
                    synopsis = Drama.synopsis
                });
            }

            return DramaDtos; // Return list of DTOs
        }

        // Find a specific drama by ID
        public async Task<DramaDto?> FindDrama(int id)
        {
            var drama = await _context.Dramas
                .FirstOrDefaultAsync(c => c.drama_id == id); // Search for drama

            if (drama == null)
            {
                return null; // Drama not found
            }

            // Convert to DTO and return
            DramaDto DramaDtos = new DramaDto()
            {
                drama_id = drama.drama_id,
                title = drama.title,
                release_year = drama.release_year,
                genre = drama.genre,
                synopsis = drama.synopsis
            };
            return DramaDtos;
        }

        // Update an existing drama
        public async Task<ServiceResponse> UpdateDrama(DramaDto DramaDto)
        {
            ServiceResponse serviceResponse = new();

            var dramaToUpdate = await _context.Dramas.FindAsync(DramaDto.drama_id); // Find drama by ID

            if (dramaToUpdate == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add($"Drama with ID {DramaDto.drama_id} not found.");
                return serviceResponse;
            }

            // Update fields
            dramaToUpdate.title = DramaDto.title;
            dramaToUpdate.release_year = DramaDto.release_year;
            dramaToUpdate.genre = DramaDto.genre;
            dramaToUpdate.synopsis = DramaDto.synopsis;

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

        // Add a new drama to the database
        public async Task<ServiceResponse> AddDrama(DramaDto DramaDto)
        {
            ServiceResponse serviceResponse = new();

            // Create new Drama entity
            Drama drama = new Drama()
            {
                drama_id = DramaDto.drama_id,
                title = DramaDto.title,
                release_year = DramaDto.release_year,
                genre = DramaDto.genre,
                synopsis = DramaDto.synopsis
            };

            try
            {
                _context.Dramas.Add(drama); // Add to context
                var rows = await _context.SaveChangesAsync(); // Save changes

                if (rows > 0)
                {
                    serviceResponse.Status = ServiceResponse.ServiceStatus.Created;
                    serviceResponse.CreatedId = drama.drama_id;
                }
                else
                {
                    serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                    serviceResponse.Messages.Add("Drama could not be added.");
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("There was an error adding the Drama.");
                if (ex.InnerException != null)
                {
                    serviceResponse.Messages.Add(ex.InnerException.Message); // Include inner exception message
                }
            }

            return serviceResponse;
        }

        // Delete a drama by ID
        public async Task<ServiceResponse> DeleteDrama(int id)
        {
            ServiceResponse response = new();

            var Drama = await _context.Dramas.FindAsync(id); // Find drama

            if (Drama == null)
            {
                response.Status = ServiceResponse.ServiceStatus.NotFound;
                response.Messages.Add("Drama cannot be deleted because it does not exist.");
                return response;
            }

            try
            {
                _context.Dramas.Remove(Drama); // Remove drama
                await _context.SaveChangesAsync(); // Save changes
            }
            catch (Exception ex)
            {
                response.Status = ServiceResponse.ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting the Drama");
                return response;
            }

            response.Status = ServiceResponse.ServiceStatus.Deleted;
            return response;
        }
    }
}
