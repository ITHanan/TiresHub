using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{
    public interface IJwtGenerator
    {
        // Generates a JWT token for the authenticated user
        string GenerateToken(User user);
    }
}
