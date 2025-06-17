using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OstaFandy.PL.BL.IBL;
using OstaFandy.PL.DTOs;

namespace OstaFandy.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersFeedbackController : ControllerBase
    {
        private readonly IOrderFeedbackService _orderFeedbackService;

        public OrdersFeedbackController(IOrderFeedbackService orderFeedbackService)
        {
            _orderFeedbackService = orderFeedbackService;
        }

        [HttpGet]
        [EndpointDescription("OrdersFeedback/GetAll")]
        [EndpointSummary("return all feedbacks and rates")]
        public IActionResult GetAll(string searchString = "", int pageNumber = 1, int pageSize = 5)
        {
            //search by service and handyman nam
            try
            {
                var result = _orderFeedbackService.GetAll(searchString, pageNumber, pageSize);

                if (result.Data == null || !result.Data.Any())
                {
                    return Ok(new
                    {
                        message = "There are no order feedback records in the system yet",
                        data = new List<OrderFeedbackDto>(),
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
                return StatusCode(500, new { message = "An error occurred while fetching order feedback", error = ex.Message });
            }
        }

    }
}
