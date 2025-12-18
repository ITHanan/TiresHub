using ApplicationLayer.Features.TodoItems.Dtos;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TodoItems.Queries.GitByIdToDo
{
    public class GetTodoByIdQuery : IRequest<OperationResult<TodoItemDto>>
    {
        public int Id { get; set; }
        public GetTodoByIdQuery() { }

        public GetTodoByIdQuery(int id)
        {
            Id = id;
        }
    }
}
