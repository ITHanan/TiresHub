using ApplicationLayer.Features.TodoItems.Dtos;
using ApplicationLayer.Interfaces;
using AutoMapper;
using DomainLayer.Common;
using DomainLayer.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TodoItems.Queries.GetAllToDo
{
    public class GetAllTodosQueryHandler : IRequestHandler<GetAllTodosQuery, OperationResult<List<TodoItemDto>>>
    {
        private readonly IGenericRepository<TodoItem> _todoRepository;
        private readonly IMapper _mapper;

        public GetAllTodosQueryHandler(IGenericRepository<TodoItem> todoRepository, IMapper mapper)
        {
            _todoRepository = todoRepository;
            _mapper = mapper;
        }

        public async Task<OperationResult<List<TodoItemDto>>> Handle(GetAllTodosQuery request, CancellationToken cancellationToken)
        {
            var repoResult = await _todoRepository.GetAllAsync();


            if (repoResult.IsSuccess)
            {

                var todoList = repoResult.Data.ToList();

                var todoDtos = _mapper.Map<List<TodoItemDto>>(todoList);

                return OperationResult<List<TodoItemDto>>.Success(todoDtos);
            }
            else
            {

                return OperationResult<List<TodoItemDto>>.Failure(repoResult.ErrorMessage);
            }
        }
    }
}
