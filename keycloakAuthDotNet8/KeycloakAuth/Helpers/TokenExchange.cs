using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static KeycloakAuth.Startup;

namespace KeycloakAuth.Helpers
{
	public class TokenExchange
	{
		static readonly HttpClient client = new HttpClient();

		public TokenExchange()
		{
		}

        public async Task<string> GetRefreshTokenAsync(string refreshToken)
        {
            /*
             * Get refresh token
             * Uses the settings injected from startup to read the configuration
             */
            try
            {
                string url = Settings.Configuration["Keycloak:TokenExchange"];
                //Important the grant type fro refresh token, must be set to this!
                string grant_type = "refresh_token";
                string client_id = Settings.Configuration["Keycloak:ClientId"];
                string client_secret = Settings.Configuration["Keycloak:ClientSecret"];
                string token = refreshToken;

                var form = new Dictionary<string, string>
                {
                    {"grant_type", grant_type},
                    {"client_id", client_id},
                    {"client_secret", client_secret},
                    {"refresh_token", token }
                };

                HttpResponseMessage tokenResponse = await client.PostAsync(url, new FormUrlEncodedContent(form));
                var jsonContent = await tokenResponse.Content.ReadAsStringAsync();
                Token tok = JsonConvert.DeserializeObject<Token>(jsonContent);
                return tok.AccessToken;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> GetTokenExchangeAsync(string accessToken)
        {
            /*
             * Get exchange token
             * ses the settings injected from startup to read the configuration
             */
            try
            {
                string url = Settings.Configuration["Keycloak:TokenExchange"];
                //Important, the grant types for token exchange, must be set to this!
                string grant_type = "urn:ietf:params:oauth:grant-type:token-exchange";
                string client_id = Settings.Configuration["Keycloak:ClientId"];
                string client_secret = Settings.Configuration["Keycloak:ClientSecret"];
                string audience = Settings.Configuration["Keycloak:Audience"];
                string token = accessToken;

                var form = new Dictionary<string, string>
                {
                    {"grant_type", grant_type},
                    {"client_id", client_id},
                    {"client_secret", client_secret},
                    {"audience", audience},
                    {"subject_token", token }
                };

                HttpResponseMessage tokenResponse = await client.PostAsync(url, new FormUrlEncodedContent(form));
                var jsonContent = await tokenResponse.Content.ReadAsStringAsync();
                Token tok = JsonConvert.DeserializeObject<Token>(jsonContent);
                return tok.AccessToken;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        internal class Token
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }
        }
    }
}

