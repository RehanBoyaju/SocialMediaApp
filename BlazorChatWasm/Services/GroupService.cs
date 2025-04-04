using BlazorChatWasm.Models;
using BlazorChatWasm.Models.Auth;
using BlazorChatWasm.Models.Chat;
using BlazorChatWasm.Models.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace BlazorChatWasm.Services
{
    public class GroupService
    {
        private readonly HttpClient _httpClient;

        public GroupService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Group>?> GetGroupsAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<Group>>("api/group");

            return data;
        }
        public async Task<List<ChatMessage>?> GetGroupConversationAsync(int groupId)
        {
            var data = await _httpClient.GetFromJsonAsync<List<ChatMessage>>($"api/group/chat/{groupId}");
            return data;
        }
        public async Task<Group> GetGroupByIdAsync(int groupId)
        {

            var data = await _httpClient.GetFromJsonAsync<BaseGroupDTO>($"api/group/{groupId}");
            if (data == null)
            {
                throw new Exception("Group not found");
            }
            var group = new Group() { Id = data.Id, Description = data.Description, Name = data.Name,ImageUrl = data.ImageUrl };
            foreach (var item in data.MembersInfo)
            {
                group.MemberIds.Add(item.Id);
                group.Members.Add(new ApplicationUser() { Id = item.Id, UserName = item.UserName, Email = item.Email, ImageUrl = item.ImageUrl });
            }
            return group;
        }
        public async Task<FormResult> AddGroup(Group group)
        {
            var response = await _httpClient.PostAsJsonAsync("api/group", group);
            if (response.IsSuccessStatusCode)
            {
                return new FormResult { Succeeded = true, Errors = null };
            }
            else
            {
                var strResponse = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonNode.Parse(strResponse);
                var errorsObject = jsonResponse!["errors"]!.AsObject();
                var errorsList = errorsObject.Select(e => e.Value![0]!.ToString()).ToList();

                return new FormResult() { Succeeded = false, Errors = errorsList.ToArray() };
            }
        }
        public async Task<FormResult> UpdateGroup(Group group)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/group/{group.Id}", group);
            if (response.IsSuccessStatusCode)
            {
                return new FormResult { Succeeded = true, Errors = null };
            }
            else
            {
                var strResponse = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonNode.Parse(strResponse);
                var errorsObject = jsonResponse!["errors"]!.AsObject();
                var errorsList = errorsObject.Select(e => e.Value![0!]!.ToString()).ToList();
                return new FormResult() { Succeeded = false, Errors = errorsList.ToArray() };
            }
        }
    }
}
