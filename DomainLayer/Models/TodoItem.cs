using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsDone { get; set; } = false;
        public string? Description { get; set; }

        // 1. Foreign Key Property
        public int UserId { get; set; }

        // 2. Navigation Property back to the User
        public User User { get; set; } = null!;
    }
}
