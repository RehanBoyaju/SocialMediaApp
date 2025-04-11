using BlazorChatWasm.Models.User;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace BlazorChatWasm.Services
{
    [Authorize]
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<ApplicationUser?> GetUserDetailsAsync(string userId)
        {
            //Console.WriteLine("Getting user details now");
            var response = await _httpClient.GetAsync($"api/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error:{response.StatusCode}");
                return null;
            }
            var data = await response.Content.ReadFromJsonAsync<ApplicationUser>();
            //Console.WriteLine("Data received in GetUserDetails");
            //Console.WriteLine(data.Id);
            return data;
        }
        public async Task<List<ApplicationUser>> GetUsersAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<ApplicationUser>>("api/users");


            return data!;
        }
    }
}
