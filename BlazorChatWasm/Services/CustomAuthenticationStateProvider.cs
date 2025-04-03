using BlazorChatWasm.Models;
using BlazorChatWasm.Models.Auth;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;
using static BlazorChatWasm.Pages.Auth.ProfileModal;

namespace BlazorChatWasm.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient httpClient;
        private readonly ISyncLocalStorageService localStorage;

        public CustomAuthenticationStateProvider(HttpClient httpClient, ISyncLocalStorageService localStorage)
        {
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
                var response = await httpClient.GetAsync("Account");
                if (response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var id = jsonResponse?["id"]?.ToString();
                    var userName = jsonResponse?["userName"]?.ToString();
                    var email = jsonResponse?["email"]?.ToString();
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.NameIdentifier,id!),
                        new Claim(ClaimTypes.Name, userName!),
                        new Claim(ClaimTypes.Email, email!)
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
                var strResponse = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonNode.Parse(strResponse);

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        if (jsonResponse?["errors"] is JsonObject errorsObject)
                        {
                            var errors = errorsObject
                                .SelectMany(e => e.Value?.AsArray()?.Select(v => v?.ToString())
                                .Where(e => e != null)
                                .ToArray()!);

                            return new FormResult
                            {
                                Succeeded = false,
                                Errors = (errors.Any() ? errors! : new[] { "Login failed" }).ToArray()!
                            };
                        }
                        // Handle other error formats
                        var errorMessage = jsonResponse?["title"]?.ToString()
                            ?? jsonResponse?["detail"]?.ToString()
                            ?? "Login failed";

                        return new FormResult
                        {
                            Succeeded = false,
                            Errors = new[] { errorMessage }
                        };
                    }
                    catch
                    {
                        return new FormResult() { Succeeded = false, Errors = new string[] { $"Login failed {response.StatusCode}" } };
                    }
                }
                try
                {
                    var accessToken = jsonResponse?["accessToken"]?.ToString();
                    var refreshToken = jsonResponse?["refreshToken"]?.ToString();
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        return new FormResult
                        {
                            Succeeded = false,
                            Errors = new[] { "Invalid token received" }
                        };
                    }

                    // Store tokens securely
                    localStorage.SetItem("accessToken", accessToken);

                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        localStorage.SetItem("refreshToken", refreshToken);
                    }

                    // Set default authorization header
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);

                    // Notify authentication state change
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                    return new FormResult { Succeeded = true };

                }
                catch (Exception ex)
                {
                    // Clean up if token processing fails
                    localStorage.RemoveItem("accessToken");
                    localStorage.RemoveItem("refreshToken");
                    httpClient.DefaultRequestHeaders.Authorization = null;

                    return new FormResult
                    {
                        Succeeded = false,
                        Errors = new[] { $"Error processing login: {ex.Message}" }
                    };
                }

            }
            catch (HttpRequestException httpEx)
            {
                return new FormResult
                {
                    Succeeded = false,
                    Errors = new[] { $"Network error: {httpEx.Message}" }
                };
            }
            catch (TaskCanceledException)
            {
                return new FormResult
                {
                    Succeeded = false,
                    Errors = new[] { "Request timed out" }
                };
            }
            catch (Exception ex)
            {
                return new FormResult
                {
                    Succeeded = false,
                    Errors = new[] { $"Unexpected error: {ex.Message}" }
                };
            }


        }
        public async Task<FormResult> RegisterAsync(RegisterModel registerModel)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("register", registerModel);
                if (response.IsSuccessStatusCode)
                {
                    return await LoginAsync(new LoginModel { Username = registerModel.Email, Password = registerModel.Password });
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
    
        public async Task<FormResult> UpdateAsync(string email, UpdateModel updateUser)
        {
            var response = await httpClient.PutAsJsonAsync($"update/{email}", updateUser);
            if (response.IsSuccessStatusCode)
            {
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
        public async Task<FormResult> ChangePasswordAsync(ChangeModel changeModel)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync($"changepassword", changeModel);

                if (response.IsSuccessStatusCode)
                {
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
            catch
            {

            }
            return new FormResult { Succeeded = false, Errors = new string[] { "Invalid password" } };
        }



        public async Task<FormResult> DeleteAsync(string email)
        {
            var response = await httpClient.DeleteAsync($"delete/{email}");
            if (response.IsSuccessStatusCode)
            {
                Logout();
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
       

        public void Logout()
        {
            localStorage.RemoveItem("accessToken");
            localStorage.RemoveItem("refreshToken");
            httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }


}
