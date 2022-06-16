using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UAF_Frontend_Registration.Helpers;
using UAF_Frontend_Registration.Settings;
using static System.Convert;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using UAF_Frontend_Registration.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace UAF_Frontend_Registration.Controllers
{
    [Route("signin-oidc")]
    public class CallbackController : Controller
    {

        private readonly IEndpointSettings _oidcConfig;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Client auth = new Client();


        public CallbackController(IEndpointSettings oidcConfig, ILoggerFactory loggerFactory)
        {
            _oidcConfig = oidcConfig;

            loggerFactory =
              LoggerFactory.Create(builder =>
                  builder.AddSimpleConsole(options =>
                  {
                      options.IncludeScopes = true;
                      options.SingleLine = true;
                      options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]: ";
                  }));
            _loggerFactory = loggerFactory;
        }

        public IActionResult Index()
        {
            var _logger = _loggerFactory.CreateLogger<CallbackController>();

            try
            {
                string responseString = auth.GetToken(HttpContext.Request.Query["code"], _oidcConfig);
                SaveDataToSession(responseString);

                Response.Cookies.Append("username", HttpContext.Session.GetString("en_fullname"));

                _logger.LogInformation("Login Dopa Success");
            }
            catch (Exception ex)
            {
                clearSession();
                _logger.LogError("Login Dopa Failed: " + ex);

                return Redirect("/Home/Error");
            }

            return Redirect("/Home/Register");
        }

        private void SaveDataToSession(string curityResponse)
        {
            var _logger = _loggerFactory.CreateLogger<CallbackController>();

            JObject jsonObj = JObject.Parse(curityResponse);

            try
            {
                HttpContext.Session.SetString("access_token", jsonObj.GetValue("access_token").ToString());
                _logger.LogInformation("access_token valid");
            }
            catch (Exception ex)
            {
                throw new Exception("access_token Invalid: " + ex);
            }

            try
            {
                HttpContext.Session.SetString("refresh_token", jsonObj.GetValue("refresh_token").ToString());
                _logger.LogInformation("refresh_token valid");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("refresh_token Invalid: " + ex);
            }

            try
            {
                HttpContext.Session.SetString("scope", jsonObj.GetValue("scope").ToString());
                _logger.LogInformation("scope: " + jsonObj.GetValue("scope").ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("scope Invalid: " + ex);
            }

            try
            {
                var id_token = jsonObj.GetValue("id_token").ToString();
                _logger.LogInformation("id_token valid");

                if (jsonObj.GetValue("id_token") != null && IsJwtValid(id_token))
                {
                    HttpContext.Session.SetString("id_token", jsonObj.GetValue("id_token").ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("id_token Invalid: " + ex);
            }
        }

        private string SafeDecodeBase64(string str)
        {
            return System.Text.Encoding.UTF8.GetString(
                getPaddedBase64String(str));
        }

        private byte[] getPaddedBase64String(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0 ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("_", "/").Replace("-", "+");
            return FromBase64String(base64);
        }

        private bool IsJwtValid(string jwt)
        {
            string th_fname = "";
            string th_lname = "";
            string en_fname = "";
            string en_lname = "";

            string[] jwtParts = jwt.Split('.');

            string decodedHeader = SafeDecodeBase64(jwtParts[0]);
            string decodedPayload = SafeDecodeBase64(jwtParts[1]);

            string keyId = JObject.Parse(decodedHeader).GetValue("kid").ToString();
            JObject keyFound = null;

            if (!string.IsNullOrEmpty(keyId))
            {
                string fetch_key = FetchKeys(keyId);
                keyFound = JObject.Parse(fetch_key);

                if (keyFound == null)
                {
                    throw new Exception("Key not found in JWKS endpoint or Application State");
                }
            }
            else
            {
                throw new Exception("keyId not validated");
            }

            if (!JObject.Parse(decodedPayload).GetValue("iss").ToString().Equals(_oidcConfig.issuer))
            {
                throw new Exception("Issuer not validated");
            }

            try
            {
                var pid = JObject.Parse(decodedPayload).GetValue("pid").ToString();
                HttpContext.Session.SetString("pid", pid);
            }
            catch (Exception ex)
            {
                throw new Exception("Not found pid in id_token: " + ex);
            }

            try
            {
                th_fname = JObject.Parse(decodedPayload).GetValue("th_fname").ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Not found th_fname in id_token: " + ex);
            }

            try
            {
                th_lname = JObject.Parse(decodedPayload).GetValue("th_lname").ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Not found th_lname in id_token: " + ex);
            }

            var th_fullname = th_fname + " " + th_lname;
            HttpContext.Session.SetString("th_fullname", th_fullname);

            try
            {
                en_fname = JObject.Parse(decodedPayload).GetValue("en_fname").ToString();
                HttpContext.Session.SetString("en_fname", en_fname);
            }
            catch (Exception ex)
            {
                throw new Exception("Not found en_fname in id_token: " + ex);
            }

            try
            {
                en_lname = JObject.Parse(decodedPayload).GetValue("en_lname").ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Not found en_lname in id_token: " + ex);
            }

            var en_fullname = en_fname + " " + en_lname;
            HttpContext.Session.SetString("en_fullname", en_fullname);

            var key = ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint
                {
                    X = System.Convert.FromBase64String(keyFound["x"].ToString()),
                    Y = System.Convert.FromBase64String(keyFound["y"].ToString())
                }
            });

            var handler = new JsonWebTokenHandler();

            TokenValidationResult result = handler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidIssuer = _oidcConfig.issuer,
                ValidAudience = "dgauth",
                IssuerSigningKey = new ECDsaSecurityKey(key)
            });

            return result.IsValid;

            #region RASA
            /*RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(
              new RSAParameters()
              {
                  Modulus = getPaddedBase64String(keyFound["x"].ToString()),
                  Exponent = getPaddedBase64String(keyFound["y"].ToString())
              });

            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(jwtParts[0] + '.' + jwtParts[1]));

            RSAPKCS1SignatureDeformatter rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            rsaDeformatter.SetHashAlgorithm("SHA256");

            if (rsaDeformatter.VerifySignature(hash, getPaddedBase64String(jwtParts[2])))
            {
                return true; //Jwt Validated
            }
            else
            {
                throw new Exception("Could not validate signature of JWT");

            }*/
            #endregion
        }

        private string FetchKeys(string keyId)
        {
            var _logger = _loggerFactory.CreateLogger<CallbackController>();

            var jwksclient = new HttpClient();
            jwksclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = jwksclient.GetAsync(_oidcConfig.jwks_uri).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                string responseString = responseContent.ReadAsStringAsync().Result;

                foreach (JToken key in JObject.Parse(responseString).GetValue("keys").ToArray())
                {
                    if (key["kid"].ToString().Equals(keyId))
                    {
                        HttpContext.Session.SetString("keyId", keyId);
                        _logger.LogInformation("key: " + key.ToString());
                        return key.ToString();
                    }
                }

                throw new Exception("Key not found in JWKS endpoint");
            }

            throw new Exception("Could not contact JWKS endpoint");
        }

        private void clearSession()
        {
            HttpContext.Session.Clear();

            foreach (string cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
        }
    }
}
