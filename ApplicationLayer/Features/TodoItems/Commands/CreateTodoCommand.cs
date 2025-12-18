using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TodoItems.Commands
{
    public class CreateTodoCommand : IRequest<OperationResult<int>>
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public int CreatedByUserId { get; set; }
    }
}
