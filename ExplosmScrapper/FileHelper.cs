using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

namespace ExplosmScrapper
{
    public static class FileHelper
    {
        private static readonly string DefaultSaveDirectory = "Comics";
        private static readonly string ComicsDbFileName = "comics.json";
        private static readonly string ComicsDbFilePath = Path.Combine(DefaultSaveDirectory, ComicsDbFileName);
        private static readonly string FilesDirectory = Path.Combine(DefaultSaveDirectory, "files");
        public static string GetFilePath(string fileName) => Path.Combine(FilesDirectory, fileName);

        private static void EnsureDefaultDirectoriesCreated()
        {
            if (!Directory.Exists(DefaultSaveDirectory))
                Directory.CreateDirectory(DefaultSaveDirectory);

            if (!Directory.Exists(FilesDirectory))
                Directory.CreateDirectory(FilesDirectory);
        }

        public static bool Exists(string fileName) {
            var filePath = GetFilePath(fileName); 
            
            if (File.Exists(filePath)) {
                var fi = new FileInfo(filePath);
                if (fi.Length == 0) {
                    File.Delete(filePath);
                    return false;
                } else {
                    
                }
                return true;
            }
            return false;
        }
        public static async Task SaveComicsDb(List<Comic> comics)
        {
            var json =
                JsonSerializer.Serialize(
                    comics,
                    new JsonSerializerOptions { WriteIndented = true });

            EnsureDefaultDirectoriesCreated();
            await File.WriteAllTextAsync(ComicsDbFilePath, json);
        }

        public static async Task<List<Comic>> GetComicsDb()
        {
            if (File.Exists(ComicsDbFilePath))
            {
                var json = await File.ReadAllTextAsync(ComicsDbFilePath);
                return JsonSerializer.Deserialize<List<Comic>>(json);
            }
            return new List<Comic>();
        }

        public static async Task SaveFile(
            string fileName,
            HttpResponseMessage responseMessage)
        {
            var filePath = GetFilePath(fileName);
            if (File.Exists(filePath)) return;

            using var fs = new FileStream(filePath, FileMode.CreateNew);
            await responseMessage.Content.CopyToAsync(fs);
        }
    }
}