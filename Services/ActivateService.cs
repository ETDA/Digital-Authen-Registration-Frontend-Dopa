
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UAF_Frontend_Registration.Models;

namespace UAF_Frontend_Registration.Services
{
    public class ActivateService
    {
        private readonly ILoggerFactory _loggerFactory;
        public ActivateService(ILoggerFactory loggerFactory)
        {
            loggerFactory =
               LoggerFactory.Create(builder =>
                   builder.AddSimpleConsole(options =>
                   {
                       options.IncludeScopes = true;
                       options.SingleLine = true;
                       options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]: ";
                   }));
            this._loggerFactory = loggerFactory;
        }

        public ActivateCodeResponse request_actCodeAsync(string callback_url, string username, string activation_code)
        {
            var _logger = _loggerFactory.CreateLogger<ActivateService>();

            ActivateCodeResponse resp = new ActivateCodeResponse();

            try
            {
                _logger.LogInformation("Curl to: " + callback_url);

                var req_body = new ActivateCodeRequest();
                req_body.username = username;
                req_body.activation_code = activation_code;

                var client = new HttpClient();
                //client.DefaultRequestHeaders.Add("Authorization", "TOKEN");
                client.Timeout = TimeSpan.FromMinutes(1);
                var response = client.PostAsJsonAsync(callback_url, req_body).Result;

                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    _logger.LogInformation("Activation Code Status: OK");

                    var result = response.Content.ReadAsStringAsync().Result;
                    resp = JsonConvert.DeserializeObject<ActivateCodeResponse>(result);

                    _logger.LogInformation("Activation Code Status Message: " + result);
                }
                else
                {
                    resp = null;
                    _logger.LogError("Activation Code Status ERROR: HTTP Status Code " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                resp = null;
                _logger.LogError("Curl to "+ callback_url + " ERROR: " + ex.Message);
                ex.StackTrace.ToString();
            }

            return resp;
        }

        public ActivateCodeResponse request_updateCodeAsync(string callback_url, string username)
        {
            var _logger = _loggerFactory.CreateLogger<ActivateService>();

            ActivateCodeResponse resp = new ActivateCodeResponse();

            try
            {
                _logger.LogInformation("Curl to: " + callback_url);

                var req_body = new ActivateCodeRequest();
                req_body.username = username;

                var client = new HttpClient();
                //client.DefaultRequestHeaders.Add("Authorization", "TOKEN");
                client.Timeout = TimeSpan.FromMinutes(1);
                var response = client.PutAsJsonAsync(callback_url, req_body).Result;

                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    _logger.LogInformation("Activation Code Status: OK");

                    var result = response.Content.ReadAsStringAsync().Result;
                    resp = JsonConvert.DeserializeObject<ActivateCodeResponse>(result);

                    _logger.LogInformation("Activation Code Status Message: " + result);
                }
                else
                {
                    resp = null;
                    _logger.LogError("Update Activation Code ERROR: HTTP Status Code " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                resp = null;
                _logger.LogError("Curl to " + callback_url + " ERROR: " + ex.Message);
                ex.StackTrace.ToString();
            }

            return resp;
        }
    }
}
