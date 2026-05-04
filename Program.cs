using System.Net;
using System.Text;
using LibraryBookManagementSystem.Models;
using LibraryBookManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var dataPath = Path.Combine(app.Environment.ContentRootPath, "library-data.json");
var store = new JsonLibraryStore(dataPath);
var library = await store.LoadAsync();
var service = new LibraryService(library);

app.MapGet("/", (string? message) => HtmlPage("Dashboard", RenderDashboard(service, message)));

app.MapGet("/books", (string? q, string? message) =>
{
    var books = string.IsNullOrWhiteSpace(q) ? service.GetBooks() : service.SearchBooks(q);
    return HtmlPage("Books", RenderBooks(books, q, message));
});

app.MapPost("/books", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var copies = int.TryParse(form["copies"], out var parsedCopies) ? parsedCopies : 0;
    var result = service.AddBook(
        form["title"].ToString(),
        form["author"].ToString(),
        form["isbn"].ToString(),
        form["category"].ToString(),
        copies);

    await store.SaveAsync(library);
    return RedirectWithMessage("/books", result.IsSuccess ? "Book added." : result.Error!);
});

app.MapGet("/members", (string? message) => HtmlPage("Members", RenderMembers(service.GetMembers(), message)));

app.MapPost("/members", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var result = service.AddMember(
        form["fullName"].ToString(),
        form["email"].ToString(),
        form["phone"].ToString());

    await store.SaveAsync(library);
    return RedirectWithMessage("/members", result.IsSuccess ? "Member added." : result.Error!);
});

app.MapGet("/loans", (string? message) => HtmlPage("Loans", RenderLoans(service, message)));

app.MapPost("/loans/borrow", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var bookIdValid = Guid.TryParse(form["bookId"], out var bookId);
    var memberIdValid = Guid.TryParse(form["memberId"], out var memberId);
    var days = int.TryParse(form["days"], out var parsedDays) ? parsedDays : 14;

    var result = bookIdValid && memberIdValid
        ? service.BorrowBook(bookId, memberId, days)
        : OperationResult<Loan>.Failure("Choose a valid book and member.");

    await store.SaveAsync(library);
    return RedirectWithMessage("/loans", result.IsSuccess ? "Book borrowed." : result.Error!);
});

app.MapPost("/loans/{loanId:guid}/return", async (Guid loanId) =>
{
    var result = service.ReturnBook(loanId);
    await store.SaveAsync(library);
    return RedirectWithMessage("/loans", result.IsSuccess ? "Book returned." : result.Error!);
});

app.Run();

static IResult RedirectWithMessage(string path, string message)
{
    var separator = path.Contains('?') ? '&' : '?';
    return Results.Redirect($"{path}{separator}message={WebUtility.UrlEncode(message)}");
}

