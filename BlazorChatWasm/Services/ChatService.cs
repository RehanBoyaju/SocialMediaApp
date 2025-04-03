using BlazorChatWasm.Models.Chat;
using BlazorChatWasm.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using BlazorChatWasm.Services;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
using BlazorChatWasm.Models.DTOs;
using BlazorChatWasm.Models.Auth;
using System.Text.Json.Nodes;
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
       
       
        public async Task SaveMessageAsync(ChatMessage message)
        {
            await _httpClient.PostAsJsonAsync("api/chat", message);
        }
        

        public async Task<List<ChatMessage>> SearchAsync(string searchTerm,string? contactId,int? groupId)
        {
            if(groupId != null)
            {
                var groupchats = await _httpClient.GetFromJsonAsync<List<ChatMessage>>($"api/chat/search/group/{groupId}/{searchTerm}");
                return groupchats!;
            }
            var chats = await _httpClient.GetFromJsonAsync<List<ChatMessage>>($"api/chat/search/{contactId}/{searchTerm}");
            return chats!;
        }
        
    }
}