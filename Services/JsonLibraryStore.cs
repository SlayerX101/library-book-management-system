using System.Text.Json;
using LibraryBookManagementSystem.Models;

namespace LibraryBookManagementSystem.Services;

public sealed class JsonLibraryStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _path;

    public JsonLibraryStore(string path)
    {
        _path = path;
    }

    public async Task<LibraryData> LoadAsync()
    {
        if (!File.Exists(_path))
        {
            return new LibraryData();
        }

        await using var stream = File.OpenRead(_path);
        var data = await JsonSerializer.DeserializeAsync<LibraryData>(stream, SerializerOptions);
        return data ?? new LibraryData();
    }

    public async Task SaveAsync(LibraryData data)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_path);
        await JsonSerializer.SerializeAsync(stream, data, SerializerOptions);
    }
}
