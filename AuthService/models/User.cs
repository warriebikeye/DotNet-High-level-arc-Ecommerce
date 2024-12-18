namespace AuthServicex.models;
// AuthService/Models/User.cs
public class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
