using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace pledgemanager.shared.Platform;

public class SignalRAuthService
{
    private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();

    public string Endpoint { get; }

    public string AccessKey { get; }

    public SignalRAuthService(string connectionString)
    {
        (Endpoint, AccessKey) = ExtractInternalInformation(connectionString);
    }

    public string GenerateAccessToken(string audience)
    {
        IEnumerable<Claim> claims = new List<Claim>();
        // if (userId != null)
        // {
        //     claims = new[]
        //     {
        //         new Claim(ClaimTypes.NameIdentifier, userId)
        //     };
        // }

        var expire = DateTime.UtcNow.Add(TimeSpan.FromHours(1));
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AccessKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenToBeIssued = JwtTokenHandler.CreateJwtSecurityToken(
            issuer: null,
            audience: audience,
            subject: claims == null ? null : new ClaimsIdentity(claims),
            expires: expire,
            signingCredentials: credentials);
        return JwtTokenHandler.WriteToken(tokenToBeIssued);
    }

    private static readonly char[] PropertySeparator = { ';' };
    private static readonly char[] KeyValueSeparator = { '=' };
    private const string EndpointProperty = "endpoint";
    private const string AccessKeyProperty = "accesskey";

    internal static (string, string) ExtractInternalInformation(string connectionString)
    {
        var properties = connectionString.Split(PropertySeparator, StringSplitOptions.RemoveEmptyEntries);
        if (properties.Length > 1)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in properties)
            {
                var kvp = property.Split(KeyValueSeparator, 2);
                if (kvp.Length != 2) continue;

                var key = kvp[0].Trim();
                if (dictionary.ContainsKey(key))
                {
                    throw new ArgumentException($"Duplicate properties found in connection string.");
                }

                dictionary.Add(key, kvp[1].Trim());
            }

            if (dictionary.ContainsKey(EndpointProperty) && dictionary.ContainsKey(AccessKeyProperty))
            {
                return (dictionary[EndpointProperty].TrimEnd('/'), dictionary[AccessKeyProperty]);
            }
        }

        throw new ArgumentException($"Connection string missing required properties");
    }
}