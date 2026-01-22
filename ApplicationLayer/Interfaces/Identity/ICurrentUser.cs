using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DomainLayer.Enums;

namespace ApplicationLayer.Interfaces.Identity
{
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        Guid UserId { get; }
        UserRole Role { get; }
    }
}
