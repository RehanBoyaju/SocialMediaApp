﻿using BlazorChatWasm.Models.Auth;
using BlazorChatWasm.Models.User;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BlazorChatWasm.Services
{
    public class FriendsService
    {
        private readonly HttpClient _httpClient;

        public FriendsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<FormResult> AddFriendsAsync(string newFriendId)
        {
            
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/friends/add", newFriendId);

                // Always attempt to read the response, even if the status is not 2xx
                var strResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                 
                    var formResult = JsonSerializer.Deserialize<FormResult>(strResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return formResult ?? new FormResult() { Errors = ["Unknown error has occured"], Succeeded = false };
                }
                else
                {
                    return new FormResult { Succeeded = false, Errors = [ "Something went wrong. Please try again."]  };
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle network-related errors
                return new FormResult { Succeeded = false, Errors = [ ex.Message ] };
            }
        }

        public async Task<FormResult> UnfriendAsync(string friendId)
        {

            try
            {
                var response = await _httpClient.DeleteAsync($"api/friends/{friendId}");
                var strResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var formResult = JsonSerializer.Deserialize<FormResult>(strResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return formResult ?? new FormResult() { Errors = ["Unknown error has occured"], Succeeded = false };
                }
                else
                {
                    return new FormResult { Succeeded = false, Errors = ["Something went wrong. Please try again."] };
                }
            }
            catch (HttpRequestException ex)
            {
                return new FormResult { Succeeded = false, Errors = [ex.Message] };
            }
        }

        public async Task<List<ApplicationUser>> GetFriendsAsync(string userId)
        {
            Console.WriteLine("Getting friends");
            var data = await _httpClient.GetFromJsonAsync<List<ApplicationUser>>($"api/friends/{userId}");
            
            return data!;
        }

        public async Task<List<ApplicationUser>> GetNonFriendsAsync(string userId)
        {
            var data = await _httpClient.GetFromJsonAsync<List<ApplicationUser>>($"api/friends/add/{userId}");

            return data!;
        }

        public async Task<int> GetFriendsCountAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<int>($"api/friends/count/{userId}");
            
        }
    }
}
