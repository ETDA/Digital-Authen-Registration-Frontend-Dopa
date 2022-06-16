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
    public class QRCodeRequestService
    {

        private static readonly string SUCCESS = "success";

        private readonly ILoggerFactory _loggerFactory;

        public QRCodeRequestService(ILoggerFactory loggerFactory)
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

        public QRCodeResp qrCodeAsync(string identity, string name, string callback_url, string callback_url_qr, string token)
        {
            var _logger = _loggerFactory.CreateLogger<QRCodeRequestService>();

            QRCodeResp qr_resp;

            try
            {

                 _logger.LogInformation("Curl to: " + callback_url);

                var qr_req_body = new QRCodeReq();
                qr_req_body.identity = identity;
                qr_req_body.name = name;

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", token);
                client.Timeout = TimeSpan.FromMinutes(1);
                var response = client.PostAsJsonAsync(callback_url, qr_req_body).Result;
                var result = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    
                    if (result.Contains(SUCCESS))
                    {
                        _logger.LogInformation("Curl to: " + callback_url_qr);
                        var qr_response = client.PostAsJsonAsync(callback_url_qr, qr_req_body).Result;
                        var qr_result = qr_response.Content.ReadAsStringAsync().Result;
                        qr_resp = JsonConvert.DeserializeObject<QRCodeResp>(qr_result);

                        if(qr_resp.qrcode == null)
                        {
                            _logger.LogError("Get Qr Code ERROR: " + qr_resp.description);
                            qr_resp = null;
                        }
                    }
                    else
                    {
                        var resp = JsonConvert.DeserializeObject<QRCodeResp>(result);
                        _logger.LogError("Get Qr Code ERROR: HTTP Status Code " + response.StatusCode + ", Desc: " + resp.description);
                        qr_resp = null;
                    }

                    //_logger.LogInformation("Doc Server Response Message: " + result);
                }
                else
                {
                    var resp = JsonConvert.DeserializeObject<QRCodeResp>(result);
                    _logger.LogError("Get Qr Code ERROR: HTTP Status Code " + response.StatusCode+", Desc: " + resp.description);
                    qr_resp = null;
                }
            }
            catch (Exception ex)
            {
                qr_resp = null;
                _logger.LogError("Get Qr Code ERROR: " + ex.Message);
                ex.StackTrace.ToString();
            }

            return qr_resp;
        }
    }
}
