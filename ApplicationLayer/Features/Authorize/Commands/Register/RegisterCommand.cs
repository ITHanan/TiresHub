using DomainLayer.Common;
using DomainLayer.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Authorize.Commands.Register
{
    public class RegisterCommand : IRequest<OperationResult<string>>
    {
        public string Name { get; set; }    
        public string UserEmail { get; set; }
        public string Phone { get; internal set; }

        public string Password { get; set; }
        public UserRole Role { get; set; }// vehicle owner, shop owner, shop manager, employee  

        public RegisterCommand(string name , string email, string password, string phone,UserRole role)
        {
            Name = name;
            UserEmail = email;
            Password = password;
            Phone = phone;
            Role = role;
        }
    }
}
