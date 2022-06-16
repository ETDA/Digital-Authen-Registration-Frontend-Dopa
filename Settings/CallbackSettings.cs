using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UAF_Frontend_Registration.Settings
{
    public class CallbackSettings : ICallbackSettings
    {

        public string Register_request { get; set; }
        public string Qrcode_request { get; set; }
        public string Token_request { get; set; }
        public string Credentials { get; set; }
}
    public interface ICallbackSettings
    {
        public string Register_request { get; set; }
        public string Qrcode_request { get; set; }
        public string Token_request { get; set; }
        public string Credentials { get; set; }
    }
}
