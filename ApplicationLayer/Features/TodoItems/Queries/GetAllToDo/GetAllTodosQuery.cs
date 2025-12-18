using ApplicationLayer.Features.TodoItems.Dtos;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TodoItems.Queries.GetAllToDo
{
    public class GetAllTodosQuery : IRequest<OperationResult<List<TodoItemDto>>>
    {
    }
}
