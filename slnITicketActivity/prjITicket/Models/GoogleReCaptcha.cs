using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoogleRecaptcha;
using System.Net.Http;

namespace prjITicket.Models
{
    public class GoogleReCaptcha
    {

        public bool Success { get; set; }
        public bool GetCaptchaResponse(string message)
        {
            
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new[]{
                      new KeyValuePair<string, string>("secret", "6LdLEP8ZAAAAAINndsRWW6iVg5w6-XsLJN841xfk"),
                      new KeyValuePair<string, string>("response", message),
                   });

                    var result = client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content).Result;
                    var resultContent = result.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<GoogleReCaptcha>(resultContent);
                    return data.Success;
                }
            }
            catch { return false; }
        }


    }
}