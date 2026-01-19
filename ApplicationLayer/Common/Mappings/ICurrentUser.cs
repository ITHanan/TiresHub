using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Common.Mappings
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
    }

}
