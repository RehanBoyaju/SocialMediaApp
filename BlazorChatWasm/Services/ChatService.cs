using BlazorChatWasm.Models.Chat;
using BlazorChatWasm.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using BlazorChatWasm.Services;
using System.Net.Http;
namespace BlazorChatWasm.Services
{
    [Authorize]
    public class ChatService
    {
        private readonly HttpClient _httpClient;

        public ChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<ChatMessage>> GetConversationAsync(string contactId)
        {
            return (await _httpClient.GetFromJsonAsync<List<ChatMessage>>($"api/chat/{contactId}"))!;
        }
        public async Task<ApplicationUser> GetUserDetailsAsync(string userId)
        {
            var pfp = await GetProfile(userId);
            var data = await _httpClient.GetFromJsonAsync<ApplicationUser>($"api/chat/users/{userId}");
            data!.profileImageUrl = pfp;
            return data;
        }
        public async Task<List<ApplicationUser>> GetUsersAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<ApplicationUser>>("api/chat/users");
            foreach (var user in data!)
            {
                user.profileImageUrl = await GetProfile(user.Id);
            }
            return data!;
        }
        public async Task SaveMessageAsync(ChatMessage message)
        {
            await _httpClient.PostAsJsonAsync("api/chat", message);
        }
        public async Task<string> GetProfile(string userId)
        {
            // var image = await HttpClient.GetByteArrayAsync($"api/account/{userId}/profileimage");
            // return $"data:image/jpeg;base64,{Convert.ToBase64String(image)}";
            var response = await _httpClient.GetAsync($"api/account/{userId}/profileimage");
            if (response.IsSuccessStatusCode)
            {
                var image = await response.Content.ReadAsByteArrayAsync();
                var mimeType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                return $"data:{mimeType};base64,{Convert.ToBase64String(image)}";
            }
            throw new Exception("Failed to load image");
        }

        public async Task<List<ChatMessage>> SearchAsync(string searchTerm,string contactId)
        {
            var chats = await _httpClient.GetFromJsonAsync<List<ChatMessage>>($"api/chat/search/{contactId}/{searchTerm}");
            return chats!;
        }
    }
}