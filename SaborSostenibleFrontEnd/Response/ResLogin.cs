﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Response
{
    public class ResLogin : ResBase
    {
        public string? SessionId { get; set; }
        public string UserRole { get; set; }
        public bool? PendingRequest { get; set; }
    }
}
