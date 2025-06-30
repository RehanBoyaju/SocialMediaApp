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
using Microsoft.AspNetCore.Components.Authorization;
using ChatApp.API.Services;
using Microsoft.AspNetCore.Authorization;
namespace ChatApp.API.Controllers
{
    [Route("")]
    [ApiController]

    public class AuthenticationController(IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManger, RoleManager<IdentityRole> roleManager, ApplicationDbContext context) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManger;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly ApplicationDbContext _context = context;

        [HttpGet("Account")]
        public async Task<IActionResult> GetAuthenticationStateAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user);
        }
        [HttpPost("register")]
        public async Task<Results<Ok, ValidationProblem>> Register([FromBody] RegisterModel model)
        {
            var existingUser = await _context.Users
                            .Where(u => u.Email == model.Email || u.UserName == model.Username)
                            .Select(u => new { u.Email, u.UserName })
                            .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                var errors = new Dictionary<string, string[]>();

                if (existingUser.Email == model.Email)
                    errors.Add(nameof(model.Email), new[] { "Email is already taken." });

                if (existingUser.UserName == model.Username)
                    errors.Add(nameof(model.Username), new[] { "Username is already taken." });

                return TypedResults.ValidationProblem(errors);
            }
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                ImageUrl = model.ImageUrl ?? GetDefaultProfileService.DefaultProfile()
            };
            
            
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }
            if (string.IsNullOrEmpty(model.Role))
            {
                IdentityResult addUserRole = await _userManager.AddToRoleAsync(user, "User");
                if (!addUserRole.Succeeded)
                {
                    return CreateValidationProblem(addUserRole);
                }
                //await SendConfirmationEmailAsync(user, userManager, context, email);
                return TypedResults.Ok();
            }
            var roleExists = await _roleManager.RoleExistsAsync(model.Role);
            if (!roleExists)
            {
                return CreateValidationProblem("Role", "Role does not exist.");
            }
            IdentityResult addRole = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addRole.Succeeded)
            {
                return CreateValidationProblem(addRole);
            }
            //await SendConfirmationEmailAsync(user, userManager, context, email);
            return TypedResults.Ok();
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

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, login.Password, isPersistent, lockoutOnFailure: true);

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

        [Authorize]
        [HttpDelete("delete/{email}")]
        public async Task<Results<Ok, ValidationProblem>> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return TypedResults.Ok();
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            _ = await _userManager.RemoveFromRolesAsync(user, userRoles);

            var userId = user.Id;

            // Remove related chat messages
            var chatMessages = _context.ChatMessages.Where(m => m.ToUserId == userId);
            _context.ChatMessages.RemoveRange(chatMessages);

            // Remove group memberships
            var groupMembers = _context.GroupMembers.Where(gm => gm.UserId == userId);
            _context.GroupMembers.RemoveRange(groupMembers);

            // Finally, delete the user

            if (user != null)
            {
                _context.Users.Remove(user);
            }

            // Save changes
            await _context.SaveChangesAsync();

            return TypedResults.Ok();
        }

        [Authorize]
        [HttpPut("update/{email}")]
        public async Task<IActionResult> UpdateUser(string email, [FromBody] UpdateModel updateModel)
        {

            using var transaction = _context.Database.BeginTransaction();
            try
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
                if (!string.IsNullOrEmpty(updateModel.Role))
                {
                    var roleExists = await _roleManager.RoleExistsAsync(updateModel.Role);
                    if (!roleExists)
                    {
                        return BadRequest("Role does not exist.");
                    }
                }
                else
                {
                    updateModel.Role = (await _userManager.GetRolesAsync(user)).First();
                }


                // Update other fields
                user.UserName = updateModel.Username;
                user.Email = updateModel.Email;

                // Update profile image only if provided
                if (!string.IsNullOrEmpty(updateModel.ImageUrl))
                {
                    user.ImageUrl = updateModel.ImageUrl;
                }


                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeRoles = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeRoles.Succeeded)
                {
                    return BadRequest("Failed to remove roles");
                }
                var addRole = await _userManager.AddToRoleAsync(user, updateModel.Role);
                if (!addRole.Succeeded)
                {
                    return BadRequest("Failed to add role");
                }


                await _signInManager.SignOutAsync();
                await transaction.CommitAsync();
                return Ok("User updated successfully.");

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("changepassword")]
        public async Task<Results<Ok, ValidationProblem>> ChangePasswordAsync(ChangeModel changeModel)
        {
            var user = await _userManager.FindByIdAsync(changeModel.Id);
            if (user == null)
            {
                return CreateValidationProblem("User", "User not found.");
            }
            var result = await _userManager.ChangePasswordAsync(user, changeModel.OldPassword, changeModel.NewPassword);

            if (!result.Succeeded)
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
        public required string Username { get; init; }

        [Required]
        public required string Password { get; init; }

        public string? TwoFactorCode { get; init; }

        public string? TwoFactorRecoveryCode { get; init; }
    }

    public class RegisterModel
    {
        [Required]
        [MinLength(3)]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }


        public string Role { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
    }

    public class UpdateModel
    {
        [Required]
        [MinLength(3)]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
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
