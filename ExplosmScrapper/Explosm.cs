using Dasync.Collections;
using LtGt;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.RegularExpressions;
using LtGtResult = Microsoft.FSharp.Core.FSharpResult<LtGt.HtmlDocument, string>;
using static ExplosmScrapper.ConsolePrint;

namespace ExplosmScrapper;

public partial class Explosm(
        HttpClient http,
        DownloadHelper downloadHelper,
        IOptionsMonitor<ExplosmOptions> opts
    ) : IDisposable
{
    private readonly HttpClient _http = http;
    private readonly DownloadHelper _downloader = downloadHelper;
    private readonly ExplosmOptions _opts = opts.CurrentValue;

    private async Task<LtGtResult?> FetchHtml(string pageUrl)
    {
        string html;

        try
        {
            html = await _http.GetStringAsync(pageUrl);
        }
        catch
        {
            WriteError($"Error Fetching Page: {pageUrl}");
            return null;
        }

        var result = Html.TryParseDocument(html);
        return result;
    }

    private async Task<Comic?> FetchComic(int id)
    {
        var pageUrl = _opts.BaseUrl + id;
        var result = await FetchHtml(pageUrl);
        if (result is null)
        {
            return null;
        }

        var validResult = (LtGtResult)result;
        var link =
            validResult.ResultValue
                  .GetElementById(_opts.ImageId)
                  ?.Attributes
                  .FirstOrDefault(a => a.Name == "src")
                  ?.Value;

        var comicInfo =
            validResult.ResultValue
                  .GetElementById(_opts.InfoId)
                  .GetInnerText();

        if (link is null || comicInfo is null)
            return null;

        var (author, publishedAt) = comicInfo.ParseComicInfo();
        var imageLink =
            link.StartsWith("http")
            ? link
            : $"http:{link}";

        var comic = new Comic
        {
            Id          = id,
            Author      = author,
            PublishedAt = publishedAt,
            PageUrl     = pageUrl,
            ImageLink   = imageLink
        };

        var fallbackFileName = $"no_name_{Guid.NewGuid()}";

        try
        {
            comic.LocalFileName = comic.GetLocalSavePathForComic();
        }
        catch (Exception e)
        {
            comic.LocalFileName = fallbackFileName;
            WriteError($"Error getting file name for: {imageLink}. Fallback: {fallbackFileName}.\n{e.Message}");
        }

        return comic;
    }

    private async Task<int> FindLastComicId()
    {
        var result = await FetchHtml(_opts.BaseUrl + "latest");
        if (result is null || (result?.IsError ?? false))
        {
            return _opts.EndIndex;
        }

        var validResult = (LtGtResult)result!;

        var secondLastHref =
            validResult.ResultValue
                .GetElementsByClassName("nav-previous").FirstOrDefault()
                ?.Attributes
                .FirstOrDefault(a => a.Name is "href")
                ?.Value;

        if (secondLastHref is null)
            return _opts.EndIndex;

        var regex = IdMatchRegex();
        var idMatch = regex.Match(secondLastHref);

        var lastIndex = _opts.EndIndex;
        if (idMatch.Success)
        {
            lastIndex = int.Parse(idMatch.Value) + 1;
            WriteInfo($"Last Comic Id: {lastIndex}.");
        }

        return lastIndex;
    }

    private async Task<List<Comic>> FetchComics(IEnumerable<int> ids)
    {
        List<Comic> comics = [];
        var listLock = new object();

        void addComic(Comic newComic)
        {
            lock (listLock)
            {
                comics?.Add(newComic);
            }
        }

        WriteInfo($"Fetching {ids.Count()} comic info...");

        var sw = new Stopwatch();
        sw.Start();
        await ids
            .ParallelForEachAsync(
                async id =>
                {
                    var maybeComic = await FetchComic(id);
                    if (!(maybeComic is null)) addComic(maybeComic);
                },
                _opts.MaxDegreeOfParallelism);

        sw.Stop();

        WriteInfo($"Stopwatch: {sw.ElapsedMilliseconds / 1000F:0.00} seconds has elapsed.");
        WriteInfo($"Found {comics.Count} comics out of {ids.Count()}.");

        return comics;
    }

    public async Task<List<Comic>> FetchAllComics()
    {
        var first = _opts.StartIndex;
        var lastComicRemote = await FindLastComicId();
        var last = Math.Min(_opts.EndIndex, lastComicRemote);

        return await FetchComics(Enumerable.Range(first, last - first + 1));
    }

    public async Task<List<Comic>> FetchNewComics(List<Comic> comics)
    {
        var localMax = comics.Max(c => c.Id);
        var remoteMax = await FindLastComicId();

        if (remoteMax > localMax)
        {
            var newComicIds = Enumerable.Range(localMax + 1, remoteMax - localMax);
            var newComics = await FetchComics(newComicIds);
            comics.AddRange(newComics);
            WriteInfo($"Fetched {newComics.Count} new comics.\n");
        }

        return comics;
    }

    public async Task DownloadComicFiles(List<Comic> comics)
    {
        var failedDownloads = new List<Comic>();
        var listLock = new object();

        void addFailedItem(Comic failedItem)
        {
            lock (listLock!)
            {
                failedDownloads?.Add(failedItem);
            }
        }

        WriteInfo("Downloading Comics...");
        var sw = new Stopwatch();
        sw.Start();
        await comics.ParallelForEachAsync(
            async comic =>
            {
                var success = await _downloader.Download(comic, false);
                if (success is false)
                {
                    addFailedItem(comic);
                }
            },
            _opts.MaxDegreeOfParallelism
        );

        if (failedDownloads.Count is not 0)
        {
            WriteInfo($"Retrying {failedDownloads.Count} Failed Downloads In Legacy Mode...");
            failedDownloads
                .ForEach(comic => _downloader.DownloadLegacy(comic));
        }

        sw.Stop();
        WriteInfo($"Stopwatch: {sw.ElapsedMilliseconds / 1000F:0.00} seconds has elapsed.");
        WriteInfo($"Comics download complete.");
    }

    public void Dispose()
    {
        _http.Dispose();
        _downloader.Dispose();
    }

    [GeneratedRegex(@"\b\d+\b")]
    private static partial Regex IdMatchRegex();
}
