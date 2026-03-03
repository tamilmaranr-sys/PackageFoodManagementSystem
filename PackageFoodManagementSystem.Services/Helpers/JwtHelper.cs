using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PackageFoodManagementSystem.Repository.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public static class JwtHelper
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) for the specified authenticated user using the provided configuration settings.
    /// </summary>
    /// <remarks>The generated token includes claims for user ID, name, email, and role. The token's
    /// expiration, issuer, audience, and signing key are determined by the configuration values under the 'Jwt'
    /// section. Ensure that the configuration contains valid values for 'Jwt:Key', 'Jwt:Issuer', 'Jwt:Audience', and
    /// 'Jwt:ExpireMinutes'.</remarks>
    /// <param name="user">The authenticated user for whom the JWT will be generated. Must contain valid identification and role
    /// information.</param>
    /// <param name="config">The application configuration containing JWT settings, including issuer, audience, signing key, and expiration
    /// time. Cannot be null.</param>
    /// <returns>A string representing the generated JWT. The token includes user claims and is signed according to the
    /// configuration settings.</returns>
    public static string GenerateJwtToken(UserAuthentication user, IConfiguration config)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(config["Jwt:ExpireMinutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
