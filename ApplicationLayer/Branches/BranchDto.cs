using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Branches;


public record BranchDto(
    Guid Id,
    string Name,
    string City,
    string Address,
    bool IsActive
);
