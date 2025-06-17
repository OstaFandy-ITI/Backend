using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OstaFandy.PL.BL;
using OstaFandy.PL.BL.IBL;
using OstaFandy.PL.DTOs;

namespace OstaFandy.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminClientController : ControllerBase
    {
        private readonly ILogger<ClientService> _logger;
        private readonly IClientService _clientService;

        public AdminClientController(IClientService clientService, ILogger<ClientService> logger)
        {
            _logger = logger;
            _clientService = clientService;
        }

        [HttpGet]
        [EndpointDescription("AdminClient/GetAll")]
        [EndpointSummary("return all Clients")]
        public IActionResult GetAll(string searchString = "", int pageNumber = 1, int pageSize = 5, bool? isActive = null)
        {
            try
            {
                var result = _clientService.GetAll(searchString, pageNumber, pageSize, isActive);

                if (result.Data == null || !result.Data.Any())
                {
                    return Ok(new
                    {
                        message = "There are no clients in the system yet Or deactivated Client",
                        data = new List<AdminDisplayClientDTO>(),
                        currentPage = result.CurrentPage,
                        totalPages = result.TotalPages,
                        totalCount = result.TotalCount,
                        searchString = result.SearchString
                    });
                }

                return Ok(new
                {
                    data = result.Data,
                    currentPage = result.CurrentPage,
                    totalPages = result.TotalPages,
                    totalCount = result.TotalCount,
                    searchString = result.SearchString
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching clients", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [EndpointDescription("AdminClient/GetById")]
        [EndpointSummary("get by id")]
        public IActionResult GetById(int id)
        {
            try
            {
                var client = _clientService.GetById(id);

                if (client == null)
                {
                    return NotFound(new { message = "Client not found" });
                }

                return Ok(new { data = client });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the client", error = ex.Message });
            }
        }
        
        [HttpGet("{clientId}/bookings")]
        [EndpointDescription("AdminClient/GetClientBookings")]
        [EndpointSummary("get client Booking..... Should be seperate end point WILL IMPLEMENT LATTER")]
        public IActionResult GetClientBookings(int clientId, string searchString = "", int pageNumber = 1, int pageSize = 5)
        {
            
            try
            {
 

                return Ok(new { message = $"Bookings for client {clientId} - implement this endpoint separately" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching client bookings", error = ex.Message });
            }
        }

        [HttpPut("EditClient")]
        [EndpointDescription("AdminClient/EditClient")]
        [EndpointSummary("edit client")]
        public IActionResult EditClient(AdminEditClientDTO admineditclientdto,int id)
        {
            if(admineditclientdto == null || admineditclientdto.Id != id)
            {
                return BadRequest(new { message = "Invalid client data" });
            }
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid model state", errors = ModelState });
                }
                var updatedClient = _clientService.EditClientDTO(admineditclientdto);
                if (updatedClient == null)
                {
                    return NotFound(new { message = "Client not found" });
                }
                return Ok(new { message = "Client updated successfully", data = updatedClient });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the client", error = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        [EndpointDescription("api/AdminHandyMan/deleteclient")]
        [EndpointSummary("SoftDelete for the client")]
        public IActionResult deleteclient(int id)
        {
            try
            {
                bool deleted = _clientService.DeleteClient(id);
                if (!deleted)
                {
                    return NotFound($"Handyman with ID {id} not found or could not be deleted");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting handyman with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
            {

            }
        }
    }
}
 
