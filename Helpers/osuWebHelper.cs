using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace osb.Helpers
{
    public class OsuWebHelper
    {
        private string baseURL = "https://osu.ppy.sh/";
        private HttpClient client = new HttpClient();

        private readonly IConfiguration Configuration;

        public OsuWebHelper(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string GetAuthorizationCode()
        {
            string URL = Configuration.GetSection("API")["AuthURL"];
            string clientID = Configuration.GetSection("API")["ClientID"];
            string redirectURI = Configuration.GetSection("API")["RedirectURL"];
            //Use Public Scope for Now
            URL = URL.Replace(":clientID", clientID).Replace(":redirect_uri", redirectURI);
            return URL;
        }
    }

}
