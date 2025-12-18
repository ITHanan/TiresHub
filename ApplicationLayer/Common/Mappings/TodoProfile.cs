using ApplicationLayer.Features.TodoItems.Dtos;
using AutoMapper;
using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Common.Mappings
{
    internal class TodoProfile :Profile
    {
        public TodoProfile()
        {
            // Map from the Domain entity to the DTO
            CreateMap<TodoItem, TodoItemDto>();

            // Optionally, map from a DTO/Command back to the entity (if needed for creation/updates)
            //CreateMap<CreateTodoItemCommand, TodoItem>(); 
        }
    }
}
