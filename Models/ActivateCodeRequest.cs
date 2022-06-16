﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UAF_Frontend_Registration.Models
{
    public class ActivateCodeRequest
    {
        [Required]
        public string username { get; set; }

        [Required]
        public string activation_code { get; set; }
    }
}
