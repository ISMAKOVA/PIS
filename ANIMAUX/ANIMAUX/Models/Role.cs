﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ANIMAUX.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AccessLevel { get; set; }
    }
}