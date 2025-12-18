using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using DomainLayer.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TodoItems.Commands
{


    public class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, OperationResult<int>>
    {
        private readonly IGenericRepository<TodoItem> _todoRepository;

        public CreateTodoCommandHandler(IGenericRepository<TodoItem> todoRepository)
        {
            _todoRepository = todoRepository;
        }

        public async Task<OperationResult<int>> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return OperationResult<int>.Failure("Title cannot be empty.");
            }

            var todoItem = new TodoItem
            {
                Title = request.Title,
                UserId = request.CreatedByUserId,
               // Description = request.Description,
            };

            var repoResult = await _todoRepository.AddAsync(todoItem, cancellationToken);

            if (repoResult.IsSuccess)
            {
                return OperationResult<int>.Success(repoResult.Data.Id);
            }
            else
            {
                return OperationResult<int>.Failure(repoResult.ErrorMessage);
            }
        }
    }
}
