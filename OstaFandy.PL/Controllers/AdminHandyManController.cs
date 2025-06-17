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
using OstaFandy.DAL.Entities;
using OstaFandy.DAL.Repos.IRepos;
using OstaFandy.PL.BL;
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
        private readonly IJWTService _jwtService;


        private readonly ILogger<HandyManService> _logger;



        public AdminHandyManController(IHandyManService HandyManService, IUserService userService, IMapper map, ILogger<HandyManService> logger, IJWTService jwtService)

        {
            _map = map;
            _handymanService = HandyManService;
            _userservice = userService;

            _logger = logger;
            _jwtService = jwtService;
        }
        [HttpGet]
        [EndpointDescription("AdminHandyMan/getall")]
        [EndpointSummary("return all handymen")]
        public IActionResult GetAll(string searchString = "", int pageNumber = 1, int pageSize = 5, bool? isActive = null)
        {
            var result = _handymanService.GetAll(searchString, pageNumber, pageSize, isActive);

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


        [HttpGet("pending")]
        [EndpointDescription("AdminHandyMan/getallpending")]
        [EndpointSummary("return all pending handymen")]
        public IActionResult GetAllPendingHandymen()
        {
            var result = _handymanService.GetAllPendingHandymen();
            if (result == null || !result.Any())
            {
                return Ok(new
                {
                    success = true,
                    message = "There are no pending handymen at the moment.",
                    data = new List<AdminHandyManDTO>(),
                    count = 0
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Found {result.Count} pending handymen",
                data = result,
                count = result.Count
            });
        }

        [HttpPut("status/{userId}")]
        [EndpointDescription("AdminHandyMan/updatestatusbyid")]
        [EndpointSummary("Update handyman status by id")]
        public async Task<IActionResult> UpdateHandymanStatusById(int userId, [FromBody] HandymanStatusUpdateDTO statusUpdate)
        {
            if (userId != statusUpdate.UserId)
            {
                return BadRequest(new { message = "User ID in URL must match User ID in request body" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var validStatus = new[] { "Approved", "Rejected" };
            if (!validStatus.Contains(statusUpdate.Status))
            {
                return BadRequest(new { message = "Invalid status. Status must be either 'Approved' or 'Rejected'" });
            }

            // Fix: Use the correct instance field `_handymanService` instead of `HandyManService`
            var result = await _handymanService.UpdateHandymanStatusById(userId, statusUpdate.Status);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Failed to update handyman status. Please try again later."
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Handyman status updated to {statusUpdate.Status} successfully"
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
            catch (Exception ex)
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


        [Route("Handyman-register")]
        [HttpPost]
        [EndpointDescription("api/AdminHandyMan/register-Handyma")]
        [EndpointSummary("add handyman application")]
        public IActionResult HandyManApplication([FromForm] HandyManApplicationDto handymandto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(new ResponseDto<string>
                {
                    IsSuccess = false,
                    Message = "Invalid input",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            var res = _handymanService.CreateHandyManApplicationAsync(handymandto).Result;
            if (res == 0)
            {
                return BadRequest(new ResponseDto<string>
                {
                    IsSuccess = false,
                    Message = "Invalid input",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            else if (res == -1)
            {
                return BadRequest(new ResponseDto<string>
                {
                    IsSuccess = false,
                    Message = "User already exists",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            else if (res == -2)
            {
                return BadRequest(new ResponseDto<string>
                {
                    IsSuccess = false,
                    Message = "Password and confirm password do not match",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            else if (res > 0)
            {
                var user = _userservice.GetById(res);
                

                var token = _jwtService.GeneratedToken(user);
                return Ok(new ResponseDto<string>
                {
                    IsSuccess = true,
                    Message = "registered successfully please wait for aprrovment",
                    Data = token,
                    StatusCode = StatusCodes.Status201Created
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto<string>
                {
                    IsSuccess = false,
                    Message = "An error occurred while registeration",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                });


            }
        }


    }
}

