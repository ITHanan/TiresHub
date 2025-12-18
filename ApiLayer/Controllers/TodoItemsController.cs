using ApplicationLayer.Features.TodoItems.Commands;
using ApplicationLayer.Features.TodoItems.Queries;
using ApplicationLayer.Features.TodoItems.Queries.GetAllToDo;

using ApplicationLayer.Features.TodoItems.Queries.GitByIdToDo;

using MediatR;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; 

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TodoItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // --- Create Endpoint (POST) ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateTodoCommand command) // Using [FromBody] for clarity
        {
            // 1. Retrieve the User ID from the JWT Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                // If the claim is missing, authorization is likely misconfigured or token is invalid
                return Unauthorized("User ID not found in token.");
            }

            // 2. Assign the User ID to the command
            // Assuming the CreateTodoCommand has a property named CreatedByUserId (or similar) of type int.
            if (!int.TryParse(userIdClaim, out int userId))
            {
                // Safety check if the claim value isn't a valid integer
                return BadRequest("Invalid user ID format in token.");
            }
            // *** NOTE: You need to add this property to your CreateTodoCommand ***
            // command.CreatedByUserId = userId; 

            // 3. Send the command
            var result = await _mediator.Send(command);

            // 4. Handle the result
            if (result.IsSuccess)
            {
                // Return 201 Created status, including the newly created resource's ID (result.Data)
                return CreatedAtAction(nameof(GetById), new { id = result.Data }, command);
            }
            else
            {
                // Return 400 Bad Request with the error message
                return BadRequest(new { result.ErrorMessage });
            }
        }


        // --- Get All Endpoint (GET) ---
        [HttpGet]
        [Authorize] 
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllTodosQuery();
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            else
            {
                // Returning 500 for general repository/server errors
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
            }
        }

        // --- Get By ID Endpoint (GET {id}) ---
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetTodoByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            // Handles specific "not found" error from the handler
            if (result.ErrorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result.ErrorMessage); // 404 Not Found
            }

            // General error or bad input
            return BadRequest(result.ErrorMessage); // 400 Bad Request
        }

        // --- Add other CRUD methods (Update, Delete) here if needed ---
    }
}