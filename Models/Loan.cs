namespace LibraryBookManagementSystem.Models;

public sealed class Loan
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid BookId { get; init; }
    public Guid MemberId { get; init; }
    public DateTime BorrowedAt { get; init; } = DateTime.UtcNow;
    public DateTime DueDate { get; init; }
    public DateTime? ReturnedAt { get; set; }
}