static IResult HtmlPage(string title, string body)
{
    var html = $$"""
        <!doctype html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{{Escape(title)}} - Library</title>
            <style>
                :root {
                    color-scheme: light;
                    --bg: #f6f4ef;
                    --panel: #ffffff;
                    --ink: #202124;
                    --muted: #64625d;
                    --line: #ddd6ca;
                    --accent: #166b5b;
                    --accent-strong: #0f4f44;
                    --warn: #9d4b21;
                }

                * { box-sizing: border-box; }
                body {
                    margin: 0;
                    font-family: "Segoe UI", Arial, sans-serif;
                    background: var(--bg);
                    color: var(--ink);
                }

                header {
                    background: #173832;
                    color: white;
                    padding: 18px clamp(16px, 4vw, 48px);
                    display: flex;
                    align-items: center;
                    justify-content: space-between;
                    gap: 16px;
                    flex-wrap: wrap;
                }

                h1 { margin: 0; font-size: clamp(1.4rem, 3vw, 2rem); }
                h2 { margin: 0 0 12px; font-size: 1.15rem; }
                nav { display: flex; gap: 10px; flex-wrap: wrap; }
                nav a, button, .button {
                    border: 0;
                    border-radius: 6px;
                    background: var(--accent);
                    color: white;
                    cursor: pointer;
                    display: inline-block;
                    font: inherit;
                    padding: 9px 12px;
                    text-decoration: none;
                }
                nav a { background: rgba(255,255,255,.12); }
                button:hover, .button:hover, nav a:hover { background: var(--accent-strong); }
                main { padding: 24px clamp(16px, 4vw, 48px); }
                .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(260px, 1fr)); gap: 18px; }
                .panel {
                    background: var(--panel);
                    border: 1px solid var(--line);
                    border-radius: 8px;
                    padding: 18px;
                }
                .metric { font-size: 2rem; font-weight: 700; color: var(--accent-strong); }
                form { display: grid; gap: 10px; }
                label { color: var(--muted); display: grid; gap: 5px; font-size: .92rem; }
                input, select {
                    border: 1px solid var(--line);
                    border-radius: 6px;
                    font: inherit;
                    padding: 10px;
                    width: 100%;
                }
                table {
                    border-collapse: collapse;
                    width: 100%;
                }
                th, td {
                    border-bottom: 1px solid var(--line);
                    padding: 10px 8px;
                    text-align: left;
                    vertical-align: top;
                }
                th { color: var(--muted); font-size: .86rem; }
                .actions { display: flex; gap: 10px; align-items: end; flex-wrap: wrap; }
                .message {
                    background: #e5f1ee;
                    border: 1px solid #b7d9d1;
                    border-radius: 6px;
                    margin-bottom: 16px;
                    padding: 10px 12px;
                }
                .muted { color: var(--muted); }
                .overdue { color: var(--warn); font-weight: 700; }
                @media (max-width: 720px) {
                    table, thead, tbody, th, td, tr { display: block; }
                    th { display: none; }
                    td { border: 0; padding: 6px 0; }
                    tr { border-bottom: 1px solid var(--line); padding: 10px 0; }
                }
            </style>
        </head>
        <body>
            <header>
                <h1>Library Book Management</h1>
                <nav>
                    <a href="/">Dashboard</a>
                    <a href="/books">Books</a>
                    <a href="/members">Members</a>
                    <a href="/loans">Loans</a>
                </nav>
            </header>
            <main>{{body}}</main>
        </body>
        </html>
        """;

    return Results.Content(html, "text/html; charset=utf-8");
}

static string RenderDashboard(LibraryService service, string? message)
{
    var totalBooks = service.GetBooks().Count;
    var availableCopies = service.GetBooks().Sum(book => book.AvailableCopies);
    var members = service.GetMembers().Count;
    var activeLoans = service.GetActiveLoans().Count();
    var overdueLoans = service.GetOverdueLoans().Count();

    return $$"""
        {{RenderMessage(message)}}
        <section class="grid">
            {{Metric("Books", totalBooks)}}
            {{Metric("Available Copies", availableCopies)}}
            {{Metric("Members", members)}}
            {{Metric("Active Loans", activeLoans)}}
            {{Metric("Overdue Loans", overdueLoans)}}
        </section>
        """;
}

static string RenderBooks(IEnumerable<Book> books, string? query, string? message)
{
    var rows = string.Join("", books.OrderBy(book => book.Title).Select(book => $$"""
        <tr>
            <td><strong>{{Escape(book.Title)}}</strong></td>
            <td>{{Escape(book.Author)}}</td>
            <td>{{Escape(book.Isbn)}}</td>
            <td>{{Escape(book.Category)}}</td>
            <td>{{book.AvailableCopies}}/{{book.TotalCopies}}</td>
        </tr>
        """));

    if (string.IsNullOrWhiteSpace(rows))
    {
        rows = """<tr><td colspan="5" class="muted">No books found.</td></tr>""";
    }

    return $$"""
        {{RenderMessage(message)}}
        <section class="grid">
            <div class="panel">
                <h2>Add Book</h2>
                <form method="post" action="/books">
                    <label>Title <input name="title" required></label>
                    <label>Author <input name="author" required></label>
                    <label>ISBN <input name="isbn" required></label>
                    <label>Category <input name="category" required></label>
                    <label>Total copies <input name="copies" type="number" min="1" value="1" required></label>
                    <button type="submit">Add Book</button>
                </form>
            </div>
            <div class="panel">
                <h2>Search</h2>
                <form method="get" action="/books" class="actions">
                    <label>Keyword <input name="q" value="{{Escape(query ?? "")}}" placeholder="Title, author, ISBN, category"></label>
                    <button type="submit">Search</button>
                    <a class="button" href="/books">Reset</a>
                </form>
            </div>
        </section>
        <section class="panel" style="margin-top:18px">
            <h2>Books</h2>
            <table>
                <thead><tr><th>Title</th><th>Author</th><th>ISBN</th><th>Category</th><th>Copies</th></tr></thead>
                <tbody>{{rows}}</tbody>
            </table>
        </section>
        """;
}

