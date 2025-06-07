using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OstaFandy.DAL.Repos;
using OstaFandy.DAL.Repos.IRepos;
using OstaFandy.PL.BL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OstaFandy.DAL.Repos.IRepos;
using OstaFandy.PL.BL.IBL;
using OstaFandy.PL.DTOs;

namespace OstaFandy.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    ///
    public class AdminHandyManController : ControllerBase
    {
        private readonly IMapper _map;
        private readonly IHandyManService _handymanService;
        private readonly IUserService _userservice;

        private readonly ILogger<HandyManService> _logger;

 

        public AdminHandyManController(IHandyManService HandyManService, IUserService userService, IMapper map, ILogger<HandyManService> logger)

        {
            _map = map;
            _handymanService = HandyManService;
            _userservice = userService;

            _logger = logger;
        }
        [HttpGet]
        [EndpointDescription("AdminHandyMan/getall")]
        [EndpointSummary("return all handymen")]
        public IActionResult GetAll(string searchString = "", int pageNumber = 1, int pageSize = 5)
        {
            var result = _handymanService.GetAll(searchString, pageNumber, pageSize);

            if (result.Data == null || !result.Data.Any())
            {
                return Ok(new
                {
                    message = "There are no handymen assigned to system yet",
                    data = new List<AdminHandyManDTO>(),
                    currentPage = result.CurrentPage,
                    totalPages = result.TotalPages,
                    totalCount = result.TotalCount,
                    searchString = result.SearchString
                });
            }

            var mappedData = _map.Map<List<AdminHandyManDTO>>(result.Data);

            return Ok(new
            {
                data = mappedData,
                currentPage = result.CurrentPage,
                totalPages = result.TotalPages,
                totalCount = result.TotalCount,
                searchString = result.SearchString
            });
        }

        [HttpPost]
        [EndpointDescription("api/AdminHandyMan/CreateHandyman")]
        [EndpointSummary("create handyman")]
        public IActionResult CreateHandyman([FromBody] CreateHandymanDTO createHandymanDto)
        {
            try
            {
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = _handymanService.CreateHandyman(createHandymanDto);
                return CreatedAtAction(nameof(GetHandymanById), new { id = result.UserId }, result);
            }
            catch (InvalidOperationException ex)
            {
                 return BadRequest(new
                {
                    error = ex.Message,
                    details = "Please check if the email, phone, or national ID is already registered."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating handyman");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [EndpointDescription("api/AdminHandyMan/GetHandymanById")]
        [EndpointSummary("get handyman by id")]
        public IActionResult GetHandymanById(int id)
        {
            try
            {
                var handyman = _handymanService.GetById(id);
                if (handyman == null)
                {
                    return NotFound($"Handyman with ID {id} not found");
                }
                return Ok(handyman);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in controller while getting handyman by id {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [EndpointDescription("api/AdminHandyMan/deletehandyman")]
        [EndpointSummary("SoftDelete for the handyman")]
        public IActionResult deletehandyman(int id)
        {
            try
            {
                bool deleted = _handymanService.DeleteHandyman(id);
                if (!deleted)
                {
                    return NotFound($"Handyman with ID {id} not found or could not be deleted");
                }
                return NoContent();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting handyman with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
            {

            }
        }

        [HttpPut("{id}")]
        [EndpointDescription("api/AdminHandyMan/EditHandyman")]
        [EndpointSummary("edit handyman, check constraint")]
        public IActionResult EditHandyman(int id, [FromBody] EditHandymanDTO editHandymanDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (id != editHandymanDto.UserId)
                {
                    return BadRequest("Route ID and payload UserId do not match.");
                }

                var updatedHandyman = _handymanService.EditHandyman(editHandymanDto);
                if (updatedHandyman == null)
                {
                    _logger.LogWarning(
                        "EditHandyman: no handyman found with UserId {UserId}",
                        editHandymanDto.UserId
                    );
                    return NotFound(new { Message = $"Handyman with ID {editHandymanDto.UserId} not found." });
                }

                return Ok(updatedHandyman);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in EditHandyman controller for UserId {UserId}",
                    editHandymanDto.UserId
                );
                return StatusCode(500, "An unexpected error occurred while updating the handyman.");
            }
        }

    }
}

