using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UAF_Frontend_Registration.Settings;

namespace UAF_Frontend_Registration.Helpers
{
    public class Client
    {
        public Client()
        {

        }

        public string GetAuthnReqUrl(IEndpointSettings _oidcConfig)
        {
            return _oidcConfig.authorization_endpoint + "?client_id=" + _oidcConfig.client_id
               + "&client_secret=" + _oidcConfig.client_secret
               + "&response_type=code"
               + "&response_mode=query"
               + "&scope=" + _oidcConfig.scope
               + "&redirect_uri=" + _oidcConfig.redirect_uri;
               
        }

        public string GetToken(string code, IEndpointSettings _oidcConfig)
        {
            /*var values = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", _oidcConfig.client_id},
                { "client_secret", _oidcConfig.client_secret },
                { "code" , code },
                { "redirect_uri", _oidcConfig.redirect_uri}
            };*/

            var values = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code" , code },
                { "redirect_uri", _oidcConfig.redirect_uri}
            };


            var auth = _oidcConfig.client_id + ":" + _oidcConfig.client_secret;
            var authBytes = System.Text.Encoding.UTF8.GetBytes(auth);
            var base64_auth = System.Convert.ToBase64String(authBytes);
            var basic_auth = "Basic " + base64_auth;

            HttpClient tokenClient = new HttpClient();
            tokenClient.DefaultRequestHeaders.Add("Authorization", basic_auth);
            tokenClient.Timeout = TimeSpan.FromMinutes(1);

            var content = new FormUrlEncodedContent(values);
            var response = tokenClient.PostAsync(_oidcConfig.token_endpoint, content).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;

                return responseContent.ReadAsStringAsync().Result;
            }

            throw new Exception("Token request failed with status code: " + response.StatusCode);
        }
    }
}
