﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UAF_Frontend_Registration.Settings
{
    public class LDAPSettings: ILDAPSettings
    {
        public string AD_domain { get; set; }
        public int AD_port { get; set; }
    }
    public interface ILDAPSettings
    {
        public string AD_domain { get; set; }
        public int AD_port { get; set; }
    }
}
