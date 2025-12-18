using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string PasswordHash { get; set; } = null!;

        public ICollection<TodoItem> ToDoList { get; set; } = new List<TodoItem>();
    }
}