static string RenderMembers(IEnumerable<Member> members, string? message)
{
    var rows = string.Join("", members.OrderBy(member => member.FullName).Select(member => $$"""
        <tr>
            <td><strong>{{Escape(member.FullName)}}</strong><br><span class="muted">{{Escape(member.Id.ToString())}}</span></td>
            <td>{{Escape(member.Email)}}</td>
            <td>{{Escape(member.Phone)}}</td>
            <td>{{member.JoinedAt:d}}</td>
        </tr>
        """));

    if (string.IsNullOrWhiteSpace(rows))
    {
        rows = """<tr><td colspan="4" class="muted">No members found.</td></tr>""";
    }

    return $$"""
        {{RenderMessage(message)}}
        <section class="grid">
            <div class="panel">
                <h2>Add Member</h2>
                <form method="post" action="/members">
                    <label>Full name <input name="fullName" required></label>
                    <label>Email <input name="email" type="email" required></label>
                    <label>Phone <input name="phone"></label>
                    <button type="submit">Add Member</button>
                </form>
            </div>
        </section>
        <section class="panel" style="margin-top:18px">
            <h2>Members</h2>
            <table>
                <thead><tr><th>Name</th><th>Email</th><th>Phone</th><th>Joined</th></tr></thead>
                <tbody>{{rows}}</tbody>
            </table>
        </section>
        """;
}

static string RenderLoans(LibraryService service, string? message)
{
    var books = service.GetBooks().OrderBy(book => book.Title).ToList();
    var members = service.GetMembers().OrderBy(member => member.FullName).ToList();
    var bookOptions = string.Join("", books.Select(book => $"""<option value="{Escape(book.Id.ToString())}">{Escape(book.Title)} ({book.AvailableCopies} available)</option>"""));
    var memberOptions = string.Join("", members.Select(member => $"""<option value="{Escape(member.Id.ToString())}">{Escape(member.FullName)}</option>"""));

    var rows = string.Join("", service.GetActiveLoans().OrderBy(loan => loan.DueDate).Select(loan =>
    {
        var book = books.FirstOrDefault(book => book.Id == loan.BookId);
        var member = members.FirstOrDefault(member => member.Id == loan.MemberId);
        var overdueClass = loan.DueDate.Date < DateTime.UtcNow.Date ? "overdue" : "";
        return $$"""
            <tr>
                <td><strong>{{Escape(book?.Title ?? "Missing book")}}</strong><br><span class="muted">{{Escape(loan.Id.ToString())}}</span></td>
                <td>{{Escape(member?.FullName ?? "Missing member")}}</td>
                <td>{{loan.BorrowedAt:d}}</td>
                <td class="{{overdueClass}}">{{loan.DueDate:d}}</td>
                <td>
                    <form method="post" action="/loans/{{loan.Id}}/return">
                        <button type="submit">Return</button>
                    </form>
                </td>
            </tr>
            """;
    }));

    if (string.IsNullOrWhiteSpace(rows))
    {
        rows = """<tr><td colspan="5" class="muted">No active loans found.</td></tr>""";
    }

    return $$"""
        {{RenderMessage(message)}}
        <section class="grid">
            <div class="panel">
                <h2>Borrow Book</h2>
                <form method="post" action="/loans/borrow">
                    <label>Book <select name="bookId" required>{{bookOptions}}</select></label>
                    <label>Member <select name="memberId" required>{{memberOptions}}</select></label>
                    <label>Loan days <input name="days" type="number" min="1" max="365" value="14" required></label>
                    <button type="submit">Borrow</button>
                </form>
            </div>
            <div class="panel">
                <h2>Loan Status</h2>
                {{Metric("Active", service.GetActiveLoans().Count())}}
                {{Metric("Overdue", service.GetOverdueLoans().Count())}}
            </div>
        </section>
        <section class="panel" style="margin-top:18px">
            <h2>Active Loans</h2>
            <table>
                <thead><tr><th>Book</th><th>Member</th><th>Borrowed</th><th>Due</th><th>Action</th></tr></thead>
                <tbody>{{rows}}</tbody>
            </table>
        </section>
        """;
}

static string Metric(string label, int value) => $$"""
    <div class="panel">
        <div class="metric">{{value}}</div>
        <div class="muted">{{Escape(label)}}</div>
    </div>
    """;

static string RenderMessage(string? message)
{
    return string.IsNullOrWhiteSpace(message) ? "" : $"""<div class="message">{Escape(message)}</div>""";
}

static string Escape(string value) => WebUtility.HtmlEncode(value);
