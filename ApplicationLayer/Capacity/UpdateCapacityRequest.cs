using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Capacity;

public record UpdateCapacityRequest(int Capacity, bool ForceIfBelowUsage = false);

