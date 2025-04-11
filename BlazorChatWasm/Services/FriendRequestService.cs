using BlazorChatWasm.Models.Auth;
using BlazorChatWasm.Models.User;
using System.Data.Common;
using System.Net.Http.Json;


namespace BlazorChatWasm.Services
{
    public class FriendRequestService
    {
        private readonly HttpClient httpClient;

        public FriendRequestService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<FriendRequest>> GetFriendRequestsAsync()
        {

            return (await httpClient.GetFromJsonAsync<List<FriendRequest>>("api/friendrequest"))!;
        }
        public async Task<List<FriendRequest>> GetFriendRequestsSentAsync()
        {

            return (await httpClient.GetFromJsonAsync<List<FriendRequest>>("api/friendrequest/sent"))!;
        }
        public async Task<FormResult> AcceptAsync(string friendId)
        {
            var response = await httpClient.PutAsJsonAsync($"api/friendrequest/accept", friendId);

            var formResult = await response.Content.ReadFromJsonAsync<FormResult>();
            if (formResult == null)
            {
                return new FormResult() { Succeeded = true, Errors = ["Unknown error occured"] };
            }
            if (formResult.Succeeded)
            {
                return new FormResult() { Succeeded = true, Errors = null };
            }
            else
            {
                return new FormResult() { Succeeded = false, Errors = formResult.Errors };
            }


        }
        public async Task<FormResult> RejectAsync(string friendId)
        {
            var response = await httpClient.PutAsJsonAsync($"api/friendrequest/reject", friendId);
            var formResult = await response.Content.ReadFromJsonAsync<FormResult>();
            if (formResult is null)
            {
                return new FormResult
                {
                    Succeeded = false,
                    Errors = new[] { "Unknown error" }
                };
            }
            return formResult;
        }
        public async Task<FormResult> CancelRequestAsync(string friendId)
        {
            try
            {
                var formResult = await httpClient.DeleteFromJsonAsync<FormResult>($"api/friendrequest/delete/{friendId}");
                if (formResult is null)
                {
                    throw new Exception("Unknown error occurred");
                }
                return formResult;

            }
            catch (Exception ex)
            {

                return new FormResult
                {
                    Succeeded = false,
                    Errors = new[] { ex.Message }
                };

            }
        }
    }
}
