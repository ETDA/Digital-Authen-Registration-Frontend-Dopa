using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UAF_Frontend_Registration.Models
{
    public class QRCodeResp
    {
        public string qrToken { get; set; }

        public string qrcode { get; set; }

        public string status { get; set; }

        public string description { get; set; }
    }
}
