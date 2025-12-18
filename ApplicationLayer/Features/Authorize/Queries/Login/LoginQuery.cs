using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Authorize.Queries.Login
{
    public class LoginQuery : IRequest<OperationResult<string>>
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public LoginQuery(string username, string password)
        {
            UserName = username;
            Password = password;
        }
    }
}
