using System.Globalization;
using System;
using System.Text.RegularExpressions;

namespace ExplosmScrapper
{
    public static class ComicInfoHelper
    {
        private static DateTimeOffset? ExtractDate(string inputText)
        {
            var regex = new Regex(@"\b\d{4}\.\d{2}.\d{2}\b");
            var dateStr = regex.Match(inputText);

            if (dateStr is null) return null;

            var success =
                DateTimeOffset.TryParseExact(
                    dateStr.Value,
                    "yyyy.MM.dd",
                    CultureInfo.InstalledUICulture,
                    DateTimeStyles.AssumeUniversal,
                    out var dto);

            if (!success) return null;
            return dto;
        }

        private static string ExtractAuthorName(string comicInfo)
        {
            var startIndex = comicInfo.IndexOf("by") + 3;
            var authorName = 
                startIndex < comicInfo.Length - 1
                ? comicInfo[startIndex..].Trim()
                : string.Empty;
            
            return authorName;
        }

        public static (string author, DateTimeOffset publishedAt) ParseComicInfo(this string comicInfo)
        {
            var maybeDate = ExtractDate(comicInfo);
            var authorName = ExtractAuthorName(comicInfo);
            return (authorName, maybeDate ?? DateTimeOffset.UnixEpoch);
        }

        private static string FlattenString(this string? str) =>
            str?.Replace(' ', '_')
                ?.ToLower()
                ?? "";

        public static string GetLocalSavePathForComic(this Comic comic)
        {
            var imageUri = new Uri(comic.ImageLink!);
            var originalFileName =
                imageUri.LocalPath.Substring(
                    imageUri.LocalPath.LastIndexOf("/") + 1);

            var localPath =
                string.IsNullOrWhiteSpace(comic.Author)
                ? originalFileName
                : $"{comic.Author.FlattenString()}_{originalFileName}";

            return localPath;
        }
    }
}