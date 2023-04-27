using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Management.DynamicKey.Windows.Service
{
    public class Dto
    {
        public record Dummy(Guid Id, string Name, string Description);
    }
}