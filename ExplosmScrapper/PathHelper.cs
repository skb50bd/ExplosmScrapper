using System.IO;
using System.Text.RegularExpressions;

namespace ExplosmScrapper {
    public static class PathHelper {
        public static string MakeValidFileName (this string path)
        {
            path = "Comics" + path;
            var dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);

            var invalidPathNameChars = new string (Path.GetInvalidPathChars ());
            var invalidChars = Regex.Escape (invalidPathNameChars + ':' + '\'');
            var invalidRegStr = @$"([{invalidChars}]*\.+$)|([{invalidChars}]+)";

            return Regex.Replace (path, invalidRegStr, "_");
        }
    }
}