# Library Book Management System

A simple C#/.NET ASP.NET Core web application for managing a small library.

## Features

- Add and list books
- Search books by title, author, ISBN, or category
- Add and list members
- Borrow and return books
- Track active and overdue loans
- Persist data to `library-data.json`

## Run Locally

```powershell
dotnet run --urls http://localhost:5058
```

Then open <http://localhost:5058> in your browser.

## Build

```powershell
dotnet build
```

## Publish

```powershell
dotnet publish -c Release
```

The app can be deployed to any host that supports ASP.NET Core, such as Azure App Service, Render, Railway, or a Windows/Linux VPS with the .NET runtime installed.

Data is saved automatically after each create, borrow, and return action.
