using System.IO;
using System.Text.RegularExpressions;

namespace ExplosmScrapper {
    public static class PathHelper {
        public static string MakeValidFileName (this string path)
        {
            path = "Comics" + path;
            string dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);

            var invalidPathNameChars = new string (Path.GetInvalidPathChars ());
            var invalidChars = Regex.Escape (invalidPathNameChars + ':' + '\'');
            var invalidRegStr = string.Format (@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace (path, invalidRegStr, "_");
        }
    }
}