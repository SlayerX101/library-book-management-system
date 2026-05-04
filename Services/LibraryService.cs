using LibraryBookManagementSystem.Models;

namespace LibraryBookManagementSystem.Services;

public sealed class LibraryService
{
    private readonly LibraryData _data;

    public LibraryService(LibraryData data)
    {
        _data = data;
    }

    public IReadOnlyCollection<Book> GetBooks() => _data.Books;

    public IReadOnlyCollection<Member> GetMembers() => _data.Members;

    public IEnumerable<Loan> GetActiveLoans() => _data.Loans.Where(loan => loan.ReturnedAt is null);

    public IEnumerable<Loan> GetOverdueLoans(DateTime? now = null)
    {
        var today = (now ?? DateTime.UtcNow).Date;
        return GetActiveLoans().Where(loan => loan.DueDate.Date < today);
    }

    public IEnumerable<Book> SearchBooks(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return GetBooks();
        }

        return _data.Books.Where(book =>
            Contains(book.Title, query)
            || Contains(book.Author, query)
            || Contains(book.Isbn, query)
            || Contains(book.Category, query));
    }

    public OperationResult<Book> AddBook(string title, string author, string isbn, string category, int totalCopies)
    {
        if (totalCopies < 1)
        {
            return OperationResult<Book>.Failure("A book must have at least one copy.");
        }

        if (_data.Books.Any(book => string.Equals(book.Isbn, isbn, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult<Book>.Failure("A book with that ISBN already exists.");
        }

        var book = new Book
        {
            Title = title.Trim(),
            Author = author.Trim(),
            Isbn = isbn.Trim(),
            Category = category.Trim(),
            TotalCopies = totalCopies,
            AvailableCopies = totalCopies
        };

        _data.Books.Add(book);
        return OperationResult<Book>.Success(book);
    }

    public OperationResult<Member> AddMember(string fullName, string email, string phone)
    {
        if (!email.Contains('@', StringComparison.Ordinal))
        {
            return OperationResult<Member>.Failure("Enter a valid email address.");
        }

        if (_data.Members.Any(member => string.Equals(member.Email, email, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult<Member>.Failure("A member with that email already exists.");
        }

        var member = new Member
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            Phone = phone.Trim()
        };

        _data.Members.Add(member);
        return OperationResult<Member>.Success(member);
    }

    public OperationResult<Loan> BorrowBook(Guid bookId, Guid memberId, int loanDays)
    {
        var book = _data.Books.FirstOrDefault(book => book.Id == bookId);
        if (book is null)
        {
            return OperationResult<Loan>.Failure("Book not found.");
        }

        if (!_data.Members.Any(member => member.Id == memberId))
        {
            return OperationResult<Loan>.Failure("Member not found.");
        }

        if (book.AvailableCopies < 1)
        {
            return OperationResult<Loan>.Failure("No copies are available for this book.");
        }

        var activeLoanExists = _data.Loans.Any(loan =>
            loan.BookId == bookId
            && loan.MemberId == memberId
            && loan.ReturnedAt is null);

        if (activeLoanExists)
        {
            return OperationResult<Loan>.Failure("This member already has an active loan for that book.");
        }

        var loan = new Loan
        {
            BookId = bookId,
            MemberId = memberId,
            DueDate = DateTime.UtcNow.Date.AddDays(loanDays)
        };

        book.AvailableCopies--;
        _data.Loans.Add(loan);
        return OperationResult<Loan>.Success(loan);
    }

    public OperationResult<Loan> ReturnBook(Guid loanId)
    {
        var loan = _data.Loans.FirstOrDefault(loan => loan.Id == loanId);
        if (loan is null)
        {
            return OperationResult<Loan>.Failure("Loan not found.");
        }

        if (loan.ReturnedAt is not null)
        {
            return OperationResult<Loan>.Failure("That loan has already been returned.");
        }

        var book = _data.Books.FirstOrDefault(book => book.Id == loan.BookId);
        if (book is null)
        {
            return OperationResult<Loan>.Failure("The book for this loan no longer exists.");
        }

        loan.ReturnedAt = DateTime.UtcNow;
        book.AvailableCopies++;
        return OperationResult<Loan>.Success(loan);
    }

    private static bool Contains(string value, string query) =>
        value.Contains(query, StringComparison.OrdinalIgnoreCase);
}
