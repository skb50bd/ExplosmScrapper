using System.Text.Json;

namespace ExplosmScrapper;

public static class FileHelper
{
    private static readonly string DefaultSaveDirectory = "Comics";
    private static readonly string ComicsDbFileName = "comics.json";
    private static readonly string ComicsDbFilePath = Path.Combine(DefaultSaveDirectory, ComicsDbFileName);
    private static readonly string FilesDirectory = Path.Combine(DefaultSaveDirectory, "files");
    public static string GetFilePath(string fileName) => Path.Combine(FilesDirectory, fileName);

    private static void EnsureDefaultDirectoriesCreated()
    {
        if (Directory.Exists(DefaultSaveDirectory) is false)
        {
            Directory.CreateDirectory(DefaultSaveDirectory);
        }

        if (Directory.Exists(FilesDirectory) is false)
        {
            Directory.CreateDirectory(FilesDirectory);
        }
    }

    public static bool Exists(string fileName) {
        var filePath = GetFilePath(fileName);

        if (File.Exists(filePath))
        {
            var fi = new FileInfo(filePath);
            if (fi.Length is 0)
            {
                File.Delete(filePath);
                return false;
            }

            return true;
        }

        return false;
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { WriteIndented = true };

    public static async Task SaveComicsDb(List<Comic> comics)
    {
        var json = JsonSerializer.Serialize(comics, _jsonSerializerOptions);
        EnsureDefaultDirectoriesCreated();
        await File.WriteAllTextAsync(ComicsDbFilePath, json);
    }

    public static async Task<List<Comic>> GetComicsDb()
    {
        if (File.Exists(ComicsDbFilePath))
        {
            var json = await File.ReadAllTextAsync(ComicsDbFilePath);
            return JsonSerializer.Deserialize<List<Comic>>(json) ?? [];
        }

        return [];
    }

    public static async Task SaveFile(
        string fileName,
        HttpResponseMessage responseMessage)
    {
        var filePath = GetFilePath(fileName);
        if (File.Exists(filePath))
        {
            return;
        }

        using var fs = new FileStream(filePath, FileMode.CreateNew);
        await responseMessage.Content.CopyToAsync(fs);
    }
}