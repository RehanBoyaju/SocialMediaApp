using BlazorChatWasm.Models.Auth;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;

namespace BlazorChatWasm.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient httpClient;
        private readonly ISyncLocalStorageService localStorage;

        public CustomAuthenticationStateProvider(HttpClient httpClient, ISyncLocalStorageService localStorage) {
            this.httpClient = httpClient;
            this.localStorage = localStorage;

            var accessToken = localStorage.GetItem<string>("accessToken");
            if (accessToken != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
           // var user = new ClaimsPrincipal(new ClaimsIdentity()); // not authenticated user
            //var claims = new List<Claim> { new Claim(ClaimTypes.Name, "John Doe") };
            //var identity = new ClaimsIdentity(claims, "Any");
            //var user = new ClaimsPrincipal(identity); //authenticated user
            //return Task.FromResult(new AuthenticationState(user));
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // not authenticated user
            try
            {
                var response = await httpClient.GetAsync("api/account/profile");
                if (response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var id = jsonResponse?["id"]?.ToString();
                    var userName = jsonResponse?["userName"]?.ToString();
                    var email = jsonResponse?["email"]?.ToString();
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.NameIdentifier,id),
                        new Claim(ClaimTypes.Name, userName),
                        new Claim(ClaimTypes.Email, email)
                    };
                    var identity = new ClaimsIdentity(claims, "Token");
                    user = new ClaimsPrincipal(identity);
                    return new AuthenticationState(user);   
                }
            }
            catch { }

            return new AuthenticationState(user);

        }
        public async Task<FormResult> LoginAsync(LoginModel loginModel)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("login", loginModel);
                if (response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var accessToken = jsonResponse?["accessToken"]?.ToString();
                    var refreshToken = jsonResponse?["refreshToken"]?.ToString();
                    
                    localStorage.SetItem("accessToken", accessToken);
                    localStorage.SetItem("refreshToken", refreshToken);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                    return new FormResult { Succeeded = true };
                }
                else
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var errorsObject = jsonResponse!["errors"]!.AsObject();
                    var errorsList = errorsObject.Select(e => e.Value![0]!.ToString()).ToList();
                    return new FormResult { Succeeded = false, Errors = errorsList.ToArray() };
                }
            }
            catch { }

            return new FormResult { Succeeded = false, Errors = new string[] { "Invalid Login Attempt" } };
        }
        public async Task<FormResult> RegisterAsync(RegisterModel registerModel)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("register", registerModel);
                if (response.IsSuccessStatusCode)
                {
                    return await LoginAsync(new LoginModel { Email = registerModel.Email, Password = registerModel.Password });
                }
                else
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var errorsObject = jsonResponse!["errors"]!.AsObject();
                    var errorsList = errorsObject.Select(e => e.Value![0]!.ToString()).ToList();

                    return new FormResult { Succeeded = false, Errors = errorsList.ToArray() };
                }
            }
            catch { }

            return new FormResult { Succeeded = false, Errors = new string[] { "Connection Error" } };
        }
        //public async Task<byte[]> GetProfileImage(string userId)
        //{
        //    var response = await httpClient.GetAsync($"api/account/{userId}/profileimage");
        //    if (response.IsSuccessStatusCode)
        //    {
        //        return await response.Content.ReadAsByteArrayAsync();
        //    }

        //    return null;


        //}

        public void Logout()
        {
            localStorage.RemoveItem("accessToken");
            localStorage.RemoveItem("refreshToken");
            httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
    
    
}
