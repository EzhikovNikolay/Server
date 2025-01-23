using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RedirectRequestsServer.Class
{
    public class WebApiManager
    {
        private const string ApiUrl = "http://localhost:5000/api/text";

        public async Task GetTextFromWebApi()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(ApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string text = await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
