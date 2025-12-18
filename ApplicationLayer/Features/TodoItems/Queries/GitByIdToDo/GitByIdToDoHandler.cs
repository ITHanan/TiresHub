using ApplicationLayer.Features.TodoItems.Dtos;
using ApplicationLayer.Features.TodoItems.Queries.GitByIdToDo;
using ApplicationLayer.Interfaces;
using AutoMapper;
using DomainLayer.Common;
using DomainLayer.Models;
using MediatR;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TodoItems.Queries
{
    // The handler processes GetTodoByIdQuery and returns a single TodoItemDto within an OperationResult
    public class GetTodoByIdQueryHandler : IRequestHandler<GetTodoByIdQuery, OperationResult<TodoItemDto>>
    {
        // Use the IGenericRepository to access TodoItem data
        private readonly IGenericRepository<TodoItem> _todoRepository;
        private readonly IMapper _mapper;

        public GetTodoByIdQueryHandler(IGenericRepository<TodoItem> todoRepository, IMapper mapper)
        {
            _todoRepository = todoRepository;
            _mapper = mapper;
        }

        public async Task<OperationResult<TodoItemDto>> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Fetch the single item asynchronously using the request's ID
            var repoResult = await _todoRepository.GetByIdAsync(request.Id);

            // 2. Check if the repository call was successful
            if (repoResult.IsSuccess)
            {
                // Check if data was actually found (GetByIdAsync might return success but with null data)
                if (repoResult.Data == null)
                {
                    // Item not found, return a specific failure message
                    return OperationResult<TodoItemDto>.Failure($"Todo item with ID {request.Id} not found.");
                }

                // 3. Map the Domain Model (TodoItem) to the DTO (TodoItemDto)
                var todoDto = _mapper.Map<TodoItemDto>(repoResult.Data);

                // 4. Return a successful result with the mapped DTO
                return OperationResult<TodoItemDto>.Success(todoDto);
            }
            else
            {
                // 5. If the repository call itself failed (e.g., database error), return the error message
                return OperationResult<TodoItemDto>.Failure(repoResult.ErrorMessage);
            }
        }
    }
}