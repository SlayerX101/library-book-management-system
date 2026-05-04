namespace LibraryBookManagementSystem.Models;

public sealed class LibraryData
{
    public List<Book> Books { get; init; } = [];
    public List<Member> Members { get; init; } = [];
    public List<Loan> Loans { get; init; } = [];
}
