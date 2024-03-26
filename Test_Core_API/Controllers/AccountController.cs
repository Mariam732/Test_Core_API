using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Test_Core_API.DTOS;
using Test_Core_API.Entity;
using Test_Core_API.Model;

namespace Test_Core_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {


        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        public Context Contxt { get; }


        // Iconfiguration : to read jwt from appsetting.json
        public AccountController(UserManager<ApplicationUser> _userManager, IConfiguration _config , Context context)
        {
            userManager = _userManager;
            config = _config;
            Contxt = context;
        }




        [HttpPost("Register")] // Create User [Identity]
        public async Task<IActionResult> Registaration(RegisterDTO registerUser)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser();
                user.Email = registerUser.Email;
                user.UserName = registerUser.UserName;

                IdentityResult res = await userManager.CreateAsync(user, registerUser.Password);
                if (res.Succeeded)
                {
                    // User creation succeeded, add it to the database
                    User newUser = new User
                    {
                        UserName = user.UserName,
                        Password = registerUser.Password,
                        Email = user.Email,
                        LastLoginTime = DateTime.Now
                    };

                    Contxt.users.Add(newUser);
                    await Contxt.SaveChangesAsync();

                    return Ok("Account Added Successfully");
                }

                // User creation failed, return the error messages
                var errorList = res.Errors.Select(err => err.Description).ToList();
                return BadRequest(errorList);
            }

            return BadRequest(ModelState);
        }





        [HttpPost("Login")] //Verify User & Create Token
        public async Task<IActionResult> Login(LoginDTO loginUser)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser userFromDB = await userManager.FindByNameAsync(loginUser.UserName);
                if (userFromDB is null)
                {
                    return Unauthorized();
                }
                bool found = await userManager.CheckPasswordAsync(userFromDB, loginUser.Password);
                if (found)
                {
                    #region Create Cliams For PayLoad
                    var Claims = new List<Claim>();
                    Claims.Add(new Claim(ClaimTypes.Name, userFromDB.UserName));
                    Claims.Add(new Claim(ClaimTypes.NameIdentifier, userFromDB.Id));
                    Claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // to make id unique use GUID
                    //Check Roles User
                    var Roles = await userManager.GetRolesAsync(userFromDB);
                    foreach (var role in Roles)
                    {
                        Claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                    #endregion



                    #region Secuirty Key
                    SecurityKey SecretKey =
                                  new SymmetricSecurityKey(
                                      Encoding.UTF8.GetBytes(config["JWT:Secret"]));
                    SigningCredentials SignCred =
                        new SigningCredentials(SecretKey, SecurityAlgorithms.HmacSha256);
                    #endregion



                    //Create Token
                    #region Create Token
                    JwtSecurityToken token = new JwtSecurityToken(
                                  issuer: config["JWT:ValidateAssure"],
                                  audience: config["JWT:ValidateAudiance"],
                                  claims: Claims,
                                  expires: DateTime.Now.AddHours(1),
                                  signingCredentials: SignCred
                                  );
                    #endregion




                    // to show token as Compact use JwtSecurityTokenHandler()
                    return Ok(
                        new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            exp = token.ValidTo // time for expire token
                        }
                        );
                }
                return Unauthorized();
            }
            return BadRequest();
        }





        [HttpPut("Update")] // Update UserEmail
        public async Task<IActionResult> UpdateUser(UpdateDTO updatedUser)
        {
            if (ModelState.IsValid)
            {
                // Find the user by username
                ApplicationUser userFromDB = await userManager.FindByNameAsync(updatedUser.UserName);
                if (userFromDB == null)
                {
                    return NotFound("User not found");
                }

                // Update user properties
                userFromDB.Email = updatedUser.Email;
                userFromDB.UserName = updatedUser.UserName;
          

                // Update user in Identity DB
                IdentityResult result = await userManager.UpdateAsync(userFromDB);
                if (result.Succeeded)
                {
                    // Update user in your custom database if needed
                    // Example: Update email
                    User dbUser = await Contxt.users.FirstOrDefaultAsync(u => u.UserName == updatedUser.UserName);
                    if (dbUser != null)
                    {
                        dbUser.Email = updatedUser.Email;
                        await Contxt.SaveChangesAsync();
                    }
                    return Ok(new { msg = "Update operation is done", UpdatedUser = updatedUser });
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            return BadRequest(ModelState);
        }




        [HttpDelete("Delete")] // Delete User
        public async Task<IActionResult> DeleteUser(string userName)
        {
            ApplicationUser userFromDB = await userManager.FindByNameAsync(userName);
            if (userFromDB == null)
            {
                return NotFound("User not found");
            }

            // Delete user from Identity DB
            IdentityResult result = await userManager.DeleteAsync(userFromDB);
            if (result.Succeeded)
            {
                // Delete user from your custom database if needed
                // Example: Delete user
                User dbUser = await Contxt.users.FirstOrDefaultAsync(u => u.UserName == userName);
                if (dbUser != null)
                {
                    Contxt.users.Remove(dbUser);
                    await Contxt.SaveChangesAsync();
                }
                return Ok(new { msg = "Delete operation is done", DeletedUser = userFromDB });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }




        [HttpGet]
        public IActionResult GetAllUsers()
        {
            List<User> users = Contxt.users.ToList();
            if (users.Count == 0)
            {
                // If there are no users, return an empty list
                return Ok(new { msg = "No users found", Users = new List<User>() });
            }

            // Return list of users
            return Ok(new { msg = "All Users Data is retrieved successfully !!", Users = users });
        }






        [HttpPost("RefreshToken")] // Refresh Token
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO refreshTokenDto)
        {
            var principal = GetPrincipalFromExpiredToken(refreshTokenDto.Token);
            var username = principal.Identity.Name; // extract username from the expired token

            // You can add more validation here, such as checking if the username exists, if needed.

            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                return BadRequest("Invalid token");
            }

            var newJwtToken = GenerateToken(principal.Claims);
            var newRefreshToken = GenerateRefreshToken();

            // Save the new refresh token in  database or any other storage mechanism for future validation

            return Ok(new { token = newJwtToken, refreshToken = newRefreshToken });
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"])),
                ValidateLifetime = false // Do not validate lifetime since it's already expired
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var SecretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]));
            var SignCred = new SigningCredentials(SecretKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["JWT:ValidateAssure"],
                audience: config["JWT:ValidateAudiance"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Update token expiration time as needed
                signingCredentials: SignCred
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }



    }
}
