namespace LibraryBookManagementSystem.Models;

public sealed class Member
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public DateTime JoinedAt { get; init; } = DateTime.UtcNow;
}
