using SaborSostenibleFrontEnd.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Response
{
    public class ResBase
    {
        public IEnumerable<Error> Errors { get; set; } = new List<Error>();
        public bool Success { get; set; } = false;
    }
}
