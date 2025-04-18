using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorChatWasm;
using BlazorChatWasm.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Blazored.Modal;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://192.168.254.6:5000") });
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5085") });
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredModal();
builder.Services.AddTransient<ChatService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<FriendsService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<FriendRequestService>();
builder.Services.AddScoped<GroupRequestService>();



await builder.Build().RunAsync();

//TODO: Add a app adminstrator where he can see everything
//TODO: Use refresh token