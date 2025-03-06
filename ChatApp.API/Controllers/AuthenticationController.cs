using ChatApp.API.Data;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
namespace ChatApp.API.Controllers
{
    [Route("")]
    [ApiController]

    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AuthenticationController(IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManger;
        }

        [HttpPost("register")]
        public async Task<Results<Ok, ValidationProblem>> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                ProfileImage = ConvertToByteArray(model.ProfileImageUrl)
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            //await SendConfirmationEmailAsync(user, userManager, context, email);
            return TypedResults.Ok();
        }
        private byte[] ConvertToByteArray(string? base64String)
        {
            string DefaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default.jpg");

            if (string.IsNullOrEmpty(base64String))
            {
                return System.IO.File.Exists(DefaultImagePath) ? System.IO.File.ReadAllBytes(DefaultImagePath) : Array.Empty<byte>();
            }
            // Remove the data prefix
            if (base64String.Contains(","))
            {
                base64String = base64String.Split(',')[1];
            }

            return Convert.FromBase64String(base64String);
        }
        [HttpPost("login")]
        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login([FromBody] LoginModel login, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies)
        {
            var user = await _userManager.FindByNameAsync(login.Username) ?? await _userManager.FindByEmailAsync(login.Username);
            if (user == null)
            {
                return TypedResults.Problem("User not found", statusCode: StatusCodes.Status401Unauthorized);
            }
            var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
            var isPersistent = (useCookies == true) && (useSessionCookies != true);
            _signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

            var result = await _signInManager.PasswordSignInAsync(user.UserName, login.Password, isPersistent, lockoutOnFailure: true);

            if (result.RequiresTwoFactor)
            {
                if (!string.IsNullOrEmpty(login.TwoFactorCode))
                {
                    result = await _signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
                }
                else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                {
                    result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
                }
            }
            if (!result.Succeeded)
            {
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }

            // The signInManager already produced the needed response in the form of a cookie or bearer token.
            return TypedResults.Empty;
        }

        [HttpDelete("delete/{email}")]
        public async Task<Results<Ok, ValidationProblem>> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return TypedResults.Ok();
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }
            return TypedResults.Ok();
        }
        [HttpPut("update/{email}")]

        public async Task<IActionResult> UpdateUser(string email, [FromBody] UpdateModel updateModel)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check if the new username or email already exists (optional validation)
            if ((await _userManager.FindByNameAsync(updateModel.Username)) != null && updateModel.Username != user.UserName)
            {
                return BadRequest("Username is already taken.");
            }

            if ((await _userManager.FindByEmailAsync(updateModel.Email)) != null && updateModel.Email != user.Email)
            {
                return BadRequest("Email is already taken.");
            }

            //// Change password properly
            //if (!string.IsNullOrEmpty(updateModel.Password))
            //{
            //    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            //    var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, updateModel.Password);

            //    if (!passwordResult.Succeeded)
            //    {
            //        return BadRequest(passwordResult.Errors);
            //    }
            //}

            // Update other fields
            user.UserName = updateModel.Username;
            user.Email = updateModel.Email;

            // Update profile image only if provided
            if (!string.IsNullOrEmpty(updateModel.ProfileImageUrl))
            {
                user.ProfileImage = ConvertToByteArray(updateModel.ProfileImageUrl);
            }

            try
            {
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Another user has modified this record.");
            }
            await _signInManager.SignOutAsync();
            return Ok("User updated successfully.");
        }

        [HttpPost("changepassword/{email}")]
        public async Task<Results<Ok, ValidationProblem>> ChangePasswordAsync(ChangeModel changeModel)
        {
            var user = await _userManager.FindByIdAsync(changeModel.Id);
            var result = await _userManager.ChangePasswordAsync(user, changeModel.OldPassword, changeModel.NewPassword);
         
            if(!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            return TypedResults.Ok();
        }

        private string GenerateToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier,user.Id!)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) =>
            TypedResults.ValidationProblem(new Dictionary<string, string[]> {
            { errorCode, [errorDescription] }
            });

        private static ValidationProblem CreateValidationProblem(IdentityResult result)
        {
            // We expect a single error code and description in the normal case.
            // This could be golfed with GroupBy and ToDictionary, but perf! :P
            Debug.Assert(!result.Succeeded);
            var errorDictionary = new Dictionary<string, string[]>(1);

            foreach (var error in result.Errors)
            {
                string[] newDescriptions;

                if (errorDictionary.TryGetValue(error.Code, out var descriptions))
                {
                    newDescriptions = new string[descriptions.Length + 1];
                    Array.Copy(descriptions, newDescriptions, descriptions.Length);
                    newDescriptions[descriptions.Length] = error.Description;
                }
                else
                {
                    newDescriptions = [error.Description];
                }

                errorDictionary[error.Code] = newDescriptions;
            }

            return TypedResults.ValidationProblem(errorDictionary);
        }
    }
    public class LoginModel
    {
        [Required]
        [MinLength(3)]
        public string Username { get; init; }

        [Required]
        public string Password { get; init; }

        public string? TwoFactorCode { get; init; }

        public string? TwoFactorRecoveryCode { get; init; }
    }


    public class RegisterModel
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string? ProfileImageUrl { get; set; }
    }
    public class UpdateModel
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string? ProfileImageUrl { get; set; }
    }
    public class ChangeModel
    {
        public string Id { get; set; } = "";

        [Required]
        public string OldPassword { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";
        
    }
}
