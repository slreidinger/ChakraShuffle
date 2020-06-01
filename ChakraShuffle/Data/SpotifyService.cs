using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace ChakraShuffle.Data {
    public class SpotifyService : PageModel {

        private readonly IConfiguration _configuration;
        private SpotifySettings SpotifySettings => _configuration.GetSection("Spotify").Get<SpotifySettings>();
        private AppSettings AppSettings => _configuration.GetSection("App").Get<AppSettings>();
        public SpotifyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetLibraryAsync()
        {
            var token = await GetToken();

            using (var client = new HttpClient())
            {
                //Define Headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.token_type, token.access_token);

                //Request Token
                var request = await client.GetAsync($"{SpotifySettings.API_URI}/v1/me/playlists");
                var response = await request.Content.ReadAsStringAsync();
                return response;
            }
        }

        private async Task<string> RequestAuthorizationCode()
        {
            string code = string.Empty;
            using var client = new HttpClient();


            var builder = new UriBuilder(SpotifySettings.AuthURI);
            builder.Query = $"client_id={SpotifySettings.ClientID}";
            builder.Query += "&response_type=code";
            builder.Query += $"&redirect_uri={AppSettings.Host}/shuffle";
            builder.Query += "&scope=user-read-private";

            var result = client.GetStringAsync(builder.Uri.ToString()).Result;


            return code;
        }

        private async Task<HttpResponseMessage> RequestAccessToken(string code)
        {
            using var client = new HttpClient();

            var parameters = new Dictionary<string, string>();
            parameters["grant_type"] = "authorization_code";
            parameters["code"] = code;
            parameters["redirect_uri"] = $"{AppSettings.Host}/shuffle";

            var encodedContent = new FormUrlEncodedContent(parameters);

            return await client.PostAsync(SpotifySettings.TokenURI, encodedContent);
        }

        private async Task<SpotifyToken> GetToken()
        {
            string credentials = $"{SpotifySettings.ClientID}:{SpotifySettings.ClientSecret}";

            using (var client = new HttpClient())
            {
                //Define Headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));

                //Prepare Request Body
                List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>();
                requestData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

                FormUrlEncodedContent requestBody = new FormUrlEncodedContent(requestData);

                //Request Token
                var request = await client.PostAsync(SpotifySettings.TokenURI, requestBody);
                var response = await request.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SpotifyToken>(response);
            }

        }

    }
}
