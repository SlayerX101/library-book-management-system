namespace LibraryBookManagementSystem.Models;

public sealed class Book
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Isbn { get; set; }
    public required string Category { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
