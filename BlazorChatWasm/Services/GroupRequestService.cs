using BlazorChatWasm.Models;
using BlazorChatWasm.Models.Auth;
using BlazorChatWasm.Models.DTOs;
using System.Data.Common;
using System.Net.Http.Json;


namespace BlazorChatWasm.Services
{
    public class GroupRequestService
    {
        private readonly HttpClient httpClient;

        public GroupRequestService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<GroupRequest>> GetGroupRequestsAsync(int groupId)
        {

            return (await httpClient.GetFromJsonAsync<List<GroupRequest>>($"api/Grouprequest/{groupId}"))!;
        }



        public async Task<List<GroupRequest>> GetGroupRequestsSentAsync()
        {

            return (await httpClient.GetFromJsonAsync<List<GroupRequest>>("api/Grouprequest/sent"))!;
        }



        public async Task<FormResult> AcceptAsync(GroupRequestDTO groupRequestDTO)
        {
            var response = await httpClient.PutAsJsonAsync($"api/Grouprequest/accept", groupRequestDTO);

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


        public async Task<FormResult> RejectAsync(GroupRequestDTO groupRequest)
        {
            var response = await httpClient.PutAsJsonAsync($"api/Grouprequest/reject", groupRequest);
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


        public async Task<FormResult> CancelRequestAsync(int GroupId)
        {
            try
            {
                var formResult = await httpClient.DeleteFromJsonAsync<FormResult>($"api/Grouprequest/delete/{GroupId}");
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
