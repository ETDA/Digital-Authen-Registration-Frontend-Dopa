using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UAF_Frontend_Registration.Settings
{
    public class EndpointSettings : IEndpointSettings
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string redirect_uri { get; set; }
        public string scope { get; set; }
        public string authorization_endpoint { get; set; }
        public string token_endpoint { get; set; }
        public string logout_endpoint { get; set; }
        public string revocation_endpoint { get; set; }
        public string jwks_uri { get; set; }
        public string issuer { get; set; }

    }
    public interface IEndpointSettings
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string redirect_uri { get; set; }
        public string scope { get; set; }
        public string authorization_endpoint { get; set; }
        public string token_endpoint { get; set; }
        public string logout_endpoint { get; set; }
        public string revocation_endpoint { get; set; }
        public string jwks_uri { get; set; }
        public string issuer { get; set; }
    }
}
