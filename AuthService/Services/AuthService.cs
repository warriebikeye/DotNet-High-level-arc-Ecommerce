namespace AuthServicex.Services;

// AuthService/Services/AuthService.cs
//using AuthService.models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AuthServicex.Data;
using AuthServicex.models;

public class AuthService
{
    private readonly DbContext _dbContext;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(DbContext dbContext, IConfiguration config, ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _config = config;
        _logger = logger;
    }

    // Hash password using SHA256
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    // Register a new user
    public async Task<User> RegisterAsync(string email, string password)
    {
        var hashedPassword = HashPassword(password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hashedPassword,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        using (var connection = _dbContext.CreateConnection())
        {
            var query = "INSERT INTO Users (Id, Email, PasswordHash, IsVerified, CreatedAt) VALUES (@Id, @Email, @PasswordHash, @IsVerified, @CreatedAt)";
            await connection.ExecuteAsync(query, user);
        }

        _logger.LogInformation("User registered with email: {Email}", email);

        // Send verification email
        await SendVerificationEmailAsync(user);

        return user;
    }

    // Send verification email with a token
    private async Task SendVerificationEmailAsync(User user)
    {
        var token = Guid.NewGuid().ToString();
        var verificationLink = $"{_config["App:BaseUrl"]}/api/auth/verify?email={user.Email}&token={token}";

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_config["Smtp:From"]),
            Subject = "Verify your email",
            Body = $"Click the link to verify your email: {verificationLink}",
            IsBodyHtml = true
        };
        mailMessage.To.Add(user.Email);

        using var smtpClient = new SmtpClient(_config["Smtp:Host"])
        {
            Port = int.Parse(_config["Smtp:Port"]),
            Credentials = new NetworkCredential(_config["Smtp:Username"], _config["Smtp:Password"]),
            EnableSsl = true
        };

        await smtpClient.SendMailAsync(mailMessage);
        _logger.LogInformation("Verification email sent to: {Email}", user.Email);
    }

    // Verify user's email
    public async Task<bool> VerifyEmailAsync(string email)
    {
        using (var connection = _dbContext.CreateConnection())
        {
            var query = "UPDATE Users SET IsVerified = TRUE WHERE Email = @Email";
            var rowsAffected = await connection.ExecuteAsync(query, new { Email = email });
            _logger.LogInformation("User email verified: {Email}", email);
            return rowsAffected > 0;
        }
    }

    // Authenticate user and generate JWT
    public async Task<string> LoginAsync(string email, string password)
    {
        var hashedPassword = HashPassword(password);
        using (var connection = _dbContext.CreateConnection())
        {
            var query = "SELECT * FROM Users WHERE Email = @Email AND PasswordHash = @PasswordHash";
            var user = await connection.QuerySingleOrDefaultAsync<User>(query, new { Email = email, PasswordHash = hashedPassword });

            if (user == null || !user.IsVerified)
            {
                _logger.LogWarning("Invalid login attempt for email: {Email}", email);
                throw new UnauthorizedAccessException("Invalid credentials or email not verified.");
            }

            var token = GenerateJwtToken(user);
            _logger.LogInformation("User logged in: {Email}", email);
            return token;
        }
    }

    // Generate JWT token
    private string GenerateJwtToken(User user)
    {
        var jwtHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Secret"]);
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = jwtHandler.CreateToken(tokenDescriptor);
        return jwtHandler.WriteToken(token);
    }
}
